using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Controlador Jump King robusto compatible con el Nuevo Input System y Legacy Input.
    /// Física: Salto táctico cargado con inercia fija, rebotes en muros, caídas y aturdimiento.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class KimayaJumpKingController : MonoBehaviour
    {
        public enum CharacterType { Falin, Mia }

        [Header("Personaje")]
        public CharacterType character = CharacterType.Falin;
        public SpriteRenderer spriteRenderer;
        public Animator animator;

        [Header("Movimiento")]
        public float walkSpeed = 4.0f;

        [Header("Física de Salto Jump King")]
        public float minJumpForce = 5.5f;
        public float maxJumpForce = 15.0f;
        public float maxChargeTime = 0.85f;
        public float horizontalJumpSpeed = 4.5f;
        public float wallBounceMultiplier = 0.65f;
        public float fallStunVelocityThreshold = -13f;
        public float stunDuration = 1.2f;

        // Estados
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsStunned { get; private set; }
        public float ChargePercent { get; private set; } // 0.0 a 1.0

        // Estadísticas
        public int TotalJumps { get; private set; }
        public int TotalFalls { get; private set; }

        private Rigidbody2D _rb;
        private Collider2D _col;
        private float _chargeTimer = 0f;
        private float _jumpDirection = 0f;
        private float _stunTimer = 0f;
        private float _maxFallVelocity = 0f;
        private int _groundContactCount = 0;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

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

            if (character == CharacterType.Falin)
            {
                // Falin: Firme y potente
                _rb.mass = 1.4f;
                _rb.gravityScale = 3.5f;
                walkSpeed = 3.8f;
                maxJumpForce = 15.5f;
                wallBounceMultiplier = 0.55f;
            }
            else
            {
                // Mía: Ágil y elástica
                _rb.mass = 1.0f;
                _rb.gravityScale = 3.0f;
                walkSpeed = 4.4f;
                maxJumpForce = 14.5f;
                wallBounceMultiplier = 0.80f;
            }
        }

        private void Update()
        {
            // Aturdimiento
            if (IsStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0f)
                {
                    IsStunned = false;
                }
                return;
            }

            // Lectura de Input universal
            bool spaceHeld = ReadSpaceKey();
            float hInput = ReadHorizontalInput();

            if (IsGrounded)
            {
                if (spaceHeld)
                {
                    // Cargando salto
                    IsCharging = true;
                    _chargeTimer += Time.deltaTime;
                    ChargePercent = Mathf.Clamp01(_chargeTimer / maxChargeTime);

                    if (Mathf.Abs(hInput) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(hInput);
                        if (spriteRenderer != null) spriteRenderer.flipX = (_jumpDirection < 0);
                    }
                    else
                    {
                        _jumpDirection = 0f;
                    }

                    // En carga no se desplaza
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                }
                else if (IsCharging)
                {
                    // Soltó el salto -> Ejecutar
                    ExecuteJump();
                }
                else
                {
                    // Caminar en el suelo
                    _rb.linearVelocity = new Vector2(hInput * walkSpeed, _rb.linearVelocity.y);

                    if (Mathf.Abs(hInput) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(hInput);
                        if (spriteRenderer != null) spriteRenderer.flipX = (_jumpDirection < 0);
                    }
                    else
                    {
                        _jumpDirection = 0f;
                    }

                    ChargePercent = 0f;
                }
            }
            else
            {
                // En el aire: monitorear velocidad de caída
                if (_rb.linearVelocity.y < _maxFallVelocity)
                {
                    _maxFallVelocity = _rb.linearVelocity.y;
                }
            }

            UpdateVisuals();
        }

        private void ExecuteJump()
        {
            IsCharging = false;
            IsJumping = true;
            TotalJumps++;

            float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, ChargePercent);
            float hVelocity = _jumpDirection * horizontalJumpSpeed;

            _rb.linearVelocity = new Vector2(hVelocity, jumpForce);

            _chargeTimer = 0f;
            ChargePercent = 0f;
            _maxFallVelocity = 0f;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            EvaluateContacts(collision);

            // Rebote en muros si estamos en el aire
            if (!IsGrounded && IsJumping)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (Mathf.Abs(contact.normal.x) > 0.6f)
                    {
                        float currentY = _rb.linearVelocity.y;
                        float bounceX = contact.normal.x * horizontalJumpSpeed * wallBounceMultiplier;
                        _rb.linearVelocity = new Vector2(bounceX, currentY);

                        if (spriteRenderer != null)
                        {
                            spriteRenderer.flipX = (bounceX < 0);
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
            // Al salir de colisión, si no hay contactos de suelo
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
                // Si la normal apunta hacia arriba, estamos pisando suelo firme
                if (contact.normal.y > 0.55f)
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
                    // Detectar impacto fuerte para aturdimiento
                    if (_maxFallVelocity <= fallStunVelocityThreshold)
                    {
                        IsStunned = true;
                        _stunTimer = stunDuration;
                        TotalFalls++;
                        _rb.linearVelocity = Vector2.zero;
                    }
                    _maxFallVelocity = 0f;
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

        private void UpdateVisuals()
        {
            if (animator == null) return;
            animator.SetBool("IsGrounded", IsGrounded);
            animator.SetBool("IsCharging", IsCharging);
            animator.SetBool("IsStunned", IsStunned);
            animator.SetFloat("ChargePercent", ChargePercent);
            animator.SetFloat("VerticalVelocity", _rb.linearVelocity.y);
            animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocity.x));
        }
    }
}
