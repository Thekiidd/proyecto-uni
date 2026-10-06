using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena Ending de Kimaya:
    /// Las tres hermanas reunidas, el círculo de hadas se ilumina parcialmente.
    /// </summary>
    public static class KimayaEndingBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Ending.unity";
        private const string FALIN_SPR  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-idle.png";
        private const string MIA_SPR    = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-Idle.png";
        private const string KIARA_SPR  = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-sitting.png";

        [MenuItem("Tools/Kimaya/Build Ending Scene")]
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
            cam.backgroundColor = new Color(0.05f, 0.1f, 0.08f);
            camGo.AddComponent<AudioListener>();

            // Canvas
            var canvasGo = new GameObject("Canvas_Ending");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Fondo del bosque mágico
            CreateImage(canvasGo.transform, "BG", new Color(0.04f, 0.1f, 0.06f),
                Vector2.zero, new Vector2(1920, 1080), Vector2.zero, Vector2.one);

            // Círculo mágico de hadas (decorativo)
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                float r = 280f;
                var fairy = new GameObject($"Hada_{i}");
                fairy.transform.SetParent(canvasGo.transform, false);
                var img = fairy.AddComponent<Image>();
                img.color = new Color(0.6f + i * 0.03f, 1f, 0.4f, 0.6f);
                var rt = fairy.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r + 50f);
                rt.sizeDelta = new Vector2(18f, 18f);
            }

            // Tres perritas en el centro
            Sprite falinSpr = LoadSprite(FALIN_SPR);
            Sprite miaSpr   = LoadSprite(MIA_SPR);
            Sprite kiaraSpr = LoadSprite(KIARA_SPR);

            CreateSpriteImage(canvasGo.transform, "Kiara_Center", kiaraSpr,
                new Color(0.75f, 0.65f, 0.95f), new Vector2(0, -80), new Vector2(220, 220));
            CreateSpriteImage(canvasGo.transform, "Falin_Left", falinSpr,
                new Color(0.9f, 0.75f, 0.3f), new Vector2(-280, -120), new Vector2(190, 190));
            CreateSpriteImage(canvasGo.transform, "Mia_Right", miaSpr,
                new Color(0.5f, 0.85f, 1f), new Vector2(280, -120), new Vector2(160, 160));

            // Título
            CreateText(canvasGo.transform, "Titulo", "¡Reunidas!",
                new Vector2(0, 380), new Vector2(700, 100),
                new Color(0.7f, 1f, 0.6f), 72, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Texto narrativo (en 3 partes)
            CreateText(canvasGo.transform, "Texto_1",
                "Las raíces del Zarzal Sombrío se debilitaron.",
                new Vector2(0, 270), new Vector2(900, 50),
                new Color(0.85f, 0.95f, 0.8f), 26, FontStyle.Normal, TextAnchor.MiddleCenter);

            CreateText(canvasGo.transform, "Texto_2",
                "Kiara quedó libre. Las tres hermanas se reunieron.",
                new Vector2(0, 220), new Vector2(900, 50),
                new Color(0.85f, 0.95f, 0.8f), 26, FontStyle.Normal, TextAnchor.MiddleCenter);

            CreateText(canvasGo.transform, "Texto_3",
                "\"El círculo de hadas vuelve a brillar...\n pero el camino a casa aún no está abierto.\"\n — Maya",
                new Vector2(0, 140), new Vector2(900, 90),
                new Color(0.75f, 0.6f, 1f, 0.9f), 22, FontStyle.Italic, TextAnchor.MiddleCenter);

            CreateText(canvasGo.transform, "Texto_Final",
                "Por ahora, las tres están juntas.\nEso es suficiente.",
                new Vector2(0, 55), new Vector2(800, 70),
                new Color(0.9f, 0.95f, 0.85f, 0.8f), 24, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Créditos
            CreateText(canvasGo.transform, "Creditos",
                "Kimaya: El Bosque del Eco\nCedillo · Baca · García · Batista · Mora · Santoyo\nUTCH IDGS102N  —  2026",
                new Vector2(0, -330), new Vector2(900, 80),
                new Color(0.5f, 0.65f, 0.5f, 0.6f), 16, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Botones
            CreateNavButton(canvasGo.transform, "Btn_Menu", "◀  Menú Principal",
                new Vector2(-160, -410), new Vector2(280, 55), new Color(0.15f, 0.4f, 0.15f));
            CreateNavButton(canvasGo.transform, "Btn_Rejugar", "↺  Jugar de Nuevo",
                new Vector2(160, -410), new Vector2(280, 55), new Color(0.1f, 0.3f, 0.35f));

            // Controlador
            var ctrl = new GameObject("EndingController");
            ctrl.AddComponent<KimayaEndingController>();

            string dir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ Ending creada",
                "Escena 'Ending.unity' construida.\n\n" +
                "Muestra a las tres perritas reunidas\n" +
                "con el círculo de hadas brillando.\n\n" +
                "Botones: Menú Principal / Jugar de Nuevo", "OK");
        }

        private static void CreateImage(Transform parent, string name, Color color,
            Vector2 pos, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = color;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateSpriteImage(Transform parent, string name, Sprite spr,
            Color color, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = spr; img.color = color; img.preserveAspect = true;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateText(Transform parent, string name, string content,
            Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = content; t.color = color; t.fontSize = fontSize;
            t.fontStyle = style; t.alignment = anchor;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static void CreateNavButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color bgColor)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>(); img.color = bgColor;
            go.AddComponent<Button>();
            var textGo = new GameObject("Label"); textGo.transform.SetParent(go.transform, false);
            var t = textGo.AddComponent<Text>();
            t.text = label; t.color = Color.white; t.fontSize = 20;
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
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
