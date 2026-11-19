using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Kimaya.EditorTools
{
    public static class KimayaBuildAutomation
    {
        [MenuItem("Kimaya/Build Standalone Windows x64")]
        public static void BuildStandaloneWindows()
        {
            Debug.Log("[KimayaBuild] Iniciando proceso de compilación para Windows x64...");

            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../Build"));
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            string exePath = Path.Combine(outDir, "Kimaya.exe");

            // En el ejecutable va exclusivamente el nivel original de Kimaya (Ascenso_Kimaya.unity),
            // mientras que Ascenso_JumpKing_Classic.unity queda guardado como escena jugable dentro del proyecto Unity.
            string[] preferredScenes = new string[]
            {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Hub.unity",
                "Assets/Scenes/Ascenso_Kimaya.unity",
                "Assets/Scenes/Prologue_Story.unity",
                "Assets/Scenes/Ending.unity",
                "Assets/Scenes/Village_Scene.unity"
            };

            List<string> validScenes = new List<string>();
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            foreach (var scenePath in preferredScenes)
            {
                string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../" + scenePath));
                if (File.Exists(fullPath))
                {
                    validScenes.Add(scenePath);
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"[KimayaBuild] Escena añadida al ejecutable: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"[KimayaBuild] Escena omitida (no encontrada en disco): {scenePath}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();

            PlayerSettings.productName = "Kimaya: El Bosque del Eco";
            PlayerSettings.companyName = "Kimaya Studios";
            PlayerSettings.defaultIsFullScreen = true;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.runInBackground = false;
            PlayerSettings.resizableWindow = true;

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = validScenes.ToArray(),
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[KimayaBuild] Compilando ejecutable en: {exePath}");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[KimayaBuild] ✅ ¡COMPILACIÓN EXITOSA! Tamaño: {summary.totalSize / (1024 * 1024)} MB");
                Debug.Log($"[KimayaBuild] Ubicación: {exePath}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[KimayaBuild] ❌ Compilación fallida: {summary.totalErrors} errores encontrados.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
