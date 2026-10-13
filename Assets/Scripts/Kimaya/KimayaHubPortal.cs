using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Portal Místico en la Aldea Hub hacia Level_01_Bosque (Misión del Templo).
    /// Muestra un resplandor pulsante y detecta la interacción con [E].
    /// </summary>
    public class KimayaHubPortal : MonoBehaviour
    {
        [Header("Destino")]
        public string targetSceneName = "Level_01_Bosque";
        public string destinationTitle = "Templo del Bosque (Nivel 1)";
        public string questLore = "Rescatar a Kiara - Puzle Cooperativo";

        [Header("Configuración")]
        public float interactionRadius = 2.4f;
        public Color runeGlowColor = new Color(0.2f, 0.9f, 0.6f, 0.85f);

        private SpriteRenderer spriteRenderer;
        private bool isPlayerNearby = false;
        private Camera mainCam;
        private static GUIStyle bannerStyle;
        private static Texture2D bgTex;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCam = Camera.main;
        }

        private void Update()
        {
            if (KimayaHubDuoController.Instance == null) return;

            Transform activeDog = KimayaHubDuoController.Instance.GetActiveTransform();
            if (activeDog == null) return;

            float dist = Vector2.Distance(transform.position, activeDog.position);
            isPlayerNearby = (dist <= interactionRadius);

            // Efecto de pulso en el portal
            if (spriteRenderer != null)
            {
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 3.5f);
                spriteRenderer.color = isPlayerNearby 
                    ? new Color(runeGlowColor.r * pulse, runeGlowColor.g * pulse, runeGlowColor.b * pulse, 1f)
                    : Color.white;
            }

            if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
            {
                EnterPortal();
            }
        }

        private void EnterPortal()
        {
            Debug.Log($"[KimayaHubPortal] Entrando al portal hacia: {targetSceneName}");
            if (Application.CanStreamedLevelBeLoaded(targetSceneName))
            {
                SceneManager.LoadScene(targetSceneName);
            }
            else
            {
                Debug.LogWarning($"La escena {targetSceneName} no está en Build Settings. Intentando cargar por nombre directo...");
                SceneManager.LoadScene(targetSceneName);
            }
        }

        private void OnGUI()
        {
            if (!isPlayerNearby) return;
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
            if (screenPos.z < 0) return;

            if (bannerStyle == null)
            {
                bgTex = new Texture2D(1, 1);
                bgTex.SetPixel(0, 0, new Color(0.08f, 0.28f, 0.18f, 0.95f));
                bgTex.Apply();

                bannerStyle = new GUIStyle();
                bannerStyle.normal.background = bgTex;
                bannerStyle.normal.textColor = Color.white;
                bannerStyle.fontSize = 15;
                bannerStyle.fontStyle = FontStyle.Bold;
                bannerStyle.alignment = TextAnchor.MiddleCenter;
                bannerStyle.padding = new RectOffset(10, 10, 6, 6);
            }

            string text = $"🌀 [E] Entrar al {destinationTitle}\n<size=12><color=#A8FFB2>{questLore}</color></size>";
            float w = 340f;
            float h = 48f;
            float x = screenPos.x - (w * 0.5f);
            float y = Screen.height - screenPos.y - h;

            GUI.Box(new Rect(x, y, w, h), text, bannerStyle);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
