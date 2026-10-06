using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena Historia — placeholder para los dibujos hechos a mano.
    /// Estructura: panel izquierdo = DIBUJO (o nota de placeholder)
    ///             panel derecho  = TEXTO narrativo con typewriter
    /// Tools > Kimaya > [2] Build Historia (Placeholder)
    /// </summary>
    public static class KimayaHistoriaBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Historia.unity";

        [MenuItem("Tools/Kimaya/[2] Build Historia (Placeholder)")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0, 0, -10);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true; cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.1f, 0.04f);
            camGo.AddComponent<AudioListener>();

            // Canvas
            var cvGo = new GameObject("Canvas"); 
            var cv = cvGo.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cvGo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            cvGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Fondo principal (cambia de color con cada panel)
            CreateImg(cvGo.transform, "Story_MainBG", new Color(0.04f, 0.08f, 0.04f),
                Vector2.zero, Vector2.zero, Vector2.one, Vector2.zero);

            // Panel IZQUIERDO — zona del dibujo
            CreateImg(cvGo.transform, "Story_BgPanel", new Color(0.08f, 0.18f, 0.07f),
                new Vector2(-480, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(880, 980));

            // Zona del DIBUJO (imagen placeholder)
            CreateImg(cvGo.transform, "Story_DrawingArea", new Color(0.06f, 0.14f, 0.06f),
                new Vector2(-480, 60), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(820, 750));

            // Nota de dónde va el dibujo
            CreateTxt(cvGo.transform, "Story_ImageNote",
                "🎨 [Dibujo: las 3 perritas corriendo por un campo soleado]",
                new Vector2(-480, 60), new Vector2(780, 700),
                new Color(0.6f, 0.85f, 0.55f, 0.7f), 22, FontStyle.Italic);

            // Número de panel (esquina del dibujo)
            CreateTxt(cvGo.transform, "Story_Counter", "1  /  6",
                new Vector2(-480, -440), new Vector2(300, 40),
                new Color(0.5f, 0.7f, 0.45f, 0.6f), 18, FontStyle.Normal);

            // Panel DERECHO — texto
            CreateImg(cvGo.transform, "TextPanel", new Color(0.03f, 0.07f, 0.03f, 0.92f),
                new Vector2(490, 0), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(880, 980));

            // Título del panel
            CreateTxt(cvGo.transform, "Story_Title", "Una mañana de paseo",
                new Vector2(490, 400), new Vector2(820, 80),
                new Color(0.6f, 1f, 0.45f), 36, FontStyle.Bold);

            // Línea separadora
            CreateImg(cvGo.transform, "TitleLine", new Color(0.4f, 0.8f, 0.3f, 0.4f),
                new Vector2(490, 350), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(760, 2));

            // Cuerpo del texto
            CreateTxt(cvGo.transform, "Story_Body", "",
                new Vector2(490, 70), new Vector2(800, 540),
                new Color(0.9f, 0.96f, 0.87f), 26, FontStyle.Normal);

            // Hint de controles
            CreateTxt(cvGo.transform, "Story_Hint",
                "[ ESPACIO / E ] Continuar",
                new Vector2(490, -390), new Vector2(500, 38),
                new Color(0.5f, 0.7f, 0.45f, 0.55f), 18, FontStyle.Normal);

            // Botón SKIP
            var skipGo = new GameObject("Btn_Skip"); skipGo.transform.SetParent(cvGo.transform, false);
            var skipImg = skipGo.AddComponent<Image>(); skipImg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
            skipGo.AddComponent<Button>();
            var skipLblGo = new GameObject("Label"); skipLblGo.transform.SetParent(skipGo.transform, false);
            var skipLbl = skipLblGo.AddComponent<Text>();
            skipLbl.text = "Saltar ▶▶"; skipLbl.color = new Color(0.7f, 0.8f, 0.65f);
            skipLbl.fontSize = 18; skipLbl.alignment = TextAnchor.MiddleCenter;
            skipLbl.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var skipLblRT = skipLblGo.GetComponent<RectTransform>();
            skipLblRT.anchorMin = Vector2.zero; skipLblRT.anchorMax = Vector2.one;
            skipLblRT.offsetMin = skipLblRT.offsetMax = Vector2.zero;
            var skipRT = skipGo.GetComponent<RectTransform>();
            skipRT.anchorMin = skipRT.anchorMax = skipRT.pivot = new Vector2(1f, 0f);
            skipRT.anchoredPosition = new Vector2(-20, 20); skipRT.sizeDelta = new Vector2(160, 45);

            // Controlador
            var ctrlGo = new GameObject("HistoriaController");
            ctrlGo.AddComponent<HistoriaController>();

            SaveScene(scene, SCENE_PATH, 1);

            EditorUtility.DisplayDialog("✅ Historia (Placeholder) creada",
                "Historia.unity lista.\n\n" +
                "📌 Cuando lleguen los dibujos hechos a mano:\n" +
                "  1. Importa las imágenes a Assets/Sprites/Historia/\n" +
                "  2. Arrastra la imagen al componente Image de 'Story_DrawingArea'\n" +
                "  3. Cambia 'Story_ImageNote' a vacío\n\n" +
                "Por ahora muestra texto + nota de qué dibujo va ahí.", "OK");
        }

        private static void CreateImg(Transform parent, string name, Color color,
            Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateTxt(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text; t.color = color; t.fontSize = fontSize;
            t.fontStyle = style; t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path, int idx)
        {
            string dir = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, path);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in list) if (s.path == path) { found = true; break; }
            if (!found) { list.Insert(Mathf.Min(idx, list.Count), new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = list.ToArray(); }
            AssetDatabase.Refresh();
        }
    }
}
