using Platformer.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.UI
{
    /// <summary>
    /// HUD en pantalla que muestra vidas y diamantes recolectados,
    /// y pantalla de Game Over con opciones de Reintentar o Menú.
    /// Se genera automáticamente al iniciar el juego y permanece en todos los niveles.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public static GameHUD Instance { get; private set; }

        private GUIStyle boxStyle;
        private GUIStyle labelStyle;
        private Texture2D bgTexture;
        private bool isGameOver = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureHUDExists()
        {
            if (Instance == null)
            {
                GameObject hudGO = new GameObject("GameHUD");
                Instance = hudGO.AddComponent<GameHUD>();
                DontDestroyOnLoad(hudGO);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreateBackgroundTexture();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static int tennisBallsCollected = 0;

        public static void RegisterTennisBallCollected()
        {
            tennisBallsCollected++;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            isGameOver = false;
            tennisBallsCollected = 0;
        }

        private void CreateBackgroundTexture()
        {
            bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, new Color(0.06f, 0.08f, 0.12f, 0.88f));
            bgTexture.Apply();
        }

        private void InitStyles()
        {
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = bgTexture;
                boxStyle.border = new RectOffset(0, 0, 0, 0);
            }

            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.richText = true;
                labelStyle.fontSize = 20;
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleLeft;
            }
        }

        public void ShowGameOver()
        {
            isGameOver = true;
        }

        public void HideGameOver()
        {
            isGameOver = false;
        }

        private void OnGUI()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "MainMenu")
                return;

            InitStyles();

            int lives = GameData.Instance != null ? GameData.Instance.lives : 3;
            int bones = GameData.Instance != null ? GameData.Instance.coins : 0;

            // Escala para verse proporcional en cualquier resolución
            float scale = Mathf.Max(1f, Screen.height / 1080f);
            Matrix4x4 prevMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);

            // ── HUD Superior Izquierdo ──
            Rect panelRect = new Rect(25f, 25f, 230f, 125f);
            GUI.Box(panelRect, GUIContent.none, boxStyle);
            DrawOutlineRect(panelRect, new Color(0.3f, 0.5f, 0.7f, 0.6f), 2f);

            // Fila de Vidas
            Rect livesRect = new Rect(38f, 30f, 210f, 32f);
            GUI.Label(livesRect, $"<size=22>❤️</size>  <color=#FFEEEE><b>x {Mathf.Max(0, lives)}</b></color>", labelStyle);

            // Fila de Huesos
            Rect bonesRect = new Rect(38f, 62f, 210f, 32f);
            GUI.Label(bonesRect, $"<size=22>🦴</size>  <color=#FFE570><b>x {bones}</b></color>", labelStyle);

            // Fila de Pelotas Secretas
            Rect tennisRect = new Rect(38f, 94f, 210f, 32f);
            GUI.Label(tennisRect, $"<size=22>🎾</size>  <color=#A5F145><b>{tennisBallsCollected}/3</b></color>", labelStyle);

            // ── Modal de Game Over ──
            if (isGameOver)
            {
                DrawGameOverModal();
            }

            GUI.matrix = prevMatrix;
        }

        private void DrawGameOverModal()
        {
            // Fondo oscuro atenuado
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float modalW = 540f;
            float modalH = 340f;
            Rect modalRect = new Rect((1920f - modalW) * 0.5f, (1080f - modalH) * 0.5f, modalW, modalH);

            GUI.Box(modalRect, GUIContent.none, boxStyle);
            DrawOutlineRect(modalRect, new Color(0.95f, 0.25f, 0.25f, 0.9f), 3f);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 40,
                fontStyle = FontStyle.Bold,
                richText = true
            };
            GUI.Label(new Rect(modalRect.x, modalRect.y + 25f, modalW, 55f), "<color=#FF4444>¡GAME OVER!</color>", titleStyle);

            GUIStyle msgStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                richText = true
            };
            int bones = GameData.Instance != null ? GameData.Instance.coins : 0;
            GUI.Label(new Rect(modalRect.x, modalRect.y + 90f, modalW, 35f), "<color=#FFFFFF>Te has quedado sin vidas.</color>", msgStyle);
            GUI.Label(new Rect(modalRect.x, modalRect.y + 125f, modalW, 35f), $"<color=#FFE570>Huesos recogidos: <b>x {bones}</b></color>", msgStyle);

            float btnW = 380f;
            float btnH = 52f;
            float btnX = modalRect.x + (modalW - btnW) * 0.5f;

            GUIStyle btnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };

            if (GUI.Button(new Rect(btnX, modalRect.y + 185f, btnW, btnH), "REINTENTAR NIVEL", btnStyle))
            {
                RestartLevel();
            }

            if (GUI.Button(new Rect(btnX, modalRect.y + 250f, btnW, btnH), "MENÚ PRINCIPAL", btnStyle))
            {
                GoToMainMenu();
            }
        }

        private void RestartLevel()
        {
            isGameOver = false;
            if (GameData.Instance != null)
            {
                int startingLives = (GameData.Instance.selectedCharacter != null) 
                    ? GameData.Instance.selectedCharacter.startingLives 
                    : 3;
                GameData.Instance.SetLives(startingLives);
                GameData.Instance.ResetLevelCoins();
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void GoToMainMenu()
        {
            isGameOver = false;
            if (GameData.Instance != null)
            {
                int startingLives = (GameData.Instance.selectedCharacter != null) 
                    ? GameData.Instance.selectedCharacter.startingLives 
                    : 3;
                GameData.Instance.SetLives(startingLives);
                GameData.Instance.ResetCoins();
            }
            SceneManager.LoadScene(GameData.MainMenuScene);
        }

        private void DrawOutlineRect(Rect r, Color color, float thickness)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y + r.height - thickness, r.width, thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x + r.width - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (bgTexture != null)
                Destroy(bgTexture);
        }
    }
}
