using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente de punto de control intermedio para el nivel Sendero de las Raíces (KIM-12).
    /// Si el jugador cae fuera de los límites del nivel tras activar este punto,
    /// reaparecerá aquí en lugar del inicio.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaCheckpointTrigger : MonoBehaviour
    {
        [Header("Configuración de Checkpoint")]
        public string checkpointName = "Punto de Control: Canopia Media";
        public Color activeGlowColor = new Color(0.3f, 1f, 0.4f, 0.9f);

        private bool _isActivated = false;
        private SpriteRenderer _sr;
        private Collider2D _col;

        private void Awake()
        {
            _col = GetComponent<Collider2D>();
            _col.isTrigger = true;
            _sr = GetComponent<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isActivated) return;

            var controller = other.GetComponent<KimayaJumpKingController>();
            if (controller == null)
            {
                controller = other.GetComponentInParent<KimayaJumpKingController>();
            }

            if (controller != null)
            {
                _isActivated = true;
                controller.SetCheckpoint(transform.position);

                if (_sr != null)
                {
                    _sr.color = activeGlowColor;
                }

                if (KimayaAudio.Instance != null)
                {
                    KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Checkpoint);
                }

                Debug.Log($"[KimayaCheckpoint] 🚩 {checkpointName} activado para {controller.character}!");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isActivated ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(2f, 2f, 0f));
        }
    }
}
