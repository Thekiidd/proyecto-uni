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
    private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";
    private const string FONT_PIXEL_PATH = "Assets/Mod Assets/Mod Resources/Fonts/PressStart2P-Regular SDF.asset";
    private const string FONT_BODY_PATH  = "Assets/Mod Assets/Mod Resources/Fonts/Merriweather-Regular SDF.asset";

    [MenuItem("Tools/Kimaya/Build Main Menu (Estilo Pixel Art)")]
    [MenuItem("Tools/Build MainMenu Scene")]
    public static void BuildScene()
    {
        string scenePath = SCENE_PATH;
        Directory.CreateDirectory(Application.dataPath + "/Scenes");

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Cámara
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.14f, 0.08f);
        camGO.tag            = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);
        camGO.AddComponent<AudioListener>();

        // 2. Singletons del juego
        var setupGO = new GameObject("[GameSetup]");
        setupGO.AddComponent<GameData>();
        setupGO.AddComponent<SceneLoader>();

        // 3. Canvas con escalado 1080p
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem e Input System UI
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // 4. Carga de Assets Visuales
        var bgSprite       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/MainMenu_Background.png");
        var btnNormSprite  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Normal.png");
        var btnHovSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Hover.png");
        var btnPressSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Pressed.png");
        var panelSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Wood_Frame.png");
        var bannerSprite   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Title_Banner.png");
        var pawSprite      = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icon_Paw.png");

        // Tipografía Pixel Art Retro (Press Start 2P)
        var pixelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PIXEL_PATH);
        var bodyFont  = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_BODY_PATH) ?? pixelFont;

        // Helpers de construcción UI
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

        void SetAnchored(GameObject go, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var r = RT(go);
            r.anchorMin = anchor; r.anchorMax = anchor;
            r.pivot     = pivot;
            r.anchoredPosition = pos;
            r.sizeDelta        = size;
        }

        TextMeshProUGUI MakeTMP(GameObject go, string text, int size, Color col, TextAlignmentOptions align = TextAlignmentOptions.Center, FontStyles style = FontStyles.Normal, TMP_FontAsset font = null)
        {
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = col;
            t.alignment = align;
            t.fontStyle = style;
            t.font = font != null ? font : pixelFont;
            t.raycastTarget = false;
            return t;
        }

        Button MakeWoodButton(GameObject go, string label, int fontSize = 20)
        {
            var img = MakeImage(go, btnNormSprite, Image.Type.Sliced);
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.transition = Selectable.Transition.SpriteSwap;
            var ss = btn.spriteState;
            ss.highlightedSprite = btnHovSprite;
            ss.pressedSprite     = btnPressSprite;
            btn.spriteState = ss;

            var txtGO = MakeChild("Text", go);
            Stretch(txtGO);
            var tmp = MakeTMP(txtGO, label, fontSize, new Color(1f, 0.96f, 0.88f), TextAlignmentOptions.Center, FontStyles.Normal, pixelFont);
            tmp.characterSpacing = 2f;
            return btn;
        }

        // 5. Fondo Campestre Pixel Art
        var bgGO = MakeChild("Background", canvasGO);
        Stretch(bgGO);
        var bgImg = MakeImage(bgGO, bgSprite);
        bgImg.preserveAspect = true;

        // Viñeta suave para contraste
        var overlayGO = MakeChild("VignetteOverlay", bgGO);
        Stretch(overlayGO);
        var ovImg = overlayGO.AddComponent<Image>();
        ovImg.color = new Color(0.04f, 0.08f, 0.05f, 0.18f);
        ovImg.raycastTarget = false;

        // 6. Banner de Título Superior
        var bannerGO = MakeChild("TitleBanner", canvasGO);
        SetAnchored(bannerGO, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 140));
        MakeImage(bannerGO, bannerSprite, Image.Type.Sliced);

        // Huellitas en las pestañas laterales del banner
        if (pawSprite != null)
        {
            var pawL = MakeChild("PawLeft", bannerGO);
            SetAnchored(pawL, new Vector2(0.07f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44));
            MakeImage(pawL, pawSprite);

            var pawR = MakeChild("PawRight", bannerGO);
            SetAnchored(pawR, new Vector2(0.93f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(44, 44));
            MakeImage(pawR, pawSprite);
        }

        // Título Principal "KIMAYA" con tipografía Pixel Art
        var titleTxtGO = MakeChild("TitleText", bannerGO);
        SetAnchored(titleTxtGO, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 52));
        var titleTMP = MakeTMP(titleTxtGO, "KIMAYA", 40, new Color(1f, 0.92f, 0.45f), TextAlignmentOptions.Center, FontStyles.Bold, pixelFont);
        titleTMP.characterSpacing = 4f;

        // Subtítulo "El Bosque del Eco"
        var subTxtGO = MakeChild("SubtitleText", bannerGO);
        SetAnchored(subTxtGO, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 32));
        var subTMP = MakeTMP(subTxtGO, "El Bosque del Eco", 16, new Color(0.98f, 0.98f, 0.92f), TextAlignmentOptions.Center, FontStyles.Normal, pixelFont);
        subTMP.characterSpacing = 2f;

        // 7. PanelMain (Botonera Centrada y Elevada)
        var panelMain = MakeChild("PanelMain", canvasGO);
        SetAnchored(panelMain, new Vector2(0.5f, 0.46f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 320));

        // 4 Botones Simplificados (JUGAR, AJUSTES, CÓMO JUGAR, SALIR)
        float startY = 105f;
        float spacing = 70f;

        var btnPlayGO = MakeChild("BtnPlay", panelMain);
        SetAnchored(btnPlayGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY), new Vector2(380, 62));
        var btnPlay = MakeWoodButton(btnPlayGO, "▶ JUGAR", 20);

        var btnSettingsGO = MakeChild("BtnSettings", panelMain);
        SetAnchored(btnSettingsGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing), new Vector2(380, 62));
        var btnSettings = MakeWoodButton(btnSettingsGO, "AJUSTES", 18);

        var btnHelpGO = MakeChild("BtnHelp", panelMain);
        SetAnchored(btnHelpGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing * 2), new Vector2(380, 62));
        var btnHelp = MakeWoodButton(btnHelpGO, "COMO JUGAR", 18);

        var btnQuitGO = MakeChild("BtnQuit", panelMain);
        SetAnchored(btnQuitGO, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, startY - spacing * 3), new Vector2(380, 62));
        var btnQuit = MakeWoodButton(btnQuitGO, "✕ SALIR", 18);

        // 8. PanelSettings (Modal de Ajustes)
        var panelSettings = MakeChild("PanelSettings", canvasGO);
        SetAnchored(panelSettings, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 500));
        MakeImage(panelSettings, panelSprite, Image.Type.Sliced);
        panelSettings.SetActive(false);

        var setHeaderGO = MakeChild("Header", panelSettings);
        SetAnchored(setHeaderGO, new Vector2(0.5f, 0.88f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(520, 50));
        MakeTMP(setHeaderGO, "AJUSTES", 26, new Color(0.25f, 0.15f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold, pixelFont);

        var setContentGO = MakeChild("Content", panelSettings);
        SetAnchored(setContentGO, new Vector2(0.5f, 0.54f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(540, 230));
        MakeTMP(setContentGO,
            "MUSICA:  [||||||||..] 80%\n\n" +
            "EFECTOS: [||||||||||] 100%\n\n" +
            "PANTALLA: COMPLETA\n\n" +
            "RESOLUCION: 1920 x 1080",
            14, new Color(0.3f, 0.2f, 0.1f), TextAlignmentOptions.Center, FontStyles.Normal, pixelFont);

        var btnSetBackGO = MakeChild("BtnSettingsBack", panelSettings);
        SetAnchored(btnSetBackGO, new Vector2(0.5f, 0.14f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250, 56));
        var btnSetBack = MakeWoodButton(btnSetBackGO, "< VOLVER", 16);

        // 9. PanelHelp (Modal Cómo Jugar)
        var panelHelp = MakeChild("PanelHelp", canvasGO);
        SetAnchored(panelHelp, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(780, 560));
        MakeImage(panelHelp, panelSprite, Image.Type.Sliced);
        panelHelp.SetActive(false);

        var helpHeaderGO = MakeChild("Header", panelHelp);
        SetAnchored(helpHeaderGO, new Vector2(0.5f, 0.90f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 50));
        MakeTMP(helpHeaderGO, "COMO JUGAR", 24, new Color(0.25f, 0.15f, 0.08f), TextAlignmentOptions.Center, FontStyles.Bold, pixelFont);

        var helpContentGO = MakeChild("Content", panelHelp);
        SetAnchored(helpContentGO, new Vector2(0.5f, 0.53f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680, 310));
        MakeTMP(helpContentGO,
            "¡KIMAYA: EL BOSQUE DEL ECO!\n\n" +
            "- MOVIMIENTO: Teclas A / D o FLECHAS\n" +
            "- SALTAR: ESPACIO o tecla W / ARRIBA\n" +
            "- BALANCINES: Inclinan segun tu peso\n" +
            "- CAJAS: Empujalas sobre las placas\n" +
            "- PUERTAS: Activa runas verdes\n" +
            "- SPEEDRUN: ¡Llega a la meta rapido!",
            13, new Color(0.26f, 0.17f, 0.1f), TextAlignmentOptions.TopLeft, FontStyles.Normal, pixelFont);

        var btnHelpBackGO = MakeChild("BtnHelpBack", panelHelp);
        SetAnchored(btnHelpBackGO, new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(250, 56));
        var btnHelpBack = MakeWoodButton(btnHelpBackGO, "ENTENDIDO", 16);

        // 10. MainMenuController
        var mmcGO = new GameObject("MainMenuController");
        var mmc   = mmcGO.AddComponent<MainMenuController>();
        var mmcSObj = new SerializedObject(mmc);
        mmcSObj.FindProperty("panelMain").objectReferenceValue       = panelMain;
        mmcSObj.FindProperty("panelSettings").objectReferenceValue   = panelSettings;
        mmcSObj.FindProperty("panelHelp").objectReferenceValue       = panelHelp;
        mmcSObj.FindProperty("btnPlay").objectReferenceValue         = btnPlay;
        mmcSObj.FindProperty("btnSettings").objectReferenceValue     = btnSettings;
        mmcSObj.FindProperty("btnHelp").objectReferenceValue         = btnHelp;
        mmcSObj.FindProperty("btnQuit").objectReferenceValue         = btnQuit;
        mmcSObj.FindProperty("btnSettingsBack").objectReferenceValue = btnSetBack;
        mmcSObj.FindProperty("btnHelpBack").objectReferenceValue     = btnHelpBack;
        mmcSObj.ApplyModifiedProperties();

        // 11. Guardar y refrescar escena
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        AddSceneToBuildSettings(scenePath, 0);
        AddSceneToBuildSettings("Assets/Scenes/Level_01_Bosque.unity", 1);
        AddSceneToBuildSettings("Assets/Scenes/Prologue_Story.unity", 2);

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("✅ Main Menu Pixel Art",
                "¡MainMenu.unity creado con éxito!\n\n" +
                "🎮 Título: Kimaya: El Bosque del Eco\n" +
                "👾 Tipografía: Press Start 2P (Pixel Art)\n" +
                "📦 Botones: JUGAR, AJUSTES, CÓMO JUGAR, SALIR", "OK");
        }

        Debug.Log("[MainMenuBuilder] ✅ Escena MainMenu reconstruida con estilo Pixel Art en: " + scenePath);
    }

    static void AddSceneToBuildSettings(string path, int index = -1)
    {
        if (!File.Exists(path)) return;
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == path)
            {
                if (index >= 0 && i != index)
                {
                    var item = scenes[i];
                    scenes.RemoveAt(i);
                    scenes.Insert(Mathf.Min(index, scenes.Count), item);
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                return;
            }
        }

        if (index >= 0 && index <= scenes.Count)
            scenes.Insert(index, new EditorBuildSettingsScene(path, true));
        else
            scenes.Add(new EditorBuildSettingsScene(path, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
