using System.Collections;
using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// NPC de aldea con sistema de diálogo multi-fase, patrulla y Y-Sorting.
    /// Soporta NPCDialogueData (ScriptableObject) O strings inline simples.
    /// </summary>
    public class VillageNPC : MonoBehaviour
    {
        // ── Datos de diálogo ──────────────────────────────────────────────────
        [Header("Diálogo (ScriptableObject — recomendado)")]
        public NPCDialogueData dialogueData;

        [Header("Diálogo inline (si no hay ScriptableObject)")]
        public string npcDisplayName = "Aldeano";
        [TextArea(2, 4)] public string[] inlineDialogueLines = new string[]
        {
            "¡Bienvenido a la aldea!",
            "¿Cómo puedo ayudarte hoy?"
        };

        // ── Patrulla ──────────────────────────────────────────────────────────
        [Header("Patrulla")]
        public float patrolDistance = 2f;
        public float moveSpeed = 0.8f;
        public float waitTime = 1.5f;

        [Header("Interacción")]
        public float interactRadius = 1.6f;

        // ── Estado interno ────────────────────────────────────────────────────
        private SpriteRenderer _sr;
        private Vector3 _pointA;
        private Vector3 _pointB;
        private bool _movingToB = true;
        private bool _waiting = false;

        private enum DialoguePhase { Greeting, General, Lore, Tip, Danger, Farewell }
        private DialoguePhase _currentPhase = DialoguePhase.Greeting;
        private bool _hasGreeted = false;
        private int _lineIndex = 0;
        private string[] _currentLines;

        private bool _showingDialogue = false;
        private string _displayName => dialogueData != null ? dialogueData.npcName : npcDisplayName;
        private Color _nameColor => dialogueData != null ? dialogueData.nameColor : new Color(1f, 0.85f, 0.4f);
        private string _currentText = "";
        private string _fullText = "";
        private float _typeTimer = 0f;
        private int _charIndex = 0;
        private const float TYPE_SPEED = 0.035f;
        private bool _typing = false;

        // Estilo GUI
        private GUIStyle _boxStyle;
        private GUIStyle _nameStyle;
        private GUIStyle _textStyle;
        private GUIStyle _hintStyle;

        // ── Unity ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();

            _pointA = transform.position;
            _pointB = transform.position + Vector3.right * patrolDistance;
        }

        private void Update()
        {
            // Y-Sorting dinámico
            if (_sr != null)
                _sr.sortingOrder = 500 - Mathf.RoundToInt(transform.position.y * 10f);

            // Efecto máquina de escribir
            if (_typing)
            {
                _typeTimer += Time.deltaTime;
                if (_typeTimer >= TYPE_SPEED)
                {
                    _typeTimer = 0f;
                    _charIndex = Mathf.Min(_charIndex + 1, _fullText.Length);
                    _currentText = _fullText.Substring(0, _charIndex);
                    if (_charIndex >= _fullText.Length) _typing = false;
                }
            }

            if (!_showingDialogue && !_waiting)
                Patrol();

            CheckInteraction();
        }

        // ── Patrulla ──────────────────────────────────────────────────────────
        private void Patrol()
        {
            Vector3 target = _movingToB ? _pointB : _pointA;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (_sr != null) _sr.flipX = !_movingToB;

            if (Vector3.Distance(transform.position, target) < 0.05f)
                StartCoroutine(WaitAndTurn());
        }

        private IEnumerator WaitAndTurn()
        {
            _waiting = true;
            yield return new WaitForSeconds(waitTime);
            _movingToB = !_movingToB;
            _waiting = false;
        }

        // ── Interacción ───────────────────────────────────────────────────────
        private void CheckInteraction()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist > interactRadius && _showingDialogue)
            {
                _showingDialogue = false;
                return;
            }

            if (dist <= interactRadius && Input.GetKeyDown(KeyCode.E))
            {
                if (_showingDialogue)
                {
                    // Si está escribiendo → mostrar texto completo
                    if (_typing)
                    {
                        _currentText = _fullText;
                        _typing = false;
                    }
                    else
                    {
                        // Avanzar al siguiente diálogo
                        AdvanceDialogue();
                    }
                }
                else
                {
                    OpenDialogue();
                }
            }
        }

        private void OpenDialogue()
        {
            _showingDialogue = true;

            if (!_hasGreeted && dialogueData != null && dialogueData.greetings.Length > 0)
            {
                _currentLines = dialogueData.greetings;
                _currentPhase = DialoguePhase.Greeting;
                _hasGreeted = true;
            }
            else
            {
                _currentLines = PickNextPhaseLines();
            }

            _lineIndex = 0;
            ShowLine(_currentLines[0]);
        }

        private string[] PickNextPhaseLines()
        {
            if (dialogueData == null) return inlineDialogueLines;

            // Rotar entre tipos de diálogo
            switch (_currentPhase)
            {
                case DialoguePhase.Greeting:
                case DialoguePhase.Farewell:
                    _currentPhase = DialoguePhase.General;
                    return dialogueData.generalLines.Length > 0 ? dialogueData.generalLines : inlineDialogueLines;
                case DialoguePhase.General:
                    _currentPhase = DialoguePhase.Lore;
                    return dialogueData.loreLines.Length > 0 ? dialogueData.loreLines : dialogueData.generalLines;
                case DialoguePhase.Lore:
                    _currentPhase = DialoguePhase.Tip;
                    return dialogueData.tipLines.Length > 0 ? dialogueData.tipLines : dialogueData.generalLines;
                case DialoguePhase.Tip:
                    _currentPhase = DialoguePhase.Farewell;
                    return dialogueData.farewells.Length > 0 ? dialogueData.farewells : dialogueData.generalLines;
                default:
                    _currentPhase = DialoguePhase.General;
                    return dialogueData.generalLines.Length > 0 ? dialogueData.generalLines : inlineDialogueLines;
            }
        }

        private void ShowLine(string text)
        {
            _fullText = text;
            _currentText = "";
            _charIndex = 0;
            _typeTimer = 0f;
            _typing = true;
        }

        private void AdvanceDialogue()
        {
            _lineIndex++;
            if (_lineIndex < _currentLines.Length)
            {
                ShowLine(_currentLines[_lineIndex]);
            }
            else
            {
                _showingDialogue = false;
                _lineIndex = 0;
            }
        }

        // ── OnGUI ──────────────────────────────────────────────────────────────
        private void OnGUI()
        {
            if (!_showingDialogue) return;

            InitStyles();

            // Pantalla: caja abajo centrada
            float boxW = Mathf.Min(420f, Screen.width - 40f);
            float boxH = 110f;
            float bx = (Screen.width - boxW) / 2f;
            float by = Screen.height - boxH - 20f;
            Rect box = new Rect(bx, by, boxW, boxH);

            // Fondo sombra
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.Box(new Rect(box.x + 3, box.y + 3, box.width, box.height), GUIContent.none, _boxStyle);
            GUI.color = Color.white;

            // Fondo principal
            GUI.Box(box, GUIContent.none, _boxStyle);

            // Nombre del NPC
            GUI.color = _nameColor;
            GUI.Label(new Rect(box.x + 12, box.y + 8, box.width - 24, 22), _displayName, _nameStyle);
            GUI.color = Color.white;

            // Separador
            GUI.color = new Color(_nameColor.r, _nameColor.g, _nameColor.b, 0.5f);
            GUI.Box(new Rect(box.x + 10, box.y + 30, box.width - 20, 1), GUIContent.none);
            GUI.color = Color.white;

            // Texto con efecto máquina de escribir
            GUI.Label(new Rect(box.x + 12, box.y + 35, box.width - 24, 58), _currentText, _textStyle);

            // Hint
            string hint = _typing ? "[E] Saltar texto" : (_lineIndex < _currentLines.Length - 1 ? "[E] Continuar ▶" : "[E] Cerrar ✖");
            GUI.color = new Color(1, 1, 1, 0.55f);
            GUI.Label(new Rect(box.x + box.width - 130, box.y + boxH - 18, 125, 16), hint, _hintStyle);
            GUI.color = Color.white;

            // Indicador de rango (E)
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (!_showingDialogue && dist <= interactRadius)
                {
                    // Muestra el prompt E sobre la cabeza del NPC
                }
            }
        }

        // Muestra el "[E]" flotante si el jugador está cerca y no hay diálogo
        private void LateUpdate()
        {
            // (se maneja en OnGUI para simplicidad)
        }

        private void InitStyles()
        {
            if (_boxStyle != null) return;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTex(2, 2, new Color(0.06f, 0.05f, 0.12f, 0.92f));
            _boxStyle.border = new RectOffset(6, 6, 6, 6);

            _nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                wordWrap = false,
                normal = { textColor = Color.white }
            };

            _textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = new Color(0.95f, 0.95f, 0.92f) },
                padding = new RectOffset(0, 0, 0, 0)
            };

            _hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.gray }
            };
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }

        // ── Gizmos ─────────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Vector3 pA = Application.isPlaying ? _pointA : transform.position;
            Vector3 pB = Application.isPlaying ? _pointB : transform.position + Vector3.right * patrolDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawWireSphere(pA, 0.12f);
            Gizmos.DrawWireSphere(pB, 0.12f);

            Gizmos.color = new Color(0, 1, 0, 0.25f);
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
