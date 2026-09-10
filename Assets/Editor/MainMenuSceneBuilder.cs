using System.IO;
using Platformer.Core;
using Platformer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MainMenuSceneBuilder
{
    [MenuItem("Tools/Build MainMenu Scene")]
    public static void BuildScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        Directory.CreateDirectory(Application.dataPath + "/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Camara
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.1f, 0.12f, 0.18f);
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);

        // 2. Singletons
        var setupGO = new GameObject("[GameSetup]");
        setupGO.AddComponent<GameData>();
        setupGO.AddComponent<SceneLoader>();

        // 3. Canvas
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

        // Cargar Sprites de UI
        var bgSprite      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MainMenu_Background.png");
        var btnNormSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Normal.png");
        var btnHovSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Hover.png");
        var btnPressSprite= AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Pressed.png");
        var panelSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Wood_Frame.png");
        var bannerSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Banner.png");
        var pawSprite     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icon_Paw.png");
        var titleFont     = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Mod Assets/Mod Resources/Fonts/BioRhymeExpanded-Regular SDF.asset");
        var bodyFont      = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Mod Assets/Mod Resources/Fonts/Merriweather-Regular SDF.asset");

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
        TextMeshProUGUI MakeTMP(GameObject go, string text, int size, Color col, TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Bold, TMP_FontAsset font = null)
        {
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = col;
            t.alignment = align;
            t.fontStyle = style;
            if (font != null) t.font = font;
            else if (titleFont != null) t.font = titleFont;
            return t;
        }
        Button MakeWoodButton(GameObject go, string label, int fontSize = 28)
        {
            var img = MakeImage(go, btnNormSprite, Image.Type.Sliced);
            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = btnHovSprite;
            ss.pressedSprite     = btnPressSprite;
            btn.spriteState = ss;

            var txtGO = MakeChild("Text", go);
            Stretch(txtGO);
            MakeTMP(txtGO, label, fontSize, new Color(1f, 0.96f, 0.88f), TextAlignmentOptions.Center, FontStyles.Bold, titleFont);
            return btn;
        }
        void SetAnchored(GameObject go, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var r = RT(go);
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot     = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta        = size;
        }

        // 4. Fondo 16:9 Artístico
        var bgGO = MakeChild("Background", canvasGO);
        Stretch(bgGO);
        var bgImg = MakeImage(bgGO, bgSprite);
        bgImg.preserveAspect = true;

        // Overlay sutil para legibilidad de botones
        var overlayGO = MakeChild("VignetteOverlay", bgGO);
        Stretch(overlayGO);
        var ovImg = overlayGO.AddComponent<Image>();
        ovImg.color = new Color(0.05f, 0.08f, 0.12f, 0.25f);

        // 5. Banner de Título
        var bannerGO = MakeChild("TitleBanner", canvasGO);
        SetAnchored(bannerGO, new Vector2(0.5f, 0.84f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 140));
        MakeImage(bannerGO, bannerSprite, Image.Type.Sliced);

        // Huellitas decorativas en el banner
        if (pawSprite != null)
        {
            var pawL = MakeChild("PawLeft", bannerGO);
            SetAnchored(pawL, new Vector2(0.08f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48, 48));
            MakeImage(pawL, pawSprite);

            var pawR = MakeChild("PawRight", bannerGO);
            SetAnchored(pawR, new Vector2(0.92f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48, 48));
            MakeImage(pawR, pawSprite);
        }

        var titleTxtGO = MakeChild("TitleText", bannerGO);
        SetAnchored(titleTxtGO, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 60));
        MakeTMP(titleTxtGO, "EL PODER DE LA AMISTAD", 46, new Color(1f, 0.92f, 0.45f));

        var subTxtGO = MakeChild("SubtitleText", bannerGO);
        SetAnchored(subTxtGO, new Vector2(0.5f, 0.24f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 36));
        MakeTMP(subTxtGO, "La Aventura de los Perros Amigos", 22, new Color(0.95f, 0.95f, 0.9f), TextAlignmentOptions.Center, FontStyles.Normal, bodyFont);

        // 6. PanelMain (Botonera)
        var panelMain = MakeChild("PanelMain", canvasGO);
        SetAnchored(panelMain, new Vector2(0.5f, 0.40f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460, 380));

        // 5 Botones de Madera
        float startY = 140f;
        float spacing = 72f;

        var btnStoryGO = MakeChild("BtnStory", panelMain);
        SetAnchored(btnStoryGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY), new Vector2(380, 64));
        var btnStory = MakeWoodButton(btnStoryGO, "MODO HISTORIA", 26);

        var btnPlayGO = MakeChild("BtnPlay", panelMain);
        SetAnchored(btnPlayGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing), new Vector2(380, 64));
        var btnPlay = MakeWoodButton(btnPlayGO, "ELEGIR PERRO", 26);

        var btnSettingsGO = MakeChild("BtnSettings", panelMain);
        SetAnchored(btnSettingsGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing * 2), new Vector2(380, 64));
        var btnSettings = MakeWoodButton(btnSettingsGO, "AJUSTES", 26);

        var btnHelpGO = MakeChild("BtnHelp", panelMain);
        SetAnchored(btnHelpGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing * 3), new Vector2(380, 64));
        var btnHelp = MakeWoodButton(btnHelpGO, "CÓMO JUGAR", 26);

        var btnQuitGO = MakeChild("BtnQuit", panelMain);
        SetAnchored(btnQuitGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing * 4), new Vector2(380, 64));
        var btnQuit = MakeWoodButton(btnQuitGO, "SALIR", 26);

        // 7. PanelSettings (Ajustes)
        var panelSettings = MakeChild("PanelSettings", canvasGO);
        SetAnchored(panelSettings, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(650, 480));
        MakeImage(panelSettings, panelSprite, Image.Type.Sliced);
        panelSettings.SetActive(false);

        var setHeaderGO = MakeChild("Header", panelSettings);
        SetAnchored(setHeaderGO, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 50));
        MakeTMP(setHeaderGO, "AJUSTES", 36, new Color(0.25f, 0.15f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold, titleFont);

        var setContentGO = MakeChild("Content", panelSettings);
        SetAnchored(setContentGO, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 220));
        var setTMP = MakeTMP(setContentGO, 
            "Música:  [||||||||..] 80%\n\n" +
            "Efectos de Sonido:  [||||||||||] 100%\n\n" +
            "Modo de Pantalla:  Pantalla Completa\n\n" +
            "Resolución:  1920 x 1080 (16:9)", 
            22, new Color(0.3f, 0.2f, 0.1f), TextAlignmentOptions.Center, FontStyles.Normal, bodyFont);

        var btnSetBackGO = MakeChild("BtnSettingsBack", panelSettings);
        SetAnchored(btnSetBackGO, new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 56));
        var btnSetBack = MakeWoodButton(btnSetBackGO, "< Volver", 24);

        // 8. PanelHelp (Cómo Jugar)
        var panelHelp = MakeChild("PanelHelp", canvasGO);
        SetAnchored(panelHelp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 540));
        MakeImage(panelHelp, panelSprite, Image.Type.Sliced);
        panelHelp.SetActive(false);

        var helpHeaderGO = MakeChild("Header", panelHelp);
        SetAnchored(helpHeaderGO, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 50));
        MakeTMP(helpHeaderGO, "CÓMO JUGAR: EL PODER DE LA AMISTAD", 30, new Color(0.25f, 0.15f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold, titleFont);

        var helpContentGO = MakeChild("Content", panelHelp);
        SetAnchored(helpContentGO, new Vector2(0.5f, 0.52f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660, 300));
        var helpTMP = MakeTMP(helpContentGO,
            "¡Uno de los perros amigos se ha perdido en las ruinas del bosque!\n" +
            "El resto de amigos deben unir sus fuerzas para rescatarlo.\n\n" +
            "- MOVIMIENTO: Flechas o teclas A / D\n" +
            "- SALTAR: Tecla Espacio o W\n" +
            "- COOPERACIÓN: Los perros grandes (San Bernardo) pueden empujar cajas pesadas. " +
            "Los perros ágiles (Akita / Golden) alcanzan salientes altos.\n" +
            "- OBJETIVO: Resuelve los puzzles de cada zona para encontrar a tu amigo.",
            20, new Color(0.28f, 0.18f, 0.1f), TextAlignmentOptions.TopLeft, FontStyles.Normal, bodyFont);

        var btnHelpBackGO = MakeChild("BtnHelpBack", panelHelp);
        SetAnchored(btnHelpBackGO, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(240, 56));
        var btnHelpBack = MakeWoodButton(btnHelpBackGO, "Entendido", 24);

        // 9. PanelCharacterSelect (Adaptado con marco de madera)
        var panelChar = MakeChild("PanelCharacterSelect", canvasGO);
        SetAnchored(panelChar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 580));
        MakeImage(panelChar, panelSprite, Image.Type.Sliced);
        panelChar.SetActive(false);

        var selTitleGO = MakeChild("SelectTitle", panelChar);
        SetAnchored(selTitleGO, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 50));
        MakeTMP(selTitleGO, "ELIGE TU PERRO COMPAÑERO", 32, new Color(0.25f, 0.15f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold, titleFont);

        var portraitGO = MakeChild("Portrait", panelChar);
        SetAnchored(portraitGO, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));
        var portraitImg = MakeImage(portraitGO, null);

        var charNameGO = MakeChild("CharName", panelChar);
        SetAnchored(charNameGO, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 45));
        MakeTMP(charNameGO, "Golden Retriever", 34, new Color(0.6f, 0.35f, 0.05f), TextAlignmentOptions.Center, FontStyles.Bold, titleFont);

        var descGO = MakeChild("Description", panelChar);
        SetAnchored(descGO, new Vector2(0.5f, 0.35f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(550, 40));
        var descTMP = MakeTMP(descGO, "Leal, veloz y lleno de energía.", 20, new Color(0.35f, 0.25f, 0.15f), TextAlignmentOptions.Center, FontStyles.Normal, bodyFont);

        var statsGO = MakeChild("Stats", panelChar);
        SetAnchored(statsGO, new Vector2(0.5f, 0.26f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(550, 40));
        var statsTMP = MakeTMP(statsGO, "Velocidad: 10  |  Salto: 14  |  Habilidad: Rescate", 19, new Color(0.2f, 0.45f, 0.15f), TextAlignmentOptions.Center, FontStyles.Normal, bodyFont);

        var specialGO = MakeChild("Special", panelChar);
        SetAnchored(specialGO, new Vector2(0.5f, 0.19f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(550, 35));
        MakeTMP(specialGO, "Amistad: +20% velocidad cerca de un amigo", 19, new Color(0.6f, 0.2f, 0.1f), TextAlignmentOptions.Center, FontStyles.Normal, bodyFont);

        var btnLeftGO = MakeChild("BtnLeft", panelChar);
        SetAnchored(btnLeftGO, new Vector2(0.12f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(65, 65));
        var btnLeft = MakeWoodButton(btnLeftGO, "<", 32);

        var btnRightGO = MakeChild("BtnRight", panelChar);
        SetAnchored(btnRightGO, new Vector2(0.88f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(65, 65));
        var btnRight = MakeWoodButton(btnRightGO, ">", 32);

        var btnBackGO = MakeChild("BtnBack", panelChar);
        SetAnchored(btnBackGO, new Vector2(0.32f, 0.08f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 56));
        var btnBack = MakeWoodButton(btnBackGO, "< Volver", 24);

        var btnStartGO = MakeChild("BtnStart", panelChar);
        SetAnchored(btnStartGO, new Vector2(0.68f, 0.08f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 56));
        var btnStart = MakeWoodButton(btnStartGO, "SELECCIONAR", 24);

        // CharacterSelectUI
        var charSelectComp = panelChar.AddComponent<CharacterSelectUI>();
        var sObj = new SerializedObject(charSelectComp);
        sObj.FindProperty("portraitImage").objectReferenceValue   = portraitImg;
        sObj.FindProperty("nameText").objectReferenceValue        = charNameGO.GetComponent<TextMeshProUGUI>();
        sObj.FindProperty("descriptionText").objectReferenceValue = descTMP;
        sObj.FindProperty("statsText").objectReferenceValue       = statsTMP;
        sObj.FindProperty("specialText").objectReferenceValue     = specialGO.GetComponent<TextMeshProUGUI>();
        sObj.FindProperty("btnLeft").objectReferenceValue         = btnLeft;
        sObj.FindProperty("btnRight").objectReferenceValue        = btnRight;
        sObj.ApplyModifiedProperties();

        // 10. MainMenuController
        var mmcGO = new GameObject("MainMenuController");
        var mmc   = mmcGO.AddComponent<MainMenuController>();
        var mmcSObj = new SerializedObject(mmc);
        mmcSObj.FindProperty("panelMain").objectReferenceValue            = panelMain;
        mmcSObj.FindProperty("panelCharacterSelect").objectReferenceValue = panelChar;
        mmcSObj.FindProperty("panelSettings").objectReferenceValue        = panelSettings;
        mmcSObj.FindProperty("panelHelp").objectReferenceValue            = panelHelp;
        mmcSObj.FindProperty("btnStory").objectReferenceValue             = btnStory;
        mmcSObj.FindProperty("btnPlay").objectReferenceValue              = btnPlay;
        mmcSObj.FindProperty("btnSettings").objectReferenceValue          = btnSettings;
        mmcSObj.FindProperty("btnHelp").objectReferenceValue              = btnHelp;
        mmcSObj.FindProperty("btnQuit").objectReferenceValue              = btnQuit;
        mmcSObj.FindProperty("btnSettingsBack").objectReferenceValue      = btnSetBack;
        mmcSObj.FindProperty("btnHelpBack").objectReferenceValue          = btnHelpBack;
        mmcSObj.FindProperty("btnStart").objectReferenceValue             = btnStart;
        mmcSObj.FindProperty("btnBack").objectReferenceValue              = btnBack;
        mmcSObj.FindProperty("characterSelectUI").objectReferenceValue    = charSelectComp;
        mmcSObj.ApplyModifiedProperties();

        // 11. Guardar y refrescar escena
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(scenePath);
        AddSceneToBuildSettings("Assets/Scenes/Prologue_Story.unity");
        AddSceneToBuildSettings("Assets/Scenes/Village_Scene.unity");
        AddSceneToBuildSettings("Assets/Scenes/Level_01.unity");
        AddSceneToBuildSettings("Assets/Scenes/Level_02.unity");

        Debug.Log("[MainMenuBuilder] ✅ Escena MainMenu reconstruida con nuevo arte en: " + scenePath);
    }

    static void AddSceneToBuildSettings(string path)
    {
        if (!File.Exists(path)) return;
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
