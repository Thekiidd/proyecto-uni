using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Sistema global de Pantalla de Carga y Transición Pixel-Art entre escenas:
    /// - Muestra a Falin (Border Collie) o Mía (Schnauzer) corriendo animada sobre la barra de progreso.
    /// - Muestra el título y subtítulo de la zona de destino.
    /// - Muestra consejos dinámicos de juego (Tips) y realiza una cortinilla suave de entrada y salida.
    /// </summary>
    public class KimayaSceneTransition : MonoBehaviour
    {
        public static KimayaSceneTransition Instance { get; private set; }

        private bool _isTransitioning = false;
        private float _fadeAlpha = 0f;
        private float _progress = 0f;
        private float _animTimer = 0f;
        private string _targetScene = "";
        private string _zoneTitle = "";
        private string _zoneSubtitle = "";
        private string _currentTip = "";
        private bool _isMia = false;

        private Texture2D[] _falinWalkTex;
        private Texture2D[] _miaWalkTex;
        private Texture2D _cookieTex;

        private static readonly string[] _tips = new string[]
        {
            "CONSEJO: Si sueltas [ESPACIO] sin presionar A ni D, saltarás recto hacia arriba en tu lugar.",
            "CONSEJO: Busca las 3 Scooby Galletas escondidas en los pisos 1, 3 y 4 de la torre.",
            "CONSEJO: En el Santuario puedes hablar con el Hada Maya [E] o alternar entre Falin y Mía con [TAB].",
            "CONSEJO: ¡Cuidado con los picos morados del Zarzal! Si los pisas te empujarán hacia el vacío.",
            "CONSEJO: En el Piso 3 hay un punto de control; si caes hasta abajo podrás reaparecer ahí.",
            "CONSEJO: Completa el ascenso en menos de 01:45 para ganar la Medalla de Oro."
        };

        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            if (Instance == null)
            {
                var go = new GameObject("KimayaSceneTransition");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<KimayaSceneTransition>();
            }

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.DoorOpen);
            }
            Instance.StartSceneTransition(sceneName);
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
            LoadDogFrames();
        }

        private void LoadDogFrames()
        {
            _falinWalkTex = new Texture2D[8];
            _miaWalkTex = new Texture2D[8];
            for (int i = 0; i < 8; i++)
            {
                _falinWalkTex[i] = LoadCrisp("UI/LoadingDogs/Falin_Walk_" + i);
                _miaWalkTex[i]   = LoadCrisp("UI/LoadingDogs/Mia_Walk_" + i);
            }
            _cookieTex = LoadCrisp("UI/PixelHUD/ScoobyGalleta_Icon");
        }

        private static Texture2D LoadCrisp(string path)
        {
            var tex = Resources.Load<Texture2D>(path);
            if (tex == null)
            {
                var sp = Resources.Load<Sprite>(path);
                if (sp != null) tex = sp.texture;
            }
            if (tex != null) tex.filterMode = FilterMode.Point;
            return tex;
        }

        private void StartSceneTransition(string sceneName)
        {
            if (_isTransitioning) return;

            Time.timeScale = 1f;
            _targetScene = sceneName;
            _isMia = PlayerPrefs.GetString("SelectedCharacter", "Falin") == "Mia";
            _currentTip = _tips[Random.Range(0, _tips.Length)];
            ResolveZoneTitles(sceneName, out _zoneTitle, out _zoneSubtitle);

            StartCoroutine(TransitionRoutine(sceneName));
        }

        private static void ResolveZoneTitles(string sceneName, out string title, out string subtitle)
        {
            if (sceneName.Contains("Ascenso"))
            {
                title = "LA TORRE DEL ZARZAL SOMBRÍO";
                subtitle = "Escala los 5 pisos y rompe la jaula espinosa donde aguarda Kiara";
            }
            else if (sceneName.Contains("Hub") || sceneName.Contains("Village"))
            {
                title = "SANTUARIO DEL BOSQUE DEL ECO";
                subtitle = "El círculo rúnico ancestral de las tres hermanas";
            }
            else if (sceneName.Contains("Prologue"))
            {
                title = "EL ECO DE LAS TRES HERMANAS";
                subtitle = "Capítulo I — El llamado del bosque";
            }
            else if (sceneName.Contains("Ending"))
            {
                title = "EL REENCUENTRO EN EL BOSQUE";
                subtitle = "Falin, Mía y Kiara reunidas de nuevo en casa";
            }
            else
            {
                title = "KIMAYA: EL BOSQUE DEL ECO";
                subtitle = "Preparando el bosque mágico...";
            }
        }

        private IEnumerator TransitionRoutine(string sceneName)
        {
            _isTransitioning = true;
            _progress = 0f;
            _animTimer = 0f;

            // 1. Cortinilla de entrada (Fade in rápido)
            float t = 0f;
            const float fadeInDur = 0.25f;
            while (t < fadeInDur)
            {
                t += Time.unscaledDeltaTime;
                _fadeAlpha = Mathf.Clamp01(t / fadeInDur);
                yield return null;
            }
            _fadeAlpha = 1f;

            // 2. Carga asíncrona + animación del perrito corriendo (mínimo 1.05s para apreciar la pantalla)
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = false;
            }

            float minLoadTime = 1.05f;
            float elapsed = 0f;
            while (elapsed < minLoadTime || (asyncOp != null && asyncOp.progress < 0.89f))
            {
                elapsed += Time.unscaledDeltaTime;
                _animTimer += Time.unscaledDeltaTime;
                float asyncPct = (asyncOp != null) ? Mathf.Clamp01(asyncOp.progress / 0.9f) : 1f;
                float timePct  = Mathf.Clamp01(elapsed / minLoadTime);
                _progress = Mathf.Min(asyncPct, timePct);
                yield return null;
            }

            _progress = 1f;
            if (asyncOp != null)
            {
                asyncOp.allowSceneActivation = true;
                while (!asyncOp.isDone)
                {
                    _animTimer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            yield return null;

            // 3. Cortinilla de salida (Fade out suave revelando la nueva escena)
            t = 0f;
            const float fadeOutDur = 0.28f;
            while (t < fadeOutDur)
            {
                t += Time.unscaledDeltaTime;
                _animTimer += Time.unscaledDeltaTime;
                _fadeAlpha = 1f - Mathf.Clamp01(t / fadeOutDur);
                yield return null;
            }

            _fadeAlpha = 0f;
            _isTransitioning = false;
        }

        private void OnGUI()
        {
            if (!_isTransitioning && _fadeAlpha <= 0.001f) return;

            GUI.depth = -1000;
            Color prev = GUI.color;
            float a = Mathf.Clamp01(_fadeAlpha);

            // Fondo nocturno del Bosque / Zarzal
            GUI.color = new Color(0.06f, 0.04f, 0.10f, a);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            if (a > 0.35f)
            {
                float panelA = Mathf.Clamp01((a - 0.35f) / 0.65f);
                float cw = Mathf.Min(720f, Screen.width - 60f);
                float ch = 340f;
                float cx = (Screen.width - cw) * 0.5f;
                float cy = (Screen.height - ch) * 0.5f;

                // Marco exterior dorado pixel-art
                GUI.color = new Color(0.92f, 0.72f, 0.24f, panelA * 0.95f);
                GUI.DrawTexture(new Rect(cx - 3f, cy - 3f, cw + 6f, ch + 6f), Texture2D.whiteTexture);

                // Caja interior oscura
                GUI.color = new Color(0.11f, 0.08f, 0.16f, panelA * 0.98f);
                GUI.DrawTexture(new Rect(cx, cy, cw, ch), Texture2D.whiteTexture);

                // Título de la Zona
                GUIStyle titleSt = KimayaUITheme.GetOnGUILabelStyle(24, TextAnchor.MiddleCenter, false);
                GUIStyle subSt   = KimayaUITheme.GetOnGUILabelStyle(18, TextAnchor.MiddleCenter, true);
                GUIStyle pctSt   = KimayaUITheme.GetOnGUILabelStyle(15, TextAnchor.MiddleCenter, false);
                GUIStyle tipSt   = KimayaUITheme.GetOnGUILabelStyle(17, TextAnchor.MiddleCenter, true);

                GUI.color = new Color(1f, 1f, 1f, panelA);
                GUI.Label(new Rect(cx + 24f, cy + 26f, cw - 48f, 38f), $"<color=#F8D458>{_zoneTitle}</color>", titleSt);
                GUI.Label(new Rect(cx + 24f, cy + 66f, cw - 48f, 30f), $"<color=#D8CEEC>{_zoneSubtitle}</color>", subSt);

                // Barra de progreso central
                float barW = cw - 110f;
                float barH = 26f;
                float barX = cx + 55f;
                float barY = cy + 190f;

                // Borde de la barra
                GUI.color = new Color(0.88f, 0.66f, 0.22f, panelA);
                GUI.DrawTexture(new Rect(barX - 2f, barY - 2f, barW + 4f, barH + 4f), Texture2D.whiteTexture);
                GUI.color = new Color(0.06f, 0.04f, 0.08f, panelA);
                GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);

                // Relleno de la barra
                float fillW = Mathf.Round(barW * Mathf.Clamp01(_progress));
                if (fillW > 0f)
                {
                    Color barCol = Color.Lerp(new Color(0.24f, 0.88f, 0.44f, panelA), new Color(1f, 0.84f, 0.26f, panelA), _progress);
                    GUI.color = barCol;
                    GUI.DrawTexture(new Rect(barX, barY, fillW, barH), Texture2D.whiteTexture);
                    GUI.color = new Color(1f, 1f, 1f, panelA * 0.28f);
                    GUI.DrawTexture(new Rect(barX, barY, fillW, 5f), Texture2D.whiteTexture);
                }

                // Perrito corriendo sobre la barra según el avance
                Texture2D[] frames = _isMia ? _miaWalkTex : _falinWalkTex;
                int frameIdx = Mathf.FloorToInt(_animTimer * 12f) % 8;
                Texture2D dogTex = (frames != null && frames.Length > frameIdx) ? frames[frameIdx] : null;
                if (dogTex != null)
                {
                    float dogSize = _isMia ? 76f : 86f;
                    float dogX = Mathf.Clamp(barX + fillW - dogSize * 0.5f, barX, barX + barW - dogSize * 0.5f);
                    float dogY = barY - dogSize + 14f;

                    // Motitas de polvo pixel-art detrás del perrito
                    for (int p = 0; p < 3; p++)
                    {
                        float phase = Mathf.Repeat(_animTimer * 4f + p * 0.33f, 1f);
                        float px = dogX + 12f - phase * 26f;
                        float py = barY - 8f - phase * 10f;
                        GUI.color = new Color(0.65f, 0.94f, 0.55f, (1f - phase) * panelA * 0.85f);
                        GUI.DrawTexture(new Rect(px, py, 5f, 5f), Texture2D.whiteTexture);
                    }

                    GUI.color = new Color(1f, 1f, 1f, panelA);
                    GUI.DrawTexture(new Rect(dogX, dogY, dogSize, dogSize), dogTex);
                }

                // Texto de porcentaje dentro de la barra
                int pctInt = Mathf.RoundToInt(_progress * 100f);
                string dogName = _isMia ? "MÍA" : "FALIN";
                GUI.color = new Color(1f, 1f, 1f, panelA);
                GUI.Label(new Rect(barX, barY - 1f, barW, barH + 2f), $"<color=#FFFFFF>{dogName} EN CAMINO... {pctInt}%</color>", pctSt);

                // Icono de Scooby Galleta al final de la barra (meta)
                if (_cookieTex != null)
                {
                    GUI.color = new Color(1f, 1f, 1f, panelA);
                    GUI.DrawTexture(new Rect(barX + barW + 8f, barY - 2f, 34f, 28f), _cookieTex);
                }

                // Caja inferior de Consejo (Tip)
                float tipBoxY = cy + 250f;
                GUI.color = new Color(0.16f, 0.12f, 0.23f, panelA * 0.95f);
                GUI.DrawTexture(new Rect(cx + 28f, tipBoxY, cw - 56f, 64f), Texture2D.whiteTexture);
                GUI.color = new Color(1f, 1f, 1f, panelA);
                GUI.Label(new Rect(cx + 42f, tipBoxY + 6f, cw - 84f, 52f), $"<color=#FFEFA6>{_currentTip}</color>", tipSt);
            }

            GUI.color = prev;
        }
    }
}
