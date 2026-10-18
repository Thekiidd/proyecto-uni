using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente disparador de la meta final (Altar de la Cumbre).
    /// Detecta la llegada del jugador mediante un Trigger 2D, activa la condición de victoria
    /// en el Game State y previene activaciones múltiples.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaAltarGoal : MonoBehaviour
    {
        [Header("Configuración de Meta")]
        [Tooltip("Mensaje de éxito que se muestra en el panel de victoria")]
        public string victoryMessage = "¡Has conquistado el Gran Árbol Sagrado y rescatado a tu compañera!";

        [Tooltip("Congelar físicas del jugador al llegar a la cima para evitar caídas posteriores")]
        public bool freezePlayerOnVictory = true;

        [Header("Efectos Opcionales")]
        public ParticleSystem victoryAura;

        private bool _isGoalReached = false;
        private Collider2D _triggerCollider;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider2D>();
            _triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Evitar activaciones redundantes
            if (_isGoalReached) return;

            var player = other.GetComponent<KimayaJumpKingController>();
            if (player == null && !other.CompareTag("Player") && !other.name.Contains("Player"))
            {
                return;
            }

            _isGoalReached = true;
            if (_triggerCollider != null) _triggerCollider.enabled = false;

            if (victoryAura != null)
            {
                victoryAura.Play();
            }

            // Reproducir sonido de victoria sagrada
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Victory);
            }

            // Disparar condición de éxito en el Manager
            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.TriggerVictory(victoryMessage);
            }
            else
            {
                Debug.Log($"[KimayaAltarGoal] ¡Meta alcanzada! {victoryMessage}");
            }
        }

        public void ResetGoal()
        {
            _isGoalReached = false;
            if (_triggerCollider != null) _triggerCollider.enabled = true;
        }
    }
}
