using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TestSceneLoader
{
    public static void LoadClassicScene()
    {
        Debug.Log("[TestSceneLoader] Opening Ascenso_JumpKing_Classic.unity...");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Ascenso_JumpKing_Classic.unity", OpenSceneMode.Single);
        Debug.Log("[TestSceneLoader] Scene valid: " + scene.IsValid() + ", name: " + scene.name);
        
        var allGOs = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int missingCount = 0;
        foreach (var go in allGOs)
        {
            var components = go.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogError("[MissingScript] GameObject '" + go.name + "' has a missing component at index " + i);
                    missingCount++;
                }
            }
        }
        Debug.Log("[TestSceneLoader] Check finished. Missing components: " + missingCount + ". Total GameObjects: " + allGOs.Length);
        EditorApplication.Exit(missingCount > 0 ? 2 : 0);
    }
}
