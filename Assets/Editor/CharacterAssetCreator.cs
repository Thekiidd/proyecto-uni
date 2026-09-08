using Platformer.Core;
using Platformer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Tools > Create Character Assets
/// Crea los 3 ScriptableObjects de personaje y los asigna al CharacterSelectUI de la escena activa.
/// </summary>
public static class CharacterAssetCreator
{
    [MenuItem("Tools/Create Character Assets")]
    public static void CreateAndAssign()
    {
        string folder = "Assets/Characters";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Characters");

        // ── Crear los 3 personajes ──
        var runner = CreateCharacter(folder, "Runner",
            "Veloz y agil. Perfecta para speedrunners.",
            moveSpeed: 10f, jumpStrength: 14f, gravity: 3f,
            lives: 3, doubleJump: false, dash: false,
            color: new Color(0.2f, 0.8f, 1f));

        var jumper = CreateCharacter(folder, "Jumper",
            "Salta muy alto. Puede saltar dos veces!",
            moveSpeed: 6f, jumpStrength: 20f, gravity: 2f,
            lives: 3, doubleJump: true, dash: false,
            color: new Color(0.3f, 1f, 0.3f));

        var tank = CreateCharacter(folder, "Tank",
            "Lento pero resistente. Tiene 6 vidas.",
            moveSpeed: 5f, jumpStrength: 12f, gravity: 3.5f,
            lives: 6, doubleJump: false, dash: false,
            color: new Color(1f, 0.8f, 0.1f));

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── Asignar al CharacterSelectUI de la escena activa ──
        var selectUI = Object.FindFirstObjectByType<CharacterSelectUI>();
        if (selectUI != null)
        {
            var so  = new SerializedObject(selectUI);
            var arr = so.FindProperty("characters");
            arr.arraySize = 3;
            arr.GetArrayElementAtIndex(0).objectReferenceValue = runner;
            arr.GetArrayElementAtIndex(1).objectReferenceValue = jumper;
            arr.GetArrayElementAtIndex(2).objectReferenceValue = tank;
            so.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[CharacterAssetCreator] Personajes asignados a CharacterSelectUI.");
        }
        else
        {
            Debug.LogWarning("[CharacterAssetCreator] CharacterSelectUI no encontrado en escena.");
        }

        EditorUtility.DisplayDialog("Personajes creados",
            "Se crearon 3 personajes en Assets/Characters/:\n" +
            "  Runner (rapido, azul)\n" +
            "  Jumper (doble salto, verde)\n" +
            "  Tank (6 vidas, amarillo)\n\n" +
            "Fueron asignados automaticamente al menu.",
            "Entendido");
    }

    static CharacterData CreateCharacter(string folder, string charName,
        string desc, float moveSpeed, float jumpStrength, float gravity,
        int lives, bool doubleJump, bool dash, Color color)
    {
        string path = $"{folder}/{charName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
        if (existing != null) return existing;

        var data = ScriptableObject.CreateInstance<CharacterData>();
        data.characterName   = charName;
        data.description     = desc;
        data.moveSpeed       = moveSpeed;
        data.jumpStrength    = jumpStrength;
        data.gravityModifier = gravity;
        data.startingLives   = lives;
        data.hasDoubleJump   = doubleJump;
        data.hasDash         = dash;
        data.characterColor  = color;

        AssetDatabase.CreateAsset(data, path);
        return data;
    }
}
