using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// HUD elegante para el Hub de Kimaya.
    /// Muestra el perro actualmente seleccionado, la misión activa y los controles de juego.
    /// </summary>
    public class KimayaHubHUD : MonoBehaviour
    {
        private static GUIStyle cardStyle;
        private static GUIStyle titleStyle;
        private static GUIStyle subStyle;
        private static GUIStyle badgeStyle;
        private static GUIStyle buttonStyle;

        private void OnGUI()
        {
            InitStyles();

            bool isFalin = KimayaHubDuoController.Instance == null || KimayaHubDuoController.Instance.isFalinActive;
            string activeName = isFalin ? "FALIN (San Bernardo)" : "MIA (Schnauzer)";
            string companionName = isFalin ? "Mia te acompaña fielmente" : "Falin te acompaña fielmente";
            string powerDesc = isFalin ? "Fuerza Bruta & Peso" : "Agilidad & Olfato";

            // 1. Tarjeta Superior Izquierda: Perro Activo
            float cardX = 20f;
            float cardY = 20f;
            float cardW = 320f;
            float cardH = 88f;

            GUI.Box(new Rect(cardX, cardY, cardW, cardH), GUIContent.none, cardStyle);

            GUI.Label(new Rect(cardX + 16, cardY + 10, 220, 24), $"🐾 <b>{activeName}</b>", titleStyle);
            GUI.Label(new Rect(cardX + 16, cardY + 34, 280, 20), $"Rol: <color=#FFE680>{powerDesc}</color>", subStyle);
            GUI.Label(new Rect(cardX + 16, cardY + 56, 280, 20), $"🤝 {companionName}", subStyle);

            // Botón rápido de cambio de perro
            if (GUI.Button(new Rect(cardX + cardW - 75, cardY + 12, 60, 28), "TAB ⇄", buttonStyle))
            {
                if (KimayaHubDuoController.Instance != null)
                {
                    KimayaHubDuoController.Instance.isFalinActive = !KimayaHubDuoController.Instance.isFalinActive;
                }
            }

            // 2. Banner Superior Central: Misión Activa
            float questW = 380f;
            float questH = 40f;
            float questX = (Screen.width - questW) * 0.5f;
            float questY = 16f;

            GUI.Box(new Rect(questX, questY, questW, questH), GUIContent.none, cardStyle);
            string questText = "🗺️ <b>Misión:</b> Entra al Portal Verde (Nivel 1: Ascenso Vertical)";
            GUI.Label(new Rect(questX, questY + 10, questW, 22), questText, badgeStyle);

            // 3. Botón Superior Derecho: Menú Principal
            float menuW = 110f;
            float menuH = 36f;
            float menuX = Screen.width - menuW - 20f;
            float menuY = 20f;

            if (GUI.Button(new Rect(menuX, menuY, menuW, menuH), "🏠 Menú", buttonStyle))
            {
                if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                    SceneManager.LoadScene("MainMenu");
            }

            // 4. Barra Inferior: Guía Rápida de Controles
            float barW = Mathf.Min(680f, Screen.width - 40f);
            float barH = 34f;
            float barX = (Screen.width - barW) * 0.5f;
            float barY = Screen.height - barH - 12f;

            GUI.Box(new Rect(barX, barY, barW, barH), GUIContent.none, cardStyle);
            string controls = "🎮 <b>[WASD]</b> Moverse   |   <b>[TAB]</b> Alternar Perro   |   <b>[SHIFT]</b> Correr   |   <b>[ESPACIO/Q]</b> Poder   |   <b>[E]</b> Interactuar";
            GUI.Label(new Rect(barX, barY + 7, barW, 20), controls, badgeStyle);
        }

        private void InitStyles()
        {
            if (cardStyle != null) return;

            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, new Color(0.08f, 0.14f, 0.10f, 0.90f));
            bg.Apply();

            Texture2D btnBg = new Texture2D(1, 1);
            btnBg.SetPixel(0, 0, new Color(0.20f, 0.45f, 0.28f, 0.95f));
            btnBg.Apply();

            cardStyle = new GUIStyle();
            cardStyle.normal.background = bg;

            titleStyle = new GUIStyle();
            titleStyle.normal.textColor = new Color(0.4f, 1f, 0.7f);
            titleStyle.fontSize = 15;
            titleStyle.fontStyle = FontStyle.Bold;

            subStyle = new GUIStyle();
            subStyle.normal.textColor = Color.white;
            subStyle.fontSize = 12;

            badgeStyle = new GUIStyle();
            badgeStyle.normal.textColor = Color.white;
            badgeStyle.fontSize = 13;
            badgeStyle.alignment = TextAnchor.MiddleCenter;

            buttonStyle = new GUIStyle();
            buttonStyle.normal.background = btnBg;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 13;
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}
