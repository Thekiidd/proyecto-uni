using Platformer.Core;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > Setup Dog Characters
/// Actualiza los CharacterData con sprites del Pet Dogs Pack.
/// </summary>
public static class DogCharacterSetup
{
    [MenuItem("Tools/Setup Dog Characters")]
    public static void Setup()
    {
        // ── Cargar sprites de cada perro ──
        var idleGolden = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Golden-Retriever/Golden-Retriever-idle.png");
        var runGolden  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Golden-Retriever/Golden-Retriever-run.png");

        var idleAkita  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Akita/Akita-Idle.png");
        var runAkita   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Akita/Akita-run.png");

        var idleSaint  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Saint-Bernard/Saint-Bernard-Idle.png");
        var runSaint   = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Saint-Bernard/Saint-Bernard-run.png");

        // ── Actualizar los 3 personajes ──
        UpdateCharacter("Assets/Characters/Runner.asset",
            charName: "Golden Retriever", description: "Rapido y energico. El corredor nato.",
            moveSpeed: 10f, jumpStrength: 14f, lives: 3, doubleJump: false, dash: false,
            color: new Color(0.85f, 0.6f, 0.2f),
            portrait: idleGolden, idleSprite: idleGolden, runSprite: runGolden);

        UpdateCharacter("Assets/Characters/Jumper.asset",
            charName: "Akita", description: "Ligero y saltarin. Puede saltar dos veces.",
            moveSpeed: 6f, jumpStrength: 20f, lives: 3, doubleJump: true, dash: false,
            color: new Color(0.95f, 0.75f, 0.4f),
            portrait: idleAkita, idleSprite: idleAkita, runSprite: runAkita);

        UpdateCharacter("Assets/Characters/Tank.asset",
            charName: "San Bernardo", description: "Grande y resistente. Tiene 6 vidas.",
            moveSpeed: 5f, jumpStrength: 12f, lives: 6, doubleJump: false, dash: false,
            color: new Color(0.8f, 0.4f, 0.2f),
            portrait: idleSaint, idleSprite: idleSaint, runSprite: runSaint);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        int found = (idleGolden != null ? 1 : 0) + (idleAkita != null ? 1 : 0) + (idleSaint != null ? 1 : 0);

        EditorUtility.DisplayDialog("Personajes Perros Configurados",
            "Personajes actualizados:\n" +
            $"  Golden Retriever  idle:{(idleGolden != null ? "OK" : "FALTA")}\n" +
            $"  Akita             idle:{(idleAkita  != null ? "OK" : "FALTA")}\n" +
            $"  San Bernardo      idle:{(idleSaint  != null ? "OK" : "FALTA")}\n\n" +
            (found == 3 ? "Sprites asignados correctamente." :
             "Algunos sprites no se encontraron. Revisa Assets/Sprites/Dogs/"),
            "Entendido");
    }

    static void UpdateCharacter(string path, string charName, string description,
        float moveSpeed, float jumpStrength, int lives,
        bool doubleJump, bool dash, Color color,
        Sprite portrait, Sprite idleSprite, Sprite runSprite)
    {
        var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
        if (data == null)
        {
            Debug.LogWarning($"[DogSetup] No se encontro {path}. Corre primero Tools > Create Character Assets.");
            return;
        }

        var so = new SerializedObject(data);
        so.FindProperty("characterName").stringValue        = charName;
        so.FindProperty("description").stringValue          = description;
        so.FindProperty("moveSpeed").floatValue             = moveSpeed;
        so.FindProperty("jumpStrength").floatValue          = jumpStrength;
        so.FindProperty("startingLives").intValue           = lives;
        so.FindProperty("hasDoubleJump").boolValue          = doubleJump;
        so.FindProperty("hasDash").boolValue                = dash;
        so.FindProperty("characterColor").colorValue        = color;
        if (portrait   != null) so.FindProperty("portrait").objectReferenceValue   = portrait;
        if (idleSprite != null) so.FindProperty("idleSprite").objectReferenceValue = idleSprite;
        if (runSprite  != null) so.FindProperty("runSprite").objectReferenceValue  = runSprite;
        so.ApplyModifiedProperties();

        Debug.Log($"[DogSetup] {charName}: idle={idleSprite != null} run={runSprite != null}");
    }
}
