using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Puerta de salida del templo estilo Fireboy & Watergirl.
    /// Al entrar el jugador:
    /// 1. Detiene el cronómetro de speedrun.
    /// 2. Sube el tiempo a Firebase Realtime Database.
    /// 3. Muestra una ventana de victoria elegante con el tiempo y botones.
    /// </summary>
    public class KimayaGoalDoor : MonoBehaviour
    {
        [Header("Configuración")]
        public string nextScene = "MainMenu";
        public bool requireAllGems = false;

        private bool _triggered = false;
        private GameObject _victoryModal;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag("Player")) return;

            if (requireAllGems)
            {
                var gems = FindObjectsByType<KimayaGem>(FindObjectsInactive.Exclude);
                if (gems != null && gems.Length > 0)
                {
                    Debug.Log($"[KimayaGoalDoor] Aún quedan {gems.Length} gemas por recoger.");
                    return;
                }
            }

            _triggered = true;
            OnVictory(other.gameObject);
        }

        private void OnVictory(GameObject player)
        {
            // 1. Detener al jugador
            var pc = player.GetComponent<PlayerController2D>();
            if (pc != null) pc.SetActive(false);

            // 2. Detener cronómetro
            float finalSeconds = 0f;
            string formattedTime = "00:00.00";
            if (KimayaTimer.Instance != null)
            {
                KimayaTimer.Instance.StopTimer();
                finalSeconds = KimayaTimer.Instance.ElapsedSeconds;
                formattedTime = KimayaTimer.Instance.FormattedTime;
            }

            // 3. Sonido de victoria
            KimayaAudio.Instance?.PlaySFX(KimayaAudio.SFX.Victory);

            // 4. Subir a Firebase
            string playerName = "Aventurero";
            if (FirebaseLeaderboardManager.Instance != null)
            {
                playerName = FirebaseLeaderboardManager.Instance.CurrentPlayerName;
                FirebaseLeaderboardManager.Instance.SaveScore(finalSeconds, (success, msg) =>
                {
                    Debug.Log($"[Firebase Callback] {msg}");
                    var statusTxt = GameObject.Find("FirebaseStatusText");
                    if (statusTxt != null)
                    {
                        var t = statusTxt.GetComponent<Text>();
                        if (t != null)
                            t.text = success ? "☁️ ¡Récord guardado en Firebase!" : $"⚠️ {msg}";
                    }
                });
            }

            // 5. Mostrar ventana de victoria
            ShowVictoryUI(formattedTime, playerName);
        }

        private void ShowVictoryUI(string timeStr, string playerName)
        {
            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _victoryModal = new GameObject("Victory_Modal");
            _victoryModal.transform.SetParent(canvas.transform, false);

            var overlay = _victoryModal.AddComponent<Image>();
            overlay.color = new Color(0.04f, 0.08f, 0.05f, 0.92f);
            var rect = _victoryModal.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            var card = new GameObject("Card");
            card.transform.SetParent(_victoryModal.transform, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = new Color(0.12f, 0.2f, 0.15f, 0.98f);
            var cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(580f, 430f);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Título
            CreateText(card.transform, "Title", "✨ ¡TEMPLO COMPLETADO! ✨", 32, FontStyle.Bold, new Color(0.4f, 1f, 0.6f), new Vector2(0f, 140f), new Vector2(520f, 50f), font);

            // Info Jugador
            CreateText(card.transform, "PlayerMsg", $"Aventurero: {playerName}", 20, FontStyle.Normal, new Color(0.9f, 0.9f, 0.9f), new Vector2(0f, 80f), new Vector2(500f, 35f), font);

            // Tiempo obtenido
            CreateText(card.transform, "TimeMsg", $"⏱️ Tiempo: {timeStr}", 36, FontStyle.Bold, new Color(1f, 0.88f, 0.35f), new Vector2(0f, 25f), new Vector2(500f, 50f), font);

            // Estado de Firebase
            var stGo = CreateText(card.transform, "FirebaseStatusText", "☁️ Guardando en Firebase Leaderboard...", 16, FontStyle.Italic, new Color(0.6f, 0.95f, 0.75f), new Vector2(0f, -35f), new Vector2(520f, 35f), font);

            // Botón Reintentar
            CreateButton(card.transform, "BtnRestart", "🔄 Reintentar", new Vector2(-135f, -125f), new Vector2(185f, 52f), new Color(0.2f, 0.55f, 0.32f), font, () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });

            // Botón Menú
            CreateButton(card.transform, "BtnMenu", "🏠 Menú Principal", new Vector2(135f, -125f), new Vector2(185f, 52f), new Color(0.3f, 0.35f, 0.48f), font, () =>
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(nextScene);
            });
        }

        private static GameObject CreateText(Transform parent, string name, string content, int size, FontStyle style, Color color, Vector2 anchoredPos, Vector2 sizeDelta, Font font)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = font;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 anchoredPos, Vector2 sizeDelta, Color bgColor, Font font, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var img = btnGo.AddComponent<Image>();
            img.color = bgColor;
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.25f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var rt = btnGo.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(btnGo.transform, false);
            var t = txtGo.AddComponent<Text>();
            t.text = text;
            t.fontSize = 20;
            t.fontStyle = FontStyle.Bold;
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = font;
            t.raycastTarget = false;
            var trt = txtGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
        }
    }
}
