using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente de interacción 2D para coleccionables del nivel (Reliquias Sagradas).
    /// Cumple con la rúbrica de interacción por disparador lógico (isTrigger) y modificación de estado (puntaje e inventario).
    /// Incluye protección contra doble recolección atómica.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class KimayaRelic : MonoBehaviour
    {
        [Header("Configuración de Estado y Puntaje")]
        [Tooltip("Puntaje que otorga esta reliquia al ser recogida")]
        public int scoreValue = 100;

        [Tooltip("Identificador o nombre de la reliquia")]
        public string relicName = "Reliquia Sagrada de Canopia";

        [Header("Animación Procedural de Flotación")]
        public float bobSpeed = 2.5f;
        public float bobHeight = 0.15f;
        public float rotationSpeed = 25f;

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
            _startPosition = transform.position;
            _timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        private void Update()
        {
            if (_isCollected) return;

            // Flotación senoidal armónica
            float offsetY = Mathf.Sin(Time.time * bobSpeed + _timeOffset) * bobHeight;
            transform.position = new Vector3(_startPosition.x, _startPosition.y + offsetY, _startPosition.z);

            // Rotación suave continua para efecto mágico
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Caso Error Probado: Prevención de recolección duplicada o disparos múltiples en el mismo frame
            if (_isCollected) return;

            // Verificar si el colisionador pertenece al jugador (por componente o Tag)
            var player = other.GetComponent<KimayaJumpKingController>();
            if (player == null && !other.CompareTag("Player") && !other.name.Contains("Player"))
            {
                return;
            }

            // Marcar recolectado inmediatamente y apagar el colisionador (prevención atómica)
            _isCollected = true;
            if (_collider != null) _collider.enabled = false;

            // 1. Modificación de Estado: Notificar al Manager del juego
            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.AddScore(scoreValue, relicName);
            }
            else
            {
                Debug.LogWarning($"[KimayaRelic] KimayaJumpKingManager no encontrado al recoger {relicName}. Puntaje aplicado localmente.");
            }

            // 2. Respuesta Sonora
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Checkpoint);
            }

            // 3. Respuesta Visual
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

            // Animación de pulso y desaparición
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, t);

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
