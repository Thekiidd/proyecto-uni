using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Platformer.Mechanics;
using Platformer.Core;

public static class Level01CoopPuzzleSetup
{
    [MenuItem("Tools/Setup Level 01 Cooperative Puzzles")]
    [MenuItem("Tools/Woods/Setup Level 01 Cooperative Puzzles")]
    public static void SetupPuzzle()
    {
        string scenePath = "Assets/Scenes/Level_01.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Cargar Sprites
        var boxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PixelAdventure/Items/Boxes/Box1/Idle.png");
        var woodPlateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Button_Wood_Normal.png");
        var woodDoorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Wood_Frame.png");
        var clueSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icon_Paw.png");
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");

        // Buscar o crear contenedor de Puzzles
        var puzzleRoot = GameObject.Find("CooperativePuzzles");
        if (puzzleRoot != null)
        {
            Undo.DestroyObjectImmediate(puzzleRoot);
        }
        puzzleRoot = new GameObject("CooperativePuzzles");

        // ==========================================
        // PUZZLE 1: CAJA DE MADERA ESCALÓN (X = 6.5, Y = 1.8)
        // ==========================================
        var crate1 = new GameObject("PushableCrate_Intro");
        crate1.transform.SetParent(puzzleRoot.transform);
        crate1.transform.position = new Vector3(6.5f, 1.8f, 0);

        var sr1 = crate1.AddComponent<SpriteRenderer>();
        sr1.sprite = boxSprite;
        sr1.sortingLayerName = "Foreground";
        sr1.sortingOrder = 5;

        var col1 = crate1.AddComponent<BoxCollider2D>();
        col1.size = new Vector2(0.9f, 0.9f);

        var rb1 = crate1.AddComponent<Rigidbody2D>();
        rb1.bodyType = RigidbodyType2D.Dynamic;
        rb1.mass = 2.5f;
        rb1.freezeRotation = true;
        rb1.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Pared obstáculo escalón en X = 9.5
        var wall1 = new GameObject("ObstacleWall");
        wall1.transform.SetParent(puzzleRoot.transform);
        wall1.transform.position = new Vector3(9.5f, 2.2f, 0);
        var wallCol = wall1.AddComponent<BoxCollider2D>();
        wallCol.size = new Vector2(0.8f, 2.8f);
        var wallSr = wall1.AddComponent<SpriteRenderer>();
        wallSr.sprite = woodDoorSprite;
        wallSr.size = new Vector2(0.8f, 2.8f);
        wallSr.sortingLayerName = "Foreground";
        wallSr.color = new Color(0.6f, 0.4f, 0.25f);

        // ==========================================
        // PUZZLE 2: PLACA DE PRESIÓN Y COMPUERTA (X = 18 a 22)
        // ==========================================
        // Compuerta (InteractiveDoor) en X = 22, Y = 2.0
        var doorGO = new GameObject("WoodGate_Door");
        doorGO.transform.SetParent(puzzleRoot.transform);
        doorGO.transform.position = new Vector3(22f, 2.0f, 0);

        var doorSr = doorGO.AddComponent<SpriteRenderer>();
        doorSr.sprite = woodDoorSprite;
        doorSr.size = new Vector2(0.8f, 3.5f);
        doorSr.sortingLayerName = "Foreground";
        doorSr.sortingOrder = 4;
        doorSr.color = new Color(0.7f, 0.5f, 0.3f);

        var doorCol = doorGO.AddComponent<BoxCollider2D>();
        doorCol.size = new Vector2(0.8f, 3.5f);

        var doorComp = doorGO.AddComponent<InteractiveDoor>();
        doorComp.openOffset = new Vector3(0, 3.8f, 0);
        doorComp.speed = 3.5f;

        // Placa de Presión (PressurePlate) en X = 16.5, Y = 0.5
        var plateGO = new GameObject("WoodPressurePlate");
        plateGO.transform.SetParent(puzzleRoot.transform);
        plateGO.transform.position = new Vector3(16.5f, 0.5f, 0);

        var plateSr = plateGO.AddComponent<SpriteRenderer>();
        plateSr.sprite = woodPlateSprite;
        plateSr.size = new Vector2(1.5f, 0.3f);
        plateSr.sortingLayerName = "Foreground";
        plateSr.sortingOrder = 2;
        plateSr.color = new Color(0.9f, 0.75f, 0.5f);

        var plateCol = plateGO.AddComponent<BoxCollider2D>();
        plateCol.size = new Vector2(1.5f, 0.4f);
        plateCol.isTrigger = true;

        var plateComp = plateGO.AddComponent<PressurePlate>();

        // Conectar eventos de la placa a la compuerta
        UnityEditor.Events.UnityEventTools.AddPersistentListener(plateComp.onActivate, doorComp.OpenDoor);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(plateComp.onDeactivate, doorComp.CloseDoor);

        // Caja 2 para empujar sobre la placa (X = 14, Y = 1.0)
        var crate2 = new GameObject("PushableCrate_Puzzle");
        crate2.transform.SetParent(puzzleRoot.transform);
        crate2.transform.position = new Vector3(14.0f, 1.0f, 0);

        var sr2 = crate2.AddComponent<SpriteRenderer>();
        sr2.sprite = boxSprite;
        sr2.sortingLayerName = "Foreground";
        sr2.sortingOrder = 5;

        var col2 = crate2.AddComponent<BoxCollider2D>();
        col2.size = new Vector2(0.9f, 0.9f);

        var rb2 = crate2.AddComponent<Rigidbody2D>();
        rb2.bodyType = RigidbodyType2D.Dynamic;
        rb2.mass = 3.0f;
        rb2.freezeRotation = true;
        rb2.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // ==========================================
        // 3. SEGUNDO PERRO COMPAÑERO & CHARACTER SWITCHER
        // ==========================================
        var switcherGO = GameObject.Find("CharacterSwitcher");
        if (switcherGO == null)
        {
            switcherGO = new GameObject("CharacterSwitcher");
        }
        var switcher = switcherGO.GetComponent<CharacterSwitcher>();
        if (switcher == null) switcher = switcherGO.AddComponent<CharacterSwitcher>();

        // Instanciar perro compañero si no existe
        var partnerDog = GameObject.Find("Player_Partner");
        if (partnerDog == null && playerPrefab != null)
        {
            partnerDog = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            partnerDog.name = "Player_Partner";
            partnerDog.transform.position = new Vector3(0.5f, 1.2f, 0);

            var partnerCtrl = partnerDog.GetComponent<PlayerController>();
            if (partnerCtrl != null)
            {
                // Asignar personaje compañero (ej. Akita o Runner)
                var akitaData = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Akita.asset");
                if (akitaData != null)
                {
                    partnerCtrl.ApplyCharacter(akitaData);
                }
            }
        }

        // Registrar ambos perros en CharacterSwitcher
        switcher.characters.Clear();
        var allPlayers = UnityEngine.Object.FindObjectsByType<PlayerController>();
        foreach (var p in allPlayers)
        {
            switcher.characters.Add(p);
        }

        // ==========================================
        // 4. PISTA NARRATIVA: BANDANA ROJA DEL AMIGO PERDIDO
        // ==========================================
        var clueGO = GameObject.Find("StoryClue_Bandana");
        if (clueGO == null)
        {
            clueGO = new GameObject("StoryClue_Bandana");
            clueGO.transform.position = new Vector3(38f, 2.5f, 0);

            var clueSr = clueGO.AddComponent<SpriteRenderer>();
            clueSr.sprite = clueSprite;
            clueSr.sortingLayerName = "Foreground";
            clueSr.sortingOrder = 6;
            clueSr.color = new Color(1f, 0.25f, 0.25f); // Rojo brillante (la bandana)

            var clueCol = clueGO.AddComponent<CircleCollider2D>();
            clueCol.radius = 0.8f;
            clueCol.isTrigger = true;

            clueGO.AddComponent<StoryClueTrigger>();
        }

        EditorSceneManager.SaveScene(scene);
        AssetDatabase.Refresh();

        Debug.Log("[Level01Setup] ✅ Puzzles cooperativos, cajas, placas, compañero canino y pista narrativa configurados con éxito en Level_01!");
    }
}
