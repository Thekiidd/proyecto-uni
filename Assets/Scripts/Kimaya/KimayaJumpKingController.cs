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
        public float walkSpeed = 2.55f;

        [Header("Física de Salto Táctico Jump King (Calibración Exacta con Gravity -9.81)")]
        [Tooltip("Tiempo de carga máximo exacto de Jump King (0.58s)")]
        public float maxChargeTime = 0.58f;
        public float minJumpForce = 6.40f;
        public float maxJumpForce = 17.80f;
        public float horizontalJumpSpeed = 6.25f;
        [Tooltip("Factor de rebote en pared (Jump King original = 0.5)")]
        public float wallBounceMultiplier = 0.50f;

        [Header("Efectos de Caída y Superficies")]
        [Tooltip("Activar aturdimiento al caer desde gran altura (Jump King SPLAT = 0.5s)")]
        public bool enableHighFallStun = false; // Desactivado a petición del usuario: solo saltar y escalar
        public float highFallThreshold = 7.5f; // Unidades de caída para activar stun
        public float splatStunDuration = 0.50f;

        [Header("Punto de Control y Reaparición (KIM-12)")]
        public Vector3 respawnPoint = new Vector3(0.00f, 0.85f, 0f);
        public bool hasReachedCheckpoint = false;

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
        public int TotalFalls { get; set; }

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private float _chargeTimer = 0f;
        private float _jumpDirection = 0f;
        private int _groundContactCount = 0;
        private float _lastGroundedY = 0f;
        private float _highestAirY = 0f;
        private float _stunTimer = 0f;
        private float _bounceTimer = 0f;
        private float _hurtFlashTimer = 0f;
        private float _iceSlideVelocityX = 0f;
        private KimayaSurface _activeSurfaceComp;
        private bool _mustReleaseSpace = false;
        private float _jumpGraceTimer = 0f;
        private float _chargeSfxTimer = 0f;
        private float _walkStepTimer = 0f;
        private bool _playedFallWhistle = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            ApplyCharacterAttributes();
        }

        private void Start()
        {
            // Garantizar que los valores calibrados prevalezcan sobre cualquier valor serializado antiguo en la escena
            ApplyCharacterAttributes();
        }

        public void SetCharacter(CharacterType newChar)
        {
            character = newChar;
            ApplyCharacterAttributes();
        }

        public void SetCheckpoint(Vector3 newPoint)
        {
            hasReachedCheckpoint = true;
            respawnPoint = newPoint;
            Debug.Log($"[KimayaJumpKingController] 🚩 ¡Punto de control intermedio alcanzado en {newPoint}! (KIM-12)");
        }

        private void ApplyCharacterAttributes()
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_col == null) _col = GetComponent<BoxCollider2D>();

            if (character == CharacterType.JumpKing)
            {
                transform.localScale = Vector3.one;
                _rb.mass = 1.0f;
                _rb.gravityScale = 3.15f;
                walkSpeed = 2.55f;
                minJumpForce = 6.20f;
                maxJumpForce = 17.00f;
                horizontalJumpSpeed = 6.10f;
                wallBounceMultiplier = 0.50f;
                maxChargeTime = 0.58f;
                splatStunDuration = 0.50f;

                if (_col != null)
                {
                    _col.size = new Vector2(0.44f, 0.662f);
                    _col.offset = new Vector2(0f, 0.361f);
                    _col.edgeRadius = 0.03f;
                }
            }
            else if (character == CharacterType.Falin)
            {
                // Falin (Border Collie): Escala pequeña estilo Jump King (0.35 -> ~22x18 px) y salto potente y fluido (~5.12u alto, ~7.20u largo)
                transform.localScale = new Vector3(0.35f, 0.35f, 1.0f);
                _rb.mass = 1.15f;
                _rb.gravityScale = 3.15f;
                walkSpeed = 2.60f;
                minJumpForce = 6.40f;
                maxJumpForce = 17.80f;
                horizontalJumpSpeed = 6.25f;
                wallBounceMultiplier = 0.50f;
                maxChargeTime = 0.58f;
                splatStunDuration = 0.50f;

                if (_col != null)
                {
                    _col.size = new Vector2(0.680f, 1.240f);
                    _col.offset = new Vector2(0f, 0.770f);
                    _col.edgeRadius = 0.015f;
                }
            }
            else
            {
                // Mía (Schnauzer): Escala pequeña estilo Jump King (0.40 -> ~15x15 px) y salto ágil y fluido (~5.14u alto, ~7.62u largo)
                transform.localScale = new Vector3(0.40f, 0.40f, 1.0f);
                _rb.mass = 0.85f;
                _rb.gravityScale = 2.90f;
                walkSpeed = 2.80f;
                minJumpForce = 6.20f;
                maxJumpForce = 17.10f;
                horizontalJumpSpeed = 6.35f;
                wallBounceMultiplier = 0.52f;
                maxChargeTime = 0.54f;
                splatStunDuration = 0.50f;

                if (_col != null)
                {
                    _col.size = new Vector2(0.540f, 0.920f);
                    _col.offset = new Vector2(0f, 0.590f);
                    _col.edgeRadius = 0.015f;
                }
            }
        }

        public void LockAtSummit(Vector3 standPosition, float facingDir = 1f)
        {
            IsCharging = false;
            IsJumping = false;
            IsBouncing = false;
            IsStunned = false;
            IsGrounded = true;
            ChargePercent = 0f;
            FacingDirection = facingDir >= 0f ? 1f : -1f;
            _chargeTimer = 0f;
            _iceSlideVelocityX = 0f;

            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0f;
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }

            transform.position = standPosition;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
                spriteRenderer.flipX = (FacingDirection < 0f);
            }

            enabled = false;
        }

        /// <summary>
        /// Aplica un empujón lateral/vertical al tocar una trampa de picos espinosos (KimayaSpikeHazard),
        /// empujando al personaje hacia el vacío para que caiga de la plataforma.
        /// </summary>
        public void ApplySpikeKnockback(Vector2 launchVelocity, float lockDuration = 0.38f)
        {
            if (_rb == null) _rb = GetComponent<Rigidbody2D>();
            if (_rb == null || _rb.bodyType == RigidbodyType2D.Kinematic) return;

            IsCharging = false;
            IsGrounded = false;
            IsJumping = true;
            IsBouncing = true;
            ChargePercent = 0f;
            _chargeTimer = 0f;
            _groundContactCount = 0;
            _jumpGraceTimer = Mathf.Max(0.28f, lockDuration);
            _bounceTimer = Mathf.Max(0.35f, lockDuration);
            _hurtFlashTimer = 0.42f;
            _mustReleaseSpace = true;

            _rb.linearVelocity = launchVelocity;
            _iceSlideVelocityX = launchVelocity.x;

            if (Mathf.Abs(launchVelocity.x) > 0.1f)
            {
                FacingDirection = Mathf.Sign(launchVelocity.x);
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = (FacingDirection < 0f);
                }
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.42f, 0.42f, 1f);
            }

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Damage);
            }

            TotalFalls++;
            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.RegisterFall();
            }
        }

        private void Update()
        {
            if (_hurtFlashTimer > 0f)
            {
                _hurtFlashTimer -= Time.deltaTime;
                if (_hurtFlashTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                }
            }

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

            // Seguimiento del punto más alto del salto y silbido de caída libre
            if (!IsGrounded)
            {
                if (transform.position.y > _highestAirY)
                {
                    _highestAirY = transform.position.y;
                }
                else if (!_playedFallWhistle && _rb != null && _rb.linearVelocity.y < -10.5f && transform.position.y < _lastGroundedY - 2.3f)
                {
                    _playedFallWhistle = true;
                    if (KimayaAudio.Instance != null)
                    {
                        KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.FallWhistle);
                    }
                }
            }

            if (_jumpGraceTimer > 0f)
            {
                _jumpGraceTimer -= Time.deltaTime;
            }

            bool spaceHeld = ReadSpaceKey();
            if (!spaceHeld)
            {
                _mustReleaseSpace = false;
            }

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

            if ((IsGrounded || IsCharging) && !IsJumping && _jumpGraceTimer <= 0f)
            {
                if (spaceHeld && !_mustReleaseSpace)
                {
                    // Cargando salto: si no se presiona A ni D durante la carga, _jumpDirection queda en 0 (salto recto hacia arriba)
                    if (!IsCharging)
                    {
                        IsCharging = true;
                        _chargeTimer = 0f;
                        _chargeSfxTimer = 0.08f;
                        _jumpDirection = (Mathf.Abs(hInput) > 0.1f) ? Mathf.Sign(hInput) : 0f;
                        if (KimayaAudio.Instance != null) KimayaAudio.Instance.PlayChargeTick(0.05f);
                    }

                    _chargeTimer += Time.deltaTime;
                    ChargePercent = Mathf.Clamp01(_chargeTimer / maxChargeTime);

                    _chargeSfxTimer -= Time.deltaTime;
                    if (_chargeSfxTimer <= 0f)
                    {
                        _chargeSfxTimer = 0.14f;
                        if (KimayaAudio.Instance != null) KimayaAudio.Instance.PlayChargeTick(ChargePercent);
                    }

                    // Dirección de salto elegida durante la carga (solo avanza si presionas A o D)
                    if (hInput > 0.1f)
                    {
                        _jumpDirection = 1f;
                        FacingDirection = 1f;
                    }
                    else if (hInput < -0.1f)
                    {
                        _jumpDirection = -1f;
                        FacingDirection = -1f;
                    }

                    // En el suelo mientras carga se detiene
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                    _iceSlideVelocityX = 0f;

                    // Salto automático al alcanzar el 100% de carga (Jump King oficial)
                    if (_chargeTimer >= maxChargeTime)
                    {
                        ExecuteJump();
                    }
                }
                else if (IsCharging)
                {
                    // Si presionó A/D justo al soltar ESPACIO, tomar esa dirección; si no presionó A/D en ningún momento, salta vertical (_jumpDirection = 0)
                    if (Mathf.Abs(hInput) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(hInput);
                        FacingDirection = _jumpDirection;
                    }

                    // Soltó el espacio -> ¡Salto inmediato!
                    ExecuteJump();
                }
                else
                {
                    // Manejo del movimiento según la superficie actual (Jump King Blocks)
                    ApplyGroundMovement(hInput);
                    ApplyLedgeOverhangSlip(hInput);
                    ChargePercent = 0f;

                    if (Mathf.Abs(hInput) > 0.1f)
                    {
                        _walkStepTimer -= Time.deltaTime;
                        if (_walkStepTimer <= 0f)
                        {
                            _walkStepTimer = 0.25f;
                            if (KimayaAudio.Instance != null) KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Step, 0.7f);
                        }
                    }
                    else
                    {
                        _walkStepTimer = 0.05f;
                    }
                }
            }
        }

        /// <summary>
        /// Evita que el perrito quede flotando en el borde con medio cuerpo en el aire:
        /// si su centro de masa ya salió de la plataforma y no está caminando hacia adentro, resbala suavemente.
        /// </summary>
        private void ApplyLedgeOverhangSlip(float hInput)
        {
            if (_col == null || !IsGrounded || IsCharging || IsJumping) return;

            int myLayer = gameObject.layer;
            int mask = Physics2D.GetLayerCollisionMask(myLayer);

            Vector2 centerOrigin = new Vector2(transform.position.x, transform.position.y + 0.14f);
            RaycastHit2D centerHit = Physics2D.Raycast(centerOrigin, Vector2.down, 0.30f, mask);
            if (centerHit.collider != null && !centerHit.collider.isTrigger && centerHit.collider != _col)
            {
                return;
            }

            float halfFoot = (_col.size.x * Mathf.Abs(transform.localScale.x)) * 0.48f;
            RaycastHit2D leftHit = Physics2D.Raycast(centerOrigin + new Vector2(-halfFoot, 0f), Vector2.down, 0.30f, mask);
            RaycastHit2D rightHit = Physics2D.Raycast(centerOrigin + new Vector2(halfFoot, 0f), Vector2.down, 0.30f, mask);

            bool hasLeft = leftHit.collider != null && !leftHit.collider.isTrigger && leftHit.collider != _col;
            bool hasRight = rightHit.collider != null && !rightHit.collider.isTrigger && rightHit.collider != _col;

            if (hasLeft && !hasRight && hInput >= -0.05f)
            {
                _rb.linearVelocity = new Vector2(2.2f, -0.6f);
            }
            else if (hasRight && !hasLeft && hInput <= 0.05f)
            {
                _rb.linearVelocity = new Vector2(-2.2f, -0.6f);
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
            _mustReleaseSpace = true;
            _jumpGraceTimer = 0.15f; // 150ms protección de despegue: no permite re-tocar suelo inmediatamente
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
            _playedFallWhistle = false;
            KimayaPixelVFX.SpawnJumpDust(transform.position);
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(ChargePercent >= 0.65f ? KimayaAudio.SFX.PowerJump : KimayaAudio.SFX.Jump);
            }

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
                        KimayaPixelVFX.SpawnWallSparks(contact.point, contact.normal.x);
                        if (KimayaAudio.Instance != null)
                        {
                            KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.WallBounce);
                        }

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
            // Protección de despegue: no aterrizar en los primeros 150ms ni mientras la velocidad vertical sea ascendente
            if (_jumpGraceTimer > 0f || _rb.linearVelocity.y > 0.05f) return;

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
                    _playedFallWhistle = false;
                    KimayaPixelVFX.SpawnLandDust(transform.position);

                    // En Jump King clásico, al aterrizar en superficie normal se detiene en seco (sin resbalar de bloques estrechos)
                    if (CurrentSurface != SurfaceType.Ice)
                    {
                        _rb.linearVelocity = new Vector2(0f, 0f);
                        _iceSlideVelocityX = 0f;
                    }

                    // Si caímos de gran altura: registrar caída y evaluar Aturdimiento (Splat)
                    float fallDistance = _highestAirY - transform.position.y;
                    bool droppedFloor = transform.position.y < _lastGroundedY - 3.0f;
                    bool isHeavySplat = enableHighFallStun && fallDistance >= highFallThreshold;

                    if (KimayaAudio.Instance != null)
                    {
                        KimayaAudio.Instance.PlaySFX((droppedFloor || isHeavySplat) ? KimayaAudio.SFX.HeavyFall : KimayaAudio.SFX.Land);
                    }

                    if (droppedFloor)
                    {
                        TotalFalls++;
                        if (KimayaJumpKingManager.Instance != null)
                        {
                            KimayaJumpKingManager.Instance.RegisterFall();
                        }
                    }

                    if (isHeavySplat)
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

            // KIM-12: Si cae fuera del escenario (abismo Y < -2.0), reaparece en el inicio o en el punto de control
            if (transform.position.y < -2.0f)
            {
                transform.position = respawnPoint;
                _rb.linearVelocity = Vector2.zero;
                TotalFalls++;
                if (KimayaJumpKingManager.Instance != null)
                {
                    KimayaJumpKingManager.Instance.RegisterFall();
                }
            }
        }

        // ================= INPUT SYSTEM UNIVERSAL =================
        private bool ReadSpaceKey()
        {
            if (Kimaya.Networking.PhonePadServer.Held("A") || Kimaya.Networking.PhonePadServer.Held("B"))
                return true;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
                return true;
#endif
            return Input.GetKey(KeyCode.Space);
        }

        private float ReadHorizontalInput()
        {
            Vector2 joy = Kimaya.Networking.PhonePadServer.GetJoystick();
            if (Mathf.Abs(joy.x) > 0.22f)
                return Mathf.Sign(joy.x);

            float val = 0f;
            if (Kimaya.Networking.PhonePadServer.Held("RIGHT")) val += 1f;
            if (Kimaya.Networking.PhonePadServer.Held("LEFT")) val -= 1f;
            if (val != 0f) return val;
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

        private bool ReadVerticalOnlyKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ||
                    Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    return true;
                }
            }
#endif
            return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ||
                   Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        }
    }
}
