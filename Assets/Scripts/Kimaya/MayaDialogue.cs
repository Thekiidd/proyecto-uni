using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// DialogueController de Maya — muestra diálogos en secuencia
    /// con efecto de máquina de escribir, retrato y avance con E/Espacio.
    /// Se activa al entrar al trigger o por evento.
    /// </summary>
    public class MayaDialogue : MonoBehaviour
    {
        [Header("Diálogos")]
        [TextArea(2, 4)]
        public string[] lines = new string[]
        {
            "Este bosque encuentra las grietas entre quienes llegan y las hace crecer.",
            "Kiara todavía está cerca, pero ninguna podrá alcanzarla por separado.",
            "Deben trabajar juntas. Usa TAB para cambiar entre Falin y Mia.",
            "Falin puede mover objetos pesados. Mia puede pasar por espacios pequeños.",
            "Presiona Q con Mia para usar su olfato y revelar caminos ocultos.",
            "¡Tengan cuidado con las raíces del Zarzal Sombrío!"
        };
        public string speakerName = "Maya";

        [Header("Visual")]
        public Sprite mayaPortrait;
        public Color  nameColor      = new Color(0.7f, 0.4f, 1f);
        public float  typeSpeed      = 0.03f;

        [Header("Trigger automático")]
        public bool activateOnTrigger = true;
        public bool onlyOnce          = true;

        [Header("Evento al terminar")]
        public UnityEvent onDialogueFinished;

        private bool _hasPlayed = false;
        private bool _showing   = false;
        private int  _lineIndex = 0;
        private string _currentText = "";
        private string _fullText    = "";
        private float  _typeTimer   = 0f;
        private bool   _typing      = false;

        // GUI styles
        private GUIStyle _boxStyle;
        private GUIStyle _nameStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _hintStyle;
        private Texture2D _portraitTex;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!activateOnTrigger) return;
            if (_hasPlayed && onlyOnce) return;
            if (!other.CompareTag("Player")) return;
            StartDialogue();
        }

        public void StartDialogue()
        {
            if (_showing) return;
            _hasPlayed  = true;
            _showing    = true;
            _lineIndex  = 0;
            ShowLine(lines[0]);

            // Detener jugadores mientras habla Maya
            foreach (var pc in FindObjectsByType<PlayerController2D>(FindObjectsInactive.Exclude))
                pc.SetActive(false);
        }

        private void ShowLine(string text)
        {
            _fullText    = text;
            _currentText = "";
            _typeTimer   = 0f;
            _typing      = true;
        }

        private void Update()
        {
            if (!_showing) return;

            // Máquina de escribir
            if (_typing)
            {
                _typeTimer += Time.deltaTime;
                if (_typeTimer >= typeSpeed)
                {
                    _typeTimer = 0f;
                    int idx = _currentText.Length + 1;
                    _currentText = _fullText.Substring(0, Mathf.Min(idx, _fullText.Length));
                    if (_currentText.Length >= _fullText.Length) _typing = false;
                }
            }

            // Avanzar con E o Espacio
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                if (_typing)
                {
                    _currentText = _fullText;
                    _typing      = false;
                }
                else
                {
                    _lineIndex++;
                    if (_lineIndex < lines.Length)
                        ShowLine(lines[_lineIndex]);
                    else
                        EndDialogue();
                }
            }
        }

        private void EndDialogue()
        {
            _showing = false;

            // Reactivar jugadores
            foreach (var pc in FindObjectsByType<PlayerController2D>(FindObjectsInactive.Exclude))
                pc.SetActive(true);

            onDialogueFinished?.Invoke();
        }

        private void OnGUI()
        {
            if (!_showing) return;
            InitStyles();

            float boxW = Mathf.Min(500f, Screen.width - 40f);
            float boxH = 120f;
            float portraitSize = 90f;
            float bx = (Screen.width  - boxW) / 2f;
            float by = Screen.height - boxH - 20f;

            // Fondo
            GUI.color = new Color(0f, 0f, 0f, 0.6f);
            GUI.Box(new Rect(bx + 3, by + 3, boxW, boxH), GUIContent.none, _boxStyle);
            GUI.color = Color.white;
            GUI.Box(new Rect(bx, by, boxW, boxH), GUIContent.none, _boxStyle);

            // Retrato de Maya
            if (mayaPortrait != null)
            {
                if (_portraitTex == null) _portraitTex = mayaPortrait.texture;
                GUI.DrawTexture(new Rect(bx + 8, by + 8, portraitSize, portraitSize),
                    _portraitTex, ScaleMode.ScaleToFit);
            }
            else
            {
                // Círculo de color placeholder
                GUI.color = nameColor;
                GUI.Box(new Rect(bx + 8, by + 8, portraitSize, portraitSize), "🧚 Maya", _boxStyle);
                GUI.color = Color.white;
            }

            float textX = bx + (mayaPortrait != null ? portraitSize + 18f : 16f);
            float textW = boxW - (mayaPortrait != null ? portraitSize + 26f : 24f);

            // Nombre
            GUI.color = nameColor;
            GUI.Label(new Rect(textX, by + 8, textW, 22), speakerName, _nameStyle);
            GUI.color = Color.white;

            // Separador
            GUI.color = new Color(nameColor.r, nameColor.g, nameColor.b, 0.4f);
            GUI.Box(new Rect(textX, by + 30, textW, 1), GUIContent.none);
            GUI.color = Color.white;

            // Texto
            GUI.Label(new Rect(textX, by + 35, textW, 70), _currentText, _bodyStyle);

            // Hint
            string hint = _typing ? "[ESPACIO] Saltar" : (_lineIndex < lines.Length - 1 ? "[ESPACIO] Siguiente ▶" : "[ESPACIO] Cerrar ✓");
            GUI.color = new Color(1f, 1f, 1f, 0.5f);
            GUI.Label(new Rect(bx + boxW - 180, by + boxH - 18, 175, 16), hint, _hintStyle);
            GUI.color = Color.white;

            // Indicador de línea
            GUI.color = new Color(1f, 1f, 1f, 0.35f);
            GUI.Label(new Rect(bx + 8, by + boxH - 18, 80, 16),
                $"{_lineIndex + 1} / {lines.Length}", _hintStyle);
            GUI.color = Color.white;
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.03f, 0.12f, 0.93f));

            _nameStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };

            _bodyStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 13, wordWrap = true, normal = { textColor = new Color(0.95f, 0.92f, 0.98f) } };

            _hintStyle = new GUIStyle(GUI.skin.label)
            { fontSize = 11, alignment = TextAnchor.MiddleRight, normal = { textColor = Color.gray } };
        }

        private static Texture2D MakeTex(int w, int h, Color c)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = c;
            var t = new Texture2D(w, h); t.SetPixels(pix); t.Apply(); return t;
        }
    }

    /// <summary>
    /// Zona de victoria — llega aquí para rescatar a Kiara y ganar.
    /// </summary>
    public class VictoryZone : MonoBehaviour
    {
        public string victoryMessage = "¡Kiara ha sido liberada!\n¡Las tres hermanas juntas de nuevo!";
        private bool _triggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;
            _triggered = true;

            // Necesitamos que AMBAS perritas hayan llegado aquí o solo una
            KimayaGameManager.Instance?.Victory();
        }
    }
}
