using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Controlador de plataformas 2D para Falin y Mia.
    /// Maneja movimiento horizontal, salto, gravedad y detección de suelo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        public float moveSpeed = 5f;
        public float jumpForce = 11f;
        public float gravityScale = 3f;

        [Header("Detección de suelo")]
        public Transform groundCheck;
        public float groundCheckRadius = 0.15f;
        public LayerMask groundLayer;

        [Header("Coyote Time & Jump Buffer")]
        public float coyoteTime = 0.12f;
        public float jumpBufferTime = 0.12f;

        [Header("Estado")]
        public bool isActive = true;

        // Componentes
        private Rigidbody2D _rb;
        private SpriteRenderer _sr;
        private Animator _anim;

        // Estado interno
        private bool _isGrounded;
        private float _coyoteTimer;
        private float _jumpBufferTimer;
        private float _moveInput;
        private bool _facingRight = true;

        // Propiedades públicas
        public bool IsGrounded => _isGrounded;
        public float VelocityX => _rb.linearVelocity.x;
        public float VelocityY => _rb.linearVelocity.y;
        public Rigidbody2D Rb => _rb;

        // Input keys configurables
        [Header("Controles")]
        public KeyCode leftKey  = KeyCode.A;
        public KeyCode rightKey = KeyCode.D;
        public KeyCode jumpKey  = KeyCode.W;

        private void Awake()
        {
            _rb  = GetComponent<Rigidbody2D>();
            _sr  = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            _anim = GetComponent<Animator>();
            if (_anim == null) _anim = GetComponentInChildren<Animator>();

            _rb.gravityScale  = gravityScale;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            if (groundCheck == null)
            {
                var gc = new GameObject("GroundCheck");
                gc.transform.SetParent(transform);
                gc.transform.localPosition = new Vector3(0f, -0.5f, 0f);
                groundCheck = gc.transform;
            }
        }

        private void Update()
        {
            if (!isActive) return;

            _moveInput = 0f;
            if (Input.GetKey(leftKey))  _moveInput = -1f;
            if (Input.GetKey(rightKey)) _moveInput =  1f;

            // Jump buffer
            if (Input.GetKeyDown(jumpKey))
                _jumpBufferTimer = jumpBufferTime;
            else
                _jumpBufferTimer -= Time.deltaTime;

            // Coyote time
            if (_isGrounded)
                _coyoteTimer = coyoteTime;
            else
                _coyoteTimer -= Time.deltaTime;

            // Saltar
            if (_jumpBufferTimer > 0f && _coyoteTimer > 0f)
            {
                Jump(jumpForce);
                _jumpBufferTimer = 0f;
                _coyoteTimer = 0f;
            }

            // Soltar salto más rápido (control de altura)
            if (!Input.GetKey(jumpKey) && _rb.linearVelocity.y > 0f)
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.y * 0.92f);

            // Flip sprite
            if (_moveInput > 0 && !_facingRight) Flip();
            if (_moveInput < 0 &&  _facingRight) Flip();

            UpdateAnimations();
        }

        private void FixedUpdate()
        {
            if (!isActive) return;

            // Detección de suelo
            _isGrounded = Physics2D.OverlapCircle(
                groundCheck.position, groundCheckRadius, groundLayer);

            // Movimiento horizontal
            _rb.linearVelocity = new Vector2(_moveInput * moveSpeed, _rb.linearVelocity.y);
        }

        public void Jump(float force)
        {
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, force);
        }

        private void Flip()
        {
            _facingRight = !_facingRight;
            if (_sr != null) _sr.flipX = !_facingRight;
        }

        private void UpdateAnimations()
        {
            if (_anim == null) return;
            _anim.SetBool("isRunning", Mathf.Abs(_moveInput) > 0.1f && _isGrounded);
            _anim.SetBool("isGrounded", _isGrounded);
            _anim.SetFloat("velocityY", _rb.linearVelocity.y);
        }

        public void SetActive(bool active)
        {
            isActive = active;
            if (!active)
                _rb.linearVelocity = new Vector2(0f, _rb.linearVelocity.y);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
