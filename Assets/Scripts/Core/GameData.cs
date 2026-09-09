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
        public int coins = 0;

        public event System.Action<int> OnCoinsChanged;
        public event System.Action<int> OnLivesChanged;

        // Nombres exactos de las escenas en Build Settings
        public const string VillageScene = "Village_Scene";
        public static readonly string[] LevelScenes = { "Level_01", "Level_02", "Level_03" };
        public const string MainMenuScene = "MainMenu";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void EnsureInstance()
        {
            if (Instance == null)
            {
                GameObject go = new GameObject("GameData");
                Instance = go.AddComponent<GameData>();
                DontDestroyOnLoad(go);

                #if UNITY_EDITOR
                if (Instance.selectedCharacter == null)
                {
                    Instance.selectedCharacter = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Runner.asset");
                }
                #endif
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
        }

        public void StartNewGame(CharacterData character)
        {
            selectedCharacter = character;
            currentLevel = 1;
            score = 0;
            coins = 0;
            levelStartCoins = 0;
            levelStartScore = 0;
            lives = character != null ? character.startingLives : 3;
            OnCoinsChanged?.Invoke(coins);
            OnLivesChanged?.Invoke(lives);
        }

        public void AddScore(int points)
        {
            score += points;
        }

        public void AddCoins(int amount = 1)
        {
            coins += amount;
            OnCoinsChanged?.Invoke(coins);
        }

        public int levelStartCoins = 0;
        public int levelStartScore = 0;

        public void SaveLevelCheckpoint()
        {
            levelStartCoins = coins;
            levelStartScore = score;
        }

        public void ResetLevelCoins()
        {
            coins = levelStartCoins;
            score = levelStartScore;
            OnCoinsChanged?.Invoke(coins);
        }

        public void ResetCoins()
        {
            coins = 0;
            score = 0;
            levelStartCoins = 0;
            levelStartScore = 0;
            OnCoinsChanged?.Invoke(coins);
        }

        public void LoseLife()
        {
            lives--;
            OnLivesChanged?.Invoke(lives);
        }

        public void SetLives(int amount)
        {
            lives = amount;
            OnLivesChanged?.Invoke(lives);
        }

        public bool HasLivesLeft() => lives > 0;

        public bool IsLastLevel() => currentLevel >= totalLevels;

        public void GoToNextLevel()
        {
            currentLevel++;
            SaveLevelCheckpoint();
        }
    }
}
