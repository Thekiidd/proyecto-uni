using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// NPC de aldea: patrulla entre dos puntos y muestra diálogo al interactuar.
    /// Usa Y-Sorting automático en el SpriteRenderer.
    /// </summary>
    public class VillageNPC : MonoBehaviour
    {
        [Header("Patrulla")]
        [Tooltip("Distancia en unidades que camina desde su posición inicial.")]
        public float patrolDistance = 2f;
        [Tooltip("Velocidad de movimiento del NPC.")]
        public float moveSpeed = 0.8f;
        [Tooltip("Tiempo que espera en cada extremo antes de girar.")]
        public float waitTime = 1.5f;

        [Header("Diálogo")]
        [Tooltip("Frases que dice este NPC al interactuar (se elige una al azar).")]
        [TextArea(2, 4)]
        public string[] dialogueLines = new string[]
        {
            "¡Bienvenido a nuestra aldea, amigo perro!",
            "¿Has visto mi gato? Creo que huyó al bosque...",
            "El mercado abre mañana temprano.",
            "Dicen que hay cristales mágicos en las ruinas del norte."
        };

        [Header("UI de diálogo")]
        [Tooltip("Radio dentro del cual el jugador puede interactuar (E).")]
        public float interactRadius = 1.5f;

        // ── Referencias internas ──────────────────────────────────────────────
        private SpriteRenderer _sr;
        private Vector3 _pointA;
        private Vector3 _pointB;
        private bool _movingToB = true;
        private bool _waiting = false;

        // Diálogo simple en pantalla
        private bool _showingDialogue = false;
        private string _currentLine = "";
        private GUIStyle _boxStyle;
        private GUIStyle _textStyle;

        // ── Unity ─────────────────────────────────────────────────────────────
        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null)
                _sr = GetComponentInChildren<SpriteRenderer>();

            _pointA = transform.position;
            _pointB = transform.position + Vector3.right * patrolDistance;
        }

        private void Update()
        {
            // Y-Sorting
            if (_sr != null)
                _sr.sortingOrder = 500 - Mathf.RoundToInt(transform.position.y * 10f);

            // No mover si está mostrando diálogo
            if (_showingDialogue) return;

            if (!_waiting)
                Patrol();

            // Detectar interacción con el jugador
            CheckInteraction();
        }

        // ── Patrulla ──────────────────────────────────────────────────────────
        private void Patrol()
        {
            Vector3 target = _movingToB ? _pointB : _pointA;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            // Flip según dirección
            if (_sr != null)
                _sr.flipX = (_movingToB == false);

            // ¿Llegó al destino?
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
            // Buscar el jugador (tag "Player")
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist <= interactRadius && Input.GetKeyDown(KeyCode.E))
            {
                if (_showingDialogue)
                    _showingDialogue = false;
                else
                    ShowDialogue();
            }

            // Cerrar diálogo si el jugador se aleja
            if (dist > interactRadius && _showingDialogue)
                _showingDialogue = false;
        }

        private void ShowDialogue()
        {
            if (dialogueLines.Length == 0) return;
            _currentLine = dialogueLines[Random.Range(0, dialogueLines.Length)];
            _showingDialogue = true;
        }

        // ── OnGUI sencillo (burbuja de diálogo) ───────────────────────────────
        private void OnGUI()
        {
            if (!_showingDialogue) return;

            // Inicializar estilos la primera vez
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box);
                _boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.1f, 0.88f));
                _boxStyle.border = new RectOffset(4, 4, 4, 4);

                _textStyle = new GUIStyle(GUI.skin.label);
                _textStyle.fontSize = 14;
                _textStyle.normal.textColor = Color.white;
                _textStyle.wordWrap = true;
                _textStyle.alignment = TextAnchor.MiddleLeft;
                _textStyle.padding = new RectOffset(8, 8, 6, 6);
            }

            // Posición: sobre la cabeza del NPC
            Vector3 worldPos = transform.position + Vector3.up * 0.8f;
            Vector3 screen = Camera.main.WorldToScreenPoint(worldPos);

            float boxW = 260f;
            float boxH = 70f;
            float x = Mathf.Clamp(screen.x - boxW / 2f, 4f, Screen.width - boxW - 4f);
            float y = Mathf.Clamp(Screen.height - screen.y - boxH, 4f, Screen.height - boxH - 4f);

            Rect boxRect = new Rect(x, y, boxW, boxH);

            // Sombra
            GUI.Box(new Rect(boxRect.x + 2, boxRect.y + 2, boxRect.width, boxRect.height),
                    GUIContent.none, _boxStyle);
            // Caja principal
            GUI.Box(boxRect, GUIContent.none, _boxStyle);
            // Nombre del NPC
            GUI.Label(new Rect(boxRect.x + 8, boxRect.y + 4, boxRect.width - 16, 18),
                      $"<b>{gameObject.name}</b>", new GUIStyle(_textStyle) { fontSize = 12, normal = { textColor = new Color(1f, 0.85f, 0.4f) } });
            // Línea de diálogo
            GUI.Label(new Rect(boxRect.x + 4, boxRect.y + 20, boxRect.width - 8, boxH - 24),
                      _currentLine, _textStyle);
            // Hint
            GUI.Label(new Rect(boxRect.x + boxRect.width - 70, boxRect.y + boxH - 16, 68, 14),
                      "[E] cerrar", new GUIStyle(_textStyle) { fontSize = 10, normal = { textColor = Color.gray } });
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

        // ── Gizmos en Editor ──────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            Vector3 pA = Application.isPlaying ? _pointA : transform.position;
            Vector3 pB = Application.isPlaying ? _pointB : transform.position + Vector3.right * patrolDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawWireSphere(pA, 0.15f);
            Gizmos.DrawWireSphere(pB, 0.15f);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
