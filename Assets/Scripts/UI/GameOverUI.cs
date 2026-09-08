using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Pantalla de Game Over. Se activa cuando el jugador se queda sin vidas.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text levelReachedText;
        [SerializeField] private Button btnRetry;   // Reintentar desde el nivel actual
        [SerializeField] private Button btnMenu;    // Volver al menú principal

        private void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        private void Start()
        {
            btnRetry.onClick.AddListener(Retry);
            btnMenu.onClick.AddListener(GoToMenu);
        }

        public void Show()
        {
            Time.timeScale = 0f;
            if (panel != null) panel.SetActive(true);

            int score = GameData.Instance != null ? GameData.Instance.score : 0;
            int level = GameData.Instance != null ? GameData.Instance.currentLevel : 1;

            if (scoreText != null)       scoreText.text       = $"Puntuación Final: {score}";
            if (levelReachedText != null) levelReachedText.text = $"Llegaste al Nivel {level}";
        }

        private void Retry()
        {
            Time.timeScale = 1f;
            // Restaurar vidas del personaje
            if (GameData.Instance != null && GameData.Instance.selectedCharacter != null)
                GameData.Instance.lives = GameData.Instance.selectedCharacter.startingLives;

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.ReloadCurrentLevel();
        }

        private void GoToMenu()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadMainMenu();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.MainMenuScene);
        }
    }
}
