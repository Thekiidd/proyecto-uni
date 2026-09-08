using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Tools > Setup Build Scenes
/// Renombra SampleScene a Level_01, duplica para Level_02/03
/// y agrega todas las escenas a Build Settings.
/// </summary>
public static class BuildSceneSetup
{
    [MenuItem("Tools/Setup Build Scenes (Level_01, 02, 03)")]
    public static void SetupScenes()
    {
        string scenesFolder = "Assets/Scenes";

        // ── Paso 1: Renombrar SampleScene → Level_01 si existe ──
        string samplePath = scenesFolder + "/SampleScene.unity";
        string level01    = scenesFolder + "/Level_01.unity";
        string level02    = scenesFolder + "/Level_02.unity";
        string level03    = scenesFolder + "/Level_03.unity";
        string mainMenu   = scenesFolder + "/MainMenu.unity";

        if (File.Exists(Application.dataPath + "/../" + samplePath) &&
            !File.Exists(Application.dataPath + "/../" + level01))
        {
            AssetDatabase.RenameAsset(samplePath, "Level_01");
            AssetDatabase.Refresh();
            Debug.Log("[BuildSceneSetup] SampleScene renombrada a Level_01.");
        }

        // ── Paso 2: Duplicar Level_01 → Level_02 y Level_03 ──
        if (File.Exists(Application.dataPath + "/../" + level01))
        {
            if (!File.Exists(Application.dataPath + "/../" + level02))
            {
                AssetDatabase.CopyAsset(level01, level02);
                Debug.Log("[BuildSceneSetup] Level_02 creado.");
            }
            if (!File.Exists(Application.dataPath + "/../" + level03))
            {
                AssetDatabase.CopyAsset(level01, level03);
                Debug.Log("[BuildSceneSetup] Level_03 creado.");
            }
        }
        else
        {
            Debug.LogWarning("[BuildSceneSetup] No se encontró Level_01.unity ni SampleScene.unity en Assets/Scenes/.");
        }

        AssetDatabase.Refresh();

        // ── Paso 3: Agregar a Build Settings en orden correcto ──
        var scenePaths = new List<string>
        {
            mainMenu,   // índice 0
            level01,    // índice 1
            level02,    // índice 2
            level03     // índice 3
        };

        var buildScenes = new List<EditorBuildSettingsScene>();
        foreach (var path in scenePaths)
        {
            if (File.Exists(Application.dataPath + "/../" + path))
            {
                buildScenes.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[BuildSceneSetup] Agregada: {path}");
            }
            else
            {
                Debug.LogWarning($"[BuildSceneSetup] No encontrada (se omite): {path}");
            }
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();

        // ── Resumen ──
        EditorUtility.DisplayDialog("✅ Build Settings Configurado",
            "Escenas en Build Settings:\n" +
            "  [0] MainMenu\n" +
            "  [1] Level_01\n" +
            "  [2] Level_02\n" +
            "  [3] Level_03\n\n" +
            "¡Ahora presiona Play en la escena MainMenu y funcionará!",
            "Entendido");
    }
}
