using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Controlador Jump King refinado:
    /// - Salto táctico cargado de alta precisión.
    /// - Sin aturdimiento (aterrizaje fluido instantáneo).
    /// - Trayectoria balística fija en el aire (sin dirección aérea).
    /// - Rebotes predecibles en paredes.
    /// - Movimiento ágil y reactivo en el suelo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class KimayaJumpKingController : MonoBehaviour
    {
        public enum CharacterType { Falin, Mia }

        [Header("Personaje")]
        public CharacterType character = CharacterType.Falin;
        public SpriteRenderer spriteRenderer;

        [Header("Movimiento en Suelo")]
        public float walkSpeed = 5.0f;

        [Header("Física de Salto Táctico Jump King")]
        public float minJumpForce = 7.0f;
        public float maxJumpForce = 18.0f;
        public float maxChargeTime = 0.65f;
        public float horizontalJumpSpeed = 6.8f;
        public float wallBounceMultiplier = 0.6f;

        // Estados
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public bool IsJumping { get; private set; }
        public float ChargePercent { get; private set; } // 0.0 a 1.0
        public float FacingDirection { get; private set; } = 1f; // 1 = derecha, -1 = izquierda

        // Estadísticas
        public int TotalJumps { get; private set; }
        public int TotalFalls { get; private set; }

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private float _chargeTimer = 0f;
        private float _jumpDirection = 0f;
        private int _groundContactCount = 0;
        private float _lastGroundedY = 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            ApplyCharacterAttributes();
        }

        public void SetCharacter(CharacterType newChar)
        {
            character = newChar;
            ApplyCharacterAttributes();
        }

        private void ApplyCharacterAttributes()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (character == CharacterType.Falin)
            {
                // Falin: Perro grande San Bernardo (perfil ancho y firme)
                _rb.mass = 1.3f;
                _rb.gravityScale = 2.4f;
                walkSpeed = 4.8f;
                minJumpForce = 7.2f;
                maxJumpForce = 18.5f;
                horizontalJumpSpeed = 6.6f;
                wallBounceMultiplier = 0.5f;

                if (_col != null)
                {
                    _col.size = new Vector2(1.0f, 0.75f);
                    _col.offset = new Vector2(0f, 0.38f);
                }
            }
            else
            {
                // Mía: Schnauzer ágil y liviana (salto más elástico)
                _rb.mass = 1.0f;
                _rb.gravityScale = 2.2f;
                walkSpeed = 5.4f;
                minJumpForce = 6.8f;
                maxJumpForce = 17.8f;
                horizontalJumpSpeed = 7.2f;
                wallBounceMultiplier = 0.75f;

                if (_col != null)
                {
                    _col.size = new Vector2(0.85f, 0.65f);
                    _col.offset = new Vector2(0f, 0.33f);
                }
            }
        }

        private void Update()
        {
            bool spaceHeld = ReadSpaceKey();
            float hInput = ReadHorizontalInput();

            // Actualizar dirección de mira al caminar
            if (Mathf.Abs(hInput) > 0.1f)
            {
                FacingDirection = Mathf.Sign(hInput);
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (FacingDirection < 0);
                }
            }

            if (IsGrounded)
            {
                if (spaceHeld)
                {
                    // Cargando salto
                    IsCharging = true;
                    _chargeTimer += Time.deltaTime;
                    ChargePercent = Mathf.Clamp01(_chargeTimer / maxChargeTime);

                    // Dirección del salto según si mantiene tecla de dirección o neutral
                    if (Mathf.Abs(hInput) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(hInput);
                    }
                    else
                    {
                        _jumpDirection = 0f;
                    }

                    // En el suelo mientras carga se detiene
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                }
                else if (IsCharging)
                {
                    // Soltó el espacio -> ¡Salto inmediato!
                    ExecuteJump();
                }
                else
                {
                    // Caminar ágilmente en el suelo
                    _rb.linearVelocity = new Vector2(hInput * walkSpeed, _rb.linearVelocity.y);
                    ChargePercent = 0f;
                }
            }
        }

        private void ExecuteJump()
        {
            IsCharging = false;
            IsJumping = true;
            IsGrounded = false;
            _groundContactCount = 0;
            TotalJumps++;

            _lastGroundedY = transform.position.y;

            // Fuerza vertical según carga
            float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, ChargePercent);

            // Fuerza horizontal según carga y dirección
            float hForce = 0f;
            if (Mathf.Abs(_jumpDirection) > 0.1f)
            {
                // Escalamiento horizontal: saltos cortos avanzan poco, saltos cargados vuelan lejos
                float hMultiplier = Mathf.Lerp(0.45f, 1.0f, ChargePercent);
                hForce = _jumpDirection * horizontalJumpSpeed * hMultiplier;
            }

            _rb.linearVelocity = new Vector2(hForce, jumpForce);

            _chargeTimer = 0f;
            ChargePercent = 0f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            EvaluateContacts(collision);

            // Rebote en muros si estamos en el aire
            if (!IsGrounded && IsJumping)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // Impacto lateral
                    if (Mathf.Abs(contact.normal.x) > 0.65f)
                    {
                        float currentY = _rb.linearVelocity.y;
                        float bounceX = contact.normal.x * horizontalJumpSpeed * wallBounceMultiplier;
                        _rb.linearVelocity = new Vector2(bounceX, currentY);

                        FacingDirection = Mathf.Sign(bounceX);
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.flipX = (FacingDirection < 0);
                        }
                        break;
                    }
                }
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            EvaluateContacts(collision);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            _groundContactCount = Mathf.Max(0, _groundContactCount - 1);
            if (_groundContactCount == 0)
            {
                IsGrounded = false;
            }
        }

        private void EvaluateContacts(Collision2D collision)
        {
            bool foundGround = false;
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si la normal apunta hacia arriba, estamos sobre suelo firme
                if (contact.normal.y > 0.5f)
                {
                    foundGround = true;
                    break;
                }
            }

            if (foundGround)
            {
                _groundContactCount = 1;
                if (!IsGrounded)
                {
                    IsJumping = false;

                    // Si caímos por debajo de donde saltamos
                    if (transform.position.y < _lastGroundedY - 3.0f)
                    {
                        TotalFalls++;
                    }

                    _lastGroundedY = transform.position.y;
                }
                IsGrounded = true;
            }
        }

        // ================= INPUT SYSTEM UNIVERSAL =================
        private bool ReadSpaceKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                return true;
#endif
            return Input.GetKey(KeyCode.Space);
        }

        private float ReadHorizontalInput()
        {
            float val = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    val += 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    val -= 1f;
                if (val != 0f) return val;
            }
#endif
            try
            {
                val = Input.GetAxisRaw("Horizontal");
            }
            catch { }
            return val;
        }
    }
}
