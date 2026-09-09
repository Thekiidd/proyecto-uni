using Platformer.Core;
using Platformer.Dialogue;
using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// Portón de salida del pueblo hacia los niveles de aventura (Nivel 1).
    /// </summary>
    public class VillageGateTrigger : MonoBehaviour
    {
        public string targetLevelName = "Level_01";
        public float interactRadius = 2.5f;

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
                if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
                    return;

                // Transición al Nivel 1
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadLevel(1);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(targetLevelName);
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
                promptBgTex.SetPixel(0, 0, new Color(0.1f, 0.4f, 0.15f, 0.95f));
                promptBgTex.Apply();

                promptStyle = new GUIStyle();
                promptStyle.normal.background = promptBgTex;
                promptStyle.normal.textColor = Color.white;
                promptStyle.fontSize = 15;
                promptStyle.fontStyle = FontStyle.Bold;
                promptStyle.alignment = TextAnchor.MiddleCenter;
            }

            string label = "🌲 [E] Salir a la Aventura (Nivel 1)";
            float w = 260f;
            float h = 32f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

            GUI.Box(new Rect(x, y, w, h), label, promptStyle);
        }
    }
}
