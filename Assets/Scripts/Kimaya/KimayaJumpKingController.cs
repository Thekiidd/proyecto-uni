using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Controlador de física y salto táctico estilo Jump King para Kimaya.
    /// Mecánicas:
    /// - Caminar a izquierda / derecha.
    /// - Carga de salto manteniendo ESPACIO. Al soltar, salta con fuerza proporcional.
    /// - Sin control aéreo (trayectoria fija).
    /// - Rebotes contra paredes (Wall Bounces).
    /// - Sin checkpoints: las caídas te regresan a pisos inferiores.
    /// - Aturdimiento si caes desde gran altura.
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
        public float walkSpeed = 3.5f;

        [Header("Física de Salto Jump King")]
        public float minJumpForce = 5f;
        public float maxJumpForce = 14f;
        public float maxChargeTime = 0.85f;
        public float horizontalJumpSpeed = 4.2f;
        public float wallBounceMultiplier = 0.6f;
        public float fallStunVelocityThreshold = -12f;
        public float stunDuration = 1.2f;

        [Header("Suelo")]
        public LayerMask groundLayer;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;

        // Estados
        public bool IsGrounded { get; private set; }
        public bool IsCharging { get; private set; }
        public bool IsJumping { get; private set; }
        public bool IsStunned { get; private set; }
        public float ChargePercent { get; private set; } // 0.0 a 1.0

        // Estadísticas de sesión
        public int TotalJumps { get; private set; }
        public int TotalFalls { get; private set; }

        private Rigidbody2D _rb;
        private float _chargeTimer = 0f;
        private float _jumpDirection = 0f;
        private float _stunTimer = 0f;
        private float _maxFallVelocity = 0f;
        private bool _wasGrounded = true;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
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
            if (character == CharacterType.Falin)
            {
                // Falin (San Bernardo): Más pesado y firme, rebota menos
                _rb.mass = 1.3f;
                _rb.gravityScale = 3.2f;
                walkSpeed = 3.2f;
                maxJumpForce = 14.5f;
                wallBounceMultiplier = 0.5f;
            }
            else
            {
                // Mía (Schnauzer): Más liviana y ágil, rebota más
                _rb.mass = 1.0f;
                _rb.gravityScale = 2.8f;
                walkSpeed = 3.8f;
                maxJumpForce = 13.8f;
                wallBounceMultiplier = 0.75f;
            }
        }

        private void Update()
        {
            CheckGrounded();

            if (IsStunned)
            {
                _stunTimer -= Time.deltaTime;
                if (_stunTimer <= 0f)
                {
                    IsStunned = false;
                }
                return;
            }

            // Si está en el suelo
            if (IsGrounded)
            {
                // Iniciar o mantener carga de salto
                if (Input.GetKey(KeyCode.Space))
                {
                    IsCharging = true;
                    _chargeTimer += Time.deltaTime;
                    ChargePercent = Mathf.Clamp01(_chargeTimer / maxChargeTime);

                    // Dirección del salto según teclas pulsadas durante la carga
                    float h = Input.GetAxisRaw("Horizontal");
                    if (Mathf.Abs(h) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(h);
                        if (spriteRenderer != null) spriteRenderer.flipX = (_jumpDirection < 0);
                    }
                    else
                    {
                        _jumpDirection = 0f;
                    }

                    // Detener velocidad horizontal mientras carga
                    _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
                }
                else if (IsCharging)
                {
                    // Soltó el botón de salto -> ¡EJECUTAR SALTO!
                    ExecuteJump();
                }
                else
                {
                    // Caminar normal en el suelo
                    float h = Input.GetAxisRaw("Horizontal");
                    _rb.linearVelocity = new Vector2(h * walkSpeed, _rb.linearVelocity.y);

                    if (Mathf.Abs(h) > 0.1f)
                    {
                        _jumpDirection = Mathf.Sign(h);
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
                // En el aire: monitorear velocidad de caída para posible aturdimiento
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

        private void CheckGrounded()
        {
            Vector2 checkPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.5f;
            Collider2D hit = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
            bool currentlyGrounded = (hit != null);

            // Aterrizaje
            if (!_wasGrounded && currentlyGrounded)
            {
                IsJumping = false;

                // Si cayó desde muy alto -> Aturdimiento
                if (_maxFallVelocity <= fallStunVelocityThreshold)
                {
                    IsStunned = true;
                    _stunTimer = stunDuration;
                    TotalFalls++;
                    _rb.linearVelocity = Vector2.zero;
                }

                _maxFallVelocity = 0f;
            }

            IsGrounded = currentlyGrounded;
            _wasGrounded = currentlyGrounded;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Rebote contra paredes si estamos en el aire
            if (!IsGrounded && IsJumping)
            {
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    // Si el impacto fue lateral (normal horizontal predominante)
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

        private void OnDrawGizmosSelected()
        {
            Vector2 checkPos = groundCheck != null ? (Vector2)groundCheck.position : (Vector2)transform.position + Vector2.down * 0.5f;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(checkPos, groundCheckRadius);
        }
    }
}
