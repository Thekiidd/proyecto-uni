using System.IO;
using Platformer.Core;
using Platformer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tools > Build MainMenu Scene
/// Crea la escena MainMenu completa con todos sus elementos de UI y scripts conectados.
/// </summary>
public static class MainMenuSceneBuilder
{
    [MenuItem("Tools/Build MainMenu Scene")]
    public static void BuildScene()
    {
        // 1 ── Crear / abrir la escena
        string scenePath = "Assets/Scenes/MainMenu.unity";
        Directory.CreateDirectory(Application.dataPath + "/../Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 2 ── Camara
        var camGO = new GameObject("Main Camera");
        var cam   = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.08f, 0.08f, 0.15f);
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10);

        // 3 ── Singletons (GameData + SceneLoader)
        var setupGO = new GameObject("[GameSetup]");
        setupGO.AddComponent<GameData>();
        setupGO.AddComponent<SceneLoader>();

        // 4 ── Canvas raiz
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem — usa el nuevo Input System
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // ─── Helpers ────────────────────────────────────────────────────────
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
        Image MakeImage(GameObject go, Color col)
        {
            var img = go.AddComponent<Image>();
            img.color = col;
            return img;
        }
        TextMeshProUGUI MakeTMP(GameObject go, string text, int size, Color col, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = col;
            t.alignment = align;
            t.fontStyle = FontStyles.Bold;
            return t;
        }
        Button MakeButton(GameObject go, string label, Color bg, Color txtCol)
        {
            MakeImage(go, bg);
            var btn = go.AddComponent<Button>();
            var cb  = btn.colors;
            cb.highlightedColor = Color.white * 1.2f;
            cb.pressedColor     = bg * 0.7f;
            btn.colors = cb;
            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(go.transform, false);
            var r = txtGO.AddComponent<RectTransform>();
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
            MakeTMP(txtGO, label, 36, txtCol);
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

        // ─── 5 Fondo ────────────────────────────────────────────────────────
        var bgGO = MakeChild("Background", canvasGO);
        Stretch(bgGO);
        MakeImage(bgGO, new Color(0.08f, 0.08f, 0.15f));

        // Degradado decorativo
        var gradGO = MakeChild("GradientTop", bgGO);
        var gradR  = RT(gradGO);
        gradR.anchorMin = new Vector2(0, 0.6f); gradR.anchorMax = Vector2.one;
        gradR.offsetMin = Vector2.zero;         gradR.offsetMax = Vector2.zero;
        MakeImage(gradGO, new Color(0.1f, 0.1f, 0.35f, 0.6f));

        // ─── 6 Titulo ────────────────────────────────────────────────────────
        var titleGO = MakeChild("Title", canvasGO);
        SetAnchored(titleGO, new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 120));
        MakeTMP(titleGO, "ULTIMO TRAYECTO", 72, Color.white);

        var subtitleGO = MakeChild("Subtitle", canvasGO);
        SetAnchored(subtitleGO, new Vector2(0.5f, 0.77f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600, 50));
        MakeTMP(subtitleGO, "Un juego de plataformas", 28, new Color(0.7f, 0.9f, 1f));

