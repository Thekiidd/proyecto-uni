using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// Controlador integral de Ajustes de Kimaya (Audio, Pantalla, Resolución y Calidad).
    /// Funciona tanto mediante componentes uGUI como con overlay interactivo OnGUI para máxima compatibilidad.
    /// Guarda y sincroniza preferencias en PlayerPrefs en tiempo real.
    /// </summary>
    public class KimayaSettingsController : MonoBehaviour
    {
        public static KimayaSettingsController Instance { get; private set; }

        private static bool _sceneHooked = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitGlobalQualityHook()
        {
            if (!_sceneHooked)
            {
                _sceneHooked = true;
                SceneManager.sceneLoaded += (s, m) =>
                {
                    int savedQ = Mathf.Clamp(PlayerPrefs.GetInt("Kimaya_Quality", 2), 0, 2);
                    ApplyQualityToEntireGame(savedQ);
                };
            }
            int q = Mathf.Clamp(PlayerPrefs.GetInt("Kimaya_Quality", 2), 0, 2);
            ApplyQualityToEntireGame(q);
        }

        [Header("Valores Actuales")]
        [Range(0f, 1f)] public float musicVolume = 0.8f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        public bool isFullScreen = true;
        public int resolutionIndex = 0;
        public int qualityIndex = 2; // 0=Baja, 1=Media, 2=Alta

        private readonly (int width, int height, string label)[] _resolutions = new (int, int, string)[]
        {
            (1920, 1080, "1920 × 1080 (FHD)"),
            (1600, 900,  "1600 × 900 (HD+)"),
            (1366, 768,  "1366 × 768 (WXGA)"),
            (1280, 720,  "1280 × 720 (HD)")
        };

        private readonly string[] _qualityNames = new string[] { "Rápida (Baja)", "Equilibrada (Media)", "Alta (60 FPS)" };

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadSettings();
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        public void LoadSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("Kimaya_MusicVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("Kimaya_SFXVolume", 0.8f);
            isFullScreen = PlayerPrefs.GetInt("Kimaya_FullScreen", Screen.fullScreen ? 1 : 0) == 1;
            resolutionIndex = Mathf.Clamp(PlayerPrefs.GetInt("Kimaya_ResolutionIdx", 0), 0, _resolutions.Length - 1);
            qualityIndex = Mathf.Clamp(PlayerPrefs.GetInt("Kimaya_Quality", 2), 0, _qualityNames.Length - 1);
        }

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("Kimaya_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("Kimaya_SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("Kimaya_FullScreen", isFullScreen ? 1 : 0);
            PlayerPrefs.SetInt("Kimaya_ResolutionIdx", resolutionIndex);
            PlayerPrefs.SetInt("Kimaya_Quality", qualityIndex);
            PlayerPrefs.Save();
        }

        public void ApplyAllSettings()
        {
            // 1. Audio (Música y SFX independientes a través de KimayaAudio)
            AudioListener.volume = 1f;
            var audio = KimayaAudio.EnsureInstance();
            if (audio != null)
            {
                audio.SetMusicVolume(musicVolume);
                audio.SetSFXVolume(sfxVolume);
            }

            // 2. Pantalla y Resolución
            var res = _resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

            // 3. Calidad Gráfica Global (afecta a todo el fondo y escenario sin volver borrosa la caja de UI)
            ApplyQualityToEntireGame(qualityIndex);
        }

        public void AdjustMusic(float delta)
        {
            musicVolume = Mathf.Clamp01(musicVolume + delta);
            AudioListener.volume = 1f;
            var audio = KimayaAudio.EnsureInstance();
            if (audio != null) audio.SetMusicVolume(musicVolume);
            SaveSettings();
        }

        public void AdjustSFX(float delta)
        {
            sfxVolume = Mathf.Clamp01(sfxVolume + delta);
            AudioListener.volume = 1f;
            var audio = KimayaAudio.EnsureInstance();
            if (audio != null)
            {
                audio.SetSFXVolume(sfxVolume);
                audio.PlaySFX(KimayaAudio.SFX.Jump);
            }
            SaveSettings();
        }

        public void ToggleFullScreen()
        {
            isFullScreen = !isFullScreen;
            var res = _resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            SaveSettings();
        }

        public void CycleResolution(int direction = 1)
        {
            resolutionIndex = (resolutionIndex + direction + _resolutions.Length) % _resolutions.Length;
            var res = _resolutions[resolutionIndex];
            Screen.SetResolution(res.width, res.height, isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            SaveSettings();
        }

        public void CycleQuality(int direction = 1)
        {
            qualityIndex = (qualityIndex + direction + _qualityNames.Length) % _qualityNames.Length;
            SaveSettings();
            ApplyQualityToEntireGame(qualityIndex);
        }

        /// <summary>
        /// Aplica la calidad gráfica elegida a TODO el juego de forma limpia y reversible:
        /// mantiene el Pixel-Art nítido (sin distorsionar texturas) y ajusta FPS, VSync,
        /// sombras, suavizado, partículas y overlays visuales.
        /// </summary>
        public static void ApplyQualityToEntireGame(int qIdx)
        {
            qIdx = Mathf.Clamp(qIdx, 0, 2);
            int unityQualityLevels = QualitySettings.names.Length;
            int targetQ = Mathf.Clamp(qIdx, 0, Mathf.Max(0, unityQualityLevels - 1));
            QualitySettings.SetQualityLevel(targetQ, true);

            // Mantener siempre nítidas las texturas pixel-art y la interfaz
            QualitySettings.globalTextureMipmapLimit = 0;

            if (qIdx == 0)
            {
                // Rápida (Baja): 30 FPS, sin VSync, sin sombras ni partículas extra
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 30;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                KimayaPixelVFX.ParticlesEnabled = false;
            }
            else if (qIdx == 1)
            {
                // Equilibrada (Media): 45 FPS, sombras básicas y partículas activas
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 45;
                QualitySettings.antiAliasing = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                KimayaPixelVFX.ParticlesEnabled = true;
            }
            else
            {
                // Alta (60 FPS): 60 FPS con VSync, calidad máxima y efectos completos
                QualitySettings.vSyncCount = 1;
                Application.targetFrameRate = 60;
                QualitySettings.antiAliasing = 2;
                QualitySettings.shadows = ShadowQuality.All;
                KimayaPixelVFX.ParticlesEnabled = true;
            }

            // Activar o desactivar overlays cinemáticos opcionales según el nivel de calidad
            var allImages = Object.FindObjectsByType<Image>(FindObjectsInactive.Include);
            foreach (var img in allImages)
            {
                if (img == null) continue;
                string n = img.gameObject.name;
                if (n == "VignetteOverlay" || n == "CinematicOverlay")
                {
                    img.enabled = (qIdx > 0);
                }
            }
        }

        // Renderizado del menú interactivo de Ajustes con tipografía y botones Pixel-Art (Crema / Oro / Negro)
        public void DrawSettingsUI(Rect containerRect, System.Action onBackClicked = null)
        {
            GUI.backgroundColor = new Color(0.11f, 0.08f, 0.14f, 0.98f);
            GUI.Box(containerRect, GUIContent.none);

            float padding = 28f;
            float contentX = containerRect.x + padding;
            float contentY = containerRect.y + 22f;
            float contentW = containerRect.width - (padding * 2f);

            // Título Pixel-Art
            GUIStyle titleStyle = KimayaUITheme.GetOnGUILabelStyle(22, TextAnchor.MiddleCenter, false);
            GUI.Label(new Rect(contentX, contentY, contentW, 36f), "<color=#F6D064>AJUSTES DEL JUEGO</color>", titleStyle);
            contentY += 48f;

            GUIStyle labelStyle = KimayaUITheme.GetOnGUILabelStyle(19, TextAnchor.MiddleLeft, true);
            GUIStyle valStyle   = KimayaUITheme.GetOnGUILabelStyle(15, TextAnchor.MiddleCenter, false);
            GUIStyle btnStyle   = KimayaUITheme.GetOnGUIButtonStyle(14);
            GUIStyle smallBtnStyle = KimayaUITheme.GetOnGUIButtonStyle(16);

            float rowH = 42f;
            float rowSpacing = 12f;

            // Fila 1: Volumen Música
            GUI.Label(new Rect(contentX, contentY, 165f, rowH), "<color=#FFF6E5>Música:</color>", labelStyle);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + 165f, contentY, 44f, rowH), "-", smallBtnStyle)) AdjustMusic(-0.1f);

            DrawProgressBar(new Rect(contentX + 216f, contentY + 9f, contentW - 344f, 24f), musicVolume, new Color(0.24f, 0.85f, 0.44f));

            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + contentW - 120f, contentY, 44f, rowH), "+", smallBtnStyle)) AdjustMusic(0.1f);
            GUI.Label(new Rect(contentX + contentW - 72f, contentY, 72f, rowH), $"<color=#F6D064>{Mathf.RoundToInt(musicVolume * 100)}%</color>", valStyle);
            contentY += rowH + rowSpacing;

            // Fila 2: Volumen Efectos
            GUI.Label(new Rect(contentX, contentY, 165f, rowH), "<color=#FFF6E5>Efectos (SFX):</color>", labelStyle);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + 165f, contentY, 44f, rowH), "-", smallBtnStyle)) AdjustSFX(-0.1f);

            DrawProgressBar(new Rect(contentX + 216f, contentY + 9f, contentW - 344f, 24f), sfxVolume, new Color(0.36f, 0.72f, 1f));

            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + contentW - 120f, contentY, 44f, rowH), "+", smallBtnStyle)) AdjustSFX(0.1f);
            GUI.Label(new Rect(contentX + contentW - 72f, contentY, 72f, rowH), $"<color=#F6D064>{Mathf.RoundToInt(sfxVolume * 100)}%</color>", valStyle);
            contentY += rowH + rowSpacing;

            // Fila 3: Pantalla Completa / Ventana
            GUI.Label(new Rect(contentX, contentY, 180f, rowH), "<color=#FFF6E5>Modo de Pantalla:</color>", labelStyle);
            string screenModeText = isFullScreen ? "PANTALLA COMPLETA" : "MODO VENTANA";
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + 185f, contentY, contentW - 185f, rowH), screenModeText, btnStyle))
            {
                ToggleFullScreen();
            }
            contentY += rowH + rowSpacing;

            // Fila 4: Resolución
            GUI.Label(new Rect(contentX, contentY, 180f, rowH), "<color=#FFF6E5>Resolución:</color>", labelStyle);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + 185f, contentY, contentW - 185f, rowH), $"< {_resolutions[resolutionIndex].label} >", btnStyle))
            {
                CycleResolution(1);
            }
            contentY += rowH + rowSpacing;

            // Fila 5: Calidad Gráfica
            GUI.Label(new Rect(contentX, contentY, 180f, rowH), "<color=#FFF6E5>Calidad Gráfica:</color>", labelStyle);
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(contentX + 185f, contentY, contentW - 185f, rowH), $"< {_qualityNames[qualityIndex]} >", btnStyle))
            {
                CycleQuality(1);
            }
            contentY += rowH + 18f;

            // Botón Volver / Guardar
            float backW = 270f;
            float backH = 48f;
            float backX = containerRect.x + (containerRect.width - backW) * 0.5f;
            GUI.backgroundColor = Color.white;
            if (GUI.Button(new Rect(backX, contentY, backW, backH), "GUARDAR Y VOLVER", btnStyle))
            {
                SaveSettings();
                if (onBackClicked != null) onBackClicked.Invoke();
            }
        }

        private void DrawProgressBar(Rect rect, float fillPercent, Color fillColor)
        {
            Color prevColor = GUI.color;
            // Fondo
            GUI.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            // Relleno
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, (rect.width - 4f) * Mathf.Clamp01(fillPercent), rect.height - 4f);
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
            GUI.color = prevColor;
        }

        private void OnGUI()
        {
            // Solo dibuja si este GameObject está activo directamente en la jerarquía (como en PanelSettings de MainMenu)
            if (gameObject.name == "PanelSettings" && gameObject.activeInHierarchy)
            {
                float w = Mathf.Min(640f, Screen.width - 40f);
                float h = 420f;
                float x = (Screen.width - w) * 0.5f;
                float y = (Screen.height - h) * 0.5f;
                DrawSettingsUI(new Rect(x, y, w, h), () =>
                {
                    gameObject.SetActive(false);
                    // Reactivar PanelMain
                    var parentCanvas = transform.parent;
                    if (parentCanvas != null)
                    {
                        var panelMain = parentCanvas.Find("PanelMain");
                        if (panelMain != null) panelMain.gameObject.SetActive(true);
                    }
                });
            }
        }
    }
}
