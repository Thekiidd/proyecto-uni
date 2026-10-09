using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kimaya
{
    /// <summary>
    /// Placa de presión animada estilo Fireboy & Watergirl.
    /// Se hunde visualmente al pararse encima (jugador o caja) y se ilumina.
    /// Se levanta suavemente cuando no hay ningún objeto sobre ella.
    /// </summary>
    public class KimayaPressurePlate : MonoBehaviour
    {
        [Header("Color y Emparejamiento")]
        public string plateColor = "green"; // Empareja con ColoredDoor
        public Color unpressedColor = new Color(0.35f, 0.45f, 0.35f);
        public Color pressedColor   = new Color(0.2f, 1f, 0.35f);

        [Header("Animación de Hundimiento")]
        public Transform plateTop; // La parte móvil de la placa
        public float sinkDistance = 0.12f;
        public float animSpeed = 8f;

        [Header("Eventos")]
        public UnityEvent onPressed;
        public UnityEvent onReleased;

        private Vector3 _initialLocalPos;
        private Vector3 _targetLocalPos;
        private SpriteRenderer _topSr;
        private int _objectsOnPlate = 0;
        private bool _isPressed = false;

        public bool IsPressed => _isPressed;

        private void Awake()
        {
            if (plateTop == null)
            {
                var topChild = transform.Find("PlateTop");
                plateTop = topChild != null ? topChild : transform;
            }

            _initialLocalPos = plateTop.localPosition;
            _targetLocalPos  = _initialLocalPos;

            _topSr = plateTop.GetComponent<SpriteRenderer>();
            if (_topSr == null) _topSr = GetComponent<SpriteRenderer>();

            UpdateVisualInstant();
        }

        private void Update()
        {
            // Mover suavemente la placa hacia la posición objetivo
            if (plateTop != null)
            {
                plateTop.localPosition = Vector3.Lerp(
                    plateTop.localPosition, _targetLocalPos, Time.deltaTime * animSpeed);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsValidTrigger(other))
            {
                _objectsOnPlate++;
                if (!_isPressed)
                {
                    _isPressed = true;
                    _targetLocalPos = _initialLocalPos - new Vector3(0f, sinkDistance, 0f);
                    if (_topSr != null) _topSr.color = pressedColor;

                    // Sonido procedural o SFX
                    KimayaAudio.Instance?.PlaySFX(KimayaAudio.SFX.Switch);

                    onPressed?.Invoke();
                    NotifyDoors(true);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsValidTrigger(other))
            {
                _objectsOnPlate = Mathf.Max(0, _objectsOnPlate - 1);
                if (_objectsOnPlate == 0 && _isPressed)
                {
                    _isPressed = false;
                    _targetLocalPos = _initialLocalPos;
                    if (_topSr != null) _topSr.color = unpressedColor;

                    onReleased?.Invoke();
                    NotifyDoors(false);
                }
            }
        }

        private bool IsValidTrigger(Collider2D col)
        {
            return col.CompareTag("Player") || col.CompareTag("Pushable") || col.GetComponent<PushableObject>() != null;
        }

        private void NotifyDoors(bool open)
        {
            foreach (var door in FindObjectsByType<ColoredDoor>(FindObjectsInactive.Exclude))
            {
                if (door.doorColor.Equals(plateColor, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (open) door.Open();
                    else door.Close();
                }
            }
        }

        private void UpdateVisualInstant()
        {
            if (_topSr != null)
                _topSr.color = _isPressed ? pressedColor : unpressedColor;
        }
    }
}
