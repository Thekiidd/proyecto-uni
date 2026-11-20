using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// Sistema global de identidad visual Pixel-Art para Kimaya:
    /// 1. Aplica tipografía Pixel-Art auténtica con soporte completo de español (áéíóúñ¡¿):
    ///    - KimayaPixelDisplay (Silkscreen-Bold) para títulos, botones, encabezados y HUD.
    ///    - KimayaPixelBody (Jersey10-Regular) para diálogos, instrucciones y párrafos largos.
    /// 2. Aplica los botones Pixel-Art de 3 estados en todos los menús (uGUI y OnGUI):
    ///    - Normal: Crema con borde pixel negro (texto oscuro #1E140E).
    ///    - Hover: Resplandor dorado (texto oscuro #1E140E).
    ///    - Pressed (Click): Negro carbón (texto dorado claro #FFF0B8).
    /// </summary>
    public static class KimayaUITheme
    {
        private static Font _displayFont;
        private static Font _bodyFont;
        private static TMP_FontAsset _displayTmp;
        private static TMP_FontAsset _bodyTmp;

        private static Texture2D _btnNormalTex;
        private static Texture2D _btnHoverTex;
        private static Texture2D _btnPressedTex;

        private static Sprite _btnNormalSpr;
        private static Sprite _btnHoverSpr;
        private static Sprite _btnPressedSpr;

        private static GUIStyle _cachedGuiBtnStyle;
        private static bool _hookRegistered = false;

        public static readonly Color DarkInkColor    = new Color(0.12f, 0.08f, 0.05f, 1f);
        public static readonly Color PressedLitColor = new Color(1.00f, 0.94f, 0.72f, 1f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitOnSceneLoad()
        {
            if (!_hookRegistered)
            {
                _hookRegistered = true;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            ApplyThemeToActiveScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyThemeToActiveScene();
        }

        public static Font DisplayFont
        {
            get
            {
                if (_displayFont == null)
                {
                    _displayFont = Resources.Load<Font>("Fonts/KimayaPixelDisplay");
                    if (_displayFont == null) _displayFont = Resources.Load<Font>("Fonts/KimayaPixelArcade");
                    if (_displayFont != null && _displayFont.material != null && _displayFont.material.mainTexture != null)
                        _displayFont.material.mainTexture.filterMode = FilterMode.Point;
                }
                return _displayFont;
            }
        }

        public static Font BodyFont
        {
            get
            {
                if (_bodyFont == null)
                {
                    _bodyFont = Resources.Load<Font>("Fonts/KimayaPixelBody");
                    if (_bodyFont == null) _bodyFont = DisplayFont;
                    if (_bodyFont != null && _bodyFont.material != null && _bodyFont.material.mainTexture != null)
                        _bodyFont.material.mainTexture.filterMode = FilterMode.Point;
                }
                return _bodyFont;
            }
        }

        public static TMP_FontAsset DisplayTMP
        {
            get
            {
                if (_displayTmp == null)
                {
                    if (DisplayFont != null)
                    {
                        _displayTmp = TMP_FontAsset.CreateFontAsset(DisplayFont);
                        if (_displayTmp != null)
                        {
                            _displayTmp.name = "KimayaPixelDisplay_RuntimeTMP";
                            if (_displayTmp.atlasTexture != null)
                                _displayTmp.atlasTexture.filterMode = FilterMode.Point;
                        }
                    }
                    if (_displayTmp == null)
                        _displayTmp = Resources.Load<TMP_FontAsset>("Fonts/PressStart2P_Pixel");
                }
                return _displayTmp;
            }
        }

        public static TMP_FontAsset BodyTMP
        {
            get
            {
                if (_bodyTmp == null)
                {
                    if (BodyFont != null)
                    {
                        _bodyTmp = TMP_FontAsset.CreateFontAsset(BodyFont);
                        if (_bodyTmp != null)
                        {
                            _bodyTmp.name = "KimayaPixelBody_RuntimeTMP";
                            if (_bodyTmp.atlasTexture != null)
                                _bodyTmp.atlasTexture.filterMode = FilterMode.Point;
                        }
                    }
                    if (_bodyTmp == null)
                        _bodyTmp = DisplayTMP;
                }
                return _bodyTmp;
            }
        }

        public static Texture2D BtnNormalTex  => LoadCrispTex(ref _btnNormalTex,  "UI/PixelButtons/Btn_Pixel_Normal");
        public static Texture2D BtnHoverTex   => LoadCrispTex(ref _btnHoverTex,   "UI/PixelButtons/Btn_Pixel_Hover");
        public static Texture2D BtnPressedTex => LoadCrispTex(ref _btnPressedTex, "UI/PixelButtons/Btn_Pixel_Pressed");

        public static Sprite BtnNormalSprite  => LoadSlicedSprite(ref _btnNormalSpr,  BtnNormalTex,  "UI/PixelButtons/Btn_Pixel_Normal");
        public static Sprite BtnHoverSprite   => LoadSlicedSprite(ref _btnHoverSpr,   BtnHoverTex,   "UI/PixelButtons/Btn_Pixel_Hover");
        public static Sprite BtnPressedSprite => LoadSlicedSprite(ref _btnPressedSpr, BtnPressedTex, "UI/PixelButtons/Btn_Pixel_Pressed");

        private static Texture2D LoadCrispTex(ref Texture2D cache, string resPath)
        {
            if (cache != null) return cache;
            cache = Resources.Load<Texture2D>(resPath);
            if (cache == null)
            {
                var sp = Resources.Load<Sprite>(resPath);
                if (sp != null) cache = sp.texture;
            }
            if (cache != null) cache.filterMode = FilterMode.Point;
            return cache;
        }

        private static Sprite LoadSlicedSprite(ref Sprite cache, Texture2D tex, string resPath)
        {
            if (cache != null) return cache;
            var loaded = Resources.Load<Sprite>(resPath);
            if (loaded != null && loaded.border.sqrMagnitude > 1f)
            {
                if (loaded.texture != null) loaded.texture.filterMode = FilterMode.Point;
                cache = loaded;
                return cache;
            }
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                cache = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(24f, 24f, 24f, 24f)
                );
            }
            return cache;
        }

        /// <summary>
        /// Actualiza todos los botones y textos de la escena activa (incluyendo paneles ocultos)
        /// al nuevo estilo Pixel-Art (botones crema/oro/negro + tipografía pixel).
        /// </summary>
        public static void ApplyThemeToActiveScene()
        {
            // 1. Actualizar todos los TextMeshProUGUI (activos e inactivos)
            var allTmps = Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include);
            foreach (var tmp in allTmps)
            {
                if (tmp == null) continue;
                bool isHelpContent = (tmp.transform.parent != null && tmp.transform.parent.name == "PanelHelp");
                bool isBodyParagraph = tmp.text != null && (tmp.text.Length > 48 || tmp.gameObject.name.Contains("Content") || tmp.gameObject.name.Contains("DialogueText") || tmp.gameObject.name.Contains("Subtitle"));
                TMP_FontAsset targetFont = isBodyParagraph ? BodyTMP : DisplayTMP;
                if (targetFont != null)
                {
                    tmp.font = targetFont;
                    if (isHelpContent && tmp.gameObject.name == "Content")
                    {
                        var rt = tmp.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchorMin = new Vector2(0.5f, 0.50f);
                            rt.anchorMax = new Vector2(0.5f, 0.50f);
                            rt.pivot = new Vector2(0.5f, 0.5f);
                            rt.anchoredPosition = Vector2.zero;
                            rt.sizeDelta = new Vector2(540f, 250f);
                        }
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 13f;
                        tmp.fontSizeMax = 18f;
                        tmp.overflowMode = TextOverflowModes.Truncate;
                    }
                    else if (isHelpContent && tmp.gameObject.name == "Header")
                    {
                        var rt = tmp.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.anchorMin = new Vector2(0.5f, 0.80f);
                            rt.anchorMax = new Vector2(0.5f, 0.80f);
                            rt.pivot = new Vector2(0.5f, 0.5f);
                            rt.anchoredPosition = Vector2.zero;
                            rt.sizeDelta = new Vector2(540f, 42f);
                        }
                        tmp.enableAutoSizing = true;
                        tmp.fontSizeMin = 16f;
                        tmp.fontSizeMax = 22f;
                    }
                    tmp.characterSpacing = isBodyParagraph ? 1f : 2f;
                    if (tmp.fontStyle == FontStyles.Italic)
                        tmp.fontStyle = FontStyles.Normal;
                }
                tmp.text = SanitizePixelText(tmp.text);
            }

            // 2. Actualizar todos los UnityEngine.UI.Text clásicos (ej. VictoryPanel, GameOverPanel)
            var allLegacyTexts = Object.FindObjectsByType<Text>(FindObjectsInactive.Include);
            foreach (var txt in allLegacyTexts)
            {
                if (txt == null) continue;
                bool isLong = txt.text != null && txt.text.Length > 45;
                Font f = isLong ? BodyFont : DisplayFont;
                if (f != null) txt.font = f;
                txt.text = SanitizePixelText(txt.text);
            }

            // 3. Aplicar los nuevos sprites de botón (Crema -> Oro -> Negro) a todos los botones de menú
            var allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
            foreach (var btn in allButtons)
            {
                StyleButton(btn);
                if (btn != null && btn.gameObject.name == "BtnHelpBack" && btn.transform.parent != null && btn.transform.parent.name == "PanelHelp")
                {
                    var rt = btn.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.19f);
                        rt.anchorMax = new Vector2(0.5f, 0.19f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.anchoredPosition = Vector2.zero;
                        rt.sizeDelta = new Vector2(240f, 50f);
                    }
                }
            }

            // 4. En MainMenu, acomodar PanelMain (botones) del lado izquierdo para dejar el panel del Mando QR del lado derecho
            if (SceneManager.GetActiveScene().name == "MainMenu")
            {
                var panelMainGo = GameObject.Find("PanelMain");
                if (panelMainGo != null)
                {
                    var pmRt = panelMainGo.GetComponent<RectTransform>();
                    if (pmRt != null)
                    {
                        pmRt.anchorMin = new Vector2(0.28f, 0.47f);
                        pmRt.anchorMax = new Vector2(0.28f, 0.47f);
                        pmRt.pivot = new Vector2(0.5f, 0.5f);
                        pmRt.anchoredPosition = Vector2.zero;
                    }
                }
            }
        }

        public static void StyleButton(Button btn)
        {
            if (btn == null) return;
            string goName = btn.gameObject.name;

            // Ignorar áreas invisibles de click de pantalla o placas especiales del HUD del Hub
            if (goName.Contains("ScreenClick") || goName.Contains("Plaque"))
                return;

            var img = btn.GetComponent<Image>();
            if (img == null) return;
            if (img.sprite == null && img.color.a < 0.05f) return;

            if (BtnNormalSprite == null) return;

            img.sprite = BtnNormalSprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f;
            img.color = Color.white;

            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.SpriteSwap;
            btn.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = Color.white,
                pressedColor = Color.white,
                selectedColor = Color.white,
                disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.8f),
                colorMultiplier = 1f,
                fadeDuration = 0.05f
            };

            var ss = new SpriteState
            {
                highlightedSprite = BtnHoverSprite,
                selectedSprite    = BtnHoverSprite,
                pressedSprite     = BtnPressedSprite,
                disabledSprite    = BtnPressedSprite
            };
            btn.spriteState = ss;

            var feedback = btn.GetComponent<KimayaPixelButtonFeedback>();
            if (feedback == null)
                feedback = btn.gameObject.AddComponent<KimayaPixelButtonFeedback>();
            feedback.RefreshVisuals(false);
        }

        public static GUIStyle GetOnGUIButtonStyle(int fontSize = 15)
        {
            if (_cachedGuiBtnStyle == null || _cachedGuiBtnStyle.normal.background == null)
            {
                _cachedGuiBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    border = new RectOffset(24, 24, 24, 24),
                    padding = new RectOffset(14, 14, 6, 6)
                };
                if (DisplayFont != null)
                    _cachedGuiBtnStyle.font = DisplayFont;

                if (BtnNormalTex != null)
                {
                    _cachedGuiBtnStyle.normal.background = BtnNormalTex;
                    _cachedGuiBtnStyle.normal.textColor = DarkInkColor;
                    _cachedGuiBtnStyle.focused.background = BtnHoverTex != null ? BtnHoverTex : BtnNormalTex;
                    _cachedGuiBtnStyle.focused.textColor = DarkInkColor;
                }
                if (BtnHoverTex != null)
                {
                    _cachedGuiBtnStyle.hover.background = BtnHoverTex;
                    _cachedGuiBtnStyle.hover.textColor = DarkInkColor;
                }
                if (BtnPressedTex != null)
                {
                    _cachedGuiBtnStyle.active.background = BtnPressedTex;
                    _cachedGuiBtnStyle.active.textColor = PressedLitColor;
                }
            }
            _cachedGuiBtnStyle.fontSize = fontSize;
            return _cachedGuiBtnStyle;
        }

        public static GUIStyle GetOnGUILabelStyle(int fontSize, TextAnchor align = TextAnchor.MiddleCenter, bool useBody = false)
        {
            var st = new GUIStyle(GUI.skin.label)
            {
                alignment = align,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                richText = true,
                wordWrap = true
            };
            Font f = useBody ? BodyFont : DisplayFont;
            if (f != null) st.font = f;
            return st;
        }

        /// <summary>
        /// Reemplaza emojis Unicode que las fuentes pixel no incluyen por equivalentes pixel limpios.
        /// Conserva al 100% acentos y signos en español (áéíóúñÁÉÍÓÚÑ¡¿).
        /// </summary>
        public static string SanitizePixelText(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input
                .Replace("▶", ">")
                .Replace("◀", "<")
                .Replace("✕", "X")
                .Replace("⚙️", "")
                .Replace("🎵", "")
                .Replace("🔊", "")
                .Replace("🖥️", "")
                .Replace("🪟", "")
                .Replace("📐", "")
                .Replace("✨", "*")
                .Replace("💾", "")
                .Replace("🔄", "")
                .Replace("🏡", "")
                .Replace("⏱️", "")
                .Replace("🏆", "")
                .Replace("🦘", "")
                .Replace("💥", "")
                .Replace("👑", "")
                .Replace("🎉", "")
                .Replace("⚠️", "!")
                .Trim();
        }
    }

    /// <summary>
    /// Invierte dinámicamente el color del texto del botón:
    /// - Oscuro (#1E140E) cuando el botón está en Crema (Normal) o Dorado (Hover).
    /// - Dorado claro (#FFF0B8) cuando el botón pasa a Negro al hacer click (Pressed).
    /// </summary>
    public class KimayaPixelButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private TextMeshProUGUI[] _tmps;
        private Text[] _legacyTexts;
        private bool _isPressed = false;

        private void CacheTexts()
        {
            if (_tmps == null) _tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            if (_legacyTexts == null) _legacyTexts = GetComponentsInChildren<Text>(true);
        }

        private void OnEnable()
        {
            _isPressed = false;
            RefreshVisuals(false);
        }

        private void OnDisable()
        {
            _isPressed = false;
            RefreshVisuals(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            RefreshVisuals(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            RefreshVisuals(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isPressed)
            {
                _isPressed = false;
                RefreshVisuals(false);
            }
        }

        public void RefreshVisuals(bool pressed)
        {
            CacheTexts();
            Color col = pressed ? KimayaUITheme.PressedLitColor : KimayaUITheme.DarkInkColor;

            if (_tmps != null)
            {
                foreach (var t in _tmps)
                {
                    if (t == null) continue;
                    if (KimayaUITheme.DisplayTMP != null) t.font = KimayaUITheme.DisplayTMP;
                    t.text = KimayaUITheme.SanitizePixelText(t.text);
                    t.color = col;
                    if (t.fontSize > 26f) t.fontSize = 25f;
                }
            }

            if (_legacyTexts != null)
            {
                foreach (var lt in _legacyTexts)
                {
                    if (lt == null) continue;
                    if (KimayaUITheme.DisplayFont != null) lt.font = KimayaUITheme.DisplayFont;
                    lt.text = KimayaUITheme.SanitizePixelText(lt.text);
                    lt.color = col;
                }
            }
        }
    }
}
