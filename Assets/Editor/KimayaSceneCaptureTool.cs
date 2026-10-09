using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class KimayaCaptureTrigger
    {
        static KimayaCaptureTrigger()
        {
            EditorApplication.delayCall += () =>
            {
                string triggerFile = "trigger_capture.txt";
                if (File.Exists(triggerFile))
                {
                    try { File.Delete(triggerFile); } catch { }
                    Debug.Log("[KimayaCaptureTrigger] Trigger detectado, iniciando captura de escenas...");
                    KimayaSceneCaptureTool.CaptureAllScreenshots();
                }
            };
        }
    }

    /// <summary>
    /// Herramienta para tomar capturas de pantalla de alta resolución (1920x1080)
    /// y secuencias de fotogramas de todas las escenas de Kimaya.
    /// Menú: Tools > Kimaya > Capturar Screenshots y Videos
    /// </summary>
    public static class KimayaSceneCaptureTool
    {
        private static readonly string OUT_DIR_SS = "Screenshots";
        private static readonly string OUT_DIR_VID = "Videos";
        private static readonly string TEMP_FRAMES = "TempFrames";

        [MenuItem("Tools/Kimaya/Capturar Screenshots de Escenas")]
        public static void CaptureAllScreenshots()
        {
            Directory.CreateDirectory(OUT_DIR_SS);
            Directory.CreateDirectory(OUT_DIR_VID);

            string[] scenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Level_01_Bosque.unity",
                "Assets/Scenes/Prologue_Story.unity",
                "Assets/Scenes/Ending.unity"
            };

            foreach (var scPath in scenes)
            {
                if (!File.Exists(scPath)) continue;

                var sc = EditorSceneManager.OpenScene(scPath, OpenSceneMode.Single);
                string sceneName = Path.GetFileNameWithoutExtension(scPath);

                CaptureSceneImage(sceneName, Path.Combine(OUT_DIR_SS, $"Captura_{sceneName}.png"));
                CaptureSceneFrames(sceneName, 30);
            }

            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("✅ Capturas Completadas",
                    "Se han generado las capturas en la carpeta 'Screenshots/'\ny los fotogramas para video en 'TempFrames/'.", "OK");
            }

            Debug.Log("[KimayaSceneCaptureTool] ✅ Capturas finalizadas en: " + Path.GetFullPath(OUT_DIR_SS));
        }

        public static void CaptureSceneImage(string sceneName, string targetPath)
        {
            var cam = Camera.main;
            if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
            var originalModes = new Dictionary<Canvas, (RenderMode mode, Camera cam)>();

            foreach (var cv in canvases)
            {
                originalModes[cv] = (cv.renderMode, cv.worldCamera);
                if (cv.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    cv.renderMode = RenderMode.ScreenSpaceCamera;
                    cv.worldCamera = cam;
                    cv.planeDistance = 10f;
                }
            }

            int w = 1920;
            int h = 1080;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevRT = RenderTexture.active;
            var prevCamRT = cam.targetTexture;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(targetPath, bytes);

            cam.targetTexture = prevCamRT;
            RenderTexture.active = prevRT;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);

            foreach (var kvp in originalModes)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.renderMode = kvp.Value.mode;
                    kvp.Key.worldCamera = kvp.Value.cam;
                }
            }

            Debug.Log($"[KimayaSceneCaptureTool] Captura guardada: {targetPath}");
        }

        public static void CaptureSceneFrames(string sceneName, int frameCount)
        {
            string framesDir = Path.Combine(TEMP_FRAMES, sceneName);
            if (Directory.Exists(framesDir)) Directory.Delete(framesDir, true);
            Directory.CreateDirectory(framesDir);

            var cam = Camera.main;
            if (cam == null) cam = Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude);
            var originalModes = new Dictionary<Canvas, (RenderMode mode, Camera cam)>();

            foreach (var cv in canvases)
            {
                originalModes[cv] = (cv.renderMode, cv.worldCamera);
                if (cv.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    cv.renderMode = RenderMode.ScreenSpaceCamera;
                    cv.worldCamera = cam;
                    cv.planeDistance = 10f;
                }
            }

            int w = 1280;
            int h = 720;
            var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
            var prevRT = RenderTexture.active;
            var prevCamRT = cam.targetTexture;

            var player = GameObject.FindWithTag("Player");
            Vector3 playerStartPos = player != null ? player.transform.position : Vector3.zero;

            for (int i = 0; i < frameCount; i++)
            {
                if (player != null)
                {
                    player.transform.position = playerStartPos + new Vector3(i * 0.08f, Mathf.Sin(i * 0.3f) * 0.1f, 0f);
                }

                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                string framePath = Path.Combine(framesDir, $"frame_{i:D4}.png");
                File.WriteAllBytes(framePath, bytes);

                Object.DestroyImmediate(tex);
            }

            if (player != null) player.transform.position = playerStartPos;

            cam.targetTexture = prevCamRT;
            RenderTexture.active = prevRT;
            Object.DestroyImmediate(rt);

            foreach (var kvp in originalModes)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.renderMode = kvp.Value.mode;
                    kvp.Key.worldCamera = kvp.Value.cam;
                }
            }
        }
    }
}
