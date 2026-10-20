using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Platformer.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Kimaya.Networking
{
    [Serializable]
    public class SessionDataPayload
    {
        public string player_id;
        public string character_id;
        public string level_id;
        public int score;
        public int coins;
        public int lives_left;
        public int time_played;
        public int jumps;
        public int falls;
        public string result;
    }

    [Serializable]
    public class SupabaseSessionResponse
    {
        public long id;
        public string player_id;
        public string character_id;
        public string level_id;
        public int score;
        public int coins;
        public int lives_left;
        public int time_played;
        public int jumps;
        public int falls;
        public string result;
        public string created_at;
    }

    /// <summary>
    /// Conexión oficial de Kimaya con la base de datos de Supabase.
    /// Guarda sesiones, puntajes (score), tiempos, saltos, caídas y notas de lore.
    /// No requiere plugins externos; utiliza llamadas directas a la API REST de Supabase.
    /// </summary>
    public class DatabaseManager : MonoBehaviour
    {
        public static DatabaseManager Instance { get; private set; }

        [Header("Configuración de Supabase (kimaya-db)")]
        public string supabaseUrl = "https://cbunyolvkojbbstjylyb.supabase.co";
        public string apiKey = "sb_publishable_6x1JiBKBMggKjRDemvrkQg_-TpyLaAs";

        [Header("Opciones")]
        public bool logRequests = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("[DatabaseManager]");
            Instance = go.AddComponent<DatabaseManager>();
            DontDestroyOnLoad(go);
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

        /// <summary>
        /// Guarda una partida completa del modo Jump King (tiempo, score, saltos, caídas y resultado).
        /// </summary>
        public void SaveJumpKingSession(string result, int score, int timeSeconds, int jumps, int falls, string hero = "falin")
        {
            string playerId = GetCurrentPlayerId();
            string charId = hero.ToLower().Contains("mia") ? "mia" : "falin";
            string levelId = KimayaIds.Level(SceneManager.GetActiveScene().name);

            string json = "{" +
                $"\"player_id\":\"{Escape(playerId)}\"," +
                $"\"character_id\":\"{Escape(charId)}\"," +
                $"\"level_id\":\"{Escape(levelId)}\"," +
                $"\"score\":{score}," +
                $"\"coins\":0," +
                $"\"lives_left\":0," +
                $"\"time_played\":{timeSeconds}," +
                $"\"jumps\":{jumps}," +
                $"\"falls\":{falls}," +
                $"\"result\":\"{Escape(result)}\"" +
                "}";

            if (logRequests)
            {
                Debug.Log($"[DatabaseManager] Subiendo partida Jump King: Score={score}, Tiempo={timeSeconds}s, Saltos={jumps}, Caídas={falls}, Result={result}");
            }

            StartCoroutine(PostCoroutine("sessions", json));

            // También guardar en player_scores para ranking general
            string scoreJson = "{" +
                $"\"player_name\":\"{Escape(hero)}\"," +
                $"\"score\":{score}," +
                $"\"time_played\":{timeSeconds}," +
                $"\"level_name\":\"{Escape(levelId)}\"" +
                "}";
            StartCoroutine(PostCoroutine("player_scores", scoreJson));
        }

        /// <summary>
        /// Guarda el estado actual del juego utilizando los datos en GameData (para niveles de plataformas).
        /// </summary>
        /// <param name="result">"nivel_completado" o "game_over"</param>
        public void SaveCurrentSession(string result)
        {
            var data = GameData.Instance;
            string playerId = GetCurrentPlayerId();
            string character = data != null && data.selectedCharacter != null ? KimayaIds.Character(data.selectedCharacter) : "falin";
            string level = KimayaIds.Level(SceneManager.GetActiveScene().name);
            int score = data != null ? (int)data.score : 0;
            int coins = data != null ? (int)data.coins : 0;
            int lives = data != null ? (int)data.lives : 3;
            int timePlayed = Mathf.RoundToInt(Time.timeSinceLevelLoad);

            string json = "{" +
                $"\"player_id\":\"{Escape(playerId)}\"," +
                $"\"character_id\":\"{Escape(character)}\"," +
                $"\"level_id\":\"{Escape(level)}\"," +
                $"\"score\":{score}," +
                $"\"coins\":{coins}," +
                $"\"lives_left\":{Mathf.Max(0, lives)}," +
                $"\"time_played\":{timePlayed}," +
                $"\"jumps\":0," +
                $"\"falls\":0," +
                $"\"result\":\"{Escape(result)}\"" +
                "}";

            if (logRequests)
            {
                Debug.Log($"[DatabaseManager] Subiendo partida de plataforma: Score={score}, Coins={coins}, Result={result}");
            }

            StartCoroutine(PostCoroutine("sessions", json));
        }

        /// <summary>
        /// Desbloquea una entrada de lore en la base de datos (según sección 8 de la guía).
        /// </summary>
        public void UnlockLore(string loreId)
        {
            string playerId = GetCurrentPlayerId();
            string json = "{" +
                $"\"lore_id\":\"{Escape(loreId)}\"," +
                $"\"player_id\":\"{Escape(playerId)}\"" +
                "}";
            StartCoroutine(PostCoroutine("lore_unlocks", json));
        }

        private IEnumerator PostCoroutine(string table, string json)
        {
            string url = $"{supabaseUrl}/rest/v1/{table}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("apikey", apiKey);
                request.SetRequestHeader("Authorization", "Bearer " + apiKey);
                request.SetRequestHeader("Prefer", "return=representation");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (logRequests)
                    {
                        Debug.Log($"[DatabaseManager] ✅ Datos guardados exitosamente en tabla [{table}]:\n{request.downloadHandler.text}");
                    }
                }
                else
                {
                    Debug.LogError($"[DatabaseManager] ❌ Error al guardar en [{table}]: {request.responseCode} - {request.error} | {request.downloadHandler.text}");
                }
            }
        }

        private string GetCurrentPlayerId()
        {
            var data = GameData.Instance;
            if (data != null && !string.IsNullOrEmpty(data.currentUserId))
            {
                return data.currentUserId;
            }
            return "1"; // Sesión 1 por defecto (Falin) como indica la guía del equipo
        }

        private string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
