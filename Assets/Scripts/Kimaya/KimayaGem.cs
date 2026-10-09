using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Cristal coleccionable (Esmeralda verde / Amatista morada) estilo Fireboy & Watergirl.
    /// Flota suavemente en el aire y desaparece con sonido al recogerlo.
    /// </summary>
    public class KimayaGem : MonoBehaviour
    {
        public enum GemType { Green, Purple }
        public GemType gemType = GemType.Green;

        public Color gemColor = new Color(0.2f, 1f, 0.4f);
        public float bobHeight = 0.12f;
        public float bobSpeed = 2.5f;

        private Vector3 _startPos;
        private float _timeOffset;
        private bool _collected = false;

        private Vector3 _baseScale;

        private void Start()
        {
            _startPos = transform.position;
            _baseScale = transform.localScale;
            _timeOffset = Random.Range(0f, Mathf.PI * 2f);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = gemColor;
        }

        private void Update()
        {
            if (_collected) return;

            // Flotación senoidal
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed + _timeOffset) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);

            // Brillo y rotación suave
            float pulse = 1f + Mathf.Sin(Time.time * 3.5f + _timeOffset) * 0.12f;
            transform.localScale = _baseScale * pulse;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2f + _timeOffset) * 8f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            _collected = true;
            KimayaAudio.Instance?.PlaySFX(KimayaAudio.SFX.Checkpoint);
            gameObject.SetActive(false);
        }
    }
}
