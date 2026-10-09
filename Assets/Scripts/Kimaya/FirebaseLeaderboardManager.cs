using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Kimaya
{
    [Serializable]
    public class PlayerRecord
    {
        public string playerName;
        public float bestTime;
        public string bestTimeFormatted;
        public int attempts;
        public string lastPlayed;
    }

    /// <summary>
    /// Conecta Unity directamente con Firebase Realtime Database mediante llamadas HTTP REST.
    /// Cero plugins externos, ligero y compatible con cualquier versión de Unity.
    /// </summary>
    public class FirebaseLeaderboardManager : MonoBehaviour
    {
        public static FirebaseLeaderboardManager Instance { get; private set; }

        [Header("Configuración de Firebase")]
        [Tooltip("URL base de tu Realtime Database de Firebase (ej: https://kimaya-game-default-rtdb.firebaseio.com)")]
        public string databaseUrl = "https://kimaya-default-rtdb.firebaseio.com";

        [Header("Jugador Local")]
        public string defaultPlayerName = "Aventurero";

        public string CurrentPlayerName
        {
            get => PlayerPrefs.GetString("Kimaya_PlayerName", defaultPlayerName);
            set
            {
                PlayerPrefs.SetString("Kimaya_PlayerName", value);
                PlayerPrefs.Save();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SaveScore(float timeSeconds, Action<bool, string> onComplete = null)
        {
            StartCoroutine(SaveScoreRoutine(CurrentPlayerName, timeSeconds, onComplete));
        }

        private IEnumerator SaveScoreRoutine(string name, float newTime, Action<bool, string> onComplete)
        {
            if (string.IsNullOrEmpty(databaseUrl) || databaseUrl.Contains("ejemplo"))
            {
                onComplete?.Invoke(false, "URL de Firebase no configurada.");
                yield break;
            }

            string cleanUrl = databaseUrl.TrimEnd('/');
            string safeName = SanitizeKey(name);
            string endpoint = $"{cleanUrl}/leaderboard/{safeName}.json";

            // 1. Obtener registro existente
            using (UnityWebRequest getReq = UnityWebRequest.Get(endpoint))
            {
                yield return getReq.SendWebRequest();

                PlayerRecord record = null;
                if (getReq.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(getReq.downloadHandler.text) && getReq.downloadHandler.text != "null")
                {
                    try
                    {
                        record = JsonUtility.FromJson<PlayerRecord>(getReq.downloadHandler.text);
                    }
                    catch { record = null; }
                }

                if (record == null)
                {
                    record = new PlayerRecord
                    {
                        playerName = name,
                        bestTime = newTime,
                        bestTimeFormatted = KimayaTimer.FormatTime(newTime),
                        attempts = 1,
                        lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                    };
                }
                else
                {
                    record.attempts++;
                    record.lastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                    if (newTime < record.bestTime || record.bestTime <= 0f)
                    {
                        record.bestTime = newTime;
                        record.bestTimeFormatted = KimayaTimer.FormatTime(newTime);
                    }
                }

                // 2. Subir a Firebase con PUT
                string jsonBody = JsonUtility.ToJson(record);
                using (UnityWebRequest putReq = UnityWebRequest.Put(endpoint, jsonBody))
                {
                    putReq.SetRequestHeader("Content-Type", "application/json");
                    yield return putReq.SendWebRequest();

                    if (putReq.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log($"[Firebase] Récord guardado con éxito: {record.playerName} - {record.bestTimeFormatted}");
                        onComplete?.Invoke(true, $"¡Récord guardado! Mejor tiempo: {record.bestTimeFormatted}");
                    }
                    else
                    {
                        Debug.LogWarning($"[Firebase] Error al guardar: {putReq.error}");
                        onComplete?.Invoke(false, putReq.error);
                    }
                }
            }
        }

        private string SanitizeKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "Jugador";
            return key.Replace(".", "_").Replace("$", "_").Replace("#", "_").Replace("[", "_").Replace("]", "_").Replace("/", "_");
        }
    }
}
