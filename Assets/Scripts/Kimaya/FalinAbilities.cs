using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Habilidades especiales de Falin (Border Collie / Great Dane):
    ///   - Empujar objetos pesados (PushableObject)
    ///   - Salto impulsado: mantén SHIFT+Salto para un salto más alto
    /// </summary>
    [RequireComponent(typeof(PlayerController2D))]
    public class FalinAbilities : MonoBehaviour
    {
        [Header("Salto Impulsado")]
        public float poweredJumpForce  = 18f;
        public float chargeTime        = 0.5f;
        public KeyCode chargeKey       = KeyCode.LeftShift;

        [Header("Empujar")]
        public float pushCheckDistance = 0.6f;
        public LayerMask pushableLayer;

        [Header("Partículas")]
        public ParticleSystem chargeParticles;
        public ParticleSystem jumpParticles;

        private PlayerController2D _pc;
        private bool _isCharging = false;
        private float _chargeTimer = 0f;
        private bool _chargeReady = false;

        private void Awake() => _pc = GetComponent<PlayerController2D>();

        private void Update()
        {
            if (!_pc.isActive) return;

            HandlePoweredJump();
        }

        private void HandlePoweredJump()
        {
            // Cargar mientras mantiene SHIFT en suelo
            if (Input.GetKey(chargeKey) && _pc.IsGrounded)
            {
                if (!_isCharging)
                {
                    _isCharging = true;
                    _chargeTimer = 0f;
                    if (chargeParticles != null && !chargeParticles.isPlaying)
                        chargeParticles.Play();
                }

                _chargeTimer += Time.deltaTime;
                if (_chargeTimer >= chargeTime)
                    _chargeReady = true;
            }
            else
            {
                if (_isCharging && chargeParticles != null)
                    chargeParticles.Stop();
                _isCharging = false;
            }

            // Ejecutar salto impulsado
            if (_chargeReady && Input.GetKeyDown(_pc.jumpKey))
            {
                _pc.Jump(poweredJumpForce);
                _chargeReady = false;
                _chargeTimer = 0f;
                if (jumpParticles != null) jumpParticles.Play();
            }
        }

        private void FixedUpdate()
        {
            if (!_pc.isActive) return;

            // Empujar objetos pesados (detectar hacia donde mira)
            Vector2 dir = _pc.VelocityX >= 0 ? Vector2.right : Vector2.left;
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, dir, pushCheckDistance, pushableLayer);

            if (hit.collider != null)
            {
                var pushable = hit.collider.GetComponent<PushableObject>();
                pushable?.Push(dir);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, Vector2.right  * pushCheckDistance);
            Gizmos.DrawRay(transform.position, Vector2.left   * pushCheckDistance);
        }
    }
}
