using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Pantalla de nivel completado. Activar el GameObject cuando el jugador llega a la VictoryZone.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        public static LevelCompleteUI Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text levelText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Button btnNext;
        [SerializeField] private Button btnMenu;

        private void Awake()
        {
            Instance = this;
            if (panel != null) panel.SetActive(false);
        }

        private void Start()
        {
            btnNext.onClick.AddListener(NextLevel);
            btnMenu.onClick.AddListener(GoToMenu);
        }

        public void Show()
        {
            Time.timeScale = 0f;
            if (panel != null) panel.SetActive(true);

            int level = GameData.Instance != null ? GameData.Instance.currentLevel : 1;
            int score = GameData.Instance != null ? GameData.Instance.score : 0;
            bool isLast = GameData.Instance != null && GameData.Instance.IsLastLevel();

            if (levelText != null)  levelText.text  = isLast ? "🏆 ¡Juego Completado!" : $"Nivel {level} Completado!";
            if (scoreText != null)  scoreText.text  = $"Puntuación: {score}";
            if (btnNext != null)    btnNext.gameObject.SetActive(!isLast);
        }

        private void NextLevel()
        {
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadNextLevel();
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
