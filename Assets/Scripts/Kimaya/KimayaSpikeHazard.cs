using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Trampa de Picos Espinosos del Zarzal (Estilo Jump King):
    /// Si el jugador pisa o toca los picos, recibe un empujón lateral/rebote hacia el vacío
    /// que lo hace resbalar y caer de la plataforma hacia los pisos inferiores.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaSpikeHazard : MonoBehaviour
    {
        [Tooltip("Dirección de empuje hacia el vacío (-1 = izquierda, +1 = derecha, 0 = alejar del centro del pico)")]
        public int pushDirection = -1;

        public float knockbackSpeedX = 5.35f;
        public float knockbackPopY = 4.60f;
        public float knockbackStunTime = 0.38f;

        private float _cooldownTimer = 0f;

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryTriggerSpikeKnockback(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryTriggerSpikeKnockback(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryTriggerSpikeKnockback(collision.gameObject);
        }

        private void TryTriggerSpikeKnockback(GameObject obj)
        {
            if (_cooldownTimer > 0f || obj == null) return;

            var player = obj.GetComponent<KimayaJumpKingController>();
            if (player == null) player = obj.GetComponentInParent<KimayaJumpKingController>();
            if (player == null || !player.enabled) return;

            _cooldownTimer = 0.35f;

            float dirX = pushDirection;
            if (Mathf.Abs(dirX) < 0.1f)
            {
                dirX = (player.transform.position.x >= transform.position.x) ? 1f : -1f;
            }

            Vector2 launchVel = new Vector2(dirX * knockbackSpeedX, knockbackPopY);
            player.ApplySpikeKnockback(launchVel, knockbackStunTime);
        }
    }
}
