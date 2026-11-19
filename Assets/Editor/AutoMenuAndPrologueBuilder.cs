using UnityEditor;
using UnityEngine;

/// <summary>
/// Desactivado en arranque automático ([InitializeOnLoad] removido) para evitar
/// sobrescribir escenas sincronizadas o lanzar errores por Level_01.unity inexistente.
/// Disponible manualmente desde el menú Tools si se requiere.
/// </summary>
public static class AutoMenuAndPrologueBuilder
{
    [MenuItem("Tools/Kimaya/Rebuild Menu & Prologue Scenes")]
    public static void RunBuildManual()
    {
        if (EditorApplication.isPlaying) return;
        MainMenuSceneBuilder.BuildScene();
        PrologueSceneBuilder.BuildScene();
        Debug.Log("[AutoMenuAndPrologueBuilder] ✅ MainMenu y Prologue_Story reconstruidas manualmente.");
    }
}
