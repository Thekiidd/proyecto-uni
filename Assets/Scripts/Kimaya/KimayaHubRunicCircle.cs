using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Gestiona el Círculo Rúnico central del Bosque del Eco y el estado de cierre del MVP (KIM-01, KIM-02, KIM-15, KIM-16, KIM-17).
    /// - 4 Runas: Runa 1 (Sendero de las Raíces), Runas 2, 3 y 4 (Regiones pendientes).
    /// - Al rescatar a Kiara, se activan las 3 hermanas reunidas en el Hub (Falin, Mia y Kiara).
    /// - La primera runa se ilumina de forma permanente y se muestra el mensaje de finalización del MVP.
    /// </summary>
    public class KimayaHubRunicCircle : MonoBehaviour
    {
        public static KimayaHubRunicCircle Instance { get; private set; }

        [Header("Referencias de Personajes")]
        public GameObject kiaraObject;
        public Sprite kiaraSprite;
        public GameObject fairyObject;
        public Sprite fairySprite;

        [Header("Posición del Hada de la Historia en el Hub")]
        public Vector3 fairyHubPosition = new Vector3(-1.48f, 0.12f, 0f);
        public Vector3 fairyHubScale = new Vector3(0.145f, 0.145f, 1.0f);

        [Header("Estado de Runas")]
        public bool rune1Active = false;
        public bool rune2Active = false;
        public bool rune3Active = false;
        public bool rune4Active = false;

        [Header("Colores de Runas")]
        public Color runeActiveColor = new Color(0.2f, 1f, 0.5f, 0.95f); // Verde esmeralda resplandeciente
        public Color runeInactiveColor = new Color(0.35f, 0.35f, 0.45f, 0.35f); // Apagada / sombría

        private bool _showCompletionModal = false;
        private bool _showHubPauseModal = false;
        private bool _showHubSettingsModal = false;
        private Camera _mainCam;

        private SpriteRenderer _kiaraSr;
        private Sprite[] _kiaraIdleFrames;
        private float _kiaraAnimTimer = 0f;
        private int _kiaraFrameIdx = 0;

        private SpriteRenderer _fairySr;
        private Sprite[] _fairyIdleFrames;
        private float _fairyAnimTimer = 0f;
        private int _fairyFrameIdx = 0;
        private Vector3 _fairyBasePos;

        private static GUIStyle _modalBoxStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _bodyStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _hudRuneStyle;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _mainCam = Camera.main;

            // Ocultar cualquier HUD viejo superpuesto en el Hub (ej. x 3 / x 0 / 0/3 con cuadros blancos)
            foreach (string legacyName in new string[] { "HUD", "GameHUD", "Canvas_HUD", "TokenHUD", "TopHUD" })
            {
                var go = GameObject.Find(legacyName);
                if (go != null) go.SetActive(false);
            }

            // Asegurar siempre al Hada de la Historia (Maya) en el borde superior izquierdo del círculo rúnico
            SetupStoryFairy();

            // Verificar si Kiara ya fue rescatada (KIM-15 & KIM-16)
            bool kiaraRescued = PlayerPrefs.GetInt("KiaraRescued", 0) == 1;
            rune1Active = kiaraRescued;

            if (kiaraRescued)
            {
                SetupReunitedSisters();

                // Mostrar el mensaje de cierre del MVP si es la primera vez que vuelve
                bool hasSeenEnding = PlayerPrefs.GetInt("HasSeenMVPEnding", 0) == 1;
                if (!hasSeenEnding)
                {
                    _showCompletionModal = true;
                    PlayerPrefs.SetInt("HasSeenMVPEnding", 1);
                    PlayerPrefs.Save();
                }
            }
        }

        public void SetupStoryFairy()
        {
            if (fairyObject == null)
            {
                fairyObject = GameObject.Find("Hada_Historia_Maya");
            }

            if (fairyObject == null)
            {
                fairyObject = new GameObject("Hada_Historia_Maya");
                fairyObject.transform.position = fairyHubPosition;
                fairyObject.transform.localScale = fairyHubScale;
                _fairySr = fairyObject.AddComponent<SpriteRenderer>();
                _fairySr.sortingOrder = 508;
            }
            else
            {
                fairyObject.SetActive(true);
                fairyObject.transform.position = fairyHubPosition;
                fairyObject.transform.localScale = fairyHubScale;
                _fairySr = fairyObject.GetComponent<SpriteRenderer>();
                if (_fairySr == null) _fairySr = fairyObject.AddComponent<SpriteRenderer>();
                _fairySr.sortingOrder = 508;
            }

            _fairyBasePos = fairyObject.transform.position;

            Sprite f0 = Resources.Load<Sprite>("NPCs/Maya/Hada_Idle_0") ?? Resources.Load<Sprite>("NPCs/Maya/Maya_Fairy");
            Sprite f1 = Resources.Load<Sprite>("NPCs/Maya/Hada_Idle_1") ?? f0;
            if (fairySprite == null) fairySprite = f0;
            if (fairySprite != null && _fairySr != null) _fairySr.sprite = fairySprite;

            if (f0 != null && f1 != null)
            {
                _fairyIdleFrames = new Sprite[] { f0, f1 };
            }

            // Forzar escala delicada del Hada Maya (más pequeña que los perros) y adjuntar el diálogo interactivo
            fairyObject.transform.localScale = new Vector3(0.145f, 0.145f, 1.0f);
            if (fairyObject.GetComponent<KimayaHubFairyDialogue>() == null)
            {
                fairyObject.AddComponent<KimayaHubFairyDialogue>();
            }
        }

        private void SetupReunitedSisters()
        {
            if (kiaraObject == null)
            {
                kiaraObject = GameObject.Find("Kiara_Schnauzer");
                if (kiaraObject == null) kiaraObject = GameObject.Find("Kiara_GoldenRetriever");
            }

            if (kiaraObject == null)
            {
                // Crear a Kiara (Schnauzer color canela/crema) dinámicamente si no existe en la escena
                kiaraObject = new GameObject("Kiara_Schnauzer");
                kiaraObject.transform.position = new Vector3(0f, -0.65f, 0f);
                kiaraObject.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
                _kiaraSr = kiaraObject.AddComponent<SpriteRenderer>();
                _kiaraSr.sortingOrder = 510;

                // Cargar sprite de Kiara desde Resources
                if (kiaraSprite == null)
                {
                    kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_south");
                    if (kiaraSprite == null) kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_east");
                }
                if (kiaraSprite != null) _kiaraSr.sprite = kiaraSprite;
            }
            else
            {
                kiaraObject.name = "Kiara_Schnauzer";
                kiaraObject.SetActive(true);
                kiaraObject.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
                _kiaraSr = kiaraObject.GetComponent<SpriteRenderer>();
            }

            _kiaraIdleFrames = Resources.LoadAll<Sprite>("NPCs/Kiara/Idle_South");

            Debug.Log("[KimayaHubRunicCircle] 🌟 ¡Las 3 hermanas (Falin, Mia y Kiara) están reunidas en el Bosque del Eco! Runa 1 activada.");
        }

        private void Update()
        {
            if (_fairySr != null && fairyObject != null && fairyObject.activeInHierarchy)
            {
                // Flotación suave mágica del Hada de la Historia
                float bobOffset = Mathf.Sin(Time.time * 2.4f) * 0.045f;
                fairyObject.transform.position = new Vector3(_fairyBasePos.x, _fairyBasePos.y + bobOffset, _fairyBasePos.z);

                if (_fairyIdleFrames != null && _fairyIdleFrames.Length > 0)
                {
                    _fairyAnimTimer += Time.deltaTime;
                    if (_fairyAnimTimer >= 0.35f)
                    {
                        _fairyAnimTimer -= 0.35f;
                        _fairyFrameIdx = (_fairyFrameIdx + 1) % _fairyIdleFrames.Length;
                        _fairySr.sprite = _fairyIdleFrames[_fairyFrameIdx];
                    }
                }
            }

            if (_kiaraSr != null && _kiaraIdleFrames != null && _kiaraIdleFrames.Length > 0 && kiaraObject != null && kiaraObject.activeInHierarchy)
            {
                _kiaraAnimTimer += Time.deltaTime;
                if (_kiaraAnimTimer >= 0.125f)
                {
                    _kiaraAnimTimer -= 0.125f;
                    _kiaraFrameIdx = (_kiaraFrameIdx + 1) % _kiaraIdleFrames.Length;
                    _kiaraSr.sprite = _kiaraIdleFrames[_kiaraFrameIdx];
                }
            }

            bool escPressed = Kimaya.Networking.PhonePadServer.Pressed("START");
            bool confirmPressed = Kimaya.Networking.PhonePadServer.Pressed("A");
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) escPressed = true;
                if (UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame || UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame) confirmPressed = true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape)) escPressed = true;
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) confirmPressed = true;
            }
            catch { }

            if (KimayaHubFairyDialogue.IsDialogueOpen)
                return;

            if (_showCompletionModal && (escPressed || confirmPressed))
            {
                _showCompletionModal = false;
                return;
            }

            if (escPressed)
            {
                if (_showHubSettingsModal)
                {
                    _showHubSettingsModal = false;
                }
                else
                {
                    _showHubPauseModal = !_showHubPauseModal;
                    if (KimayaHubDuoController.Instance != null)
                    {
                        KimayaHubDuoController.Instance.controlsEnabled = !_showHubPauseModal;
                    }
                }
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // Dibujar el estado de las 4 runas en la esquina superior izquierda (KIM-01, KIM-02, KIM-16)
            DrawRunicCircleHUD();

            // Botón discreto en esquina superior derecha para abrir Menú / Volver al Menú Principal
            GUIStyle topBtnStyle = KimayaUITheme.GetOnGUIButtonStyle(12);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(Screen.width - 148f, 16f, 132f, 36f), "[ESC] MENÚ", topBtnStyle))
            {
                _showHubPauseModal = !_showHubPauseModal;
                _showHubSettingsModal = false;
                if (KimayaHubDuoController.Instance != null)
                {
                    KimayaHubDuoController.Instance.controlsEnabled = !_showHubPauseModal;
                }
            }

            // Si está activo el modal de felicitación de cierre del MVP (KIM-17)
            if (_showCompletionModal)
            {
                DrawMVPCompletionModal();
            }
            else if (_showHubPauseModal)
            {
                DrawHubPauseModal();
            }
        }

        private void DrawHubPauseModal()
        {
            Color prevCol = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prevCol;

            if (_showHubSettingsModal)
            {
                float sw = Mathf.Min(640f, Screen.width - 40f);
                float sh = 420f;
                float sx = (Screen.width - sw) * 0.5f;
                float sy = (Screen.height - sh) * 0.5f;

                var settings = KimayaSettingsController.Instance ?? FindAnyObjectByType<KimayaSettingsController>();
                if (settings == null) settings = gameObject.AddComponent<KimayaSettingsController>();
                settings.DrawSettingsUI(new Rect(sx, sy, sw, sh), () => _showHubSettingsModal = false);
                return;
            }

            float mw = 420f;
            float mh = 290f;
            float mx = (Screen.width - mw) * 0.5f;
            float my = (Screen.height - mh) * 0.5f;

            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.98f);
            GUI.DrawTexture(new Rect(mx - 3f, my - 3f, mw + 6f, mh + 6f), Texture2D.whiteTexture);
            GUI.color = new Color(0.10f, 0.08f, 0.16f, 0.98f);
            GUI.DrawTexture(new Rect(mx, my, mw, mh), Texture2D.whiteTexture);
            GUI.color = prevCol;

            GUIStyle hdr = KimayaUITheme.GetOnGUILabelStyle(20, TextAnchor.MiddleCenter, false);
            GUI.Label(new Rect(mx, my + 18f, mw, 34f), "<color=#F8D458>MENÚ DEL SANTUARIO</color>", hdr);

            GUIStyle btnSt = KimayaUITheme.GetOnGUIButtonStyle(14);
            GUI.backgroundColor = Color.white;
            float bx = mx + 55f;
            float bw = mw - 110f;

            if (GUI.Button(new Rect(bx, my + 74f, bw, 50f), "CONTINUAR", btnSt))
            {
                _showHubPauseModal = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
            }
            if (GUI.Button(new Rect(bx, my + 138f, bw, 50f), "AJUSTES", btnSt))
            {
                _showHubSettingsModal = true;
            }
            if (GUI.Button(new Rect(bx, my + 202f, bw, 50f), "MENÚ PRINCIPAL", btnSt))
            {
                _showHubPauseModal = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
                KimayaSceneTransition.LoadScene("MainMenu");
            }
        }

        private void DrawRunicCircleHUD()
        {
            float hudX = 16f;
            float hudY = 16f;
            float hudW = 310f;
            float hudH = 84f;

            Color prevCol = GUI.color;
            // Marco dorado pixel-art + fondo oscuro sólido legible
            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.96f);
            GUI.DrawTexture(new Rect(hudX - 2f, hudY - 2f, hudW + 4f, hudH + 4f), Texture2D.whiteTexture);
            GUI.color = new Color(0.10f, 0.08f, 0.16f, 0.95f);
            GUI.DrawTexture(new Rect(hudX, hudY, hudW, hudH), Texture2D.whiteTexture);
            GUI.color = new Color(0.24f, 0.19f, 0.34f, 0.90f);
            GUI.DrawTexture(new Rect(hudX + 4f, hudY + 28f, hudW - 8f, 2f), Texture2D.whiteTexture);
            GUI.color = prevCol;

            GUIStyle headerSt = KimayaUITheme.GetOnGUILabelStyle(13, TextAnchor.MiddleLeft, false);
            GUIStyle bodySt   = KimayaUITheme.GetOnGUILabelStyle(15, TextAnchor.MiddleLeft, true);

            GUI.Label(new Rect(hudX + 12f, hudY + 4f, hudW - 24f, 24f), "<color=#F8D458>CÍRCULO MÁGICO (4 RUNAS)</color>", headerSt);

            string r1 = rune1Active
                ? "<color=#4DFF88>● Runa 1 (Raíces): ACTIVA</color>"
                : "<color=#9EA8B8>○ Runa 1 (Raíces): Sellada</color>";
            string rRest = "<color=#8B84A0>○ Runas 2, 3 y 4: Pendientes</color>";

            GUI.Label(new Rect(hudX + 12f, hudY + 32f, hudW - 24f, 22f), r1, bodySt);
            GUI.Label(new Rect(hudX + 12f, hudY + 54f, hudW - 24f, 22f), rRest, bodySt);
        }

        private void DrawMVPCompletionModal()
        {
            float modalW = Mathf.Min(660f, Screen.width - 30f);
            float modalH = 340f;
            float modalX = (Screen.width - modalW) * 0.5f;
            float modalY = (Screen.height - modalH) * 0.5f;

            GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none, _modalBoxStyle);

            // Título KIM-17
            GUI.Label(new Rect(modalX, modalY + 20f, modalW, 32f), "🌟 ¡DEMOSTRACIÓN DEL MVP COMPLETADA! 🌟", _titleStyle);

            // Contenido obligatorio KIM-17
            string content = "<b>¡Kiara ha sido rescatada exitosamente!</b>\n\n" +
                             "• <b>Primera Runa Restaurada:</b> La energía del Sendero de las Raíces fluye de nuevo por el círculo central.\n" +
                             "• <b>Tres Regiones Pendientes:</b> Aún restan 3 regiones místicas del reino que continúan selladas por la oscuridad.\n" +
                             "• <b>Las Tres Hermanas Unidas:</b> Falin (Border Collie), Mía y Kiara (Schnauzers) están a salvo juntas en el Bosque del Eco.\n\n" +
                             "<i>¡La aventura de Fairytopia continuará posteriormente en las siguientes fases del proyecto!</i>";

            GUI.Label(new Rect(modalX + 25f, modalY + 65f, modalW - 50f, 190f), content, _bodyStyle);

            // Botón de continuar
            if (GUI.Button(new Rect(modalX + (modalW - 220f) * 0.5f, modalY + modalH - 52f, 220f, 38f), "✨ Explorar el Bosque [E / Space]", _buttonStyle))
            {
                _showCompletionModal = false;
            }
        }

        public void ReplayDemo()
        {
            PlayerPrefs.DeleteKey("KiaraRescued");
            PlayerPrefs.DeleteKey("FirstRuneActive");
            PlayerPrefs.DeleteKey("HasSeenMVPEnding");
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static void InitStyles()
        {
            if (_modalBoxStyle != null) return;

            Texture2D boxTex = new Texture2D(1, 1);
            boxTex.SetPixel(0, 0, new Color(0.06f, 0.10f, 0.16f, 0.98f));
            boxTex.Apply();

            _modalBoxStyle = new GUIStyle();
            _modalBoxStyle.normal.background = boxTex;

            _titleStyle = new GUIStyle();
            _titleStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);
            _titleStyle.fontSize = 18;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;

            _bodyStyle = new GUIStyle();
            _bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 1f);
            _bodyStyle.fontSize = 13;
            _bodyStyle.wordWrap = true;

            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.18f, 0.65f, 0.35f, 1f));
            btnTex.Apply();

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = btnTex;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 14;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _hudRuneStyle = new GUIStyle();
            _hudRuneStyle.normal.textColor = Color.white;
            _hudRuneStyle.fontSize = 11;
        }
    }
}
