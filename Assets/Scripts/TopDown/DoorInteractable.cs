using Platformer.Dialogue;
using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// Componente que se añade a las puertas de las casas en la aldea.
    /// Permite tocar la puerta y leer mensajes de los habitantes.
    /// </summary>
    public class DoorInteractable : MonoBehaviour
    {
        public string houseName = "Casa del Aldeano";
        [TextArea(2, 4)]
        public string doorMessage = "Tocas la puerta... pero parece que nadie está en casa por ahora. Deben estar trabajando en el campo.";
        public float interactRadius = 2.0f;

        private Transform playerTransform;
        private bool isPlayerNear = false;
        private Camera mainCamera;
        private static GUIStyle promptStyle;
        private static Texture2D promptBgTex;

        private void Start()
        {
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

            bool ePressed = false;
            if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
                ePressed = true;

            if (isPlayerNear && ePressed)
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.IsOpen)
                {
                    var data = new DialogueData
                    {
                        npcName = houseName,
                        lines = new string[] { doorMessage }
                    };
                    DialogueManager.Instance.StartDialogue(data);
                }
            }
        }

        private void OnGUI()
        {
            if (!isPlayerNear || (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen))
                return;

            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 1f);
            if (screenPos.z < 0) return;

            if (promptStyle == null)
            {
                promptBgTex = new Texture2D(1, 1);
                promptBgTex.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.15f, 0.9f));
                promptBgTex.Apply();

                promptStyle = new GUIStyle();
                promptStyle.normal.background = promptBgTex;
                promptStyle.normal.textColor = new Color(0.4f, 0.9f, 1f);
                promptStyle.fontSize = 14;
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.alignment = TextAnchor.MiddleCenter;
            }

            string label = "[E] Tocar puerta";
            float w = 130f;
            float h = 26f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

            GUI.Box(new Rect(x, y, w, h), label, promptStyle);
        }
    }
}
