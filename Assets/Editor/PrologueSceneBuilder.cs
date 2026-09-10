using System.IO;
using Platformer.Core;
using Platformer.Dialogue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PrologueSceneBuilder
{
    [MenuItem("Tools/Build Prologue Story Scene")]
    public static void BuildScene()
    {
        string scenePath = "Assets/Scenes/Prologue_Story.unity";
        Directory.CreateDirectory(Application.dataPath + "/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Camara
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.12f, 0.14f, 0.2f);
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);

        // 2. Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // Cargar Sprites
        var bgSprite        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MainMenu_Background.png");
        var dialoguePanelSpr= AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Dialogue_Box_Panel.png");
        var portraitFrameSpr= AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Wood_Frame.png");
        var btnNormSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Normal.png");
        var btnHovSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Hover.png");
        var btnPressSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Pressed.png");
        var dialogueData    = AssetDatabase.LoadAssetAtPath<StoryDialogueData>("Assets/Story/Prologue_Dialogue.asset");
        var titleFont       = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Mod Assets/Mod Resources/Fonts/BioRhymeExpanded-Regular SDF.asset");
        var bodyFont        = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Mod Assets/Mod Resources/Fonts/Merriweather-Regular SDF.asset");

        // Helpers
        RectTransform RT(GameObject go) => go.GetComponent<RectTransform>();
        void Stretch(GameObject go)
        {
            var r = RT(go);
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }
        GameObject MakeChild(string name, GameObject parent)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent.transform, false);
            g.AddComponent<RectTransform>();
            return g;
        }
        Image MakeImage(GameObject go, Sprite spr, Image.Type type = Image.Type.Simple)
        {
            var img = go.AddComponent<Image>();
            img.sprite = spr;
            img.type = type;
            return img;
        }
        TextMeshProUGUI MakeTMP(GameObject go, string text, int size, Color col, TextAlignmentOptions align = TextAlignmentOptions.Left, FontStyles style = FontStyles.Bold, TMP_FontAsset font = null)
        {
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = col;
            t.alignment = align;
            t.fontStyle = style;
            if (font != null) t.font = font;
            else if (bodyFont != null) t.font = bodyFont;
            return t;
        }
        void SetAnchored(GameObject go, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var r = RT(go);
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot     = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta        = size;
        }

        // Fondo Villa
        var bgGO = MakeChild("Background", canvasGO);
        Stretch(bgGO);
        var bgImg = MakeImage(bgGO, bgSprite);
        bgImg.preserveAspect = true;

        // Overlay sutil para enfoque cinemático
        var overlayGO = MakeChild("CinematicOverlay", bgGO);
        Stretch(overlayGO);
        var ovImg = overlayGO.AddComponent<Image>();
        ovImg.color = new Color(0.04f, 0.06f, 0.1f, 0.2f);

        // ÁREA DE CLICK EN TODA LA PANTALLA (Para avanzar con click en cualquier lugar)
        var clickAreaGO = MakeChild("ScreenClickArea", canvasGO);
        Stretch(clickAreaGO);
        var clickImg = clickAreaGO.AddComponent<Image>();
        clickImg.color = new Color(0, 0, 0, 0); // Totalmente transparente pero interactivo
        var screenClickBtn = clickAreaGO.AddComponent<Button>();
        screenClickBtn.transition = Selectable.Transition.None;

        // 3. DIALOGUE BOX (Novela Visual)
        var diagBoxGO = MakeChild("DialogueBox", canvasGO);
        SetAnchored(diagBoxGO, new Vector2(0.5f, 0.02f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(1300, 260));
        MakeImage(diagBoxGO, dialoguePanelSpr, Image.Type.Sliced);

        // Marco del Retrato (Izquierda)
        var frameGO = MakeChild("PortraitFrame", diagBoxGO);
        SetAnchored(frameGO, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(25, 0), new Vector2(200, 200));
        MakeImage(frameGO, portraitFrameSpr, Image.Type.Sliced);

        var portraitGO = MakeChild("PortraitImage", frameGO);
        Stretch(portraitGO);
        var pR = RT(portraitGO);
        pR.offsetMin = new Vector2(18, 18);
        pR.offsetMax = new Vector2(-18, -18);
        var portraitImg = MakeImage(portraitGO, null);
        portraitImg.preserveAspect = true;

        // Nombre del Hablante
        var nameGO = MakeChild("SpeakerName", diagBoxGO);
        SetAnchored(nameGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250, -25), new Vector2(600, 45));
        var nameTMP = MakeTMP(nameGO, "Personaje", 32, new Color(1f, 0.88f, 0.4f), TextAlignmentOptions.Left, FontStyles.Bold, titleFont);

        // Texto del Diálogo
        var textGO = MakeChild("DialogueText", diagBoxGO);
        SetAnchored(textGO, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(250, -80), new Vector2(980, 120));
        var textTMP = MakeTMP(textGO, "Texto de ejemplo...", 26, new Color(0.95f, 0.95f, 0.92f), TextAlignmentOptions.TopLeft, FontStyles.Normal, bodyFont);

        // Indicador de continuar (parpadeo)
        var indGO = MakeChild("ContinueIndicator", diagBoxGO);
        SetAnchored(indGO, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-35, 25), new Vector2(340, 35));
        var indTMP = MakeTMP(indGO, "Click o Espacio para continuar >>", 19, new Color(1f, 0.92f, 0.5f, 0.85f), TextAlignmentOptions.Right, FontStyles.Italic, bodyFont);

        // Botón Saltar Diálogo (En frente, para que reciba click prioritario)
        var skipGO = MakeChild("BtnSkip", diagBoxGO);
        SetAnchored(skipGO, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-25, -20), new Vector2(140, 44));
        var skipImg = MakeImage(skipGO, btnNormSprite, Image.Type.Sliced);
        var skipBtn = skipGO.AddComponent<Button>();
        skipBtn.transition = Selectable.Transition.SpriteSwap;
        var ss = skipBtn.spriteState;
        ss.highlightedSprite = btnHovSprite;
        ss.pressedSprite     = btnPressSprite;
        skipBtn.spriteState = ss;

        var skipTxtGO = MakeChild("Text", skipGO);
        Stretch(skipTxtGO);
        MakeTMP(skipTxtGO, "Saltar >>", 20, Color.white, TextAlignmentOptions.Center, FontStyles.Bold, titleFont);

        // Componente DialogueUI
        var diagComp = diagBoxGO.AddComponent<DialogueUI>();
        diagComp.dialoguePanel        = diagBoxGO;
        diagComp.speakerNameText      = nameTMP;
        diagComp.dialogueContentText  = textTMP;
        diagComp.portraitImage        = portraitImg;
        diagComp.continueIndicator    = indGO;
        diagComp.nextButton           = screenClickBtn;
        diagComp.screenClickButton    = screenClickBtn;
        diagComp.skipButton           = skipBtn;
        diagComp.currentDialogue      = dialogueData;

        // Guardar escena
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[PrologueBuilder] ✅ Escena Prologue_Story creada exitosamente en: " + scenePath);
    }
}
