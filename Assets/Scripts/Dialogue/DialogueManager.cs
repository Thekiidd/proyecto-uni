using System;
using System.Collections;
using Platformer.Core;
using Platformer.TopDown;
using UnityEngine;

namespace Platformer.Dialogue
{
    /// <summary>
    /// Gestor de diálogos RPG en pantalla.
    /// Renderiza un marco elegante con OnGUI (100% inmune a errores de fuentes o canvas en Unity 6).
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public bool IsOpen { get; private set; }

        private DialogueData currentDialogue;
        private int currentLineIndex;
        private string displayedText = "";
        private bool isTyping = false;
        private Coroutine typingCoroutine;
        private Action onDialogueComplete;

        private Texture2D boxTex;
        private Texture2D nameBadgeTex;
        private GUIStyle boxStyle;
        private GUIStyle nameStyle;
        private GUIStyle textStyle;
        private GUIStyle hintStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartDialogue(DialogueData dialogue, Action onComplete = null)
        {
            if (dialogue == null || dialogue.lines == null || dialogue.lines.Length == 0) return;

            currentDialogue = dialogue;
            currentLineIndex = 0;
            onDialogueComplete = onComplete;
            IsOpen = true;

            // Pausar movimiento del perro
            SetPlayerControl(false);

            ShowLine(currentLineIndex);
        }

        private void ShowLine(int index)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeLine(currentDialogue.lines[index]));
        }

        private IEnumerator TypeLine(string line)
        {
            isTyping = true;
            displayedText = "";
            foreach (char letter in line)
            {
                displayedText += letter;
                yield return new WaitForSeconds(0.02f);
            }
            isTyping = false;
        }

        private void Update()
        {
            if (!IsOpen) return;

            // Avanzar diálogo con E, Espacio, Enter o clic usando el nuevo Input System
            bool advance = false;
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
                    advance = true;
            }
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                advance = true;
            }

            if (advance)
            {
                if (isTyping)
                {
                    // Si aún está escribiendo, autocompletar la frase inmediatamente
                    StopCoroutine(typingCoroutine);
                    displayedText = currentDialogue.lines[currentLineIndex];
                    isTyping = false;
                }
                else
                {
                    // Pasar a la siguiente frase
                    currentLineIndex++;
                    if (currentLineIndex < currentDialogue.lines.Length)
                    {
                        ShowLine(currentLineIndex);
                    }
                    else
                    {
                        EndDialogue();
                    }
                }
            }
        }

        private void EndDialogue()
        {
            IsOpen = false;
            displayedText = "";

            // Entregar recompensa si aplica
            if (currentDialogue != null && currentDialogue.rewardCoins > 0 && GameData.Instance != null)
            {
                GameData.Instance.AddCoins(currentDialogue.rewardCoins);
                currentDialogue.rewardCoins = 0; // Solo una vez
            }

            // Restaurar control del perro
            SetPlayerControl(true);

            onDialogueComplete?.Invoke();
            currentDialogue = null;
        }

        private void SetPlayerControl(bool enabled)
        {
            var dog = FindAnyObjectByType<TopDownDogController>();
            if (dog != null) dog.controlEnabled = enabled;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            boxTex = MakeColorTex(new Color(0.05f, 0.05f, 0.12f, 0.92f));
            nameBadgeTex = MakeColorTex(new Color(0.18f, 0.12f, 0.05f, 0.95f));

            boxStyle = new GUIStyle();
            boxStyle.normal.background = boxTex;

            nameStyle = new GUIStyle();
            nameStyle.normal.background = nameBadgeTex;
            nameStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            nameStyle.fontSize = 20;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;

            textStyle = new GUIStyle();
            textStyle.normal.textColor = Color.white;
            textStyle.fontSize = 18;
            textStyle.wordWrap = true;
            textStyle.alignment = TextAnchor.UpperLeft;

            hintStyle = new GUIStyle();
            hintStyle.normal.textColor = new Color(0.7f, 0.9f, 1f, 0.8f);
            hintStyle.fontSize = 14;
            hintStyle.fontStyle = FontStyle.Italic;
            hintStyle.alignment = TextAnchor.LowerRight;

            stylesInitialized = true;
        }

        private Texture2D MakeColorTex(Color col)
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            if (!IsOpen || currentDialogue == null) return;

            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;

            // Dimensiones de la caja de diálogo (centrada abajo)
            float boxW = Mathf.Min(800f, screenW * 0.9f);
            float boxH = 160f;
            float boxX = (screenW - boxW) * 0.5f;
            float boxY = screenH - boxH - 25f;

            // 1. Marco exterior
            GUI.Box(new Rect(boxX, boxY, boxW, boxH), GUIContent.none, boxStyle);

            // Borde dorado simulado
            GUI.color = new Color(0.85f, 0.7f, 0.2f, 0.8f);
            GUI.Box(new Rect(boxX - 2, boxY - 2, boxW + 4, 2), GUIContent.none);
            GUI.Box(new Rect(boxX - 2, boxY + boxH, boxW + 4, 2), GUIContent.none);
            GUI.Box(new Rect(boxX - 2, boxY, 2, boxH), GUIContent.none);
            GUI.Box(new Rect(boxX + boxW, boxY, 2, boxH), GUIContent.none);
            GUI.color = Color.white;

            // 2. Placa con el nombre del NPC
            float nameW = Mathf.Max(160f, currentDialogue.npcName.Length * 14f);
            float nameH = 34f;
            GUI.Box(new Rect(boxX + 20f, boxY - 18f, nameW, nameH), currentDialogue.npcName, nameStyle);

            // 3. Texto del diálogo
            Rect textRect = new Rect(boxX + 25f, boxY + 28f, boxW - 50f, boxH - 55f);
            GUI.Label(textRect, displayedText, textStyle);

            // 4. Indicador de avanzar
            string hint = isTyping ? "[E] Saltar ►" : "[E / Espacio] Continuar ►";
            Rect hintRect = new Rect(boxX + 20f, boxY + boxH - 28f, boxW - 40f, 22f);
            GUI.Label(hintRect, hint, hintStyle);
        }
    }
}
