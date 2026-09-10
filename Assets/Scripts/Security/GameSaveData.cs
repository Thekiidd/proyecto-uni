using System;

namespace Platformer.Security
{
    /// <summary>
    /// Modelo serializable de partida y configuraciones seguras.
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int saveVersion = 1;
        public long saveTimestampUtc;

        // Progreso de Historia y Niveles
        public string selectedDogName = "Golden Retriever";
        public int currentLevel = 1;
        public int unlockedLevels = 1;
        public int totalCoins = 0;
        public int highScore = 0;
        public bool prologueCompleted = false;

        // Ajustes de Usuario
        public float musicVolume = 0.8f;
        public float sfxVolume = 1.0f;
        public bool isFullscreen = true;
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;

        public GameSaveData()
        {
            saveTimestampUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
