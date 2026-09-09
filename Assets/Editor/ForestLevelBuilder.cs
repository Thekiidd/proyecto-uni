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
    /// Constructor del Nivel 1 ("Senderos del Bosque") con temática canina completa:
    /// - Tilemap detallado de bosque (Woods).
    /// - Fondo Parallax de 7 capas.
    /// - Huesos dorados giratorios, galletas de vida y 3 pelotas de tenis secretas.
    /// - Caseta de perro interactiva como punto de control (Checkpoint).
    /// - Hongos trampolín para alcanzar rutas secretas en los árboles.
    /// </summary>
    [InitializeOnLoad]
    public static class ForestLevelBuilder
    {
        const string SCENE_LEVEL_01 = "Assets/Scenes/Level_01.unity";
        const string FLAG_FILE = "Library/ForestLevel1Built_v1.flag";

        static ForestLevelBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfMissing;
        }

        private static void AutoBuildIfMissing()
        {
            if (!File.Exists(FLAG_FILE))
            {
                Debug.Log("[ForestLevelBuilder] Auto-construyendo nivel 1 del bosque...");
                BuildLevel(false);
                File.WriteAllText(FLAG_FILE, "built");
            }
        }

        [MenuItem("Tools/Dogs/🌲 Construir Nivel 1 del Bosque (Temática Canina)")]
        public static void MenuBuildForestLevel()
        {
            BuildLevel(true);
        }

        public static void BuildLevel(bool showDialog)
        {
            // 1. Asegurar configuración de sprites de pickups y bosque
            DogPickupAssetImporter.ProcessAllPickups();
            WoodsAssetImporter.ProcessAll(false);

            // 2. Abrir Level_01.unity
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.path != SCENE_LEVEL_01)
            {
                if (File.Exists(SCENE_LEVEL_01))
                    scene = EditorSceneManager.OpenScene(SCENE_LEVEL_01, OpenSceneMode.Single);
                else
                {
                    Debug.LogError($"[ForestLevelBuilder] No existe la escena {SCENE_LEVEL_01}");
                    return;
                }
            }

            // 3. Cargar Tiles de Woods
            Tile tileGrassLeft   = LoadTile("Woods_Tile_0_0");
            Tile tileGrassMid    = LoadTile("Woods_Tile_0_1");
            Tile tileGrassRight  = LoadTile("Woods_Tile_0_2");
            Tile tileDirtLeft    = LoadTile("Woods_Tile_1_0");
            Tile tileDirtMid     = LoadTile("Woods_Tile_1_1");
            Tile tileDirtRight   = LoadTile("Woods_Tile_1_2");
            Tile tileBotLeft     = LoadTile("Woods_Tile_2_0");
            Tile tileBotMid      = LoadTile("Woods_Tile_2_1");
            Tile tileBotRight    = LoadTile("Woods_Tile_2_2");

            Tile tilePlatLeft    = LoadTile("Woods_Tile_3_4");
            Tile tilePlatMid     = LoadTile("Woods_Tile_3_5");
            Tile tilePlatRight   = LoadTile("Woods_Tile_3_6");

            if (tileGrassMid == null)
            {
                Debug.LogError("[ForestLevelBuilder] ❌ No se pudieron cargar los tiles de Woods.");
                return;
            }

            tileGrassLeft  = tileGrassLeft  ?? tileGrassMid;
            tileGrassRight = tileGrassRight ?? tileGrassMid;
            tileDirtMid    = tileDirtMid    ?? tileGrassMid;
            tileDirtLeft   = tileDirtLeft   ?? tileDirtMid;
            tileDirtRight  = tileDirtRight  ?? tileDirtMid;
            tileBotMid     = tileBotMid     ?? tileDirtMid;
            tileBotLeft    = tileBotLeft    ?? tileBotMid;
            tileBotRight   = tileBotRight   ?? tileBotMid;
            tilePlatMid    = tilePlatMid    ?? tileGrassMid;
            tilePlatLeft   = tilePlatLeft   ?? tilePlatMid;
            tilePlatRight  = tilePlatRight  ?? tilePlatMid;

            // 4. Configurar Grid y Tilemap
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

            // 5. PINTAR EL TERRENO DEL NIVEL 1
            void PaintGround(int startX, int endX, int topY, int bottomY)
            {
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = topY; y >= bottomY; y--)
                    {
                        Tile t;
                        if (y == topY)
                            t = (x == startX) ? tileGrassLeft : (x == endX) ? tileGrassRight : tileGrassMid;
                        else if (y == bottomY)
                            t = (x == startX) ? tileBotLeft : (x == endX) ? tileBotRight : tileBotMid;
                        else
                            t = (x == startX) ? tileDirtLeft : (x == endX) ? tileDirtRight : tileDirtMid;

                        levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                    }
                }
            }

            void PaintPlatform(int startX, int endX, int y)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Tile t = (x == startX) ? tilePlatLeft : (x == endX) ? tilePlatRight : tilePlatMid;
                    levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }

            // Zona 1: Prado inicial
            PaintGround(-6, 8, 0, -3);

            // Zona 2: Salto sobre arroyo con plataforma de ramas
            PaintPlatform(10, 14, 2);

            // Plataforma secreta alta en los árboles (para Pelota Secreta 1)
            PaintPlatform(14, 18, 7);

            // Zona 3: Meseta central (donde estará la Caseta de Perro Checkpoint)
            PaintGround(16, 32, 1, -3);

            // Zona 4: Plataformas escalonadas sobre el desfiladero
            PaintPlatform(34, 38, 3);
            PaintPlatform(40, 44, 5);

            // Plataforma secreta en la copa de árboles altos (Pelota Secreta 2)
            PaintPlatform(39, 43, 9);

            // Zona 5: Valle rocoso
            PaintGround(46, 58, 1, -3);
            PaintPlatform(50, 54, 4); // Plataforma alta con Pelota Secreta 3

            // Zona 6: Colina final y meta
            PaintGround(61, 78, 2, -3);

            levelTilemap.RefreshAllTiles();
            EditorUtility.SetDirty(levelTilemap);
            EditorUtility.SetDirty(levelTilemap.gameObject);

            // 6. FONDOS PARALLAX MULTICAPA (7 capas de bosque)
            var oldBgs = GameObject.Find("[WoodsBackgrounds]");
            if (oldBgs != null) Object.DestroyImmediate(oldBgs);

            var bgRoot = new GameObject("[WoodsBackgrounds]");
            string[] bgFiles = new string[]
            {
                "Assets/Sprites/Woods/Backgrounds/BACKGROUND.png",
                "Assets/Sprites/Woods/Backgrounds/WOODS - Fourth.png",
                "Assets/Sprites/Woods/Backgrounds/WOODS - Third.png",
                "Assets/Sprites/Woods/Backgrounds/WOODS - Second.png",
                "Assets/Sprites/Woods/Backgrounds/VINES - Second.png",
                "Assets/Sprites/Woods/Backgrounds/BUSH - BACKGROUND.png",
                "Assets/Sprites/Woods/Backgrounds/WOODS - First.png",
            };

            int[] sortingOrders = new int[] { -20, -18, -16, -14, -12, -10, -8 };
            float[] yOffsets     = new float[] { 4f,   4f,   4f,   4.5f, 5f,   3f,   4f };
            const float BG_WIDTH = 16f;

            for (int layerIdx = 0; layerIdx < bgFiles.Length; layerIdx++)
            {
                var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgFiles[layerIdx]);
                if (bgSprite == null) continue;

                var layerGO = new GameObject($"Layer_{layerIdx}_{Path.GetFileNameWithoutExtension(bgFiles[layerIdx])}");
                layerGO.transform.SetParent(bgRoot.transform, false);

                for (float x = -16; x <= 96; x += BG_WIDTH)
                {
                    var segGO = new GameObject($"Seg_{x}");
                    segGO.transform.SetParent(layerGO.transform, false);
                    segGO.transform.position = new Vector3(x, yOffsets[layerIdx], 0);
                    var sr = segGO.AddComponent<SpriteRenderer>();
                    sr.sprite = bgSprite;
                    sr.sortingOrder = sortingOrders[layerIdx];
                }
            }

            // 7. DECORACIONES DEL BOSQUE
            var oldDeco = GameObject.Find("[WoodsDecorations]");
            if (oldDeco != null) Object.DestroyImmediate(oldDeco);

            var decoRoot = new GameObject("[WoodsDecorations]");
            string decoFolder = "Assets/Sprites/Woods/Decorations";
            if (Directory.Exists(decoFolder))
            {
                (string sName, float x, float y)[] decoPlacements = new (string, float, float)[]
                {
                    ("GRASS 1-1.png", -4f, 0.5f),
                    ("BUSH FOREGROUND 1-1.png", 2f, 0.5f),
                    ("MUSHROOM 1-2.png", 5f, 0.5f),
                    ("GRASS 2-1.png", 7f, 0.5f),
                    ("GRASS 1-2.png", 11f, 2.5f),
                    ("BUSH FOREGROUND 1-2.png", 17f, 1.5f),
                    ("GRASS 3-1.png", 21f, 1.5f),
                    ("BUSH FOREGROUND 1-3.png", 29f, 1.5f),
                    ("GRASS 2-2.png", 35f, 3.5f),
                    ("BUSH FOREGROUND 1-4.png", 47f, 1.5f),
                    ("GRASS 1-1.png", 56f, 1.5f),
                    ("BUSH FOREGROUND 1-5.png", 64f, 2.5f),
                    ("MUSHROOM 1-1.png", 68f, 2.5f),
                    ("GRASS 3-2.png", 72f, 2.5f)
                };

                foreach (var (sName, dx, dy) in decoPlacements)
                {
                    var spr = AssetDatabase.LoadAssetAtPath<Sprite>($"{decoFolder}/{sName}");
                    if (spr == null) continue;
                    var dGO = new GameObject($"Deco_{sName}");
                    dGO.transform.SetParent(decoRoot.transform, false);
                    dGO.transform.position = new Vector3(dx, dy, 0);
                    var sr = dGO.AddComponent<SpriteRenderer>();
                    sr.sprite = spr;
                    sr.sortingOrder = 1;
                }
            }

            // 8. HONGOS TRAMPOLÍN (BouncyMushrooms)
            var oldMush = GameObject.Find("[BouncyMushrooms]");
            if (oldMush != null) Object.DestroyImmediate(oldMush);

            var mushRoot = new GameObject("[BouncyMushrooms]");
            var mushSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Woods/Decorations/MUSHROOM 1-1.png");
            var jumpAudio = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/jump.wav");

            void CreateBouncyMushroom(float x, float y, float force)
            {
                var mGO = new GameObject($"BouncyMushroom_{x}");
                mGO.transform.SetParent(mushRoot.transform, false);
                mGO.transform.position = new Vector3(x, y, 0);
                var sr = mGO.AddComponent<SpriteRenderer>();
                sr.sprite = mushSpr;
                sr.sortingOrder = 2;
                mGO.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

                var col = mGO.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.8f, 0.6f);
                col.offset = new Vector2(0, 0.2f);

                var bm = mGO.AddComponent<BouncyMushroom>();
                bm.bounceForce = force;
                bm.bounceAudio = jumpAudio;
            }

            // Hongo 1: En x = 12.5 sobre el terreno para alcanzar la plataforma secreta en y = 7
            CreateBouncyMushroom(12.5f, 2.3f, 18f);
            // Hongo 2: En x = 42 sobre plataforma de salto para alcanzar la plataforma altísima en y = 9
            CreateBouncyMushroom(42f, 5.3f, 20f);

            // 9. CASETA DE PERRO CHECKPOINT (DogHouseCheckpoint)
            var oldHouse = GameObject.Find("[DogHouseCheckpoint]");
            if (oldHouse != null) Object.DestroyImmediate(oldHouse);

            var houseGO = new GameObject("[DogHouseCheckpoint]");
            houseGO.transform.position = new Vector3(25f, 1.05f, 0f);

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
            houseCp.activeSprite   = houseActive;
            houseCp.activateAudio  = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Collectable.wav");

            // 10. COLECCIONABLES CANINOS (Huesos, Galletas de Vida y Pelotas Secretas)
            var oldPickups = GameObject.Find("[DogPickups]");
            if (oldPickups != null) Object.DestroyImmediate(oldPickups);

            var pickupRoot = new GameObject("[DogPickups]");

            // Cargar frames del hueso giratorio
            var boneAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Pickups/Bone_Spin.png");
            var boneFrames = new List<Sprite>();
            foreach (var a in boneAssets)
            {
                if (a is Sprite s) boneFrames.Add(s);
            }

            var biscuitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/DogBiscuit.png");
            var ballSprite    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Pickups/TennisBall.png");
            var coinAudio     = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Collectable.wav");

            // Desactivar tokens viejos
            var oldTokens = Object.FindObjectsByType<TokenInstance>(FindObjectsInactive.Include);
            foreach (var ot in oldTokens)
            {
                ot.gameObject.SetActive(false);
            }

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

            // Huesos dorados en el camino
            Vector3[] bonePositions = new Vector3[]
            {
                new Vector3(0f, 1.4f, 0),
                new Vector3(2f, 1.4f, 0),
                new Vector3(4f, 1.4f, 0),
                new Vector3(6f, 1.4f, 0),

                // Salto a plataforma flotante 1
                new Vector3(9f, 2.2f, 0),
                new Vector3(11f, 3.2f, 0),
                new Vector3(13f, 3.2f, 0),

                // Meseta intermedia
                new Vector3(18f, 2.2f, 0),
                new Vector3(20f, 2.2f, 0),
                new Vector3(22f, 2.2f, 0),
                new Vector3(28f, 2.2f, 0),
                new Vector3(30f, 2.2f, 0),

                // Plataformas escalonadas
                new Vector3(36f, 4.2f, 0),
                new Vector3(42f, 6.2f, 0),

                // Valle y camino final
                new Vector3(48f, 2.2f, 0),
                new Vector3(52f, 2.2f, 0),
                new Vector3(56f, 2.2f, 0),
                new Vector3(64f, 3.2f, 0),
                new Vector3(67f, 3.2f, 0),
                new Vector3(70f, 3.2f, 0)
            };

            foreach (var bp in bonePositions)
            {
                CreateToken(bp, TokenInstance.DogTokenType.Bone);
            }

            // Galleta de vida curativa frente a la Caseta Checkpoint y en el valle
            CreateToken(new Vector3(27f, 1.5f, 0), TokenInstance.DogTokenType.DogBiscuit);
            CreateToken(new Vector3(54f, 1.5f, 0), TokenInstance.DogTokenType.DogBiscuit);

            // Las 3 Pelotas de Tenis Secretas del Nivel 1
            // Secreto 1: En las ramas altas alcanzables por el primer hongo trampolín
            CreateToken(new Vector3(16f, 8.2f, 0), TokenInstance.DogTokenType.TennisBall);
            // Secreto 2: En la copa altísima del desfiladero alcanzable por el segundo hongo
            CreateToken(new Vector3(41f, 10.2f, 0), TokenInstance.DogTokenType.TennisBall);
            // Secreto 3: En la plataforma oculta del valle
            CreateToken(new Vector3(52f, 5.2f, 0), TokenInstance.DogTokenType.TennisBall);

            // Re-vincular TokenController en la escena
            var tokenCtrl = Object.FindAnyObjectByType<TokenController>();
            if (tokenCtrl != null)
            {
                tokenCtrl.tokens = pickupRoot.GetComponentsInChildren<TokenInstance>();
                EditorUtility.SetDirty(tokenCtrl);
            }

            // 11. POSICIONAR AL JUGADOR Y SPAWNPOINT
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

            // 12. AJUSTAR DEATHZONE (Abismo inferior)
            var deathZoneGO = GameObject.Find("DeathZone");
            if (deathZoneGO != null)
            {
                deathZoneGO.transform.position = new Vector3(35f, -7f, 0f);
                var col = deathZoneGO.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.size = new Vector2(160f, 6f);
                }
                EditorUtility.SetDirty(deathZoneGO);
            }

            // 13. AJUSTAR VICTORYZONE (Meta al final)
            var victoryGO = GameObject.Find("VictoryZone");
            if (victoryGO != null)
            {
                victoryGO.transform.position = new Vector3(75f, 3.5f, 0f);
                var col = victoryGO.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.size = new Vector2(4f, 5f);
                }
                EditorUtility.SetDirty(victoryGO);
            }

            // 14. ENEMIGOS (patrullas de bosque)
            var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include);
            if (enemies.Length > 0)
            {
                enemies[0].transform.position = new Vector3(20f, 2.2f, 0);
                if (enemies[0].path != null)
                {
                    enemies[0].path.startPosition = new Vector2(17f, 2.2f);
                    enemies[0].path.endPosition   = new Vector2(23f, 2.2f);
                    EditorUtility.SetDirty(enemies[0].path);
                }
                EditorUtility.SetDirty(enemies[0].gameObject);

                if (enemies.Length > 1)
                {
                    enemies[1].transform.position = new Vector3(50f, 2.2f, 0);
                    if (enemies[1].path != null)
                    {
                        enemies[1].path.startPosition = new Vector2(47f, 2.2f);
                        enemies[1].path.endPosition   = new Vector2(55f, 2.2f);
                        EditorUtility.SetDirty(enemies[1].path);
                    }
                    EditorUtility.SetDirty(enemies[1].gameObject);
                }

                for (int e = 2; e < enemies.Length; e++)
                {
                    enemies[e].gameObject.SetActive(false);
                    EditorUtility.SetDirty(enemies[e].gameObject);
                }
            }

            // 15. Guardar escena
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log("[ForestLevelBuilder] ✅ ¡Nivel 1 del Bosque construido con éxito!");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("✅ Nivel 1 Mejorado",
                    "¡El Nivel 1 del Bosque ha sido renovado con éxito!\n\n" +
                    "- Fondo Parallax de 7 capas del bosque\n" +
                    "- Huesos Dorados giratorios y Galletas de curación\n" +
                    "- Caseta de perro Checkpoint interactiva\n" +
                    "- Hongos trampolín y 3 Pelotas de Tenis secretas ocultas en las ramas",
                    "¡Genial!");
            }
        }

        private static Tile LoadTile(string tileName)
        {
            string path = $"Assets/Tiles/Woods/{tileName}.asset";
            return AssetDatabase.LoadAssetAtPath<Tile>(path);
        }
    }
}
