using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye el Main Menu SENCILLO de Kimaya:
    ///   - Fondo de bosque con color
    ///   - Título del juego
    ///   - Un solo botón PLAY
    ///   - Nombres del equipo abajo
    /// Tools > Kimaya > [1] Build Main Menu Simple
    /// </summary>
    public static class KimayaSimpleMenuBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Tools/Kimaya/[1] Build Main Menu Simple")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic    = true;
            cam.orthographicSize = 5f;
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.1f, 0.04f);
            camGo.AddComponent<AudioListener>();

            BuildBackground();

            // Canvas
            var cvGo = new GameObject("Canvas");
            var cv   = cvGo.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cvGo.AddComponent<CanvasScaler>();
            sc.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            cvGo.AddComponent<GraphicRaycaster>();

            // Fondo semi-oscuro sobre el canvas
            CreatePanel(cvGo.transform, "Overlay",
                new Color(0f, 0f, 0f, 0.45f),
                Vector2.zero, new Vector2(1920, 1080), Vector2.zero, Vector2.one);

            // Título principal
            CreateLabel(cvGo.transform, "TitleKimaya", "KIMAYA",
                new Vector2(0, 260), new Vector2(900, 160),
                new Color(0.55f, 1f, 0.45f), 110, FontStyle.Bold);

            CreateLabel(cvGo.transform, "TitleSub", "El Bosque del Eco",
                new Vector2(0, 160), new Vector2(700, 70),
                new Color(0.78f, 0.95f, 0.65f, 0.9f), 40, FontStyle.Italic);

            // Separador decorativo
            CreatePanel(cvGo.transform, "Separator",
                new Color(0.45f, 0.85f, 0.35f, 0.4f),
                new Vector2(0, 105), new Vector2(400, 3),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            // Botón PLAY — grande y centrado
            CreatePlayButton(cvGo.transform, new Vector2(0, 0));

            // Hint de controles
            CreateLabel(cvGo.transform, "HintControls",
                "TAB: cambiar personaje   |   WASD: Falin   |   ←↑→: Mia   |   SHIFT+W: Salto fuerte   |   Q: Olfato   |   ESC: Pausa",
                new Vector2(0, -220), new Vector2(1200, 35),
                new Color(0.7f, 0.85f, 0.6f, 0.45f), 15, FontStyle.Normal);

            // Créditos equipo
            CreateLabel(cvGo.transform, "Team",
                "Cedillo · Baca · García · Batista · Mora · Santoyo     UTCH — IDGS102N — 2026",
                new Vector2(0, -295), new Vector2(1000, 32),
                new Color(0.55f, 0.68f, 0.52f, 0.5f), 16, FontStyle.Normal);

            // Controlador del menú
            var mgrGo = new GameObject("MenuController");
            mgrGo.AddComponent<SimpleMenuController>();

            SaveScene(scene, SCENE_PATH, 0);

            EditorUtility.DisplayDialog("✅ Main Menu Simple creado",
                "MainMenu.unity listo.\n\n" +
                "El botón PLAY carga 'Historia' (o Level_01_Bosque si\n" +
                "Historia aún no existe en Build Settings).\n\n" +
                "Siguiente: Tools > Kimaya > [2] Build Historia Placeholder", "OK");
        }

        private static void BuildBackground()
        {
            // Capas de fondo estilo bosque nocturno
            (Color color, float y, float scaleX, float scaleY, int order)[] layers = {
                (new Color(0.04f, 0.10f, 0.04f), 0f,   22f, 12f, -100),
                (new Color(0.06f, 0.14f, 0.05f), -1f,  20f,  6f,  -90),
                (new Color(0.08f, 0.18f, 0.06f), -2f,  18f,  5f,  -80),
                (new Color(0.10f, 0.22f, 0.07f), -3f,  18f,  4f,  -70),
            };

            foreach (var (color, y, sx, sy, order) in layers)
            {
                var go = new GameObject($"BG_Layer_{-order}");
                go.transform.position   = new Vector3(0, y, 0);
                go.transform.localScale = new Vector3(sx, sy, 1f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.color = color; sr.sortingOrder = order;
            }

            // Árboles laterales
            float[] txArr = { -9f, -7f, -5f, 5f, 7f, 9f };
            float[] thArr = { 7f,   5f,  6f, 6f, 5f, 7f };
            for (int i = 0; i < txArr.Length; i++)
            {
                var t = new GameObject($"Tree_{i}");
                t.transform.position   = new Vector3(txArr[i], thArr[i] / 2f - 4f, 0.3f);
                t.transform.localScale = new Vector3(1.6f, thArr[i], 1f);
                var sr = t.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.07f, 0.19f, 0.05f, 0.8f + i * 0.03f);
                sr.sortingOrder = -60 + i;
            }

            // Lucecitas de hadas flotantes
            for (int i = 0; i < 10; i++)
            {
                var f = new GameObject($"Fairy_{i}");
                float fx = -7f + i * 1.55f;
                float fy = -1.5f + (i % 3) * 1.2f;
                f.transform.position   = new Vector3(fx, fy, -0.5f);
                f.transform.localScale = Vector3.one * 0.06f;
                var sr = f.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.5f + i * 0.04f, 1f, 0.4f + i * 0.05f, 0.75f);
                sr.sortingOrder = -45;
            }
        }

        private static void CreatePanel(Transform parent, string name, Color color,
            Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = color;
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateLabel(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var t  = go.AddComponent<Text>();
            t.text = text; t.color = color; t.fontSize = fontSize;
            t.fontStyle = style; t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreatePlayButton(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Btn_Play"); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.6f, 0.18f);
            var btn = go.AddComponent<Button>();
            var cols = btn.colors;
            cols.normalColor      = new Color(0.18f, 0.6f, 0.18f);
            cols.highlightedColor = new Color(0.25f, 0.78f, 0.25f);
            cols.pressedColor     = new Color(0.12f, 0.42f, 0.12f);
            btn.colors = cols;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one * 0.5f;
            rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(380, 90);

            var lblGo = new GameObject("Label"); lblGo.transform.SetParent(go.transform, false);
            var lbl   = lblGo.AddComponent<Text>();
            lbl.text = "▶   JUGAR"; lbl.color = Color.white;
            lbl.fontSize = 42; lbl.alignment = TextAnchor.MiddleCenter; lbl.fontStyle = FontStyle.Bold;
            lbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var lrt = lblGo.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path, int index)
        {
            string dir = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, path);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            for (int i = 0; i < list.Count; i++) if (list[i].path == path) { found = true; break; }
            if (!found) { list.Insert(Mathf.Min(index, list.Count), new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = list.ToArray(); }
            AssetDatabase.Refresh();
        }
    }
}
