using UnityEngine;
using UnityEngine.Events;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Placa de presión que se activa cuando un Player o un objeto con Rigidbody2D se para encima.
    /// Útil para puzzles cooperativos donde un perro debe quedarse parado para abrirle paso al otro.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate : MonoBehaviour
    {
        [Header("Eventos")]
        [Tooltip("Se dispara cuando se pisa la placa")]
        public UnityEvent onActivate;

        [Tooltip("Se dispara cuando se deja de pisar la placa")]
        public UnityEvent onDeactivate;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer plateRenderer;
        [SerializeField] private Color normalColor = Color.gray;
        [SerializeField] private Color activatedColor = Color.green;

        private int objectsOnPlate = 0;

        private void Start()
        {
            if (plateRenderer == null)
                plateRenderer = GetComponent<SpriteRenderer>();

            UpdateVisual(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsValidTrigger(collision))
            {
                objectsOnPlate++;
                if (objectsOnPlate == 1)
                {
                    UpdateVisual(true);
                    onActivate?.Invoke();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (IsValidTrigger(collision))
            {
                objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
                if (objectsOnPlate == 0)
                {
                    UpdateVisual(false);
                    onDeactivate?.Invoke();
                }
            }
        }

        private bool IsValidTrigger(Collider2D collision)
        {
            // Se activa con jugadores o cajas físicas
            return collision.CompareTag("Player") || collision.attachedRigidbody != null;
        }

        private void UpdateVisual(bool active)
        {
            if (plateRenderer != null)
                plateRenderer.color = active ? activatedColor : normalColor;
        }
    }
}
