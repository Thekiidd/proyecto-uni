using Platformer.Core;

namespace Kimaya.Networking
{
    public static class KimayaIds
    {
        // La app y base de datos reconocen "falin" y "mia"
        public static string Character(CharacterData data)
        {
            if (data == null) return "falin";
            string name = data.characterName.ToLower();
            return name.Contains("schnauzer") || name.Contains("mia") ? "mia" : "falin";
        }

        // Vidas a porcentaje 0-100
        public static int HealthPercent()
        {
            var data = GameData.Instance;
            if (data == null) return 100;
            int max = data.selectedCharacter != null ? data.selectedCharacter.startingLives : 3;
            if (max <= 0) return 0;
            return UnityEngine.Mathf.Clamp((int)data.lives * 100 / max, 0, 100);
        }

        // Convierte el nombre de escena a identificador de nivel
        public static string Level(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return "unknown";
            if (sceneName == GameData.VillageScene) return "village";
            return sceneName.ToLower();
        }
    }
}
