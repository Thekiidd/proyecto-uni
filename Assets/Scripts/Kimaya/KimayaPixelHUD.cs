using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// Interfaz Pixel-Art auténtica generada con sprites de PixelLab + fuente pixel PressStart2P:
    /// - Esquina superior izquierda del área de juego: Barra de Scooby Galletas (3 ranuras de galleta + contador pixel 0/3).
    /// - Centro superior: Placa tallada en madera con icono pixel de cronómetro (00:00) y corona de piso (PISO 1/5).
    /// - Barra inferior de carga de salto con marco pixel-art (solo visible al mantener ESPACIO).
    /// - Elimina por completo el fondo gris translúcido superior, el texto "[ESC] Pausa" y los textos inferiores de debug.
    /// </summary>
    public class KimayaPixelHUD : MonoBehaviour
    {
        private Image[] _cookieSlotImages;
        private RectTransform[] _cookieSlotRects;
        private TextMeshProUGUI _cookieCountTmp;
        private TextMeshProUGUI _timerTmp;
        private TextMeshProUGUI _floorTmp;

        private GameObject _jumpBarRoot;
        private RectTransform _jumpFillRect;
        private Image _jumpFillImg;
        private TextMeshProUGUI _jumpPctTmp;
        private Sprite _sprSolidWhite;
        private float _lastJumpHoldTimer = 0f;
        private float _lastJumpPeakPct = 0f;

        private Sprite _sprCookieFull;
        private Sprite _sprCookieEmpty;
        private Sprite _sprCookiePlaque;
        private Sprite _sprTimerPlaque;
        private Sprite _sprClock;
        private Sprite _sprCrown;
        private Sprite _sprJumpFrame;
        private TMP_FontAsset _pixelFont;

        private int _lastCollected = -1;
        private int _lastFloor = -1;
        private float _zoneBannerTimer = 0f;
        private CanvasGroup _zoneBannerGroup;
        private TextMeshProUGUI _zoneBannerTmp;
        private float[] _slotPopTimers = new float[3];
        private bool _initialized = false;

        private static readonly string[] _floorZoneNames = new string[]
        {
            "PISO 1 — RAÍCES DEL BOSQUE MÁGICO",
            "PISO 2 — DOSEL DE LAS RAMAS ANTIGUAS",
            "PISO 3 — FRONTERA DEL CREPÚSCULO",
            "PISO 4 — MURALLAS DEL ZARZAL SOMBRÍO",
            "PISO 5 — CUMBRE DE LA JAULA ESPINOSA"
        };

        public static Sprite LoadPixelSprite(string resourcePath)
        {
            Sprite sp = Resources.Load<Sprite>(resourcePath);
            if (sp != null) return sp;

            Texture2D tex = Resources.Load<Texture2D>(resourcePath);
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 36f);
            }
            return null;
        }

        public void Initialize(Canvas targetCanvas)
        {
            if (_initialized || targetCanvas == null) return;
            _initialized = true;

            // 1. Asegurar CanvasScaler en 1920x1080 para proporción nítida en cualquier ventana
            var scaler = targetCanvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = targetCanvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // 2. Ocultar barras grises y textos viejos de la escena
            HideLegacyChild(targetCanvas.transform, "TopHUD");
            HideLegacyChild(targetCanvas.transform, "Txt_Controls");
            HideLegacyChild(targetCanvas.transform, "ChargeBar_Panel");

            // Limpiar instancia previa si existiera
            var oldRoot = targetCanvas.transform.Find("PixelLab_HUD_Root");
            if (oldRoot != null) Destroy(oldRoot.gameObject);

            // 3. Cargar recursos PixelLab y fuente pixel PressStart2P
            _sprCookieFull   = LoadPixelSprite("UI/PixelHUD/ScoobyGalleta_Icon");
            _sprCookieEmpty  = LoadPixelSprite("UI/PixelHUD/ScoobyGalleta_Empty");
            _sprCookiePlaque = LoadPixelSprite("UI/PixelHUD/HUD_CookiePlaque");
            _sprTimerPlaque  = LoadPixelSprite("UI/PixelHUD/HUD_TimerPlaque");
            _sprClock        = LoadPixelSprite("UI/PixelHUD/Icon_PixelClock");
            _sprCrown        = LoadPixelSprite("UI/PixelHUD/Icon_PixelCrown");
            _sprJumpFrame    = LoadPixelSprite("UI/PixelHUD/HUD_JumpBar_Frame");
            _pixelFont       = KimayaUITheme.DisplayTMP ?? Resources.Load<TMP_FontAsset>("Fonts/PressStart2P_Pixel");

            // Contenedor raíz del HUD Pixel-Art (debajo de VictoryPanel/GameOverPanel para no tapar modales)
            var hudRoot = CreateRect("PixelLab_HUD_Root", targetCanvas.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            hudRoot.transform.SetAsFirstSibling();

            // -------------------------------------------------------------------------
            // A) BARRA DE SCOOBY GALLETAS (Arriba a la izquierda dentro del marco 4:3)
            // -------------------------------------------------------------------------
            var cookiePlaque = CreateRect("HUD_ScoobyPlaque", hudRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 1f),
                new Vector2(-696f, -14f), new Vector2(302f, 86f));
            var cpImg = cookiePlaque.AddComponent<Image>();
            cpImg.sprite = _sprCookiePlaque;
            cpImg.raycastTarget = false;

            _cookieSlotImages = new Image[3];
            _cookieSlotRects  = new RectTransform[3];
            for (int i = 0; i < 3; i++)
            {
                var slotGo = CreateRect($"ScoobySlot_{i}", cookiePlaque.transform,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(73f + i * 48f, 0f), new Vector2(50f, 38f));
                var sImg = slotGo.AddComponent<Image>();
                sImg.sprite = _sprCookieEmpty != null ? _sprCookieEmpty : _sprCookieFull;
                sImg.color = _sprCookieEmpty != null ? Color.white : new Color(0.2f, 0.15f, 0.12f, 0.75f);
                sImg.preserveAspect = true;
                sImg.raycastTarget = false;

                _cookieSlotImages[i] = sImg;
                _cookieSlotRects[i]  = slotGo.GetComponent<RectTransform>();
            }

            _cookieCountTmp = CreatePixelTMP("Txt_ScoobyCount", cookiePlaque.transform,
                new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-72f, -1f), new Vector2(96f, 40f),
                "0/3", 20f, new Color(1f, 0.86f, 0.36f), TextAlignmentOptions.Center);

            // -------------------------------------------------------------------------
            // B) PLACA CENTRAL DE TIEMPO Y PISO (Arriba al centro, sin fondo gris)
            // -------------------------------------------------------------------------
            var timerPlaque = CreateRect("HUD_TimerPlaque", hudRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(404f, 86f));
            var tpImg = timerPlaque.AddComponent<Image>();
            tpImg.sprite = _sprTimerPlaque;
            tpImg.raycastTarget = false;

            // Icono Reloj Pixel
            var clkGo = CreateRect("Icon_Clock", timerPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(72f, 1f), new Vector2(38f, 44f));
            var clkImg = clkGo.AddComponent<Image>();
            clkImg.sprite = _sprClock;
            clkImg.preserveAspect = true;
            clkImg.raycastTarget = false;

            // Texto Tiempo (ej. 00:27)
            _timerTmp = CreatePixelTMP("Txt_Timer", timerPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(98f, -1f), new Vector2(118f, 40f),
                "00:00", 19f, new Color(1f, 0.97f, 0.88f), TextAlignmentOptions.Left);

            // Icono Corona Pixel
            var crnGo = CreateRect("Icon_Crown", timerPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(232f, 1f), new Vector2(38f, 35f));
            var crnImg = crnGo.AddComponent<Image>();
            crnImg.sprite = _sprCrown;
            crnImg.preserveAspect = true;
            crnImg.raycastTarget = false;

            // Texto Piso (ej. PISO 1/5)
            _floorTmp = CreatePixelTMP("Txt_Floor", timerPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(256f, -1f), new Vector2(118f, 40f),
                "PISO 1/5", 13f, new Color(0.68f, 1f, 0.74f), TextAlignmentOptions.Left);

            // -------------------------------------------------------------------------
            // C) BARRA DE CARGA DE SALTO PIXEL-ART (Siempre visible abajo y se llena al cargar con ESPACIO)
            // -------------------------------------------------------------------------
            Texture2D whiteTex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] wCols = new Color[16];
            for (int i = 0; i < 16; i++) wCols[i] = Color.white;
            whiteTex.SetPixels(wCols);
            whiteTex.filterMode = FilterMode.Point;
            whiteTex.Apply();
            _sprSolidWhite = Sprite.Create(whiteTex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);

            _jumpBarRoot = CreateRect("HUD_PixelJumpBar", hudRoot.transform,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 28f), new Vector2(316f, 56f));

            // 1. Fondo oscuro interior de la ranura
            var jbBg = CreateRect("SlotBg", _jumpBarRoot.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(-60f, -26f));
            var jbBgImg = jbBg.AddComponent<Image>();
            jbBgImg.sprite = _sprSolidWhite;
            jbBgImg.color = new Color(0.11f, 0.06f, 0.09f, 0.96f);
            jbBgImg.raycastTarget = false;

            // 2. Relleno dinámico de potencia (se dibuja sobre SlotBg y dentro de la ventana transparente del marco)
            var jbFillContainer = CreateRect("FillArea", _jumpBarRoot.transform,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(-62f, -28f));
            var jbFill = CreateRect("Fill", jbFillContainer.transform,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                Vector2.zero, Vector2.zero);
            _jumpFillRect = jbFill.GetComponent<RectTransform>();
            _jumpFillImg = jbFill.AddComponent<Image>();
            _jumpFillImg.sprite = _sprSolidWhite;
            _jumpFillImg.type = Image.Type.Simple;
            _jumpFillImg.color = new Color(0.22f, 0.95f, 0.40f, 1f);
            _jumpFillImg.raycastTarget = false;

            // Brillo superior pixel-art dentro de la barra de carga
            var jbShine = CreateRect("FillShine", jbFill.transform,
                new Vector2(0f, 0.65f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, Vector2.zero);
            var jbShineImg = jbShine.AddComponent<Image>();
            jbShineImg.sprite = _sprSolidWhite;
            jbShineImg.color = new Color(1f, 1f, 1f, 0.28f);
            jbShineImg.raycastTarget = false;

            // 3. Marco PixelLab de madera y oro (con ventana central transparente)
            var jbFrame = CreateRect("PixelFrame", _jumpBarRoot.transform,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            var jbFrameImg = jbFrame.AddComponent<Image>();
            jbFrameImg.sprite = _sprJumpFrame;
            jbFrameImg.raycastTarget = false;

            // 4. Texto indicador de porcentaje/salto encima de la barra
            _jumpPctTmp = CreatePixelTMP("Txt_JumpCharge", _jumpBarRoot.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 0f), new Vector2(220f, 28f),
                "ESPACIO: SALTAR", 11f, new Color(1f, 0.96f, 0.82f, 0.9f), TextAlignmentOptions.Center);

            _jumpBarRoot.SetActive(true);

            // -------------------------------------------------------------------------
            // D) CARTELITO CINEMÁTICO DE ZONA AL CAMBIAR DE PISO (Desvanecimiento suave)
            // -------------------------------------------------------------------------
            var zoneBannerGo = CreateRect("HUD_ZoneFloorBanner", hudRoot.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -108f), new Vector2(560f, 38f));
            var zbBg = zoneBannerGo.AddComponent<Image>();
            zbBg.sprite = _sprSolidWhite;
            zbBg.color = new Color(0.08f, 0.05f, 0.12f, 0.84f);
            zbBg.raycastTarget = false;

            _zoneBannerGroup = zoneBannerGo.AddComponent<CanvasGroup>();
            _zoneBannerGroup.alpha = 0f;
            _zoneBannerGroup.blocksRaycasts = false;
            _zoneBannerGroup.interactable = false;

            _zoneBannerTmp = CreatePixelTMP("Txt_ZoneBanner", zoneBannerGo.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(540f, 32f),
                _floorZoneNames[0], 14f, new Color(1f, 0.88f, 0.42f, 1f), TextAlignmentOptions.Center);
        }

        public void Refresh(int collectedCookies, int totalCookies, float sessionSeconds, int currentFloor, int totalFloors, bool isCharging, float chargePercent)
        {
            if (!_initialized) return;

            // 1. Actualizar ranuras de Scooby Galletas + animación pop al recoger
            int clampedCollected = Mathf.Clamp(collectedCookies, 0, 3);
            if (_lastCollected >= 0 && clampedCollected > _lastCollected)
            {
                for (int i = _lastCollected; i < clampedCollected && i < 3; i++)
                {
                    _slotPopTimers[i] = 0.35f;
                }
            }
            _lastCollected = clampedCollected;

            for (int i = 0; i < 3; i++)
            {
                bool hasCookie = i < clampedCollected;
                if (_cookieSlotImages[i] != null)
                {
                    if (hasCookie)
                    {
                        _cookieSlotImages[i].sprite = _sprCookieFull;
                        _cookieSlotImages[i].color = Color.white;
                    }
                    else
                    {
                        _cookieSlotImages[i].sprite = _sprCookieEmpty != null ? _sprCookieEmpty : _sprCookieFull;
                        _cookieSlotImages[i].color = _sprCookieEmpty != null ? Color.white : new Color(0.2f, 0.15f, 0.12f, 0.75f);
                    }
                }

                if (_slotPopTimers[i] > 0f)
                {
                    _slotPopTimers[i] -= Time.deltaTime;
                    float t = 1f - Mathf.Clamp01(_slotPopTimers[i] / 0.35f);
                    float popScale = 1f + Mathf.Sin(t * Mathf.PI) * 0.38f;
                    _cookieSlotRects[i].localScale = new Vector3(popScale, popScale, 1f);
                }
                else if (_cookieSlotRects[i] != null)
                {
                    _cookieSlotRects[i].localScale = Vector3.one;
                }
            }

            if (_cookieCountTmp != null)
            {
                _cookieCountTmp.text = $"{collectedCookies}/{totalCookies}";
                _cookieCountTmp.color = (collectedCookies >= totalCookies)
                    ? new Color(0.42f, 1f, 0.58f)
                    : new Color(1f, 0.86f, 0.36f);
            }

            // 2. Actualizar Cronómetro y Piso
            int mins = Mathf.FloorToInt(sessionSeconds / 60f);
            int secs = Mathf.FloorToInt(sessionSeconds % 60f);
            if (_timerTmp != null)
            {
                _timerTmp.text = $"{mins:00}:{secs:00}";
            }
            if (_floorTmp != null)
            {
                _floorTmp.text = $"PISO {currentFloor}/{totalFloors}";
            }

            // Mostrar cartelito de zona cada vez que se entra a un nuevo piso
            int floorIdx = Mathf.Clamp(currentFloor - 1, 0, _floorZoneNames.Length - 1);
            if (currentFloor != _lastFloor)
            {
                _lastFloor = currentFloor;
                _zoneBannerTimer = 2.4f;
                if (_zoneBannerTmp != null)
                {
                    _zoneBannerTmp.text = _floorZoneNames[floorIdx];
                }
            }

            if (_zoneBannerGroup != null)
            {
                if (_zoneBannerTimer > 0f)
                {
                    _zoneBannerTimer -= Time.deltaTime;
                    float fadeIn = Mathf.Clamp01((2.4f - _zoneBannerTimer) / 0.25f);
                    float fadeOut = Mathf.Clamp01(_zoneBannerTimer / 0.55f);
                    _zoneBannerGroup.alpha = Mathf.Min(fadeIn, fadeOut);
                }
                else
                {
                    _zoneBannerGroup.alpha = 0f;
                }
            }

            // 3. Actualizar Barra de Carga de Salto (Siempre visible + retención breve del pico de carga al saltar)
            if (_jumpBarRoot != null)
            {
                if (!_jumpBarRoot.activeSelf)
                {
                    _jumpBarRoot.SetActive(true);
                }

                float displayPct = 0f;
                if (isCharging)
                {
                    displayPct = Mathf.Clamp01(chargePercent);
                    _lastJumpPeakPct = displayPct;
                    _lastJumpHoldTimer = 0.22f;
                }
                else if (_lastJumpHoldTimer > 0f)
                {
                    _lastJumpHoldTimer -= Time.deltaTime;
                    displayPct = _lastJumpPeakPct;
                }
                else
                {
                    _lastJumpPeakPct = 0f;
                    displayPct = 0f;
                }

                if (_jumpFillRect != null)
                {
                    _jumpFillRect.anchorMin = new Vector2(0f, 0f);
                    _jumpFillRect.anchorMax = new Vector2(displayPct, 1f);
                    _jumpFillRect.offsetMin = Vector2.zero;
                    _jumpFillRect.offsetMax = Vector2.zero;

                    if (_jumpFillImg != null)
                    {
                        _jumpFillImg.color = displayPct < 0.5f
                            ? Color.Lerp(new Color(0.22f, 0.95f, 0.40f), new Color(1f, 0.86f, 0.20f), displayPct * 2f)
                            : Color.Lerp(new Color(1f, 0.86f, 0.20f), new Color(1f, 0.22f, 0.26f), (displayPct - 0.5f) * 2f);
                    }
                }

                if (_jumpPctTmp != null)
                {
                    if (isCharging || _lastJumpHoldTimer > 0f)
                    {
                        int pctInt = Mathf.RoundToInt(displayPct * 100f);
                        _jumpPctTmp.text = $"CARGA: {pctInt}%";
                        _jumpPctTmp.color = Color.white;
                    }
                    else
                    {
                        _jumpPctTmp.text = "MANTÉN [ESPACIO]";
                        _jumpPctTmp.color = new Color(1f, 0.94f, 0.78f, 0.80f);
                    }
                }
            }
        }

        private static void HideLegacyChild(Transform parent, string childName)
        {
            var tr = parent.Find(childName);
            if (tr != null) tr.gameObject.SetActive(false);
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private TextMeshProUGUI CreatePixelTMP(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, string text, float fontSize, Color color, TextAlignmentOptions align)
        {
            // Sombra pixel 2px abajo-derecha para máxima legibilidad tipo arcade 16-bit
            var shadowGo = CreateRect(name + "_Shadow", parent, anchor, anchor, pivot, anchoredPos + new Vector2(2f, -2f), size);
            var shadowTmp = shadowGo.AddComponent<TextMeshProUGUI>();
            if (_pixelFont != null) shadowTmp.font = _pixelFont;
            shadowTmp.text = text;
            shadowTmp.fontSize = fontSize;
            shadowTmp.color = new Color(0.06f, 0.04f, 0.03f, 0.95f);
            shadowTmp.alignment = align;
            shadowTmp.textWrappingMode = TextWrappingModes.NoWrap;
            shadowTmp.overflowMode = TextOverflowModes.Overflow;
            shadowTmp.raycastTarget = false;

            var mainGo = CreateRect(name, parent, anchor, anchor, pivot, anchoredPos, size);
            var mainTmp = mainGo.AddComponent<TextMeshProUGUI>();
            if (_pixelFont != null) mainTmp.font = _pixelFont;
            mainTmp.text = text;
            mainTmp.fontSize = fontSize;
            mainTmp.color = color;
            mainTmp.alignment = align;
            mainTmp.textWrappingMode = TextWrappingModes.NoWrap;
            mainTmp.overflowMode = TextOverflowModes.Overflow;
            mainTmp.raycastTarget = false;

            // Sincronizar texto de sombra automáticamente cuando cambia el texto principal
            mainGo.AddComponent<PixelShadowSync>().shadowTarget = shadowTmp;
            return mainTmp;
        }
    }

    public class PixelShadowSync : MonoBehaviour
    {
        public TextMeshProUGUI shadowTarget;
        private TextMeshProUGUI _source;

        private void Awake()
        {
            _source = GetComponent<TextMeshProUGUI>();
        }

        private void LateUpdate()
        {
            if (_source != null && shadowTarget != null && shadowTarget.text != _source.text)
            {
                shadowTarget.text = _source.text;
            }
        }
    }
}
