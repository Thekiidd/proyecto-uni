using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// Singleton que persiste entre escenas.
    /// Guarda el personaje seleccionado, nivel actual y puntuación.
    /// </summary>
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }

        [Header("Personaje Seleccionado")]
        public CharacterData selectedCharacter;

        [Header("Estado del Juego")]
        public int currentLevel = 1;
        public int totalLevels = 3;
        public int score = 0;
        public int lives = 3;

        // Nombres exactos de las escenas en Build Settings
        public static readonly string[] LevelScenes = { "Level_01", "Level_02", "Level_03" };
        public const string MainMenuScene = "MainMenu";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartNewGame(CharacterData character)
        {
            selectedCharacter = character;
            currentLevel = 1;
            score = 0;
            lives = character != null ? character.startingLives : 3;
        }

        public void AddScore(int points)
        {
            score += points;
        }

        public void LoseLife()
        {
            lives--;
        }

        public bool HasLivesLeft() => lives > 0;

        public bool IsLastLevel() => currentLevel >= totalLevels;

        public void GoToNextLevel()
        {
            currentLevel++;
        }
    }
}
