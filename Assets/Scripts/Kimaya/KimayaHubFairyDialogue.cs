using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Sistema interactivo de diálogo con el Hada Guardiana Maya en el Santuario (Hub).
    /// - Muestra los diálogos oficiales entre Falin, Mía y Maya con las viñetas expresivas HUB-01 a HUB-27.
    /// - Bloquea el acceso al Portal Verde hasta que el jugador hable con Maya por primera vez.
    /// </summary>
    public class KimayaHubFairyDialogue : MonoBehaviour
    {
        public static bool HasTalkedToMaya =>
            PlayerPrefs.GetInt("Kimaya_TalkedToMaya", 0) == 1 ||
            PlayerPrefs.GetInt("KiaraRescued", 0) == 1;

        public static bool IsDialogueOpen { get; private set; } = false;

        private struct HubLine
        {
            public int portraitId;
            public string speaker;
            public Color badgeColor;
            public string text;

            public HubLine(int portraitId, string speaker, Color badgeColor, string text)
            {
                this.portraitId = portraitId;
                this.speaker = speaker;
                this.badgeColor = badgeColor;
                this.text = text;
            }
        }

        private static readonly HubLine[] IntroHubLines = new HubLine[]
        {
            new HubLine(1,  "FALIN  &  MÍA", new Color(0.18f, 0.42f, 0.72f),
                "— Falin: «Entonces habla. ¿Qué tenemos que hacer?»\n— Mía: «Falin, ni siquiera sabemos quién es.»"),
            new HubLine(3,  "MAYA  &  MÍA", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «Tiene razón. Me llamo Maya. Soy la guardiana de este Círculo de Hadas.»\n— Mía: «Yo soy Mía. Y ella es Falin.»"),
            new HubLine(7,  "MAYA  &  MÍA", new Color(0.74f, 0.22f, 0.18f),
                "— Maya: «Lo sé. Están asustadas por...»\n— Mía: «Kiara. Nuestra hermana mayor se llama Kiara.»"),
            new HubLine(8,  "MAYA (GUARDIANA)", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «El Zarzal Sombrío se la llevó. Es una fuerza que corrompe el Bosque del Eco con raíces y espinas... nadie conoce su verdadera forma.»"),
            new HubLine(10, "FALIN  &  MAYA", new Color(0.18f, 0.42f, 0.72f),
                "— Falin: «Averiguaremos qué es después. Primero recuperaremos a Kiara.»\n— Maya: «No pueden enfrentarlo de frente. Primero necesitan abrir un camino seguro.»"),
            new HubLine(15, "MAYA, FALIN & MÍA", new Color(0.74f, 0.22f, 0.18f),
                "— Maya: «El portal por el que llegaron se cerró.»   — Falin: «¡¿Qué quieres decir?!»\n— Mía: «Dijo que se cerró, no que fuera imposible abrirlo.»"),
            new HubLine(17, "MAYA (GUARDIANA)", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «Exactamente. Cuatro runas sostienen este círculo. Si restauran las cuatro runas, el portal volverá a abrirse y podrán regresar a casa.»"),
            new HubLine(20, "MÍA  &  MAYA", new Color(0.12f, 0.52f, 0.34f),
                "— Mía: «¿Y Kiara?»\n— Maya: «El Zarzal la llevó por el Sendero de las Raíces. La conexión es débil: una deberá entrar y la otra mantendrá abierto el vínculo.»"),
            new HubLine(22, "FALIN  &  MÍA", new Color(0.18f, 0.42f, 0.72f),
                "— Falin: «Entonces iré yo.»\n— Mía: «Eso no lo decides tú sola. También puedo hacerlo.»"),
            new HubLine(25, "MAYA, FALIN & MÍA", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «La decisión es de ambas. Ninguna hará esto sola.»\n— Falin: «Bien. Abramos el camino.»   — Mía: «Y traigamos a Kiara de vuelta.»"),
            new HubLine(27, "MAYA (PORTAL ABIERTO)", new Color(0.12f, 0.58f, 0.36f),
                "— Maya: «Elijan con cuidado con [TAB / X]: cada una salta diferente. ¡Acabo de abrir el Portal Verde del Sendero de las Raíces para ustedes!»"),
        };

        private static readonly HubLine[] PostRescueLines = new HubLine[]
        {
            new HubLine(17, "MAYA (GUARDIANA)", new Color(0.12f, 0.58f, 0.36f),
                "— Maya: «¡Lo lograron! Kiara está a salvo y la Primera Runa del Sendero de las Raíces vuelve a brillar en el Círculo de Hadas.»"),
            new HubLine(27, "MAYA (GUARDIANA)", new Color(0.12f, 0.52f, 0.34f),
                "— Maya: «Si desean entrenar y mejorar su tiempo récord en la torre, el Portal Verde permanecerá abierto para ustedes.»"),
        };

        public float interactRadius = 2.4f;
        private bool _playerNearby = false;
        private int _lineIndex = 0;
        private float _lineTimer = 0f;
        private int _lastTypedChars = 0;
        private HubLine[] _activeLines;
        private Texture2D _currentPortraitTex;
        private float _unlockToastTimer = 0f;
        private Camera _mainCam;

        private void Start()
        {
            IsDialogueOpen = false;
            _mainCam = Camera.main;
        }

        private void OnDestroy()
        {
            IsDialogueOpen = false;
        }

        private void Update()
        {
            if (_unlockToastTimer > 0f)
                _unlockToastTimer -= Time.deltaTime;

            Transform activeDog = null;
            if (KimayaHubDuoController.Instance != null)
                activeDog = KimayaHubDuoController.Instance.GetActiveTransform();

            if (activeDog != null)
            {
                float dist = Vector2.Distance(transform.position, activeDog.position);
                _playerNearby = dist <= interactRadius;
            }
            else
            {
                _playerNearby = false;
            }

            bool interactPressed = Kimaya.Networking.PhonePadServer.Pressed("A") || Kimaya.Networking.PhonePadServer.Pressed("B");
            bool skipPressed = Kimaya.Networking.PhonePadServer.Pressed("START");

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame) interactPressed = true;
                if (IsDialogueOpen && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                    interactPressed = true;
                if (Keyboard.current.escapeKey.wasPressedThisFrame) skipPressed = true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.E)) interactPressed = true;
                if (IsDialogueOpen && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                    interactPressed = true;
                if (Input.GetKeyDown(KeyCode.Escape)) skipPressed = true;
            }
            catch { }

            if (IsDialogueOpen)
            {
                _lineTimer += Time.deltaTime;
                HubLine cur = _activeLines[_lineIndex];
                int vis = Mathf.Min(cur.text.Length, Mathf.FloorToInt(_lineTimer * 48f));
                if (vis > _lastTypedChars && vis % 5 == 0 && vis < cur.text.Length)
                {
                    if (KimayaAudio.Instance != null)
                        KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Step, 0.20f);
                }
                _lastTypedChars = vis;

                if (skipPressed)
                {
                    CloseDialogue(markCompleted: true);
                    return;
                }

                if (interactPressed)
                {
                    if (vis < cur.text.Length - 2)
                    {
                        _lineTimer = cur.text.Length / 48f;
                    }
                    else
                    {
                        AdvanceLine();
                    }
                }
                return;
            }

            // Iniciar diálogo al acercarse al Hada Maya y presionar E / A
            if (_playerNearby && interactPressed)
            {
                OpenDialogue();
            }
        }

        public void OpenDialogue()
        {
            bool rescued = PlayerPrefs.GetInt("KiaraRescued", 0) == 1;
            _activeLines = rescued ? PostRescueLines : IntroHubLines;
            _lineIndex = 0;
            _lineTimer = 0f;
            _lastTypedChars = 0;
            IsDialogueOpen = true;
            LoadCurrentPortrait();

            if (KimayaHubDuoController.Instance != null)
                KimayaHubDuoController.Instance.controlsEnabled = false;
            if (KimayaAudio.Instance != null)
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Switch, 0.85f);
        }

        private void AdvanceLine()
        {
            _lineIndex++;
            if (_lineIndex >= _activeLines.Length)
            {
                CloseDialogue(markCompleted: true);
                return;
            }
            _lineTimer = 0f;
            _lastTypedChars = 0;
            LoadCurrentPortrait();
        }

        private void LoadCurrentPortrait()
        {
            if (_activeLines == null || _lineIndex < 0 || _lineIndex >= _activeLines.Length) return;
            int id = _activeLines[_lineIndex].portraitId;
            _currentPortraitTex = Resources.Load<Texture2D>($"Story/HubPortraits/HUB_{id:00}");
        }

        private void CloseDialogue(bool markCompleted)
        {
            IsDialogueOpen = false;
            if (KimayaHubDuoController.Instance != null)
                KimayaHubDuoController.Instance.controlsEnabled = true;

            if (markCompleted)
            {
                bool wasFirstTime = PlayerPrefs.GetInt("Kimaya_TalkedToMaya", 0) == 0;
                PlayerPrefs.SetInt("Kimaya_TalkedToMaya", 1);
                PlayerPrefs.Save();
                if (wasFirstTime)
                {
                    _unlockToastTimer = 4.5f;
                    if (KimayaAudio.Instance != null)
                        KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.DoorOpen, 0.95f);
                }
            }
        }

        private void OnGUI()
        {
            if (_mainCam == null) _mainCam = Camera.main;

            // Toast de portal desbloqueado tras hablar con Maya
            if (_unlockToastTimer > 0f && !IsDialogueOpen)
            {
                float tw = Mathf.Min(640f, Screen.width - 40f);
                float tx = (Screen.width - tw) * 0.5f;
                float ty = 78f;
                Color prev = GUI.color;
                GUI.color = new Color(0.22f, 0.88f, 0.48f, 0.96f);
                GUI.DrawTexture(new Rect(tx - 3f, ty - 3f, tw + 6f, 48f), Texture2D.whiteTexture);
                GUI.color = new Color(0.08f, 0.14f, 0.10f, 0.95f);
                GUI.DrawTexture(new Rect(tx, ty, tw, 42f), Texture2D.whiteTexture);
                GUI.color = prev;
                GUIStyle toastSt = KimayaUITheme.GetOnGUILabelStyle(14, TextAnchor.MiddleCenter, false);
                GUI.Label(new Rect(tx + 8f, ty + 2f, tw - 16f, 38f),
                    "<color=#4DFF88>¡PORTAL VERDE DESBLOQUEADO! Camina al Sendero de las Raíces</color>", toastSt);
            }

            // Indicador flotante sobre el Hada Maya cuando no está el diálogo abierto
            if (!IsDialogueOpen && _mainCam != null)
            {
                bool needsTalk = !HasTalkedToMaya;
                if (needsTalk || _playerNearby)
                {
                    Vector3 worldPos = transform.position + new Vector3(0f, 1.05f, 0f);
                    Vector3 sp = _mainCam.WorldToScreenPoint(worldPos);
                    if (sp.z > 0f)
                    {
                        float pulse = Mathf.Sin(Time.time * 4.5f) * 4f;
                        float bw = _playerNearby ? 270f : 210f;
                        float bh = 30f;
                        float bx = Mathf.Clamp(sp.x - bw * 0.5f, 12f, Screen.width - bw - 12f);
                        float by = Mathf.Clamp(Screen.height - sp.y - bh + pulse, 12f, Screen.height - bh - 12f);

                        Color prev = GUI.color;
                        GUI.color = needsTalk ? new Color(0.95f, 0.78f, 0.24f, 0.96f) : new Color(0.30f, 0.85f, 0.58f, 0.92f);
                        GUI.DrawTexture(new Rect(bx - 2f, by - 2f, bw + 4f, bh + 4f), Texture2D.whiteTexture);
                        GUI.color = new Color(0.09f, 0.08f, 0.16f, 0.94f);
                        GUI.DrawTexture(new Rect(bx, by, bw, bh), Texture2D.whiteTexture);
                        GUI.color = prev;

                        GUIStyle pSt = KimayaUITheme.GetOnGUILabelStyle(12, TextAnchor.MiddleCenter, false);
                        string label = _playerNearby
                            ? "<color=#F8D458>[E / A] HABLAR CON MAYA</color>"
                            : "<color=#FFE066>[!] HABLA CON MAYA</color>";
                        GUI.Label(new Rect(bx, by, bw, bh), label, pSt);
                    }
                }
                return;
            }

            if (!IsDialogueOpen || _activeLines == null || _lineIndex >= _activeLines.Length) return;

            GUI.depth = -15;
            HubLine cur = _activeLines[_lineIndex];
            Color prevCol = GUI.color;

            // Fondo semitransparente para enfocar la conversación
            GUI.color = new Color(0f, 0f, 0f, 0.52f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            float boxW = Mathf.Min(920f, Screen.width - 44f);
            float boxH = 220f;
            float boxX = (Screen.width - boxW) * 0.5f;
            float boxY = Screen.height - boxH - 22f;

            // Viñeta de retratos expresivos (HUB-01..27) flotando sobre la caja de diálogo
            if (_currentPortraitTex != null)
            {
                float pW = Mathf.Min(480f, boxW * 0.62f);
                float pH = pW * ((float)_currentPortraitTex.height / Mathf.Max(1, _currentPortraitTex.width));
                float pX = (Screen.width - pW) * 0.5f;
                float pY = boxY - pH - 18f;

                GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.98f);
                GUI.DrawTexture(new Rect(pX - 4f, pY - 4f, pW + 8f, pH + 8f), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(pX, pY, pW, pH), _currentPortraitTex, ScaleMode.ScaleToFit);
            }

            // Caja de Diálogo RPG Pixel-Art
            GUI.color = new Color(0.88f, 0.68f, 0.22f, 0.98f);
            GUI.DrawTexture(new Rect(boxX - 3f, boxY - 3f, boxW + 6f, boxH + 6f), Texture2D.whiteTexture);
            GUI.color = new Color(0.09f, 0.08f, 0.16f, 0.97f);
            GUI.DrawTexture(new Rect(boxX, boxY, boxW, boxH), Texture2D.whiteTexture);

            // Placa de hablante
            GUI.color = cur.badgeColor;
            GUI.DrawTexture(new Rect(boxX + 18f, boxY - 16f, 290f, 32f), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUIStyle badgeSt = KimayaUITheme.GetOnGUILabelStyle(14, TextAnchor.MiddleCenter, false);
            GUI.Label(new Rect(boxX + 18f, boxY - 16f, 290f, 32f), $"<color=#FFFFFF>{cur.speaker}</color>", badgeSt);

            GUIStyle cntSt = KimayaUITheme.GetOnGUILabelStyle(12, TextAnchor.MiddleRight, false);
            GUI.Label(new Rect(boxX + boxW - 180f, boxY + 8f, 160f, 22f), $"<color=#94A3B8>{_lineIndex + 1} / {_activeLines.Length}</color>", cntSt);

            // Texto con typewriter
            int visChars = Mathf.Min(cur.text.Length, Mathf.FloorToInt(_lineTimer * 48f));
            string shown = cur.text.Substring(0, visChars);
            GUIStyle bodySt = KimayaUITheme.GetOnGUILabelStyle(21, TextAnchor.UpperLeft, true);
            GUI.Label(new Rect(boxX + 24f, boxY + 32f, boxW - 48f, boxH - 82f), $"<color=#F8FAFC>{shown}</color>", bodySt);

            GUIStyle btnSt = KimayaUITheme.GetOnGUIButtonStyle(12);
            if (GUI.Button(new Rect(boxX + boxW - 390f, boxY + boxH - 42f, 200f, 34f), "SIGUIENTE [E / A]", btnSt))
            {
                AdvanceLine();
            }
            if (GUI.Button(new Rect(boxX + boxW - 180f, boxY + boxH - 42f, 160f, 34f), "SALTAR [ESC]", btnSt))
            {
                CloseDialogue(markCompleted: true);
            }

            GUI.color = prevCol;
        }
    }
}
