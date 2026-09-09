using Platformer.TopDown;
using UnityEngine;

namespace Platformer.Dialogue
{
    /// <summary>
    /// Componente que se añade a cualquier personaje (aldeano, sabio, niña) del pueblo.
    /// Muestra un prompt [E] Hablar cuando el perro se acerca, y abre su diálogo al interactuar.
    /// </summary>
    public class NPCInteractable : MonoBehaviour
    {
        public DialogueData dialogue;
        public float interactRadius = 2.2f;

        private Transform playerTransform;
        private SpriteRenderer spriteRenderer;
        private bool isPlayerNear = false;
        private Camera mainCamera;

        private static GUIStyle promptStyle;
        private static Texture2D promptBgTex;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;

            var player = FindAnyObjectByType<TopDownDogController>();
            if (player != null) playerTransform = player.transform;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                var player = FindAnyObjectByType<TopDownDogController>();
                if (player != null) playerTransform = player.transform;
                return;
            }

            float dist = Vector2.Distance(transform.position, playerTransform.position);
            isPlayerNear = (dist <= interactRadius);

            // Orientar sprite del NPC hacia el perro
            if (isPlayerNear && spriteRenderer != null)
            {
                if (playerTransform.position.x < transform.position.x - 0.2f)
                    spriteRenderer.flipX = true;
                else if (playerTransform.position.x > transform.position.x + 0.2f)
                    spriteRenderer.flipX = false;
            }

            // Profundidad de orden de capa Top-Down (siempre visible sobre el fondo de la aldea)
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 500 - Mathf.RoundToInt(transform.position.y * 10f);
            }

            // Iniciar charla al presionar E con el nuevo Input System
            bool ePressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                ePressed = true;

            if (isPlayerNear && ePressed)
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
                {
                    DialogueManager.Instance.StartDialogue(dialogue);
                }
            }
        }

        private void OnGUI()
        {
            // Mostrar aviso [E] Hablar si el jugador está cerca y no hay diálogo abierto
            if (!isPlayerNear || (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen))
                return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
            if (screenPos.z < 0) return; // Detrás de la cámara

            if (promptStyle == null)
            {
                promptBgTex = new Texture2D(1, 1);
                promptBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.9f));
                promptBgTex.Apply();

                promptStyle = new GUIStyle();
                promptStyle.normal.background = promptBgTex;
                promptStyle.normal.textColor = Color.yellow;
                promptStyle.fontSize = 14;
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.alignment = TextAnchor.MiddleCenter;
            }

            string label = $"[E] Hablar con {dialogue?.npcName ?? "Aldeano"}";
            float w = Mathf.Max(130f, label.Length * 9f);
            float h = 26f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

            GUI.Box(new Rect(x, y, w, h), label, promptStyle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
