using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Coleccionable del nivel: Scooby Galleta Pixel-Art (generada con PixelLab).
    /// Otorga puntaje y llena las ranuras de la barra superior de Scooby Galletas.
    /// Incluye protección atómica contra doble recolección.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class KimayaRelic : MonoBehaviour
    {
        [Header("Configuración de Estado y Puntaje")]
        [Tooltip("Puntaje que otorga esta Scooby Galleta al ser recogida")]
        public int scoreValue = 100;

        [Tooltip("Identificador o nombre de la galleta")]
        public string relicName = "Scooby Galleta";

        [Header("Animación Procedural de Flotación")]
        public float bobSpeed = 2.8f;
        public float bobHeight = 0.11f;
        public float swayAngle = 9f;

        [Header("Efectos Opcionales (Inspector)")]
        public ParticleSystem collectParticles;
        public SpriteRenderer relicSpriteRenderer;

        private Vector3 _startPosition;
        private float _timeOffset;
        private bool _isCollected = false;
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
            if (_collider is CircleCollider2D cc)
            {
                cc.radius = 0.45f;
            }

            if (relicSpriteRenderer == null)
            {
                relicSpriteRenderer = GetComponent<SpriteRenderer>();
                if (relicSpriteRenderer == null)
                {
                    relicSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        private void Start()
        {
            // Actualizar automáticamente al nuevo sprite PixelLab de Scooby Galleta y tamaño compacto
            Sprite cookieSpr = KimayaPixelHUD.LoadPixelSprite("UI/PixelHUD/ScoobyGalleta_Icon");
            if (cookieSpr != null && relicSpriteRenderer != null)
            {
                relicSpriteRenderer.sprite = cookieSpr;
                relicSpriteRenderer.color = Color.white;
                relicSpriteRenderer.sortingOrder = 22;
            }

            transform.localScale = new Vector3(0.72f, 0.72f, 1f);
            transform.rotation = Quaternion.identity;

            if (string.IsNullOrEmpty(relicName) || relicName.Contains("Reliquia"))
            {
                relicName = "Scooby Galleta";
            }

            _startPosition = transform.position;
            _timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (_isCollected) return;

            // Flotación senoidal suave y balanceo juguetón (sin ponerse de cabeza)
            float offsetY = Mathf.Sin(Time.time * bobSpeed + _timeOffset) * bobHeight;
            transform.position = new Vector3(_startPosition.x, _startPosition.y + offsetY, _startPosition.z);

            float tilt = Mathf.Sin(Time.time * (bobSpeed * 0.85f) + _timeOffset) * swayAngle;
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isCollected) return;

            var player = other.GetComponent<KimayaJumpKingController>();
            if (player == null && !other.CompareTag("Player") && !other.name.Contains("Player"))
            {
                return;
            }

            _isCollected = true;
            if (_collider != null) _collider.enabled = false;

            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.AddScore(scoreValue, relicName);
            }

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Checkpoint);
            }

            if (collectParticles != null)
            {
                collectParticles.transform.parent = null;
                collectParticles.Play();
                Destroy(collectParticles.gameObject, 1.5f);
            }

            StartCoroutine(CollectAnimationRoutine());
        }

        private IEnumerator CollectAnimationRoutine()
        {
            float elapsed = 0f;
            float duration = 0.25f;
            Vector3 startScale = transform.localScale;
            Vector3 startPos = transform.position;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.45f, t);
                transform.position = startPos + Vector3.up * (0.35f * t);

                if (relicSpriteRenderer != null)
                {
                    Color c = relicSpriteRenderer.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    relicSpriteRenderer.color = c;
                }
                yield return null;
            }

            gameObject.SetActive(false);
        }
    }
}
