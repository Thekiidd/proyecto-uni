using System.Collections.Generic;
using System.IO;
using Platformer.Gameplay;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Constructor del Nivel 2 utilizando el pack "Pixel Adventure 1":
    /// - Terreno Pixel Adventure 16x16.
    /// - Fondo temático colorido de Pixel Adventure.
    /// - Trampolines mecánicos con rebote elástico.
    /// - Caseta de perro Checkpoint interactiva.
    /// - Coleccionables caninos (Huesos dorados, galletas de vida y pelotas de tenis).
    /// </summary>
    public static class PixelAdventureLevelBuilder
    {
        const string SCENE_LEVEL_02 = "Assets/Scenes/Level_02.unity";
        const string TEMPLATE_SCENE = "Assets/Scenes/Level_01.unity";

        [MenuItem("Tools/Dogs/🎮 Construir Nivel 2 (Pixel Adventure 1)")]
        public static void MenuBuildPixelAdventureLevel()
        {
            BuildLevel(true);
        }

        public static void BuildLevel(bool showDialog)
        {
            // 1. Abrir o crear Level_02.unity
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != SCENE_LEVEL_02)
            {
                if (File.Exists(SCENE_LEVEL_02))
                    scene = EditorSceneManager.OpenScene(SCENE_LEVEL_02, OpenSceneMode.Single);
                else if (File.Exists(TEMPLATE_SCENE))
                {
                    scene = EditorSceneManager.OpenScene(TEMPLATE_SCENE, OpenSceneMode.Single);
                    EditorSceneManager.SaveScene(scene, SCENE_LEVEL_02);
                }
            }

            // 2. Configurar Grid y Tilemap
            var grid = Object.FindAnyObjectByType<Grid>();
            if (grid != null)
            {
                grid.cellSize = Vector3.one;
                EditorUtility.SetDirty(grid);
            }

            var tilemaps = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include);
            Tilemap levelTilemap = null;
            foreach (var tm in tilemaps)
            {
                tm.ClearAllTiles();
                EditorUtility.SetDirty(tm);
                if (tm.name == "Level" || tm.name.ToLower().Contains("ground"))
                    levelTilemap = tm;
            }

            if (levelTilemap == null && tilemaps.Length > 0)
                levelTilemap = tilemaps[0];

            if (levelTilemap == null)
            {
                var gridGO = (grid != null) ? grid.gameObject : new GameObject("Grid");
                if (grid == null) grid = gridGO.AddComponent<Grid>();
                grid.cellSize = Vector3.one;

                var tmGO = new GameObject("Level");
                tmGO.transform.SetParent(gridGO.transform, false);
                levelTilemap = tmGO.AddComponent<Tilemap>();
                var tr = tmGO.AddComponent<TilemapRenderer>();
                tr.sortingOrder = 0;
            }

            levelTilemap.transform.localPosition = Vector3.zero;
            levelTilemap.transform.localScale = Vector3.one;

            var tileCollider = levelTilemap.GetComponent<TilemapCollider2D>();
            if (tileCollider == null) tileCollider = levelTilemap.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            var rb = levelTilemap.GetComponent<Rigidbody2D>();
            if (rb == null) rb = levelTilemap.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var compCollider = levelTilemap.GetComponent<CompositeCollider2D>();
            if (compCollider == null) compCollider = levelTilemap.gameObject.AddComponent<CompositeCollider2D>();

            // 3. Cargar Tiles de Pixel Adventure (o Woods como fallback de alta calidad)
            Tile tGrassMid = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Sprites/PixelAdventure/Terrain/Tiles/Terrain (16x16) 1_1.asset");
            Tile tGrassLeft = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Sprites/PixelAdventure/Terrain/Tiles/Terrain (16x16) 1_0.asset");
            Tile tGrassRight = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Sprites/PixelAdventure/Terrain/Tiles/Terrain (16x16) 1_2.asset");
            Tile tDirtMid = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Sprites/PixelAdventure/Terrain/Tiles/Terrain (16x16) 1_23.asset");

            if (tGrassMid == null)
            {
                // Fallback a Woods tiles
                tGrassMid = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Woods/Woods_Tile_0_1.asset");
                tGrassLeft = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Woods/Woods_Tile_0_0.asset") ?? tGrassMid;
                tGrassRight = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Woods/Woods_Tile_0_2.asset") ?? tGrassMid;
                tDirtMid = AssetDatabase.LoadAssetAtPath<Tile>("Assets/Tiles/Woods/Woods_Tile_1_1.asset") ?? tGrassMid;
            }

            // 4. PINTAR EL TERRENO DEL NIVEL 2
            void PaintGround(int startX, int endX, int topY, int bottomY)
            {
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = topY; y >= bottomY; y--)
                    {
                        Tile t = (y == topY)
                            ? ((x == startX) ? tGrassLeft : (x == endX) ? tGrassRight : tGrassMid)
                            : tDirtMid;
                        levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                    }
                }
            }

            void PaintPlatform(int startX, int endX, int y)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Tile t = (x == startX) ? tGrassLeft : (x == endX) ? tGrassRight : tGrassMid;
                    levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }

            // Diseño dinámico de plataformas acrobáticas
            PaintGround(-6, 6, 0, -4);
            PaintPlatform(8, 12, 2);
            PaintPlatform(14, 18, 4);
            PaintGround(20, 36, 1, -4); // Meseta con caseta de perro
            PaintPlatform(38, 42, 3);
            PaintPlatform(44, 48, 6);
            PaintPlatform(50, 54, 4);
            PaintGround(56, 75, 2, -4); // Zona de meta

            levelTilemap.RefreshAllTiles();
            EditorUtility.SetDirty(levelTilemap);
            EditorUtility.SetDirty(levelTilemap.gameObject);

            // 5. FONDO PIXEL ADVENTURE
            var oldBgs = GameObject.Find("[PABackground]");
            if (oldBgs != null) Object.DestroyImmediate(oldBgs);

            var bgRoot = new GameObject("[PABackground]");
            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PixelAdventure/Background/Blue.png");
            if (bgSprite != null)
            {
                // Tiled sprite de 64x64 repetido
                for (float bx = -16; bx <= 80; bx += 8f)
                {
                    for (float by = -6; by <= 16; by += 8f)
                    {
                        var seg = new GameObject($"Bg_{bx}_{by}");
                        seg.transform.SetParent(bgRoot.transform, false);
                        seg.transform.position = new Vector3(bx, by, 0);
                        var sr = seg.AddComponent<SpriteRenderer>();
                        sr.sprite = bgSprite;
                        sr.sortingOrder = -20;
                        seg.transform.localScale = new Vector3(4f, 4f, 1f);
                    }
                }
            }

            // 6. TRAMPOLINES DE PIXEL ADVENTURE
            var oldTramps = GameObject.Find("[PATrampolines]");
            if (oldTramps != null) Object.DestroyImmediate(oldTramps);

            var trampRoot = new GameObject("[PATrampolines]");
            var trampSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PixelAdventure/Traps/Trampoline/Idle.png");
            var jumpAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/jump.wav");

            void CreateTrampoline(float x, float y, float force)
            {
                var trGO = new GameObject($"Trampoline_{x}");
                trGO.transform.SetParent(trampRoot.transform, false);
                trGO.transform.position = new Vector3(x, y, 0);
                var sr = trGO.AddComponent<SpriteRenderer>();
                if (trampSpr != null) sr.sprite = trampSpr;
                sr.sortingOrder = 2;
                trGO.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

                var col = trGO.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(1f, 0.6f);

                var bm = trGO.AddComponent<BouncyMushroom>();
                bm.bounceForce = force;
                bm.bounceAudio = jumpAudio;
            }

            CreateTrampoline(13f, 4.3f, 19f);
            CreateTrampoline(43f, 6.3f, 21f);

            // 7. CASETA DE PERRO CHECKPOINT
            var oldHouse = GameObject.Find("[DogHouseCheckpoint]");
            if (oldHouse != null) Object.DestroyImmediate(oldHouse);

            var houseGO = new GameObject("[DogHouseCheckpoint]");
            houseGO.transform.position = new Vector3(28f, 1.05f, 0f);

            var subSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Props/DogHouse_Checkpoint.png");
            Sprite houseInactive = null;
            Sprite houseActive = null;
            foreach (var sub in subSprites)
            {
                if (sub is Sprite s)
                {
                    if (s.name.Contains("Inactive") || houseInactive == null) houseInactive = s;
                    if (s.name.Contains("Active")) houseActive = s;
                }
            }

            var houseSR = houseGO.AddComponent<SpriteRenderer>();
            houseSR.sprite = houseInactive;
            houseSR.sortingOrder = 2;

            var houseCol = houseGO.AddComponent<BoxCollider2D>();
            houseCol.isTrigger = true;
            houseCol.size = new Vector2(1.2f, 1.2f);
            houseCol.offset = new Vector2(0f, 0.6f);

            var houseCp = houseGO.AddComponent<DogHouseCheckpoint>();
            houseCp.inactiveSprite = houseInactive;
            houseCp.activeSprite = houseActive;
            houseCp.activateAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Collectable.wav");

            // 8. COLECCIONABLES (Huesos dorados, Galleta y Pelotas secretas)
            var oldPickups = GameObject.Find("[DogPickups]");
            if (oldPickups != null) Object.DestroyImmediate(oldPickups);

            var pickupRoot = new GameObject("[DogPickups]");
            var boneAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Pickups/Bone_Spin.png");
            var boneFrames = new List<Sprite>();
            foreach (var a in boneAssets)
            {
                if (a is Sprite s) boneFrames.Add(s);
            }

            var biscuitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/DogBiscuit.png");
            var ballSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/TennisBall.png");
            var coinAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Collectable.wav");

            void CreateToken(Vector3 pos, TokenInstance.DogTokenType type)
            {
                var tGO = new GameObject($"Pickup_{type}_{pos.x}");
                tGO.transform.SetParent(pickupRoot.transform, false);
                tGO.transform.position = pos;

                var sr = tGO.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 3;

                var col = tGO.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.35f;

                var inst = tGO.AddComponent<TokenInstance>();
                inst.tokenType = type;
                inst.tokenCollectAudio = coinAudio;

                if (type == TokenInstance.DogTokenType.Bone && boneFrames.Count > 0)
                {
                    inst.idleAnimation = boneFrames.ToArray();
                    inst.randomAnimationStartTime = true;
                    sr.sprite = boneFrames[0];
                }
                else if (type == TokenInstance.DogTokenType.DogBiscuit && biscuitSprite != null)
                {
                    inst.idleAnimation = new[] { biscuitSprite };
                    sr.sprite = biscuitSprite;
                }
                else if (type == TokenInstance.DogTokenType.TennisBall && ballSprite != null)
                {
                    inst.idleAnimation = new[] { ballSprite };
                    sr.sprite = ballSprite;
                }
            }

            // Huesos a lo largo del recorrido
            float[] boneXs = new float[] { 0, 2, 4, 9, 11, 15, 17, 22, 24, 26, 30, 32, 39, 41, 46, 51, 58, 60, 64, 68 };
            foreach (var bx in boneXs)
            {
                CreateToken(new Vector3(bx, 2.2f, 0), TokenInstance.DogTokenType.Bone);
            }

            // Galleta de curación
            CreateToken(new Vector3(26f, 1.5f, 0), TokenInstance.DogTokenType.DogBiscuit);

            // 3 Pelotas de tenis secretas
            CreateToken(new Vector3(16f, 7.5f, 0), TokenInstance.DogTokenType.TennisBall);
            CreateToken(new Vector3(46f, 9.5f, 0), TokenInstance.DogTokenType.TennisBall);
            CreateToken(new Vector3(52f, 7.0f, 0), TokenInstance.DogTokenType.TennisBall);

            // Re-vincular TokenController
            var tokenCtrl = Object.FindAnyObjectByType<TokenController>();
            if (tokenCtrl != null)
            {
                tokenCtrl.tokens = pickupRoot.GetComponentsInChildren<TokenInstance>();
                EditorUtility.SetDirty(tokenCtrl);
            }

            // 9. JUGADOR, SPAWN, DEATHZONE Y META
            var spawnGO = GameObject.Find("SpawnPoint") ?? GameObject.Find("PlayerSpawn");
            if (spawnGO != null)
            {
                spawnGO.transform.position = new Vector3(-2f, 1.2f, 0f);
                EditorUtility.SetDirty(spawnGO);
            }

            var playerGO = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (playerGO != null)
            {
                playerGO.transform.position = new Vector3(-2f, 1.2f, 0f);
                EditorUtility.SetDirty(playerGO);
            }

            var deathZoneGO = GameObject.Find("DeathZone");
            if (deathZoneGO != null)
            {
                deathZoneGO.transform.position = new Vector3(35f, -7f, 0f);
                var col = deathZoneGO.GetComponent<BoxCollider2D>();
                if (col != null) col.size = new Vector2(160f, 6f);
                EditorUtility.SetDirty(deathZoneGO);
            }

            var victoryGO = GameObject.Find("VictoryZone");
            if (victoryGO != null)
            {
                victoryGO.transform.position = new Vector3(72f, 3.5f, 0f);
                var col = victoryGO.GetComponent<BoxCollider2D>();
                if (col != null) col.size = new Vector2(4f, 5f);
                EditorUtility.SetDirty(victoryGO);
            }

            // 10. Guardar escena
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[PixelAdventureLevelBuilder] ✅ ¡Nivel 2 con Pixel Adventure construido con éxito!");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("✅ Nivel 2 Creado",
                    "¡El Nivel 2 con Pixel Adventure 1 ha sido creado con éxito!\n\n" +
                    "- Fondo y tiles de Pixel Adventure\n" +
                    "- Trampolines mecánicos elásticos\n" +
                    "- Caseta de perro Checkpoint interactiva\n" +
                    "- Huesos dorados y 3 Pelotas de tenis secretas",
                    "¡Entendido!");
            }
        }
    }
}
