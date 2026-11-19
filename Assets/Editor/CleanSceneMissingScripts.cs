using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace Kimaya.EditorTools
{
    public static class CleanSceneMissingScripts
    {
        public static void CleanAndValidateScenes()
        {
            Debug.Log("[CleanScene] Iniciando limpieza de Missing Scripts en escenas...");

            string[] scenesToClean = new string[]
            {
                "Assets/Scenes/Ascenso_JumpKing_Classic.unity",
                "Assets/Scenes/Hub.unity",
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/Prologue_Story.unity",
                "Assets/Scenes/Ending.unity",
                "Assets/Scenes/Village_Scene.unity",
                "Assets/Scenes/Test_Mecanica_Interaccion.unity"
            };

            foreach (var scenePath in scenesToClean)
            {
                if (!File.Exists(scenePath)) continue;

                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                int removedCount = 0;

                foreach (var go in allGOs)
                {
                    int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    if (removed > 0)
                    {
                        Debug.Log($"[CleanScene] Eliminados {removed} missing script(s) en GameObject: '{go.name}' ({scenePath})");
                        removedCount += removed;
                    }
                }

                if (removedCount > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[CleanScene] ✅ Escena '{scenePath}' guardada limpia tras remover {removedCount} componentes corruptos.");
                }
                else
                {
                    Debug.Log($"[CleanScene] ✨ Escena '{scenePath}' estaba 100% limpia.");
                }
            }

            // Validar que Ascenso_JumpKing_Classic ya no tenga ningun missing component
            var classicScene = EditorSceneManager.OpenScene("Assets/Scenes/Ascenso_JumpKing_Classic.unity", OpenSceneMode.Single);
            var remainingGOs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int remainingMissing = 0;
            foreach (var go in remainingGOs)
            {
                var comps = go.GetComponents<Component>();
                for (int i = 0; i < comps.Length; i++)
                {
                    if (comps[i] == null) remainingMissing++;
                }
            }

            Debug.Log($"[CleanScene] Validación final de Ascenso_JumpKing_Classic: {remainingMissing} missing components.");
            EditorApplication.Exit(remainingMissing == 0 ? 0 : 1);
        }
    }
}
