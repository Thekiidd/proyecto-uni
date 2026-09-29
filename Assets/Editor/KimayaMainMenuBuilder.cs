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
    /// Actualiza el Main Menu al estilo de Kimaya: El Bosque del Eco.
    /// Menú: Tools > Kimaya > Build Main Menu
    /// </summary>
    public static class KimayaMainMenuBuilder
    {
        private const string SCENE_PATH  = "Assets/Scenes/MainMenu.unity";
        private const string FALIN_SPR   = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-idle.png";
        private const string MIA_SPR     = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-Idle.png";

        [MenuItem("Tools/Kimaya/Build Main Menu")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara ─────────────────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 5f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.05f, 0.12f, 0.05f);
            camGo.AddComponent<AudioListener>();

            // ── Fondo del bosque ───────────────────────────────────────────
            BuildBackground();

            // ── UI Canvas ──────────────────────────────────────────────────
            BuildMenuCanvas();

            // ── Perritas decorativas ───────────────────────────────────────
            BuildDecorativeDogs();

            // ── Guardar ────────────────────────────────────────────────────
            string dir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddToBuildSettings(SCENE_PATH, 0);
            AddToBuildSettings("Assets/Scenes/Level_01_Bosque.unity", 1);
            AddToBuildSettings("Assets/Scenes/Ending.unity", 2);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ Main Menu Kimaya",
                "Menú principal de 'Kimaya: El Bosque del Eco' creado.\n\n" +
                "Asegúrate de que estas escenas existan en Build Settings:\n" +
                "  0 - MainMenu\n  1 - Level_01_Bosque\n  2 - Ending\n\n" +
                "Tools > Kimaya > Build Level 01 - Bosque del Eco\npara crear el nivel.", "OK");
        }

        private static void BuildBackground()
        {
            // Capas de fondo con colores tipo bosque mágico
            string[] bgNames = { "BG_Sky", "BG_Forest_Far", "BG_Forest_Mid", "BG_Forest_Near", "BG_Ground" };
            Color[] bgColors = {
                new Color(0.05f, 0.08f, 0.05f),
                new Color(0.08f, 0.18f, 0.07f, 0.8f),
                new Color(0.1f,  0.22f, 0.09f, 0.85f),
                new Color(0.12f, 0.28f, 0.1f,  0.9f),
                new Color(0.15f, 0.32f, 0.1f)
            };
            float[] yPos   = { 0f, 1f, -0.5f, -1.5f, -4.5f };
            float[] scales = { 20f, 18f, 16f, 14f, 20f };
            int[] orders   = { -100, -90, -80, -70, -60 };

            for (int i = 0; i < bgNames.Length; i++)
            {
                var go = new GameObject(bgNames[i]);
                go.transform.position   = new Vector3(0f, yPos[i], i * 0.1f);
                go.transform.localScale = new Vector3(scales[i], 6f, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.color        = bgColors[i];
                sr.sortingOrder = orders[i];
            }

            // Árboles de fondo decorativos
            float[] treeX = { -8f, -5f, -2f, 2f, 5f, 8f };
            float[] treeH = { 6f, 4.5f, 7f, 5f, 6.5f, 4f };
            for (int i = 0; i < treeX.Length; i++)
            {
                var tree = new GameObject($"Tree_BG_{i}");
                tree.transform.position   = new Vector3(treeX[i], treeH[i] / 2f - 3f, 0.5f);
                tree.transform.localScale = new Vector3(1.5f, treeH[i], 1f);
                var sr = tree.AddComponent<SpriteRenderer>();
                sr.color        = new Color(0.1f, 0.25f, 0.08f, 0.7f + i * 0.04f);
                sr.sortingOrder = -55 + i;
            }

            // Partículas de luciérnagas (esferas brillantes)
            for (int i = 0; i < 8; i++)
            {
                var firefly = new GameObject($"Luciernaga_{i}");
                firefly.transform.position = new Vector3(
                    Random.Range(-8f, 8f), Random.Range(-2f, 3f), -0.5f);
                firefly.transform.localScale = Vector3.one * 0.08f;
                var sr = firefly.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.6f + i * 0.05f, 1f, 0.4f, 0.8f);
                sr.sortingOrder = -40;
            }
        }

        private static void BuildMenuCanvas()
        {
            var canvasGo = new GameObject("Canvas_Menu");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Título del juego
            CreateText(canvasGo.transform, "Titulo_Kimaya", "KIMAYA",
                new Vector2(0, 220), new Vector2(700, 120),
                new Color(0.7f, 1f, 0.6f), 80, FontStyle.Bold, TextAnchor.MiddleCenter);

            CreateText(canvasGo.transform, "Subtitulo", "El Bosque del Eco",
                new Vector2(0, 145), new Vector2(500, 50),
                new Color(0.8f, 0.9f, 0.6f, 0.85f), 32, FontStyle.Italic, TextAnchor.MiddleCenter);

            // Créditos del equipo
            CreateText(canvasGo.transform, "Equipo",
                "Cedillo · Baca · García · Batista · Mora · Santoyo  |  UTCH IDGS102N",
                new Vector2(0, 88), new Vector2(800, 30),
                new Color(0.7f, 0.8f, 0.6f, 0.6f), 16, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Botones
            CreateButton(canvasGo.transform, "Btn_Jugar",   "▶  JUGAR",
                new Vector2(0, -20), new Vector2(320, 65),
                new Color(0.2f, 0.65f, 0.25f), new Color(0.95f, 0.95f, 0.9f), 28, "Level_01_Bosque");

            CreateButton(canvasGo.transform, "Btn_Creditos", "★  CRÉDITOS",
                new Vector2(0, -105), new Vector2(280, 55),
                new Color(0.15f, 0.45f, 0.2f), new Color(0.8f, 0.95f, 0.7f), 22, null);

            CreateButton(canvasGo.transform, "Btn_Salir",  "✕  SALIR",
                new Vector2(0, -175), new Vector2(220, 50),
                new Color(0.5f, 0.15f, 0.1f), new Color(1f, 0.7f, 0.6f), 20, null, quit: true);

            // Controles hint
            CreateText(canvasGo.transform, "Controles",
                "TAB: cambiar personaje  |  WASD: Falin  |  ←→↑: Mia  |  SHIFT+W: Salto potenciado  |  Q: Olfato de Mia",
                new Vector2(0, -280), new Vector2(900, 30),
                new Color(0.7f, 0.85f, 0.65f, 0.5f), 14, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Versión
            CreateText(canvasGo.transform, "Version", "v0.1 — Proyecto Integradora 2026",
                new Vector2(0, -330), new Vector2(500, 25),
                new Color(0.5f, 0.6f, 0.5f, 0.4f), 12, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private static void BuildDecorativeDogs()
        {
            Sprite falinSpr = LoadSprite(FALIN_SPR);
            Sprite miaSpr   = LoadSprite(MIA_SPR);

            // Falin a la izquierda
            if (falinSpr != null)
            {
                var go = new GameObject("Falin_Deco");
                go.transform.position   = new Vector3(-5.5f, -3f, 0f);
                go.transform.localScale = new Vector3(-2f, 2f, 1f); // flip
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = falinSpr;
                sr.color        = new Color(0.9f, 0.75f, 0.4f);
                sr.sortingOrder = 10;
            }

            // Mia a la derecha
            if (miaSpr != null)
            {
                var go = new GameObject("Mia_Deco");
                go.transform.position   = new Vector3(5.5f, -3.2f, 0f);
                go.transform.localScale = Vector3.one * 1.5f;
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite       = miaSpr;
                sr.color        = new Color(0.6f, 0.85f, 1f);
                sr.sortingOrder = 10;
            }
        }

        // ── Helpers UI ──────────────────────────────────────────────────────
        private static void CreateText(Transform parent, string name, string content,
            Vector2 anchoredPos, Vector2 size, Color color, int fontSize,
            FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text      = content;
            t.color     = color;
            t.fontSize  = fontSize;
            t.fontStyle = style;
            t.alignment = anchor;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = size;
        }

        private static void CreateButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color bgColor, Color textColor, int fontSize,
            string targetScene, bool quit = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = bgColor;
            colors.highlightedColor = bgColor * 1.25f;
            colors.pressedColor     = bgColor * 0.75f;
            btn.colors = colors;

            // Texto del botón
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var t = textGo.AddComponent<Text>();
            t.text      = label;
            t.color     = textColor;
            t.fontSize  = fontSize;
            t.alignment = TextAnchor.MiddleCenter;
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var tRT = textGo.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = tRT.offsetMax = Vector2.zero;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta        = size;

            // Listener
            if (quit)
            {
                btn.onClick.AddListener(() => Application.Quit());
            }
            else if (!string.IsNullOrEmpty(targetScene))
            {
                string scene = targetScene;
                btn.onClick.AddListener(() => SceneManager.LoadScene(scene));
            }
        }

        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) { var all = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in all) if (a is Sprite sp) return sp; }
            return s;
        }

        private static void AddToBuildSettings(string path, int index)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in list) if (s.path == path) return;
            if (index < list.Count) list.Insert(index, new EditorBuildSettingsScene(path, true));
            else list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
