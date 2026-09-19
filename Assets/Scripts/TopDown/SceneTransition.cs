using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.TopDown
{
    /// <summary>
    /// Puerta de transición entre escenas.
    /// Coloca este componente en cualquier objeto con Collider2D (trigger).
    /// Al presionar E dentro del trigger → carga la escena destino.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        [Header("Destino")]
        [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings).")]
        public string targetScene = "Village_Scene";

        [Tooltip("Tiempo de fade antes de cargar (segundos).")]
        public float fadeDuration = 0.4f;

        [Header("Prompt visual")]
        [Tooltip("Mensaje que se muestra al acercarse.")]
        public string promptMessage = "[E] Entrar";

        // ── Estado ────────────────────────────────────────────────────────────
        private bool _playerInside = false;
        private bool _transitioning = false;

        // Fade overlay
        private static Texture2D _fadeTexture;
        private float _fadeAlpha = 0f;
        private bool _fadingOut = false;

        // GUI styles
        private GUIStyle _promptStyle;

        // ── Unity ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            // Asegurar que el collider sea trigger
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            // Crear textura de fade
            if (_fadeTexture == null)
            {
                _fadeTexture = new Texture2D(1, 1);
                _fadeTexture.SetPixel(0, 0, Color.black);
                _fadeTexture.Apply();
            }
        }

        private void Update()
        {
            if (_transitioning) return;

            if (_playerInside && Input.GetKeyDown(KeyCode.E))
                StartCoroutine(TransitionRoutine());
        }

        // ── Triggers ──────────────────────────────────────────────────────────
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _playerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _playerInside = false;
        }

        // ── Transición con fade ───────────────────────────────────────────────
        private IEnumerator TransitionRoutine()
        {
            _transitioning = true;
            _fadingOut = true;

            // Fade OUT (negro)
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                _fadeAlpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            _fadeAlpha = 1f;

            // Cargar escena
            yield return SceneManager.LoadSceneAsync(targetScene);
        }

        // ── OnGUI ─────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            // Prompt al acercarse
            if (_playerInside && !_transitioning)
            {
                if (_promptStyle == null)
                {
                    _promptStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 16,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = Color.white }
                    };
                }

                // Sombra
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.Label(new Rect(Screen.width / 2f - 101, Screen.height - 71, 202, 32), promptMessage, _promptStyle);
                // Texto
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width / 2f - 100, Screen.height - 72, 200, 32), promptMessage, _promptStyle);
                GUI.color = Color.white;
            }

            // Fade overlay
            if (_fadeAlpha > 0f)
            {
                GUI.color = new Color(0, 0, 0, _fadeAlpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _fadeTexture);
                GUI.color = Color.white;
            }
        }

        // ── Gizmos ────────────────────────────────────────────────────────────
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
            var col = GetComponent<BoxCollider2D>();
            if (col != null)
                Gizmos.DrawCube((Vector2)transform.position + col.offset, col.size);
        }
    }
}
