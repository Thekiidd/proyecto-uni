using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena de Introducción (cutscene) de Kimaya.
    /// Muestra la historia con paneles estilo novela visual:
    ///   Panel 1 — El paseo matutino
    ///   Panel 2 — La pelota cae en el bosque
    ///   Panel 3 — Las perritas entran al círculo de hongos
    ///   Panel 4 — Caen al Bosque del Eco
    ///   Panel 5 — Kiara es capturada por el Zarzal
    ///   Panel 6 — Maya aparece y explica
    /// </summary>
    public static class KimayaIntroBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Intro.unity";
        private const string FALIN_SPR  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-run.png";
        private const string MIA_SPR    = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-run.png";
        private const string KIARA_SPR  = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-Idle.png";

        [MenuItem("Tools/Kimaya/Build Intro Cutscene")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.07f, 0.03f);
            camGo.AddComponent<AudioListener>();

            // Canvas
            var canvasGo = new GameObject("Canvas_Intro");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Fondo negro full screen
            CreateImage(canvasGo.transform, "Background", Color.black, Vector2.zero,
                new Vector2(1920, 1080), Vector2.zero, Vector2.one);

            // Panel de imagen de escena (izquierda)
            CreateImage(canvasGo.transform, "ScenePanel", new Color(0.08f, 0.15f, 0.07f),
                new Vector2(-480, 0), new Vector2(900, 700), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // Personajes en el panel
            Sprite falinSpr = LoadSprite(FALIN_SPR);
            Sprite miaSpr   = LoadSprite(MIA_SPR);
            Sprite kiaraSpr = LoadSprite(KIARA_SPR);

            CreateSpriteImage(canvasGo.transform, "Falin_Panel", falinSpr,
                new Color(0.9f, 0.7f, 0.3f), new Vector2(-650, -150), new Vector2(200, 200));
            CreateSpriteImage(canvasGo.transform, "Mia_Panel", miaSpr,
                new Color(0.6f, 0.85f, 1f), new Vector2(-480, -160), new Vector2(150, 150));
            CreateSpriteImage(canvasGo.transform, "Kiara_Panel", kiaraSpr,
                new Color(0.7f, 0.6f, 0.9f), new Vector2(-310, -155), new Vector2(170, 170));

            // Panel de diálogo (derecha)
            CreateImage(canvasGo.transform, "DialoguePanel", new Color(0.04f, 0.08f, 0.04f, 0.95f),
                new Vector2(480, 0), new Vector2(820, 700), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // Número de panel
            CreateText(canvasGo.transform, "PanelNum", "1 / 6",
                new Vector2(480, 295), new Vector2(200, 40),
                new Color(0.5f, 0.7f, 0.5f, 0.6f), 18, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Título del panel
            CreateText(canvasGo.transform, "PanelTitle", "El Paseo Matutino",
                new Vector2(480, 240), new Vector2(760, 60),
                new Color(0.6f, 1f, 0.5f), 28, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Texto del panel
            string firstText =
                "Era una mañana perfecta.\n\n" +
                "Falin, Mia y Kiara salieron a pasear con su familia\n" +
                "por los campos cerca del bosque.\n\n" +
                "Falin corría adelante, como siempre.\n" +
                "Mia olfateaba cada flor del camino.\n" +
                "Kiara las vigilaba con ojo atento.";

            CreateText(canvasGo.transform, "PanelText", firstText,
                new Vector2(480, 30), new Vector2(760, 420),
                new Color(0.9f, 0.95f, 0.88f), 22, FontStyle.Normal, TextAnchor.MiddleLeft);

            // Botones de navegación
            CreateNavButton(canvasGo.transform, "Btn_Continuar", "Continuar  ▶",
                new Vector2(480, -270), new Vector2(220, 55), new Color(0.15f, 0.45f, 0.15f));
            CreateNavButton(canvasGo.transform, "Btn_Saltar", "Saltar intro",
                new Vector2(480, -330), new Vector2(180, 40), new Color(0.3f, 0.3f, 0.3f, 0.7f),
                fontSize: 16);

            // Controlador de paneles
            var ctrlGo = new GameObject("IntroController");
            ctrlGo.AddComponent<KimayaIntroController>();

            string dir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ Intro creada",
                "Escena 'Intro.unity' construida.\n\n" +
                "El controlador KimayaIntroController maneja\n" +
                "los 6 paneles de la historia con:\n" +
                "  • Espacio / E / Click → siguiente panel\n" +
                "  • Skip → salta directo a Level_01_Bosque", "OK");
        }

        // ── UI Helpers ─────────────────────────────────────────────────────
        private static void CreateImage(Transform parent, string name, Color color,
            Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateSpriteImage(Transform parent, string name, Sprite spr,
            Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = spr; img.color = color;
            img.preserveAspect = true;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateText(Transform parent, string name, string content,
            Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content; t.color = color; t.fontSize = fontSize;
            t.fontStyle = style; t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateNavButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color bgColor, int fontSize = 20)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = bgColor;
            go.AddComponent<Button>();
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.AddComponent<Text>();
            t.text = label; t.color = Color.white; t.fontSize = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tRT = textGo.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = tRT.offsetMax = Vector2.zero;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite sp) return sp;
            return null;
        }

        private static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in list) if (s.path == path) return;
            list.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
