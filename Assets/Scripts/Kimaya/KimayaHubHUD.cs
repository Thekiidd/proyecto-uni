using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// HUD Pixel-Art para el Santuario / Hub de Kimaya usando placas talladas de PixelLab
    /// y tipografía pixel PressStart2P (reemplaza los recuadros grises planos de OnGUI).
    /// </summary>
    public class KimayaHubHUD : MonoBehaviour
    {
        private Canvas _hudCanvas;
        private TextMeshProUGUI _activeDogTmp;
        private TextMeshProUGUI _roleTmp;
        private Image _portraitImg;
        private Sprite _falinPortrait;
        private Sprite _miaPortrait;
        private bool _lastFalinState = true;
        private bool _initialized = false;

        private void Start()
        {
            BuildPixelHubHUD();
        }

        private void Update()
        {
            if (!_initialized) return;

            bool isFalin = KimayaHubDuoController.Instance == null || KimayaHubDuoController.Instance.isFalinActive;
            if (isFalin != _lastFalinState)
            {
                _lastFalinState = isFalin;
                RefreshDogDisplay(isFalin);
            }
        }

        private void BuildPixelHubHUD()
        {
            if (_initialized) return;
            _initialized = true;

            var canvasGo = new GameObject("Hub_PixelLab_Canvas");
            canvasGo.transform.SetParent(transform, false);
            _hudCanvas = canvasGo.AddComponent<Canvas>();
            _hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _hudCanvas.sortingOrder = 50;

            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            Sprite sprTimerPlaque  = KimayaPixelHUD.LoadPixelSprite("UI/PixelHUD/HUD_TimerPlaque");
            Sprite sprCookiePlaque = KimayaPixelHUD.LoadPixelSprite("UI/PixelHUD/HUD_CookiePlaque");
            Sprite sprCrown        = KimayaPixelHUD.LoadPixelSprite("UI/PixelHUD/Icon_PixelCrown");
            TMP_FontAsset pxFont   = KimayaUITheme.DisplayTMP ?? Resources.Load<TMP_FontAsset>("Fonts/PressStart2P_Pixel");

            if (KimayaHubDuoController.Instance != null)
            {
                if (KimayaHubDuoController.Instance.falinSprites != null)
                    _falinPortrait = KimayaHubDuoController.Instance.falinSprites.defaultPortrait;
                if (KimayaHubDuoController.Instance.miaSprites != null)
                    _miaPortrait = KimayaHubDuoController.Instance.miaSprites.defaultPortrait;
            }

            // 1. Placa Superior Izquierda: Perrita Activa + Botón [TAB]
            var dogPlaque = CreateRect("HUD_ActiveDogPlaque", canvasGo.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(20f, -16f), new Vector2(420f, 92f));
            var dpImg = dogPlaque.AddComponent<Image>();
            dpImg.sprite = sprTimerPlaque;

            var dogBtn = dogPlaque.AddComponent<Button>();
            dogBtn.transition = Selectable.Transition.ColorTint;
            dogBtn.onClick.AddListener(() =>
            {
                if (KimayaHubDuoController.Instance != null)
                {
                    KimayaHubDuoController.Instance.isFalinActive = !KimayaHubDuoController.Instance.isFalinActive;
                }
            });

            var portGo = CreateRect("DogPortrait", dogPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(78f, 4f), new Vector2(54f, 54f));
            _portraitImg = portGo.AddComponent<Image>();
            _portraitImg.preserveAspect = true;
            _portraitImg.raycastTarget = false;

            _activeDogTmp = CreatePxTMP("Txt_ActiveDog", dogPlaque.transform, pxFont,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(112f, 11f), new Vector2(260f, 28f),
                "FALIN - BORDER", 13f, new Color(1f, 0.88f, 0.40f), TextAlignmentOptions.Left);

            _roleTmp = CreatePxTMP("Txt_DogRole", dogPlaque.transform, pxFont,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(112f, -13f), new Vector2(260f, 24f),
                "[TAB] CAMBIAR", 10f, new Color(0.72f, 1f, 0.80f), TextAlignmentOptions.Left);

            // 2. Placa Superior Central: Misión Activa
            var questPlaque = CreateRect("HUD_QuestPlaque", canvasGo.transform,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(460f, 82f));
            var qpImg = questPlaque.AddComponent<Image>();
            qpImg.sprite = sprTimerPlaque;
            qpImg.raycastTarget = false;

            var qIconGo = CreateRect("QuestCrown", questPlaque.transform,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(72f, 0f), new Vector2(32f, 28f));
            var qIconImg = qIconGo.AddComponent<Image>();
            qIconImg.sprite = sprCrown;
            qIconImg.preserveAspect = true;
            qIconImg.raycastTarget = false;

            CreatePxTMP("Txt_Quest", questPlaque.transform, pxFont,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(18f, -1f), new Vector2(310f, 36f),
                "PORTAL VERDE: NIVEL 1", 12f, new Color(1f, 0.96f, 0.85f), TextAlignmentOptions.Center);

            // 3. Botón Superior Derecho: Menú Principal en Placa PixelLab
            var menuPlaque = CreateRect("Btn_MenuPlaque", canvasGo.transform,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -16f), new Vector2(210f, 76f));
            var mpImg = menuPlaque.AddComponent<Image>();
            mpImg.sprite = sprCookiePlaque != null ? sprCookiePlaque : sprTimerPlaque;

            var menuBtn = menuPlaque.AddComponent<Button>();
            menuBtn.onClick.AddListener(() =>
            {
                if (Application.CanStreamedLevelBeLoaded("MainMenu"))
                    SceneManager.LoadScene("MainMenu");
            });

            CreatePxTMP("Txt_Menu", menuPlaque.transform, pxFont,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -1f), new Vector2(140f, 32f),
                "MENU", 14f, new Color(1f, 0.90f, 0.50f), TextAlignmentOptions.Center);

            bool initFalin = KimayaHubDuoController.Instance == null || KimayaHubDuoController.Instance.isFalinActive;
            _lastFalinState = initFalin;
            RefreshDogDisplay(initFalin);
        }

        private void RefreshDogDisplay(bool isFalin)
        {
            if (_activeDogTmp != null)
            {
                _activeDogTmp.text = isFalin ? "FALIN (BORDER)" : "MIA (SCHNAUZER)";
                _activeDogTmp.color = isFalin ? new Color(1f, 0.88f, 0.40f) : new Color(0.55f, 0.92f, 1f);
            }
            if (_roleTmp != null)
            {
                _roleTmp.text = isFalin ? "FUERZA | [TAB]" : "AGILIDAD | [TAB]";
            }
            if (_portraitImg != null)
            {
                Sprite target = isFalin ? _falinPortrait : _miaPortrait;
                _portraitImg.sprite = target;
                _portraitImg.enabled = (target != null);
            }
        }

        private static GameObject CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        private static TextMeshProUGUI CreatePxTMP(string name, Transform parent, TMP_FontAsset font, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, string text, float fontSize, Color col, TextAlignmentOptions align)
        {
            var shGo = CreateRect(name + "_Sh", parent, anchor, anchor, pivot, pos + new Vector2(2f, -2f), size);
            var sh = shGo.AddComponent<TextMeshProUGUI>();
            if (font != null) sh.font = font;
            sh.text = text;
            sh.fontSize = fontSize;
            sh.color = new Color(0.06f, 0.04f, 0.03f, 0.95f);
            sh.alignment = align;
            sh.textWrappingMode = TextWrappingModes.NoWrap;
            sh.overflowMode = TextOverflowModes.Overflow;
            sh.raycastTarget = false;

            var go = CreateRect(name, parent, anchor, anchor, pivot, pos, size);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = col;
            tmp.alignment = align;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.raycastTarget = false;

            go.AddComponent<PixelShadowSync>().shadowTarget = sh;
            return tmp;
        }
    }
}