        // ─── 7 PanelMain ────────────────────────────────────────────────────
        var panelMain = MakeChild("PanelMain", canvasGO);
        SetAnchored(panelMain, new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400, 260));

        var btnPlayGO = MakeChild("BtnPlay", panelMain);
        SetAnchored(btnPlayGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(320, 70));
        var btnPlay = MakeButton(btnPlayGO, "JUGAR", new Color(0.2f, 0.7f, 0.3f), Color.white);

        var btnQuitGO = MakeChild("BtnQuit", panelMain);
        SetAnchored(btnQuitGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -100), new Vector2(320, 70));
        var btnQuit = MakeButton(btnQuitGO, "SALIR", new Color(0.7f, 0.2f, 0.2f), Color.white);

        var versionGO = MakeChild("Version", panelMain);
        SetAnchored(versionGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -195), new Vector2(320, 40));
        MakeTMP(versionGO, "v1.0", 22, new Color(1,1,1,0.4f));

        // ─── 8 PanelCharacterSelect ─────────────────────────────────────────
        var panelChar = MakeChild("PanelCharacterSelect", canvasGO);
        SetAnchored(panelChar, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 550));
        MakeImage(panelChar, new Color(0, 0, 0, 0.7f));
        panelChar.SetActive(false);

        // Titulo selector
        var selTitleGO = MakeChild("SelectTitle", panelChar);
        SetAnchored(selTitleGO, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(600, 60));
        MakeTMP(selTitleGO, "Elige tu personaje", 40, Color.white);

        // Retrato
        var portraitGO = MakeChild("Portrait", panelChar);
        SetAnchored(portraitGO, new Vector2(0.5f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(160, 160));
        var portraitImg = MakeImage(portraitGO, new Color(0.3f, 0.3f, 0.8f));

        // Nombre personaje
        var charNameGO = MakeChild("CharName", panelChar);
        SetAnchored(charNameGO, new Vector2(0.5f, 0.36f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 50));
        MakeTMP(charNameGO, "Runner", 42, Color.yellow);

        // Descripcion
        var descGO = MakeChild("Description", panelChar);
        SetAnchored(descGO, new Vector2(0.5f, 0.28f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 45));
        var descTMP = MakeTMP(descGO, "Veloz y agil.", 24, new Color(0.8f,0.8f,0.8f));
        descTMP.fontStyle = FontStyles.Normal;

        // Stats
        var statsGO = MakeChild("Stats", panelChar);
        SetAnchored(statsGO, new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 90));
        var statsTMP = MakeTMP(statsGO, "Velocidad: 10  |  Salto: 14  |  Vidas: 3", 22, new Color(0.7f,1f,0.7f));
        statsTMP.fontStyle = FontStyles.Normal;

        // Especial
        var specialGO = MakeChild("Special", panelChar);
        SetAnchored(specialGO, new Vector2(0.5f, 0.10f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500, 40));
        MakeTMP(specialGO, "Especial: Ninguna", 22, new Color(1f,0.8f,0.2f));

        // Botones nav
        var btnLeftGO = MakeChild("BtnLeft", panelChar);
        SetAnchored(btnLeftGO, new Vector2(0.05f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
        var btnLeft = MakeButton(btnLeftGO, "◄", new Color(0.3f, 0.3f, 0.3f), Color.white);

        var btnRightGO = MakeChild("BtnRight", panelChar);
        SetAnchored(btnRightGO, new Vector2(0.95f, 0.65f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
        var btnRight = MakeButton(btnRightGO, "►", new Color(0.3f, 0.3f, 0.3f), Color.white);

        // Botón iniciar
        var btnStartGO = MakeChild("BtnStart", panelChar);
        SetAnchored(btnStartGO, new Vector2(0.65f, 0.02f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(260, 65));
        var btnStart = MakeButton(btnStartGO, "¡JUGAR!", new Color(0.15f, 0.6f, 0.25f), Color.white);

        // Botón atrás
        var btnBackGO = MakeChild("BtnBack", panelChar);
        SetAnchored(btnBackGO, new Vector2(0.32f, 0.02f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(200, 65));
        var btnBack = MakeButton(btnBackGO, "← Atrás", new Color(0.4f, 0.4f, 0.4f), Color.white);

        // ─── 9 CharacterSelectUI component ──────────────────────────────────
        var charSelectComp = panelChar.AddComponent<CharacterSelectUI>();
        // Asignar referencias via SerializedObject
        var sObj = new SerializedObject(charSelectComp);
        sObj.FindProperty("portraitImage").objectReferenceValue  = portraitImg;
        sObj.FindProperty("nameText").objectReferenceValue       = charNameGO.GetComponent<TextMeshProUGUI>();
        sObj.FindProperty("descriptionText").objectReferenceValue = descTMP;
        sObj.FindProperty("specialText").objectReferenceValue    = specialGO.GetComponent<TextMeshProUGUI>();
        sObj.FindProperty("btnLeft").objectReferenceValue        = btnLeft;
        sObj.FindProperty("btnRight").objectReferenceValue       = btnRight;
        sObj.ApplyModifiedProperties();

        // ─── 10 MainMenuController ───────────────────────────────────────────
        var mmcGO   = new GameObject("MainMenuController");
        var mmc     = mmcGO.AddComponent<MainMenuController>();
        var mmcSObj = new SerializedObject(mmc);
        mmcSObj.FindProperty("panelMain").objectReferenceValue           = panelMain;
        mmcSObj.FindProperty("panelCharacterSelect").objectReferenceValue = panelChar;
        mmcSObj.FindProperty("btnPlay").objectReferenceValue             = btnPlay;
        mmcSObj.FindProperty("btnQuit").objectReferenceValue             = btnQuit;
        mmcSObj.FindProperty("btnStart").objectReferenceValue            = btnStart;
        mmcSObj.FindProperty("btnBack").objectReferenceValue             = btnBack;
        mmcSObj.FindProperty("characterSelectUI").objectReferenceValue   = charSelectComp;
        mmcSObj.ApplyModifiedProperties();

        // ─── 11 Guardar escena ───────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        // Agregar a Build Settings
        AddSceneToBuildSettings(scenePath);
        AddSceneToBuildSettings("Assets/Scenes/Level_01.unity");
        AddSceneToBuildSettings("Assets/Scenes/Level_02.unity");
        AddSceneToBuildSettings("Assets/Scenes/Level_03.unity");

        Debug.Log("[MainMenuBuilder] Escena MainMenu creada en: " + scenePath);
        EditorUtility.DisplayDialog("✅ Listo",
            "Escena MainMenu creada correctamente.\n\n" +
            "SIGUIENTE PASO:\n" +
            "1. Asigna los 3 CharacterData en CharacterSelectUI > Characters\n" +
            "2. Duplica Level_01 para crear Level_02 y Level_03",
            "Entendido");
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
