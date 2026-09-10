using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class AutoMenuAndPrologueBuilder
{
    static AutoMenuAndPrologueBuilder()
    {
        EditorApplication.delayCall += RunBuild;
    }

    private static void RunBuild()
    {
        if (EditorApplication.isPlaying) return;

        string flagFile = Application.dataPath + "/../Library/menu_built_v5.txt";
        if (File.Exists(flagFile)) return;

        Debug.Log("[AutoMenuAndPrologueBuilder] Iniciando construcción automática con puzzles de Nivel 1...");
        try
        {
            MainMenuSceneBuilder.BuildScene();
            PrologueSceneBuilder.BuildScene();
            Level01CoopPuzzleSetup.SetupPuzzle();
            File.WriteAllText(flagFile, "done");
            Debug.Log("[AutoMenuAndPrologueBuilder] ✅ MainMenu, Prologue_Story y Level_01 (Puzzles) configuradas automáticamente!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[AutoMenuAndPrologueBuilder] Error al construir escenas: " + ex);
        }
    }
}
