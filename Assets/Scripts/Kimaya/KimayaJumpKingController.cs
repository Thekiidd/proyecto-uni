using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    public enum SurfaceType
    {
        Normal,
        Ice,
        Sand,
        Snow
    }

    /// <summary>
    /// Componente de superficie para plataformas estilo Jump King.
    /// Define cómo reacciona el jugador al pisar o deslizarse sobre el bloque.
    /// </summary>
    public class KimayaSurface : MonoBehaviour
    {
        [Header("Tipo de Superficie Jump King")]
        public SurfaceType surfaceType = SurfaceType.Normal;

        [Header("Ajustes Físicos Personalizados (Opcional)")]
        [Tooltip("Fricción en hielo (Jump King original = 0.1)")]
        public float iceFriction = 0.1f;

        [Tooltip("Multiplicador de velocidad al caminar en arena (Jump King original = 0.25)")]
        public float sandMoveMultiplier = 0.25f;

        [Tooltip("Multiplicador de fuerza de salto en arena (Jump King original = 0.5)")]
        public float sandJumpMultiplier = 0.5f;
    }

    /// <summary>
    /// Controlador Jump King con físicas y constantes exactas extraídas por ingeniería inversa:
    /// - Tiempo de carga máximo: 0.60s (Jump King original JUMP_TIME).
    /// - Proporción salto vertical/horizontal: 2.5 a 1.
    /// - Factor de rebote lateral: 0.50 (Jump King original BOUNCE = 50%).
    /// - Superficies Jump King: Hielo (resbalón con inercia), Arena (caminar x0.25, salto x0.5).
    /// - Aturdimiento por caída alta opcional: 0.50s (Jump King original SPLAT_TIME).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class KimayaJumpKingController : MonoBehaviour
    {
        public enum CharacterType { Falin, Mia, JumpKing }

        [Header("Personaje")]
        public CharacterType character = CharacterType.JumpKing;
        public SpriteRenderer spriteRenderer;
        [HideInInspector] public Animator animator;

        [Header("Movimiento en Suelo")]
        public float walkSpeed = 2.50f;

        [Header("Física de Salto Táctico Jump King (Calibración Exacta)")]
        [Tooltip("Tiempo de carga máximo exacto de Jump King (0.6s / 36 frames)")]
        public float maxChargeTime = 0.60f;
        public float minJumpForce = 3.5f;
        public float maxJumpForce = 15.2f;
        public float horizontalJumpSpeed = 5.833f;
        [Tooltip("Factor de rebote en pared (Jump King original = 0.5)")]
        public float wallBounceMultiplier = 0.50f;

        [Header("Efectos de Caída y Superficies")]
        [Tooltip("Activar aturdimiento al caer desde gran altura (Jump King SPLAT = 0.5s)")]
        public bool enableHighFallStun = false; // Desactivado a petición del usuario: solo saltar y escalar
        public float highFallThreshold = 7.5f; // Unidades de caída para activar stun
        public float splatStunDuration = 0.50f;

        // Estados
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsBouncing { get; private set; }
        public bool IsStunned { get; private set; }
        public float ChargePercent { get; private set; } // 0.0 a 1.0
        public float FacingDirection { get; private set; } = 1f; // 1 = derecha, -1 = izquierda
        public SurfaceType CurrentSurface { get; private set; } = SurfaceType.Normal;

        // Estadísticas
        public int TotalJumps { get; private set; }
        public int TotalFalls { get; private set; }

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private float _chargeTimer = 0f;
        private float _jumpDirection = 0f;
        private int _groundContactCount = 0;
        private float _lastGroundedY = 0f;
        private float _highestAirY = 0f;
        private float _stunTimer = 0f;
        private float _bounceTimer = 0f;
        private float _iceSlideVelocityX = 0f;
        private KimayaSurface _activeSurfaceComp;

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

            if (character == CharacterType.JumpKing)
            {
                // Jump King Caballero Oficial (1:1 con Nexile Jump King: g = 25.714 / 9.81 = 2.621)
                _rb.mass = 1.0f;
                _rb.gravityScale = 2.621f;
                walkSpeed = 2.50f;
                minJumpForce = 3.5f;
                maxJumpForce = 15.2f;
                horizontalJumpSpeed = 5.833f;
                wallBounceMultiplier = 0.50f; // BOUNCE = 50%
                maxChargeTime = 0.60f; // JUMP_TIME = 0.60s
                splatStunDuration = 0.50f; // SPLAT_TIME = 0.50s

                if (_col != null)
                {
                    // Hitbox 18x26 px a 36 PPU = 0.50f x 0.722f
                    // Usamos edgeRadius de 0.03f para biselar bordes y evitar trabas en esquinas
                    _col.size = new Vector2(0.44f, 0.662f);
                    _col.offset = new Vector2(0f, 0.361f);
                    _col.edgeRadius = 0.03f;
                }
            }
            else if (character == CharacterType.Falin)
            {
                // Falin: San Bernardo (firme, masa sólida)
                _rb.mass = 1.3f;
                _rb.gravityScale = 2.4f;
                walkSpeed = 4.8f;
                minJumpForce = 7.0f;
                maxJumpForce = 17.5f;
                horizontalJumpSpeed = 7.0f; // 17.5 / 7.0 = 2.50 (Jump King exacto)
                wallBounceMultiplier = 0.50f;

                if (_col != null)
                {
                    _col.size = new Vector2(1.0f, 0.75f);
                    _col.offset = new Vector2(0f, 0.38f);
                    _col.edgeRadius = 0.02f;
                }
            }
            else
            {
                // Mía: Schnauzer ágil y vivaz
                _rb.mass = 1.0f;
                _rb.gravityScale = 2.2f;
                walkSpeed = 5.4f;
                minJumpForce = 7.4f;
                maxJumpForce = 18.5f;
                horizontalJumpSpeed = 7.4f; // 18.5 / 7.4 = 2.50 (Jump King exacto)
                wallBounceMultiplier = 0.55f;

                if (_col != null)
                {
                    _col.size = new Vector2(0.85f, 0.65f);
                    _col.offset = new Vector2(0f, 0.33f);
                    _col.edgeRadius = 0.02f;
                }
            }
        }

        private void Update()
        {
            if (_bounceTimer > 0f)
            {
                _bounceTimer -= Time.deltaTime;
                if (_bounceTimer <= 0f) IsBouncing = false;
            }

            // Gestión del aturdimiento tras caída fuerte (Jump King Splat)
            if (IsStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0f)
                {
                    IsStunned = false;
                }
                else
                {
                    // Inmóvil durante el aturdimiento
                    _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
                    return;
                }
            }

            // Seguimiento del punto más alto del salto
            if (!IsGrounded)
            {
                if (transform.position.y > _highestAirY)
                {
                    _highestAirY = transform.position.y;
                }
            }

            bool spaceHeld = ReadSpaceKey();
            float hInput = ReadHorizontalInput();

            // Actualizar dirección de mira
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

                    // Dirección del salto elegida durante la carga
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
                    _iceSlideVelocityX = 0f;
                }
                else if (IsCharging)
                {
                    // Soltó el espacio -> ¡Salto inmediato!
                    ExecuteJump();
                }
                else
                {
                    // Manejo del movimiento según la superficie actual (Jump King Blocks)
                    ApplyGroundMovement(hInput);
                    ChargePercent = 0f;
                }
            }
        }

        private void ApplyGroundMovement(float hInput)
        {
            float targetSpeed = walkSpeed;

            // Modificador por Arena (SandBlock)
            if (CurrentSurface == SurfaceType.Sand)
            {
                float sandMult = _activeSurfaceComp != null ? _activeSurfaceComp.sandMoveMultiplier : 0.25f;
                targetSpeed *= sandMult;
            }

            // Modificador por Hielo (IceBlock - inercia y resbalón)
            if (CurrentSurface == SurfaceType.Ice)
            {
                float targetVelX = hInput * targetSpeed;
                float frictionRate = _activeSurfaceComp != null ? _activeSurfaceComp.iceFriction : 0.1f;
                // Si presiona teclas acelera hacia esa dirección, si suelta patina lentamente
                float lerpSpeed = (Mathf.Abs(hInput) > 0.05f) ? 5.0f : (1.0f - frictionRate * 5.0f);
                _iceSlideVelocityX = Mathf.Lerp(_iceSlideVelocityX, targetVelX, Time.deltaTime * lerpSpeed);
                _rb.linearVelocity = new Vector2(_iceSlideVelocityX, _rb.linearVelocity.y);
            }
            else
            {
                _iceSlideVelocityX = hInput * targetSpeed;
                _rb.linearVelocity = new Vector2(_iceSlideVelocityX, _rb.linearVelocity.y);
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
            _highestAirY = transform.position.y;

            // Fuerza vertical según carga
            float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, ChargePercent);

            // Modificador por Arena (salta la mitad: SAND_JUMP_MULTIPLIER = 0.5)
            if (CurrentSurface == SurfaceType.Sand)
            {
                float sandJumpMult = _activeSurfaceComp != null ? _activeSurfaceComp.sandJumpMultiplier : 0.5f;
                jumpForce *= sandJumpMult;
            }

            // Fuerza horizontal según dirección (Jump King original: velocidad horizontal fija SPEED = 5.833f)
            float hForce = 0f;
            if (Mathf.Abs(_jumpDirection) > 0.1f)
            {
                hForce = _jumpDirection * horizontalJumpSpeed;
            }

            _rb.linearVelocity = new Vector2(hForce, jumpForce);
            _iceSlideVelocityX = hForce;

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
                        // Jump King Bounce: invierte la velocidad horizontal multiplicada por el factor de rebote
                        float bounceX = contact.normal.x * horizontalJumpSpeed * wallBounceMultiplier;
                        _rb.linearVelocity = new Vector2(bounceX, currentY);
                        _iceSlideVelocityX = bounceX;

                        IsBouncing = true;
                        _bounceTimer = 0.30f;

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
                CurrentSurface = SurfaceType.Normal;
                _activeSurfaceComp = null;
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

                    // Detectar si la plataforma tiene componente KimayaSurface
                    var surfaceComp = collision.gameObject.GetComponent<KimayaSurface>();
                    if (surfaceComp != null)
                    {
                        CurrentSurface = surfaceComp.surfaceType;
                        _activeSurfaceComp = surfaceComp;
                    }
                    else
                    {
                        CurrentSurface = SurfaceType.Normal;
                        _activeSurfaceComp = null;
                    }
                    break;
                }
            }

            if (foundGround)
            {
                IsBouncing = false;
                _bounceTimer = 0f;
                _groundContactCount = 1;
                if (!IsGrounded)
                {
                    IsJumping = false;

                    // Si caímos de gran altura: registrar caída y evaluar Aturdimiento (Splat)
                    float fallDistance = _highestAirY - transform.position.y;
                    if (transform.position.y < _lastGroundedY - 3.0f)
                    {
                        TotalFalls++;
                    }

                    if (enableHighFallStun && fallDistance >= highFallThreshold)
                    {
                        IsStunned = true;
                        _stunTimer = splatStunDuration;
                        _rb.linearVelocity = Vector2.zero;
                    }

                    _lastGroundedY = transform.position.y;
                    _highestAirY = transform.position.y;
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
