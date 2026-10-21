using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Portal Místico en la Aldea Hub de Kimaya (Bosque del Eco).
    /// KIM-02: 4 caminos (1 disponible: Sendero de las Raíces, 3 bloqueados con aviso).
    /// KIM-03 & SRS-KIM-08: Pantalla de Selección de Personaje (Falin vs Mia) antes de entrar al nivel,
    /// con descripción de físicas diferenciadas y advertencia de no poder cambiar en el nivel.
    /// </summary>
    public class KimayaHubPortal : MonoBehaviour
    {
        [Header("Estado del Portal")]
        public bool isLocked = false;
        public string lockedTitle = "🔒 Camino Bloqueado";
        public string lockedReason = "Una densa energía oscura sella esta entrada. Restaura las runas restantes en próximas versiones.";

        [Header("Destino (si está activo)")]
        public string targetSceneName = "Ascenso_JumpKing_Classic";
        public string destinationTitle = "Sendero de las Raíces";
        public string questLore = "Nivel de precisión 2D vertical para rescatar a Kiara";

        [Header("Configuración de Interacción")]
        public float interactionRadius = 2.2f;
        public Color runeGlowColor = new Color(0.2f, 0.95f, 0.45f, 0.9f);
        public Color lockedGlowColor = new Color(0.85f, 0.25f, 0.25f, 0.6f);

        private SpriteRenderer _spriteRenderer;
        private bool _isPlayerNearby = false;
        private bool _showCharacterSelectModal = false;
        private Camera _mainCam;

        private static GUIStyle _activeBannerStyle;
        private static GUIStyle _lockedBannerStyle;
        private static GUIStyle _modalBoxStyle;
        private static GUIStyle _headerStyle;
        private static GUIStyle _cardStyle;
        private static GUIStyle _cardTitleStyle;
        private static GUIStyle _cardDescStyle;
        private static GUIStyle _falinBtnStyle;
        private static GUIStyle _miaBtnStyle;
        private static GUIStyle _warningStyle;

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

            // Efecto de pulso en el portal
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

            if (_isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !_showCharacterSelectModal)
            {
                if (!isLocked)
                {
                    // Abrir modal de selección de personaje (KIM-03)
                    _showCharacterSelectModal = true;
                    if (KimayaHubDuoController.Instance != null)
                        KimayaHubDuoController.Instance.controlsEnabled = false;
                }
            }

            if (_showCharacterSelectModal && Input.GetKeyDown(KeyCode.Escape))
            {
                _showCharacterSelectModal = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
            }
        }

        private void SelectAndEnter(string characterName)
        {
            PlayerPrefs.SetString("SelectedCharacter", characterName);
            PlayerPrefs.Save();
            Debug.Log($"[KimayaHubPortal] Personaje confirmado: {characterName}. Iniciando {targetSceneName}...");
            _showCharacterSelectModal = false;

            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                SceneManager.LoadScene("Ascenso_JumpKing_Classic");
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (_showCharacterSelectModal)
            {
                DrawCharacterSelectModal();
                return;
            }

            if (!_isPlayerNearby) return;
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            Vector3 screenPos = _mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (screenPos.z < 0) return;

            float w = 420f;
            float h = 64f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

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

        private void DrawCharacterSelectModal()
        {
            float modalW = Mathf.Min(720f, Screen.width - 30f);
            float modalH = 430f;
            float modalX = (Screen.width - modalW) * 0.5f;
            float modalY = (Screen.height - modalH) * 0.5f;

            GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none, _modalBoxStyle);

            // Título
            GUI.Label(new Rect(modalX, modalY + 18f, modalW, 32f), "🌲 SELECCIÓN DE PERSONAJE — SENDERO DE LAS RAÍCES", _headerStyle);

            float cardW = (modalW - 50f) * 0.5f;
            float cardH = 240f;
            float cardY = modalY + 65f;

            // Tarjeta Falin
            float falinX = modalX + 20f;
            GUI.Box(new Rect(falinX, cardY, cardW, cardH), GUIContent.none, _cardStyle);
            GUI.Label(new Rect(falinX + 15f, cardY + 15f, cardW - 30f, 26f), "🐕 FALIN (San Bernardo)", _cardTitleStyle);
            string falinDesc = "• <b>Salto:</b> Pesado, firme y potente.\n" +
                              "• <b>Inercia:</b> Mayor masa y velocidad de descenso.\n" +
                              "• <b>Carga:</b> Requiere más tiempo para carga máxima.\n" +
                              "• <b>Estilo:</b> Ideal para calcular saltos con anticipación y aterrizajes sólidos.";
            GUI.Label(new Rect(falinX + 15f, cardY + 45f, cardW - 30f, 125f), falinDesc, _cardDescStyle);
            if (GUI.Button(new Rect(falinX + 20f, cardY + cardH - 50f, cardW - 40f, 38f), "▶ Elegir a Falin", _falinBtnStyle))
            {
                SelectAndEnter("Falin");
            }

            // Tarjeta Mia
            float miaX = modalX + 30f + cardW;
            GUI.Box(new Rect(miaX, cardY, cardW, cardH), GUIContent.none, _cardStyle);
            GUI.Label(new Rect(miaX + 15f, cardY + 15f, cardW - 30f, 26f), "🐩 MIA (Schnauzer Miniatura)", _cardTitleStyle);
            string miaDesc = "• <b>Salto:</b> Ligero, ágil y de rápida respuesta.\n" +
                            "• <b>Inercia:</b> Gran tiempo en el aire (flotante).\n" +
                            "• <b>Carga:</b> Carga veloz y sensible a pequeñas variaciones.\n" +
                            "• <b>Estilo:</b> Ideal para soltar con precisión milimétrica.";
            GUI.Label(new Rect(miaX + 15f, cardY + 45f, cardW - 30f, 125f), miaDesc, _cardDescStyle);
            if (GUI.Button(new Rect(miaX + 20f, cardY + cardH - 50f, cardW - 40f, 38f), "▶ Elegir a Mia", _miaBtnStyle))
            {
                SelectAndEnter("Mia");
            }

            // Advertencia estricta KIM-10
            string warningText = "⚠️ <b>REGLA DEL MVP (KIM-10):</b> No será posible alternar personajes durante el Sendero de las Raíces. " +
                                 "Deberás completar todo el recorrido con la hermana seleccionada.";
            GUI.Label(new Rect(modalX + 20f, modalY + cardH + 75f, modalW - 40f, 35f), warningText, _warningStyle);

            // Botón cancelar
            if (GUI.Button(new Rect(modalX + (modalW - 140f) * 0.5f, modalY + modalH - 42f, 140f, 30f), "✖ Cancelar [ESC]"))
            {
                _showCharacterSelectModal = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
            }
        }

        private static void InitStyles()
        {
            if (_activeBannerStyle != null) return;

            Texture2D activeBg = new Texture2D(1, 1);
            activeBg.SetPixel(0, 0, new Color(0.04f, 0.22f, 0.14f, 0.95f));
            activeBg.Apply();

            _activeBannerStyle = new GUIStyle();
            _activeBannerStyle.normal.background = activeBg;
            _activeBannerStyle.normal.textColor = Color.white;
            _activeBannerStyle.fontSize = 14;
            _activeBannerStyle.alignment = TextAnchor.MiddleCenter;
            _activeBannerStyle.padding = new RectOffset(12, 12, 6, 6);

            Texture2D lockedBg = new Texture2D(1, 1);
            lockedBg.SetPixel(0, 0, new Color(0.28f, 0.08f, 0.08f, 0.95f));
            lockedBg.Apply();

            _lockedBannerStyle = new GUIStyle();
            _lockedBannerStyle.normal.background = lockedBg;
            _lockedBannerStyle.normal.textColor = Color.white;
            _lockedBannerStyle.fontSize = 14;
            _lockedBannerStyle.alignment = TextAnchor.MiddleCenter;
            _lockedBannerStyle.padding = new RectOffset(12, 12, 6, 6);

            Texture2D modalBg = new Texture2D(1, 1);
            modalBg.SetPixel(0, 0, new Color(0.07f, 0.09f, 0.15f, 0.98f));
            modalBg.Apply();

            _modalBoxStyle = new GUIStyle();
            _modalBoxStyle.normal.background = modalBg;
            _modalBoxStyle.border = new RectOffset(4, 4, 4, 4);

            _headerStyle = new GUIStyle();
            _headerStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
            _headerStyle.fontSize = 17;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.alignment = TextAnchor.MiddleCenter;

            Texture2D cardBg = new Texture2D(1, 1);
            cardBg.SetPixel(0, 0, new Color(0.12f, 0.16f, 0.25f, 0.9f));
            cardBg.Apply();

            _cardStyle = new GUIStyle();
            _cardStyle.normal.background = cardBg;
            _cardStyle.padding = new RectOffset(12, 12, 12, 12);

            _cardTitleStyle = new GUIStyle();
            _cardTitleStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            _cardTitleStyle.fontSize = 15;
            _cardTitleStyle.fontStyle = FontStyle.Bold;

            _cardDescStyle = new GUIStyle();
            _cardDescStyle.normal.textColor = new Color(0.9f, 0.92f, 0.95f);
            _cardDescStyle.fontSize = 12;
            _cardDescStyle.wordWrap = true;

            Texture2D falinBtnBg = new Texture2D(1, 1);
            falinBtnBg.SetPixel(0, 0, new Color(0.85f, 0.55f, 0.15f, 1f));
            falinBtnBg.Apply();

            _falinBtnStyle = new GUIStyle();
            _falinBtnStyle.normal.background = falinBtnBg;
            _falinBtnStyle.normal.textColor = Color.white;
            _falinBtnStyle.fontSize = 14;
            _falinBtnStyle.fontStyle = FontStyle.Bold;
            _falinBtnStyle.alignment = TextAnchor.MiddleCenter;

            Texture2D miaBtnBg = new Texture2D(1, 1);
            miaBtnBg.SetPixel(0, 0, new Color(0.25f, 0.65f, 0.85f, 1f));
            miaBtnBg.Apply();

            _miaBtnStyle = new GUIStyle();
            _miaBtnStyle.normal.background = miaBtnBg;
            _miaBtnStyle.normal.textColor = Color.white;
            _miaBtnStyle.fontSize = 14;
            _miaBtnStyle.fontStyle = FontStyle.Bold;
            _miaBtnStyle.alignment = TextAnchor.MiddleCenter;

            _warningStyle = new GUIStyle();
            _warningStyle.normal.textColor = new Color(1f, 0.75f, 0.35f);
            _warningStyle.fontSize = 11;
            _warningStyle.alignment = TextAnchor.MiddleCenter;
            _warningStyle.wordWrap = true;
        }
    }
}
