using UnityEngine;
using UnityEngine.Events;

namespace Kimaya
{
    /// <summary>
    /// Switch / palanca que puede activar puertas de colores.
    /// Se activa al pisarla (pressure plate) o al interactuar (E).
    /// </summary>
    public class Switch : MonoBehaviour
    {
        public enum SwitchType { PressurePlate, Lever }

        [Header("Tipo")]
        public SwitchType switchType = SwitchType.PressurePlate;
        public string switchColor    = "green"; // para emparejar con Door

        [Header("Estado")]
        public bool isOn = false;
        public bool toggleable = true; // si puede volver a apagarse

        [Header("Visuales")]
        public Color offColor = new Color(0.8f, 0.3f, 0.2f);
        public Color onColor  = new Color(0.3f, 0.9f, 0.3f);
        public Sprite offSprite;
        public Sprite onSprite;

        [Header("Eventos")]
        public UnityEvent onActivated;
        public UnityEvent onDeactivated;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            UpdateVisual();
        }

        // Pressure plate — se activa al entrar
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (switchType != SwitchType.PressurePlate) return;
            if (other.CompareTag("Player") || other.CompareTag("Pushable"))
                Activate();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (switchType != SwitchType.PressurePlate || !toggleable) return;
            if (other.CompareTag("Player") || other.CompareTag("Pushable"))
                Deactivate();
        }

        // Lever — se activa al presionar E
        private void Update()
        {
            if (switchType != SwitchType.Lever) return;
            if (!Input.GetKeyDown(KeyCode.E)) return;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            if (Vector2.Distance(transform.position, player.transform.position) > 1.5f) return;

            if (isOn && toggleable) Deactivate();
            else                    Activate();
        }

        public void Activate()
        {
            if (isOn) return;
            isOn = true;
            UpdateVisual();
            onActivated?.Invoke();

            // Notificar a todas las puertas del mismo color
            foreach (var door in FindObjectsByType<ColoredDoor>(FindObjectsInactive.Exclude))
                if (door.doorColor == switchColor) door.Open();
        }

        public void Deactivate()
        {
            if (!isOn) return;
            isOn = false;
            UpdateVisual();
            onDeactivated?.Invoke();

            foreach (var door in FindObjectsByType<ColoredDoor>(FindObjectsInactive.Exclude))
                if (door.doorColor == switchColor) door.Close();
        }

        private void UpdateVisual()
        {
            if (_sr == null) return;
            _sr.color  = isOn ? onColor : offColor;
            if (isOn && onSprite  != null) _sr.sprite = onSprite;
            if (!isOn && offSprite != null) _sr.sprite = offSprite;
        }
    }

    /// <summary>
    /// Puerta de color que abre/cierra según su Switch correspondiente.
    /// </summary>
    public class ColoredDoor : MonoBehaviour
    {
        [Header("Identificación")]
        public string doorColor = "green";

        [Header("Animación de apertura")]
        public float openSpeed   = 3f;
        public Vector3 openOffset = new Vector3(0f, -2f, 0f); // se hunde en el suelo
        public bool startsOpen   = false;

        [Header("Sonido")]
        public AudioClip openSound;
        public AudioClip closeSound;

        private Vector3 _closedPos;
        private Vector3 _openPos;
        private bool    _isOpen;
        private AudioSource _audio;

        private void Awake()
        {
            _closedPos = transform.position;
            _openPos   = transform.position + openOffset;
            _isOpen    = startsOpen;
            _audio     = GetComponent<AudioSource>();
            if (startsOpen) transform.position = _openPos;
        }

        public void Open()
        {
            _isOpen = true;
            StopAllCoroutines();
            StartCoroutine(MoveTo(_openPos));
            if (_audio != null && openSound != null) _audio.PlayOneShot(openSound);
        }

        public void Close()
        {
            _isOpen = false;
            StopAllCoroutines();
            StartCoroutine(MoveTo(_closedPos));
            if (_audio != null && closeSound != null) _audio.PlayOneShot(closeSound);
        }

        private System.Collections.IEnumerator MoveTo(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, target, openSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }
    }
}
