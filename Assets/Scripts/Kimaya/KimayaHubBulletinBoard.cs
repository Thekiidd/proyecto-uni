using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Tablón de Récords en la plaza del Hub de Kimaya.
    /// Muestra la tabla de clasificación de speedrun conectada con Firebase
    /// y permite abrir la web interactiva del Leaderboard.
    /// </summary>
    public class KimayaHubBulletinBoard : MonoBehaviour
    {
        public float interactionRadius = 2.5f;
        public string webLeaderboardUrl = "file:///C:/Users/lbati/Web1Dia/kimaya-leaderboard/index.html";

        private bool isPlayerNear = false;
        private bool isOpen = false;
        private Camera mainCam;

        private static GUIStyle promptStyle;
        private static GUIStyle winStyle;
        private static GUIStyle headerStyle;
        private static GUIStyle rowStyle;
        private static GUIStyle btnStyle;

        private struct RecordEntry
        {
            public string rank;
            public string player;
            public string time;
            public string badge;
        }

        private readonly RecordEntry[] sampleRecords = new RecordEntry[]
        {
            new RecordEntry { rank = "#1 🥇", player = "FalinSpeed", time = "00:23.41", badge = "Maestro Rúnico" },
            new RecordEntry { rank = "#2 🥈", player = "MiaAgile", time = "00:26.85", badge = "Gema Veloz" },
            new RecordEntry { rank = "#3 🥉", player = "DuoGuardian", time = "00:29.10", badge = "Veterano" },
            new RecordEntry { rank = "#4", player = "ShadowPaw", time = "00:32.44", badge = "Explorador" },
            new RecordEntry { rank = "#5", player = "KiaraFan", time = "00:35.19", badge = "Rescatista" }
        };

        private void Start()
        {
            mainCam = Camera.main;
        }

        private void Update()
        {
            if (KimayaHubDuoController.Instance == null) return;
            Transform activeDog = KimayaHubDuoController.Instance.GetActiveTransform();
            if (activeDog == null) return;

            float dist = Vector2.Distance(transform.position, activeDog.position);
            isPlayerNear = (dist <= interactionRadius);

            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                isOpen = !isOpen;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = !isOpen;
            }

            if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || !isPlayerNear))
            {
                isOpen = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            if (isPlayerNear && !isOpen)
            {
                Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
                if (screenPos.z >= 0)
                {
                    string prompt = "🏆 [E] Tablón de Récords";
                    float w = 210f;
                    float h = 34f;
                    float x = screenPos.x - (w * 0.5f);
                    float y = Screen.height - screenPos.y - h;
                    GUI.Box(new Rect(x, y, w, h), prompt, promptStyle);
                }
            }

            if (isOpen)
            {
                DrawLeaderboardWindow();
            }
        }

        private void DrawLeaderboardWindow()
        {
            float w = Mathf.Min(560f, Screen.width - 40f);
            float h = 350f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none, winStyle);

            // Cabecera
            GUI.Label(new Rect(x + 20, y + 16, w - 40, 30), "🏆 Salón de la Fama - Kimaya Speedrun", headerStyle);
            GUI.Label(new Rect(x + 20, y + 46, w - 40, 20), "Récords globales sincronizados con Firebase Realtime Database", rowStyle);

            // Tabla
            float rowY = y + 80;
            GUI.Label(new Rect(x + 25, rowY, 60, 22), "Puesto", headerStyle);
            GUI.Label(new Rect(x + 100, rowY, 150, 22), "Jugador", headerStyle);
            GUI.Label(new Rect(x + 270, rowY, 100, 22), "Mejor Tiempo", headerStyle);
            GUI.Label(new Rect(x + 400, rowY, 120, 22), "Título", headerStyle);

            rowY += 26;
            for (int i = 0; i < sampleRecords.Length; i++)
            {
                var r = sampleRecords[i];
                GUI.Label(new Rect(x + 25, rowY, 60, 20), r.rank, rowStyle);
                GUI.Label(new Rect(x + 100, rowY, 150, 20), r.player, rowStyle);
                GUI.Label(new Rect(x + 270, rowY, 100, 20), $"<color=#FFE855><b>{r.time}</b></color>", rowStyle);
                GUI.Label(new Rect(x + 400, rowY, 120, 20), r.badge, rowStyle);
                rowY += 26;
            }

            // Botones inferiores
            float btnY = y + h - 50;
            if (GUI.Button(new Rect(x + 25, btnY, 240, 34), "🌐 Abrir Leaderboard Web", btnStyle))
            {
                Application.OpenURL(webLeaderboardUrl);
            }

            if (GUI.Button(new Rect(x + w - 140, btnY, 115, 34), "Cerrar [ESC]", btnStyle))
            {
                isOpen = false;
                if (KimayaHubDuoController.Instance != null)
                    KimayaHubDuoController.Instance.controlsEnabled = true;
            }
        }

        private void InitStyles()
        {
            if (promptStyle != null) return;

            Texture2D woodBg = new Texture2D(1, 1);
            woodBg.SetPixel(0, 0, new Color(0.24f, 0.16f, 0.10f, 0.96f));
            woodBg.Apply();

            Texture2D btnBg = new Texture2D(1, 1);
            btnBg.SetPixel(0, 0, new Color(0.45f, 0.32f, 0.18f, 0.95f));
            btnBg.Apply();

            promptStyle = new GUIStyle();
            promptStyle.normal.background = woodBg;
            promptStyle.normal.textColor = new Color(1f, 0.9f, 0.6f);
            promptStyle.fontSize = 14;
            promptStyle.fontStyle = FontStyle.Bold;
            promptStyle.alignment = TextAnchor.MiddleCenter;

            winStyle = new GUIStyle();
            winStyle.normal.background = woodBg;
            winStyle.padding = new RectOffset(20, 20, 20, 20);

            headerStyle = new GUIStyle();
            headerStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            headerStyle.fontSize = 15;
            headerStyle.fontStyle = FontStyle.Bold;

            rowStyle = new GUIStyle();
            rowStyle.normal.textColor = Color.white;
            rowStyle.fontSize = 13;

            btnStyle = new GUIStyle();
            btnStyle.normal.background = btnBg;
            btnStyle.normal.textColor = Color.white;
            btnStyle.fontSize = 13;
            btnStyle.fontStyle = FontStyle.Bold;
            btnStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}
