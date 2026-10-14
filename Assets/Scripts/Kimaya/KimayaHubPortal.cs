using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Portal Místico en la Aldea Hub de Kimaya.
    /// Soporta portales activos y bloqueados (desactivados) con indicadores visuales y sonoros.
    /// </summary>
    public class KimayaHubPortal : MonoBehaviour
    {
        [Header("Estado del Portal")]
        public bool isLocked = false;
        public string lockedTitle = "🔒 Portal Desactivado";
        public string lockedReason = "Canalización inactiva. Completa el Nivel 1 (Ascenso Vertical) primero.";

        [Header("Destino (si está activo)")]
        public string targetSceneName = "Ascenso_Vertical";
        public string destinationTitle = "Ascenso Vertical Ancestral (Nivel 1)";
        public string questLore = "El Gran Salto hacia la Cima - Modo Jump King";

        [Header("Configuración de Interacción")]
        public float interactionRadius = 2.0f;
        public Color runeGlowColor = new Color(0.2f, 0.95f, 0.45f, 0.9f);
        public Color lockedGlowColor = new Color(0.85f, 0.25f, 0.25f, 0.6f);

        private SpriteRenderer _spriteRenderer;
        private bool _isPlayerNearby = false;
        private Camera _mainCam;

        private static GUIStyle _activeBannerStyle;
        private static GUIStyle _lockedBannerStyle;
        private static Texture2D _activeBgTex;
        private static Texture2D _lockedBgTex;

        private void Start()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _mainCam = Camera.main;
        }

        private void Update()
        {
            Transform activeDog = null;
            if (KimayaHubDuoController.Instance != null)
            {
                activeDog = KimayaHubDuoController.Instance.GetActiveTransform();
            }

            if (activeDog == null)
            {
                // Fallback por si acaso
                GameObject p = GameObject.FindWithTag("Player");
                if (p != null) activeDog = p.transform;
            }

            if (activeDog != null)
            {
                float dist = Vector2.Distance(transform.position, activeDog.position);
                _isPlayerNearby = (dist <= interactionRadius);
            }
            else
            {
                _isPlayerNearby = false;
            }

            // Efecto de pulso en el portal si tiene sprite propio
            if (_spriteRenderer != null)
            {
                if (isLocked)
                {
                    _spriteRenderer.color = lockedGlowColor;
                }
                else
                {
                    float pulse = 0.82f + 0.18f * Mathf.Sin(Time.time * 3.5f);
                    _spriteRenderer.color = _isPlayerNearby
                        ? new Color(runeGlowColor.r * pulse, runeGlowColor.g * pulse, runeGlowColor.b * pulse, 1f)
                        : runeGlowColor;
                }
            }

            if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            {
                if (!isLocked)
                {
                    EnterPortal();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && !isLocked)
            {
                EnterPortal();
            }
        }

        public void EnterPortal()
        {
            Debug.Log($"[KimayaHubPortal] Entrando al portal hacia: {targetSceneName}");
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning($"La escena {targetSceneName} no está cargable en Build Settings. Intentando cargar por nombre directo...");
                SceneManager.LoadScene(targetSceneName);
            }
        }

        private void OnGUI()
        {
            if (!_isPlayerNearby) return;
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            Vector3 screenPos = _mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (screenPos.z < 0) return;

            InitStyles();

            float w = 400f;
            float h = 60f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

            // Evitar salirse de los límites de pantalla
            x = Mathf.Clamp(x, 20f, Screen.width - w - 20f);
            y = Mathf.Clamp(y, 20f, Screen.height - h - 20f);

            if (isLocked)
            {
                string text = $"<b>{lockedTitle}</b>\n<size=12><color=#FFB499>{lockedReason}</color></size>";
                GUI.Box(new Rect(x, y, w, h), text, _lockedBannerStyle);
            }
            else
            {
                string text = $"<b>🌀 [E] Entrar al {destinationTitle}</b>\n<size=12><color=#A8FFB2>{questLore}</color></size>";
                GUI.Box(new Rect(x, y, w, h), text, _activeBannerStyle);
            }
        }

        private static void InitStyles()
        {
            if (_activeBannerStyle == null)
            {
                _activeBgTex = new Texture2D(1, 1);
                _activeBgTex.SetPixel(0, 0, new Color(0.04f, 0.22f, 0.14f, 0.95f));
                _activeBgTex.Apply();

                _activeBannerStyle = new GUIStyle();
                _activeBannerStyle.normal.background = _activeBgTex;
                _activeBannerStyle.normal.textColor = Color.white;
                _activeBannerStyle.fontSize = 14;
                _activeBannerStyle.fontStyle = FontStyle.Normal;
                _activeBannerStyle.alignment = TextAnchor.MiddleCenter;
                _activeBannerStyle.padding = new RectOffset(12, 12, 6, 6);
            }

            if (_lockedBannerStyle == null)
            {
                _lockedBgTex = new Texture2D(1, 1);
                _lockedBgTex.SetPixel(0, 0, new Color(0.28f, 0.08f, 0.08f, 0.95f));
                _lockedBgTex.Apply();

                _lockedBannerStyle = new GUIStyle();
                _lockedBannerStyle.normal.background = _lockedBgTex;
                _lockedBannerStyle.normal.textColor = Color.white;
                _lockedBannerStyle.fontSize = 14;
                _lockedBannerStyle.fontStyle = FontStyle.Normal;
                _lockedBannerStyle.alignment = TextAnchor.MiddleCenter;
                _lockedBannerStyle.padding = new RectOffset(12, 12, 6, 6);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isLocked ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
