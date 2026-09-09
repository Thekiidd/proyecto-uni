using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Core
{
    /// <summary>
    /// Maneja la carga de escenas con una transición de fade negro.
    /// Coloca este objeto en la escena MainMenu y él persiste en todas.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;

        // CanvasGroup en un panel negro que cubre toda la pantalla
        private CanvasGroup _fadeCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupFadeCanvas();
        }

        private void SetupFadeCanvas()
        {
            // Crear canvas de fade dinámicamente
            var go = new GameObject("FadeCanvas");
            go.transform.SetParent(transform);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var panel = new GameObject("FadePanel");
            panel.transform.SetParent(go.transform, false);
            var img = panel.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.black;
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            _fadeCanvas = go.AddComponent<CanvasGroup>();
            _fadeCanvas.alpha = 0f;
            _fadeCanvas.blocksRaycasts = false;
        }

        // ── Pública API ──────────────────────────────────────────────────────

        public void LoadMainMenu()
        {
            StartCoroutine(FadeAndLoad(GameData.MainMenuScene));
        }

        public void LoadVillage()
        {
            StartCoroutine(FadeAndLoad(GameData.VillageScene));
        }

        public void LoadLevel(int levelNumber)
        {
            int index = Mathf.Clamp(levelNumber - 1, 0, GameData.LevelScenes.Length - 1);
            StartCoroutine(FadeAndLoad(GameData.LevelScenes[index]));
        }

        public void LoadNextLevel()
        {
            if (GameData.Instance == null) { LoadMainMenu(); return; }

            if (GameData.Instance.IsLastLevel())
            {
                // Ganó el juego completo
                StartCoroutine(FadeAndLoad("Victory"));
                return;
            }
            GameData.Instance.GoToNextLevel();
            LoadLevel(GameData.Instance.currentLevel);
        }

        public void ReloadCurrentLevel()
        {
            if (GameData.Instance == null) { LoadMainMenu(); return; }
            LoadLevel(GameData.Instance.currentLevel);
        }

        // ── Coroutines ────────────────────────────────────────────────────────

        private IEnumerator FadeAndLoad(string sceneName)
        {
            // Fade OUT (negro)
            yield return StartCoroutine(Fade(0f, 1f));
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] La escena '{sceneName}' no existe o no está en Build Settings.\n" +
                               "Ve a File > Build Settings y agrégala.");
                // Fade de vuelta para no quedar en negro
                yield return StartCoroutine(Fade(1f, 0f));
                yield break;
            }
            while (!op.isDone) yield return null;
            // Fade IN (transparente)
            yield return StartCoroutine(Fade(1f, 0f));
        }


        private IEnumerator Fade(float from, float to)
        {
            _fadeCanvas.blocksRaycasts = (to > 0f);
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                _fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }
            _fadeCanvas.alpha = to;
            _fadeCanvas.blocksRaycasts = (to >= 1f);
        }
    }
}
