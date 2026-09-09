using System.Collections.Generic;
using System.IO;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class WoodsLevelBuilder
    {
        const string TEMPLATE_SCENE     = "Assets/Scenes/Level_01.unity";
        const string TARGET_SCENE_02    = "Assets/Scenes/Level_02.unity";
        const string TARGET_SCENE_WOODS = "Assets/Scenes/Level_Woods.unity";
        const string FLAG_FILE          = "Library/WoodsLevelBuilt_v2.flag";

        static WoodsLevelBuilder()
        {
            EditorApplication.delayCall += AutoBuildIfFirstTime;
        }

        private static void AutoBuildIfFirstTime()
        {
            if (!File.Exists(FLAG_FILE))
            {
                Debug.Log("[WoodsLevelBuilder] Auto-construyendo nivel del bosque...");
                BuildWoodsLevel(false);
            }
        }

        [MenuItem("Tools/Woods/🌲 Construir Nivel del Bosque (Level 2)")]
        [MenuItem("Tools/Woods/Build Woods Level Scene")]
        public static void MenuBuild()
        {
            BuildWoodsLevel(true);
        }

        public static void BuildWoodsLevel(bool showDialog)
        {
            // 1. Asegurar que los assets de Woods estén importados
            WoodsAssetImporter.ProcessAll(false);

            // 2. Cargar tiles de Woods
            Tile tileGrassLeft   = LoadTile("Woods_Tile_0_0");
            Tile tileGrassMid    = LoadTile("Woods_Tile_0_1");
            Tile tileGrassRight  = LoadTile("Woods_Tile_0_2");
            Tile tileDirtLeft    = LoadTile("Woods_Tile_1_0");
            Tile tileDirtMid     = LoadTile("Woods_Tile_1_1");
            Tile tileDirtRight   = LoadTile("Woods_Tile_1_2");
            Tile tileBotLeft     = LoadTile("Woods_Tile_2_0");
            Tile tileBotMid      = LoadTile("Woods_Tile_2_1");
            Tile tileBotRight    = LoadTile("Woods_Tile_2_2");

            // Plataformas flotantes (filas 3 y 4 del spritesheet)
            Tile tilePlatLeft    = LoadTile("Woods_Tile_3_4");
            Tile tilePlatMid     = LoadTile("Woods_Tile_3_5");
            Tile tilePlatRight   = LoadTile("Woods_Tile_3_6");

            if (tileGrassMid == null)
            {
                Debug.LogError("[WoodsLevelBuilder] ❌ ERROR CRÍTICO: No se pudo cargar Woods_Tile_0_1.asset.");
                return;
            }

            // Fallbacks de seguridad para asegurar que siempre haya tiles válidos
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

            // 3. Usar la escena activa si ya está abierta, o cargar TEMPLATE_SCENE
            Scene scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "Level_Woods" && scene.name != "Level_02")
            {
                if (File.Exists(TEMPLATE_SCENE))
                    scene = EditorSceneManager.OpenScene(TEMPLATE_SCENE, OpenSceneMode.Single);
            }

            // 4. Configurar el Grid a tamaño 1x1x1 para que coincida exactamente con los tiles 32x32 (PPU = 32)
            var grid = UnityEngine.Object.FindAnyObjectByType<Grid>();
            if (grid != null)
            {
                grid.cellSize = new Vector3(1f, 1f, 1f);
                EditorUtility.SetDirty(grid);
            }

            // 5. Buscar el Tilemap principal de colisiones ('Level')
            var tilemaps = UnityEngine.Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include);
            Tilemap levelTilemap = null;
            foreach (var tm in tilemaps)
            {
                // Limpiar todos los tiles de todas las capas existentes
                tm.ClearAllTiles();
                EditorUtility.SetDirty(tm);

                if (tm.name == "Level" || tm.name.ToLower().Contains("ground") || tm.name.ToLower().Contains("midground"))
                {
                    if (levelTilemap == null) levelTilemap = tm;
                }
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

            // Asegurar posición cero local del Tilemap para alineación exacta
            levelTilemap.transform.localPosition = Vector3.zero;
            levelTilemap.transform.localScale = Vector3.one;

            // Asegurar que el Tilemap tenga TilemapCollider2D y CompositeCollider2D funcionales
            var tileCollider = levelTilemap.GetComponent<TilemapCollider2D>();
            if (tileCollider == null) tileCollider = levelTilemap.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;

            var rb = levelTilemap.GetComponent<Rigidbody2D>();
            if (rb == null) rb = levelTilemap.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            var compCollider = levelTilemap.GetComponent<CompositeCollider2D>();
            if (compCollider == null) compCollider = levelTilemap.gameObject.AddComponent<CompositeCollider2D>();

            EditorUtility.SetDirty(levelTilemap.gameObject);

            // 6. PINTAR EL TERRENO DEL NIVEL DEL BOSQUE
            // Pintar bloque de terreno sólido con pasto arriba y tierra abajo
            void PaintGround(int startX, int endX, int topY, int bottomY)
            {
                for (int x = startX; x <= endX; x++)
                {
                    for (int y = topY; y >= bottomY; y--)
                    {
                        Tile t;
                        if (y == topY)
                        {
                            if (x == startX) t = tileGrassLeft;
                            else if (x == endX) t = tileGrassRight;
                            else t = tileGrassMid;
                        }
                        else if (y == bottomY)
                        {
                            if (x == startX) t = tileBotLeft;
                            else if (x == endX) t = tileBotRight;
                            else t = tileBotMid;
                        }
                        else
                        {
                            if (x == startX) t = tileDirtLeft;
                            else if (x == endX) t = tileDirtRight;
                            else t = tileDirtMid;
                        }
                        levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                    }
                }
            }

            // Pintar plataforma flotante de salto
            void PaintPlatform(int startX, int endX, int y)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Tile t;
                    if (x == startX) t = tilePlatLeft;
                    else if (x == endX) t = tilePlatRight;
                    else t = tilePlatMid;
                    levelTilemap.SetTile(new Vector3Int(x, y, 0), t);
                }
            }

            // ─── MAPA DETALLADO DEL BOSQUE ───
            // 1. Plataforma Inicial (Spawn): x = -6 hasta x = 8, superficie en y = 0, espesor hasta y = -3
            PaintGround(-6, 8, 0, -3);

            // 2. Primer salto sobre abismo con plataforma flotante
            // Hueco de x = 9 a x = 11
            PaintPlatform(10, 14, 2);

            // 3. Meseta intermedia del bosque: x = 16 a x = 28, superficie en y = 1
            PaintGround(16, 28, 1, -3);

            // Plataforma flotante alta en la meseta intermedia: x = 20 a 25, y = 5
            PaintPlatform(20, 25, 5);

            // 4. Zona de saltos escalonados:
            // Hueco x = 29 a 31
            PaintPlatform(32, 36, 2);
            // Hueco x = 37 a 38
            PaintPlatform(39, 43, 4);

            // 5. Valle de ruinas y árboles: x = 45 a x = 58, superficie en y = 1
            PaintGround(45, 58, 1, -3);

            // Plataforma flotante alta hacia la meta: x = 48 a 53, y = 6
            PaintPlatform(48, 53, 6);

            // 6. Colina final y zona de meta: x = 61 a x = 75, superficie en y = 2
            PaintGround(61, 75, 2, -3);

            // Forzar actualización y serialización del Tilemap en el editor
            levelTilemap.RefreshAllTiles();
            EditorUtility.SetDirty(levelTilemap);
            EditorUtility.SetDirty(levelTilemap.gameObject);

            // 7. FONDOS MULTICAPA DEL BOSQUE (Woods Parallax)
            var oldBgs = GameObject.Find("[WoodsBackgrounds]");
            if (oldBgs != null) UnityEngine.Object.DestroyImmediate(oldBgs);

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

            const float BG_WIDTH = 16f; // 512px / 32 PPU = 16 unidades
            for (int layerIdx = 0; layerIdx < bgFiles.Length; layerIdx++)
            {
                var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgFiles[layerIdx]);
                if (bgSprite == null) continue;

                var layerGO = new GameObject($"Layer_{layerIdx}_{Path.GetFileNameWithoutExtension(bgFiles[layerIdx])}");
                layerGO.transform.SetParent(bgRoot.transform, false);

                // Repetir horizontalmente de x = -16 a x = 90
                for (float x = -16; x <= 90; x += BG_WIDTH)
                {
                    var segGO = new GameObject($"Seg_{x}");
                    segGO.transform.SetParent(layerGO.transform, false);
                    segGO.transform.position = new Vector3(x, yOffsets[layerIdx], 0);
                    var sr = segGO.AddComponent<SpriteRenderer>();
                    sr.sprite = bgSprite;
                    sr.sortingOrder = sortingOrders[layerIdx];
                }
            }

            // 8. DECORACIONES DEL BOSQUE (Hongos, pastos y arbustos)
            var oldDeco = GameObject.Find("[WoodsDecorations]");
            if (oldDeco != null) UnityEngine.Object.DestroyImmediate(oldDeco);

            var decoRoot = new GameObject("[WoodsDecorations]");
            string decoFolder = "Assets/Sprites/Woods/Decorations";
            if (Directory.Exists(decoFolder))
            {
                (string spriteName, float x, float y)[] decoPlacements = new (string, float, float)[]
                {
                    ("MUSHROOM 1-1.png", -4f, 0.5f),
                    ("GRASS 1-1.png",     -2f, 0.5f),
                    ("BUSH FOREGROUND 1-1.png", 2f, 0.5f),
                    ("MUSHROOM 1-2.png",  5f, 0.5f),
                    ("GRASS 2-1.png",     7f, 0.5f),

                    // Plataforma flotante 1
                    ("MUSHROOM 2-1.png", 11f, 2.5f),
                    ("GRASS 1-2.png",    13f, 2.5f),

                    // Meseta intermedia
                    ("BUSH FOREGROUND 1-2.png", 17f, 1.5f),
                    ("GRASS 3-1.png",    19f, 1.5f),
                    ("MUSHROOM 1-1.png", 22f, 5.5f), // En la plataforma alta
                    ("BUSH FOREGROUND 1-3.png", 24f, 5.5f),
                    ("MUSHROOM 1-2.png", 26f, 1.5f),

                    // Plataformas escalonadas
                    ("GRASS 2-2.png",    33f, 2.5f),
                    ("MUSHROOM 2-1.png", 40f, 4.5f),

                    // Valle y colina final
                    ("BUSH FOREGROUND 1-4.png", 46f, 1.5f),
                    ("MUSHROOM 1-1.png", 50f, 6.5f),
                    ("GRASS 1-1.png",    53f, 1.5f),
                    ("BUSH FOREGROUND 1-5.png", 63f, 2.5f),
                    ("MUSHROOM 1-2.png", 67f, 2.5f),
                    ("GRASS 3-2.png",    70f, 2.5f)
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
                    sr.sortingOrder = 1; // Delante del suelo
                }
            }

            // 9. COLOCAR AL JUGADOR Y SPAWNPOINT SOBRE EL PISO SEGURO
            // Superficie inicial en y = 0. SpawnPoint en y = 1.2f
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

            // 10. AJUSTAR DEATHZONE (Abismo inferior bajo todo el nivel)
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

            // 11. AJUSTAR VICTORYZONE (Meta al final de la colina)
            var victoryGO = GameObject.Find("VictoryZone");
            if (victoryGO != null)
            {
                victoryGO.transform.position = new Vector3(72f, 3.5f, 0f);
                var col = victoryGO.GetComponent<BoxCollider2D>();
                if (col != null)
                {
                    col.size = new Vector2(4f, 4f);
                }
                EditorUtility.SetDirty(victoryGO);
            }

            // 12. DIAMANTES / TOKENS ANIMADOS
            var existingTokens = UnityEngine.Object.FindObjectsByType<TokenInstance>(FindObjectsInactive.Include);
            Vector3[] tokenPositions = new Vector3[]
            {
                // Inicio
                new Vector3(0f, 1.5f, 0),
                new Vector3(3f, 1.5f, 0),
                new Vector3(6f, 1.5f, 0),

                // Salto a plataforma flotante 1
                new Vector3(9f, 2.5f, 0),
                new Vector3(11.5f, 3.5f, 0),
                new Vector3(13.5f, 3.5f, 0),

                // Meseta intermedia
                new Vector3(17f, 2.5f, 0),
                new Vector3(21f, 6.2f, 0),
                new Vector3(23f, 6.2f, 0),
                new Vector3(27f, 2.5f, 0),

                // Saltos escalonados
                new Vector3(33f, 3.5f, 0),
                new Vector3(35f, 3.5f, 0),
                new Vector3(40f, 5.5f, 0),
                new Vector3(42f, 5.5f, 0),

                // Valle y plataforma alta
                new Vector3(47f, 2.5f, 0),
                new Vector3(50f, 7.2f, 0),
                new Vector3(52f, 7.2f, 0),
                new Vector3(56f, 2.5f, 0),

                // Camino a la meta
                new Vector3(63f, 3.5f, 0),
                new Vector3(67f, 3.5f, 0)
            };

            var tokenList = new List<TokenInstance>(existingTokens);
            if (tokenList.Count > 0)
            {
                var templateToken = tokenList[0];
                while (tokenList.Count < tokenPositions.Length)
                {
                    var clone = UnityEngine.Object.Instantiate(templateToken.gameObject, templateToken.transform.parent);
                    clone.name = $"Token_Woods_{tokenList.Count}";
                    var inst = clone.GetComponent<TokenInstance>();
                    tokenList.Add(inst);
                }

                for (int i = 0; i < tokenList.Count; i++)
                {
                    if (i < tokenPositions.Length)
                    {
                        tokenList[i].gameObject.SetActive(true);
                        tokenList[i].transform.position = tokenPositions[i];
                        EditorUtility.SetDirty(tokenList[i].gameObject);
                    }
                    else
                    {
                        tokenList[i].gameObject.SetActive(false);
                    }
                }
            }

            // 13. ENEMIGOS (patrullas de bosque)
            var enemies = UnityEngine.Object.FindObjectsByType<EnemyController>(FindObjectsInactive.Include);
            if (enemies.Length > 0)
            {
                enemies[0].transform.position = new Vector3(20f, 2.2f, 0);
                if (enemies[0].path != null)
                {
                    enemies[0].path.startPosition = new Vector2(17f, 2.2f);
                    enemies[0].path.endPosition   = new Vector2(25f, 2.2f);
                    EditorUtility.SetDirty(enemies[0].path);
                }
                EditorUtility.SetDirty(enemies[0].gameObject);

                if (enemies.Length > 1)
                {
                    enemies[1].transform.position = new Vector3(51f, 2.2f, 0);
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
                }
            }

            // 14. VIRTUAL CAMERA Y CONFINER
            var confinerGO = GameObject.Find("CinemachineConfiner");
            if (confinerGO != null)
            {
                var bc = confinerGO.GetComponent<BoxCollider2D>();
                if (bc != null)
                {
                    bc.offset = new Vector2(35f, 3f);
                    bc.size   = new Vector2(130f, 30f);
                    EditorUtility.SetDirty(bc);
                }
            }

            // 15. MARCAR ESCENA SUCIA Y GUARDAR
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, TARGET_SCENE_02);
            EditorSceneManager.SaveScene(scene, TARGET_SCENE_WOODS);

            AddSceneToBuildSettings(TARGET_SCENE_02);
            AddSceneToBuildSettings(TARGET_SCENE_WOODS);

            File.WriteAllText(FLAG_FILE, "built_v2");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[WoodsLevelBuilder] ✅ ¡NIVEL DEL BOSQUE CONSTRUIDO CON PISO SÓLIDO Y FÍSICAS! ({TARGET_SCENE_02})");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Nivel del Bosque Listo",
                    "¡El Nivel del Bosque ha sido creado con piso sólido, colisiones y plataformas!\n\n" +
                    "• Grid ajustado a 1x1x1 con física de colisiones activas.\n" +
                    "• El perro ahora cae y aterriza perfectamente en el pasto.\n" +
                    "• Saltos, plataformas flotantes, 20 diamantes y enemigos colocados.\n\n" +
                    "Presiona Play para jugar.",
                    "¡A Jugar!");
            }
        }

        private static Tile LoadTile(string tileName)
        {
            string path = $"{WoodsAssetImporter.TILES_OUT_PATH}/{tileName}.asset";
            return AssetDatabase.LoadAssetAtPath<Tile>(path);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            if (!File.Exists(path)) return;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenes.Exists(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
