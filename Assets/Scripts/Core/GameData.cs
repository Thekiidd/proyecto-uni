using UnityEngine;
using Platformer.Security;

namespace Platformer.Core
{
    /// <summary>
    /// Singleton que persiste entre escenas.
    /// Guarda el personaje seleccionado, nivel actual y puntuación con protección de memoria y cifrado.
    /// </summary>
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }

        [Header("Personaje Seleccionado")]
        public CharacterData selectedCharacter;

        [Header("Estado del Juego (Protegido en Memoria contra Cheat Engine)")]
        public int currentLevel = 1;
        public int totalLevels = 3;
        public ObfuscatedInt score = 0;
        public ObfuscatedInt lives = 3;
        public ObfuscatedInt coins = 0;

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
            LoadSecureProgress();
        }

        public void LoadSecureProgress()
        {
            var data = SecureSaveSystem.Load(out var status);
            if (data != null && status == SaveLoadStatus.Success)
            {
                currentLevel = data.currentLevel;
                coins = data.totalCoins;
                score = data.highScore;
                Debug.Log($"[GameData] Partida cargada de forma segura. Nivel: {currentLevel}, Monedas: {coins}, Puntuación: {score}");
            }
        }

        public void SaveSecureProgress()
        {
            var data = new GameSaveData
            {
                selectedDogName = selectedCharacter != null ? selectedCharacter.characterName : "Golden Retriever",
                currentLevel = currentLevel,
                totalCoins = coins.Value,
                highScore = score.Value
            };
            SecureSaveSystem.Save(data);
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
            SaveSecureProgress();
        }

        public void AddScore(int points)
        {
            score += points;
            SaveSecureProgress();
        }

        public void AddCoins(int amount = 1)
        {
            coins += amount;
            OnCoinsChanged?.Invoke(coins);
            SaveSecureProgress();
        }

        public ObfuscatedInt levelStartCoins = 0;
        public ObfuscatedInt levelStartScore = 0;

        public void SaveLevelCheckpoint()
        {
            levelStartCoins = coins;
            levelStartScore = score;
            SaveSecureProgress();
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
            SaveSecureProgress();
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
