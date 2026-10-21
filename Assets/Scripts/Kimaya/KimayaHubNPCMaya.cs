using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Guía espiritual Maya en la Aldea Hub.
    /// Explica la misión de rescate de Kiara, las mecánicas únicas de Falin y Mia,
    /// y da consejos sobre cómo resolver los templos cooperativos y consultar la tabla de récords.
    /// </summary>
    public class KimayaHubNPCMaya : MonoBehaviour
    {
        [Header("Configuración de Maya")]
        public string npcName = "Maya (Guía Espiritual)";
        public float interactionRadius = 2.2f;

        [Header("Diálogos")]
        [TextArea(2, 4)]
        public string[] dialogueLines = new string[]
        {
            "¡Saludos, Falin y Mia! Bienvenidos al Bosque del Eco.\nPara moverte por el hub usa [W, A, S, D] o las flechas de dirección.",
            "Para interactuar con personajes, objetos o entradas a los niveles, acércate y presiona la tecla [E].",
            "¡Nuestra hermana Kiara ha quedado atrapada por el Zarzal Sombrío en la cima del Sendero de las Raíces y debemos rescatarla cuanto antes!",
            "El portal luminoso al frente conduce al Sendero de las Raíces. Es la única entrada disponible en este momento.",
            "Las otras tres entradas del bosque están bloqueadas por barreras mágicas oscuras y solo se abrirán cuando restauremos las cuatro runas del círculo central.",
            "¡Acérquense a la entrada disponible y elijan con sabiduría a su campeona para el ascenso! Fairytopia confía en ustedes."
        };

        private bool isPlayerNear = false;
        private bool isDialogueOpen = false;
        private int currentLineIndex = 0;
        private Camera mainCam;
        private Vector3 startPos;

        private static GUIStyle promptStyle;
        private static GUIStyle boxStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle textStyle;
        private static GUIStyle buttonStyle;

        private void Start()
        {
            mainCam = Camera.main;
            startPos = transform.position;
        }

        private void Update()
        {
            // Efecto flotante suave de hada/espíritu
            float bob = Mathf.Sin(Time.time * 2.5f) * 0.12f;
            transform.position = startPos + new Vector3(0, bob, 0);

            if (KimayaHubDuoController.Instance == null) return;
            Transform activeDog = KimayaHubDuoController.Instance.GetActiveTransform();
            if (activeDog == null) return;

            float dist = Vector2.Distance(startPos, activeDog.position);
            isPlayerNear = (dist <= interactionRadius);

            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                if (!isDialogueOpen)
                {
                    OpenDialogue();
                }
                else
                {
                    AdvanceDialogue();
                }
            }

            if (isDialogueOpen && (Input.GetKeyDown(KeyCode.Escape) || !isPlayerNear))
            {
                CloseDialogue();
            }
        }

        private void OpenDialogue()
        {
            isDialogueOpen = true;
            currentLineIndex = 0;
            if (KimayaHubDuoController.Instance != null)
                KimayaHubDuoController.Instance.controlsEnabled = false;
        }

        private void AdvanceDialogue()
        {
            currentLineIndex++;
            if (currentLineIndex >= dialogueLines.Length)
            {
                CloseDialogue();
            }
        }

        private void CloseDialogue()
        {
            isDialogueOpen = false;
            currentLineIndex = 0;
            if (KimayaHubDuoController.Instance != null)
                KimayaHubDuoController.Instance.controlsEnabled = true;
        }

        private void OnGUI()
        {
            InitStyles();

            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            if (isPlayerNear && !isDialogueOpen)
            {
                Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
                if (screenPos.z >= 0)
                {
                    string prompt = "🧚 [E] Hablar con Maya";
                    float w = 200f;
                    float h = 34f;
                    float x = screenPos.x - (w * 0.5f);
                    float y = Screen.height - screenPos.y - h;
                    GUI.Box(new Rect(x, y, w, h), prompt, promptStyle);
                }
            }

            if (isDialogueOpen)
            {
                DrawDialogueWindow();
            }
        }

        private void DrawDialogueWindow()
        {
            float w = Mathf.Min(640f, Screen.width - 40f);
            float h = 180f;
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height - h - 30f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none, boxStyle);

            // Título
            GUI.Label(new Rect(x + 20, y + 15, w - 40, 28), $"✨ {npcName}", titleStyle);

            // Contenido
            string line = dialogueLines[Mathf.Clamp(currentLineIndex, 0, dialogueLines.Length - 1)];
            GUI.Label(new Rect(x + 20, y + 48, w - 40, 75), line, textStyle);

            // Indicador de avance
            string footer = $"({currentLineIndex + 1}/{dialogueLines.Length})   [E] Continuar  |  [ESC] Salir";
            if (GUI.Button(new Rect(x + w - 170, y + h - 42, 150, 30), "Siguiente ▶", buttonStyle) ||
                (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            {
                AdvanceDialogue();
            }

            GUI.Label(new Rect(x + 20, y + h - 38, 300, 25), footer, textStyle);
        }

        private void InitStyles()
        {
            if (promptStyle != null) return;

            Texture2D darkBg = new Texture2D(1, 1);
            darkBg.SetPixel(0, 0, new Color(0.08f, 0.12f, 0.22f, 0.94f));
            darkBg.Apply();

            Texture2D btnBg = new Texture2D(1, 1);
            btnBg.SetPixel(0, 0, new Color(0.2f, 0.5f, 0.8f, 0.95f));
            btnBg.Apply();

            promptStyle = new GUIStyle();
            promptStyle.normal.background = darkBg;
            promptStyle.normal.textColor = Color.cyan;
            promptStyle.fontSize = 14;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;

            boxStyle = new GUIStyle();
            boxStyle.normal.background = darkBg;
            boxStyle.padding = new RectOffset(15, 15, 15, 15);

            titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
            titleStyle.fontSize = 17;
            titleStyle.fontStyle = FontStyle.Bold;

            textStyle = new GUIStyle();
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 14;
            textStyle.wordWrap = true;

            buttonStyle = new GUIStyle();
            buttonStyle.normal.background = btnBg;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 13;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}
