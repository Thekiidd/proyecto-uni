using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Platformer.TopDown;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye una nueva escena de aldea completa usando los tiles de Pixel Crawler.
    /// Menú: Tools > Village > Build Pixel Crawler Village
    ///
    /// Layout de la aldea (cada celda = 1 Unity unit = 16px):
    ///   - Plaza central de piedra (8x8)
    ///   - Taberna al norte (6x5)
    ///   - 4 casas en esquinas
    ///   - Zona de granja al sureste
    ///   - Pozo en el centro de la plaza
    ///   - Árboles y vegetación en bordes
    ///   - Murallas perimetrales
    ///   - 9 NPCs con diálogos completos
    /// </summary>
    public static class PixelCrawlerVillageBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/PixelCrawler_Village.unity";
        private const string PC_ROOT = "Assets/Sprites/PixelCrawler";

        // Tilesets
        private const string FLOOR_TS  = PC_ROOT + "/Environment/Tilesets/Floors_Tiles.png";
        private const string WALL_TS   = PC_ROOT + "/Environment/Tilesets/Wall_Tiles.png";
        private const string WATER_TS  = PC_ROOT + "/Environment/Tilesets/Water_tiles.png";
        private const string VEGE_TS   = PC_ROOT + "/Environment/Props/Static/Vegetation.png";
        private const string FARM_TS   = PC_ROOT + "/Environment/Props/Static/Farm.png";
        private const string FURN_TS   = PC_ROOT + "/Environment/Props/Static/Furniture.png";

        // Buildings
        private const string BUILD_WALLS = PC_ROOT + "/Environment/Structures/Buildings/Walls.png";
        private const string BUILD_ROOFS = PC_ROOT + "/Environment/Structures/Buildings/Roofs.png";
        private const string BUILD_PROPS = PC_ROOT + "/Environment/Structures/Buildings/Props.png";

        // Trees
        private const string TREE1_S2 = PC_ROOT + "/Environment/Props/Static/Trees/Model_01/Size_02.png";
        private const string TREE1_S3 = PC_ROOT + "/Environment/Props/Static/Trees/Model_01/Size_03.png";
        private const string TREE2_S3 = PC_ROOT + "/Environment/Props/Static/Trees/Model_02/Size_03.png";
        private const string TREE3_S3 = PC_ROOT + "/Environment/Props/Static/Trees/Model_03/Size_03.png";

        // NPC sprites
        private const string PEASANT_IDLE = PC_ROOT + "/Entities/Npc's/Citizen_F/Peasant_A/Idle/Idle-Sheet.png";
        private const string PEASANT_WALK = PC_ROOT + "/Entities/Npc's/Citizen_F/Peasant_A/Walk/Walk-Sheet.png";
        private const string KNIGHT_IDLE  = PC_ROOT + "/Entities/Npc's/Knight/Idle/Idle-Sheet.png";
        private const string WIZZARD_IDLE = PC_ROOT + "/Entities/Npc's/Wizzard/Idle/Idle-Sheet.png";
        private const string ROGUE_IDLE   = PC_ROOT + "/Entities/Npc's/Rogue/Idle/Idle-Sheet.png";

        // Dialogues
        private const string DLG_FOLDER = "Assets/Data/Dialogues/Village";

        // Tamaño del mapa
        private const int MAP_W = 32;
        private const int MAP_H = 32;

        // ── Layout del mapa ────────────────────────────────────────────────────
        // G=suelo piedra  D=suelo tierra  W=pared  T=puerta/entrada casa
        // H=casa          B=taberna       F=granja  ~=agua  .=exterior
        // Nota: [0,0] = esquina inferior izquierda
        private static readonly string[] MAP = new string[]
        {
            // Fila 31 (norte)
            "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",
            "W..............................TW",
            "W.BBBBBBBB.....HHHHH...HHHHH..TW",
            "W.BBBBBBBB.....HHHHH...HHHHH...W",
            "W.BBBBBBBB.....HHHHH...HHHHH...W",
            "W.BBBBBBBB.....TTTTT...TTTTT...W",
            "W..........DDDD................W",
            "W.GGGGGGGGGGGGGGGGGGGGGGG......W",
            "W.GGGGGGGGGGGGGGGGGGGGGGG......W",
            "W.GGGGG..PPPPP..GGGGGGGGG......W",  // P=pozo
            "W.GGGGGGGGGGGGGGGGGGGGGGG......W",
            "W.GGGGGGGGGGGGGGGGGGGGGGG......W",
            "W..........DDDD...FFFFF.FFFFF..W",
            "W.HHHHH....DDDD...FFFFF.FFFFF..W",
            "W.HHHHH....DDDD...FFFFF.FFFFF..W",
            "W.HHHHH....DDDD...FFFFF.FFFFF..W",
            "W.TTTTT....DDDD................W",
            "W..........DDDD................W",
            "W..........DDDD................W",
            "W.HHHHH....DDDD...FFFFF........W",
            "W.HHHHH....DDDD...FFFFF...~~~~~W",
            "W.HHHHH....DDDD...FFFFF...~~~~~W",
            "W.TTTTT....DDDD...........~~~~~W",
            "W..........DDDD...........~~~~~W",
            "W..........DDDD................W",
            "W....................HHHHH.....W",
            "W....................HHHHH.....W",
            "W....................HHHHH.....W",
            "W....................TTTTT.....W",
            "W..............................W",
            "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",  // Fila 1
            "WWWWWWWWWWWWWWWWWWWWWWWWWWWWWWW",  // Fila 0 (sur)
        };

        [MenuItem("Tools/Village/Build Pixel Crawler Village")]
        public static void Build()
        {
            bool ok = EditorUtility.DisplayDialog("PixelCrawlerVillageBuilder",
                "Esto creará la escena:\n" + SCENE_PATH +
                "\n\nSe generará una aldea completa con:\n" +
                "• Plaza central de piedra\n• Taberna y 4 casas\n• Zona de granja\n• Lago\n• 9 NPCs con diálogos\n• Vegetación y árboles\n\n¿Continuar?",
                "¡Sí, construir!", "Cancelar");
            if (!ok) return;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Forzar importación de sprites como Multiple
            SetupPixelCrawlerImportSettings();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara ─────────────────────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(8f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 7f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.15f, 0.22f, 0.15f);
            camGo.AddComponent<AudioListener>();
            var follow = camGo.AddComponent<TopDownCameraFollow>();
            follow.minBounds  = new Vector2(-1f, -16f);
            follow.maxBounds  = new Vector2(18f, 16f);
            follow.smoothSpeed = 6f;

            // ── Suelo base (color verde oscuro, toda la escena) ─────────────────
            BuildGroundLayer();

            // ── Tiles del mapa ──────────────────────────────────────────────────
            BuildMapTiles();

            // ── Edificios ───────────────────────────────────────────────────────
            BuildBuildings();

            // ── Vegetación y Árboles ───────────────────────────────────────────
            BuildVegetation();

            // ── Colisiones perimetrales ────────────────────────────────────────
            BuildBoundaryColliders();

            // ── NPCs ────────────────────────────────────────────────────────────
            BuildNPCs(follow);

            // ── Jugador ─────────────────────────────────────────────────────────
            var playerGo = BuildPlayer(follow);

            // ── Guardar escena ──────────────────────────────────────────────────
            string dir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("¡Villa Construida!",
                "✅ PixelCrawler_Village.unity creada con éxito.\n\n" +
                "Para añadir los diálogos completos:\n" +
                "  Tools > Village > Generate NPC Dialogues\n\n" +
                "Para construir el interior de casas:\n" +
                "  Tools > Village > Build House Interior Scene\n\n" +
                "Luego agrega esta escena al flujo en lugar de Village_Scene si lo prefieres, o úsala como zona extra.", "OK");
        }

        // ── Suelo base ─────────────────────────────────────────────────────────
        private static void BuildGroundLayer()
        {
            Sprite grass = LoadSprite(VEGE_TS, 0);

            var go = new GameObject("[Ground_Base]");
            go.transform.position = new Vector3(MAP_W / 2f - 0.5f, -(MAP_H / 2f) + 0.5f, 0f);
            go.transform.localScale = new Vector3(MAP_W, MAP_H, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.25f, 0.38f, 0.22f);
            sr.sortingOrder = -200;
        }

        // ── Tiles del mapa ──────────────────────────────────────────────────────
        private static void BuildMapTiles()
        {
            Sprite stoneFloor = LoadSprite(FLOOR_TS, 0);   // primer tile de suelo
            Sprite dirtFloor  = LoadSprite(FLOOR_TS, 5);   // tile de tierra
            Sprite water      = LoadSprite(WATER_TS, 0);
            Sprite wallTile   = LoadSprite(WALL_TS, 0);

            var tileRoot = new GameObject("[Map_Tiles]");
            int rows = MAP.Length;

            for (int row = 0; row < rows; row++)
            {
                string line = MAP[rows - 1 - row]; // invertir Y
                for (int col = 0; col < line.Length && col < MAP_W; col++)
                {
                    char cell = line[col];
                    Sprite spr = null;
                    int order = -100;

                    switch (cell)
                    {
                        case 'G': spr = stoneFloor; order = -90; break;
                        case 'D': spr = dirtFloor;  order = -90; break;
                        case '~': spr = water;      order = -90; break;
                        case 'W': spr = wallTile;   order = -80; break;
                        case 'T': spr = dirtFloor;  order = -90; break; // umbral puerta
                        case 'P': spr = stoneFloor; order = -90; break; // plaza pozo
                        case 'F': spr = dirtFloor;  order = -90; break; // granja
                    }

                    if (spr == null) continue;

                    var tile = new GameObject($"Tile_{col}_{row}");
                    tile.transform.parent   = tileRoot.transform;
                    tile.transform.position = new Vector3(col - MAP_W / 2f + 0.5f, row - MAP_H / 2f + 0.5f, 0f);

                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite       = spr;
                    sr.sortingOrder = order;

                    // Collider para paredes
                    if (cell == 'W')
                    {
                        var col2d = tile.AddComponent<BoxCollider2D>();
                        col2d.size = new Vector2(1f, 1f);
                    }
                    // Trigger para agua
                    if (cell == '~')
                    {
                        sr.color = new Color(0.3f, 0.55f, 0.9f, 0.85f);
                        var col2d = tile.AddComponent<BoxCollider2D>();
                        col2d.isTrigger = true;
                    }
                }
            }
        }

        // ── Edificios ───────────────────────────────────────────────────────────
        private static void BuildBuildings()
        {
            var root = new GameObject("[Buildings]");
            Sprite wallSpr  = LoadSprite(BUILD_WALLS, 0);
            Sprite roofSpr  = LoadSprite(BUILD_ROOFS, 0);

            // Taberna (norte)
            CreateBuilding(root.transform, "Taberna_Norte",
                pos: new Vector3(-9f, 10f, 0f), sizeX: 5, sizeY: 4,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Casa 1 — noroeste
            CreateBuilding(root.transform, "Casa_Noroeste",
                pos: new Vector3(-13f, 4f, 0f), sizeX: 4, sizeY: 3,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Casa 2 — noreste
            CreateBuilding(root.transform, "Casa_Noreste",
                pos: new Vector3(-2f, 4f, 0f), sizeX: 4, sizeY: 3,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Casa 3 — suroeste 1
            CreateBuilding(root.transform, "Casa_Suroeste_1",
                pos: new Vector3(-13f, -3f, 0f), sizeX: 4, sizeY: 3,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Casa 4 — suroeste 2
            CreateBuilding(root.transform, "Casa_Suroeste_2",
                pos: new Vector3(-13f, -9f, 0f), sizeX: 4, sizeY: 3,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Casa 5 — sureste (mercader)
            CreateBuilding(root.transform, "Casa_Mercader",
                pos: new Vector3(5f, -7f, 0f), sizeX: 4, sizeY: 3,
                wallSpr, roofSpr, withDoor: true, doorScene: "House_Interior");

            // Pozo (centro de plaza)
            CreateWell(root.transform, new Vector3(-3f, 1f, 0f));
        }

        private static void CreateBuilding(Transform parent, string name,
            Vector3 pos, int sizeX, int sizeY,
            Sprite wallSpr, Sprite roofSpr,
            bool withDoor = false, string doorScene = "House_Interior")
        {
            var go = new GameObject(name);
            go.transform.parent   = parent;
            go.transform.position = pos;

            // Paredes (cuerpo)
            var body = new GameObject("Body");
            body.transform.parent = go.transform;
            body.transform.localPosition = new Vector3(sizeX / 2f - 0.5f, sizeY / 2f - 0.5f, 0f);
            var bodySR = body.AddComponent<SpriteRenderer>();
            bodySR.sprite       = wallSpr;
            bodySR.sortingOrder = 10;
            bodySR.color        = new Color(0.85f, 0.78f, 0.65f);
            body.transform.localScale = new Vector3(sizeX, sizeY, 1f);

            // Collider del cuerpo
            var bodyCol = body.AddComponent<BoxCollider2D>();
            bodyCol.size = new Vector2(1f, 1f); // en espacio local (scale lo estira)

            // Techo
            var roof = new GameObject("Roof");
            roof.transform.parent = go.transform;
            roof.transform.localPosition = new Vector3(sizeX / 2f - 0.5f, sizeY + 0.5f, 0f);
            var roofSR = roof.AddComponent<SpriteRenderer>();
            roofSR.sprite       = roofSpr;
            roofSR.sortingOrder = 500 - Mathf.RoundToInt((pos.y + sizeY) * 10f);
            roofSR.color        = new Color(0.7f, 0.35f, 0.25f);
            roof.transform.localScale = new Vector3(sizeX, 1.5f, 1f);

            // Puerta de entrada
            if (withDoor)
            {
                var door = new GameObject("Door_Trigger");
                door.transform.parent = go.transform;
                door.transform.localPosition = new Vector3(sizeX / 2f - 0.5f, -0.25f, 0f);

                var doorCol2 = door.AddComponent<BoxCollider2D>();
                doorCol2.size      = new Vector2(1.2f, 0.5f);
                doorCol2.isTrigger = true;

                var trans = door.AddComponent<SceneTransition>();
                trans.targetScene   = doorScene;
                trans.promptMessage = $"[E] Entrar a {name.Replace("_", " ")}";
                trans.fadeDuration  = 0.4f;
            }
        }

        private static void CreateWell(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Pozo_Central");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.5f, 0.4f, 0.3f);
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);
            go.transform.localScale = Vector3.one * 0.8f;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.4f;

            // Interactable simple
            var npc = go.AddComponent<VillageNPC>();
            npc.npcDisplayName     = "Pozo del Pueblo";
            npc.patrolDistance     = 0f;
            npc.inlineDialogueLines = new[] {
                "Un viejo pozo de piedra. El agua está fresca y cristalina.",
                "Cuentan que en el fondo hay una moneda por cada deseo pedido en los últimos 100 años.",
                "El agua del pozo brilla ligeramente por las noches. Nadie sabe por qué."
            };
        }

        // ── Vegetación ─────────────────────────────────────────────────────────
        private static void BuildVegetation()
        {
            var root = new GameObject("[Vegetation]");

            // Árboles grandes en bordes
            PlaceTree(root.transform, TREE1_S3, new Vector3(-15f,  12f, 0f), 2.5f);
            PlaceTree(root.transform, TREE2_S3, new Vector3(-14f,  9f,  0f), 2f);
            PlaceTree(root.transform, TREE3_S3, new Vector3( 8f,  12f, 0f), 2.5f);
            PlaceTree(root.transform, TREE1_S3, new Vector3( 9f,   8f, 0f), 2f);
            PlaceTree(root.transform, TREE2_S3, new Vector3(-15f, -12f, 0f), 2.5f);
            PlaceTree(root.transform, TREE3_S3, new Vector3(-14f, -10f, 0f), 2f);
            PlaceTree(root.transform, TREE1_S2, new Vector3( 8f, -10f,  0f), 2f);
            PlaceTree(root.transform, TREE2_S3, new Vector3( 9f, -13f,  0f), 2.5f);

            // Arbustos de vegetación (tiles de Vegetation.png)
            Sprite vegeSprite = LoadSprite(VEGE_TS, 0);
            PlaceDecoration(root.transform, "Arbusto_1", vegeSprite, new Vector3(-7f,  13f, 0f), 1f, new Color(0.3f, 0.7f, 0.3f));
            PlaceDecoration(root.transform, "Arbusto_2", vegeSprite, new Vector3( 4f,  13f, 0f), 1f, new Color(0.3f, 0.7f, 0.3f));
            PlaceDecoration(root.transform, "Arbusto_3", vegeSprite, new Vector3(-7f, -13f, 0f), 1f, new Color(0.3f, 0.7f, 0.3f));
            PlaceDecoration(root.transform, "Flores_1",  vegeSprite, new Vector3( 1f,   2f, 0f), 0.7f, new Color(0.9f, 0.6f, 0.8f));
            PlaceDecoration(root.transform, "Flores_2",  vegeSprite, new Vector3(-5f,   2f, 0f), 0.7f, new Color(0.9f, 0.8f, 0.3f));

            // Props de granja
            Sprite farmSprite = LoadSprite(FARM_TS, 0);
            PlaceDecoration(root.transform, "Heno_1",    farmSprite, new Vector3( 4f, -5f, 0f), 1f, Color.white);
            PlaceDecoration(root.transform, "Heno_2",    farmSprite, new Vector3( 6f, -5f, 0f), 1f, Color.white);
            PlaceDecoration(root.transform, "Heno_3",    farmSprite, new Vector3( 4f, -8f, 0f), 1f, Color.white);
            PlaceDecoration(root.transform, "Barril_1",  farmSprite, new Vector3( 6f, -8f, 0f), 0.9f, new Color(0.7f, 0.5f, 0.3f));
        }

        private static void PlaceTree(Transform parent, string path, Vector3 pos, float scale)
        {
            Sprite spr = LoadSprite(path, 0);
            if (spr == null) return;

            var go = new GameObject("Tree");
            go.transform.parent      = parent;
            go.transform.position    = pos;
            go.transform.localScale  = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.35f / scale;
            col.offset = new Vector2(0f, -0.4f / scale);
        }

        private static void PlaceDecoration(Transform parent, string name, Sprite spr,
            Vector3 pos, float scale, Color color)
        {
            if (spr == null) return;
            var go = new GameObject(name);
            go.transform.parent     = parent;
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.color        = color;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);
        }

        // ── Colisiones perimetrales ─────────────────────────────────────────────
        private static void BuildBoundaryColliders()
        {
            var root = new GameObject("[Boundaries]");
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType    = RigidbodyType2D.Static;
            rb.gravityScale = 0f;

            float hw = MAP_W / 2f;
            float hh = MAP_H / 2f;
            CreateWallCollider(root, new Vector2(0f,   hh + 0.5f), new Vector2(MAP_W + 2f, 1f));
            CreateWallCollider(root, new Vector2(0f,  -hh - 0.5f), new Vector2(MAP_W + 2f, 1f));
            CreateWallCollider(root, new Vector2( hw + 0.5f, 0f),  new Vector2(1f, MAP_H + 2f));
            CreateWallCollider(root, new Vector2(-hw - 0.5f, 0f),  new Vector2(1f, MAP_H + 2f));
        }

        private static void CreateWallCollider(GameObject root, Vector2 offset, Vector2 size)
        {
            var col = root.AddComponent<BoxCollider2D>();
            col.offset = offset;
            col.size   = size;
        }

        // ── NPCs ────────────────────────────────────────────────────────────────
        private static void BuildNPCs(TopDownCameraFollow cameraFollow)
        {
            var root = new GameObject("[NPCs]");

            // Datos: (nombre_obj, nombreDisplay, spritePath, pos, patrol, inline fallback)
            CreateVillageNPC(root.transform, "Sabio_Eldor",
                sprite:    LoadSprite(WIZZARD_IDLE, 0),
                dialogueAsset: "Sabio_Eldor",
                pos:       new Vector3(-9f, 2f,  0f),
                patrol:    0.5f,
                scale:     2f,
                lines: new[]{"¡Espera! Hay algo importante que debes saber sobre los cristales de luz..."});

            CreateVillageNPC(root.transform, "Maya_Tejedora",
                sprite:    LoadSprite(PEASANT_IDLE, 0),
                dialogueAsset: "Aldeana_Maya",
                pos:       new Vector3(-11f, 0f, 0f),
                patrol:    2f,
                scale:     2f,
                lines: new[]{"¡Hola! ¿Eres nuevo en la aldea? ¡Bienvenido a Lumina!"});

            CreateVillageNPC(root.transform, "Rosa_Panadera",
                sprite:    LoadSprite(PEASANT_IDLE, 0),
                dialogueAsset: "Aldeana_Rosa",
                pos:       new Vector3(-6f, -1f, 0f),
                patrol:    1.5f,
                scale:     2f,
                lines: new[]{"¡Acabo de sacar pan del horno! ¿Quieres probar?"});

            CreateVillageNPC(root.transform, "Lena_Herbolaria",
                sprite:    LoadSprite(PEASANT_IDLE, 0),
                dialogueAsset: "Aldeana_Lena",
                pos:       new Vector3(-4f,  4f, 0f),
                patrol:    1.8f,
                scale:     2f,
                lines: new[]{"Shh... ¿escuchas lo que dicen las plantas?"});

            CreateVillageNPC(root.transform, "Guardia_Aldric",
                sprite:    LoadSprite(KNIGHT_IDLE, 0),
                dialogueAsset: "Guardia_Aldric",
                pos:       new Vector3(-15f, -1f, 0f),
                patrol:    0.8f,
                scale:     2f,
                lines: new[]{"¡Alto! Esta es la entrada a la Aldea Lumina."});

            CreateVillageNPC(root.transform, "Mago_Erwin",
                sprite:    LoadSprite(WIZZARD_IDLE, 0),
                dialogueAsset: "Mago_Erwin",
                pos:       new Vector3(6f, 3f, 0f),
                patrol:    0.3f,
                scale:     2f,
                lines: new[]{"Hmm... tus ondas áuricas son compatibles con la frecuencia cristalina."});

            CreateVillageNPC(root.transform, "Lili_Nina",
                sprite:    LoadSprite(PEASANT_IDLE, 0),
                dialogueAsset: "Lili_Nina",
                pos:       new Vector3(-2f, -3f, 0f),
                patrol:    2.5f,
                scale:     1.5f,
                lines: new[]{"¡¡¡PERRITOOO!!! ¡Qué bonito!"});

            CreateVillageNPC(root.transform, "Granjero_Tomas",
                sprite:    LoadSprite(PEASANT_IDLE, 0),
                dialogueAsset: "Granjero_Tomas",
                pos:       new Vector3(4f, -4f, 0f),
                patrol:    2f,
                scale:     2f,
                lines: new[]{"¡Hola! ¿Vienes a comprar verduras de temporada?"});

            CreateVillageNPC(root.transform, "Mercader_Bruno",
                sprite:    LoadSprite(ROGUE_IDLE, 0),
                dialogueAsset: "Mercader_Bruno",
                pos:       new Vector3(7f, -6f, 0f),
                patrol:    1f,
                scale:     2f,
                lines: new[]{"¡Bienvenido al Emporium de Bruno! ¡El mejor comercio del reino!"});
        }

        private static void CreateVillageNPC(Transform parent, string objName, Sprite sprite,
            string dialogueAsset, Vector3 pos, float patrol, float scale, string[] lines)
        {
            var go = new GameObject(objName);
            go.transform.parent     = parent;
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = sprite;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType      = RigidbodyType2D.Kinematic;
            rb.gravityScale  = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.offset = new Vector2(0f, -0.3f);

            var npc = go.AddComponent<VillageNPC>();
            npc.patrolDistance     = patrol;
            npc.moveSpeed          = 0.7f;
            npc.inlineDialogueLines = lines;
            npc.npcDisplayName     = objName.Replace("_", " ");

            // Asignar ScriptableObject de diálogo si existe
            string dlgPath = $"{DLG_FOLDER}/{dialogueAsset}.asset";
            var dlgData = AssetDatabase.LoadAssetAtPath<Platformer.TopDown.NPCDialogueData>(dlgPath);
            if (dlgData != null) npc.dialogueData = dlgData;
        }

        // ── Jugador ─────────────────────────────────────────────────────────────
        private static GameObject BuildPlayer(TopDownCameraFollow cameraFollow)
        {
            var go = new GameObject("PlayerDog");
            go.tag = "Player";
            go.transform.position = new Vector3(-8f, 0f, 0f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 500;

            // Intentar cargar sprite del perro actual
            var dogSprites = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/Dogs" });
            if (dogSprites.Length > 0)
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(dogSprites[0]));

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.32f;
            col.offset = new Vector2(0f, -0.15f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale   = 0f;
            rb.constraints    = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var ctrl = go.AddComponent<TopDownDogController>();
            ctrl.spriteRenderer = sr;

            cameraFollow.target = go.transform;
            return go;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────
        private static Sprite LoadSprite(string path, int index)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            int count = 0;
            foreach (var a in all)
            {
                if (a is Sprite sp)
                {
                    if (count == index) return sp;
                    count++;
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void SetupPixelCrawlerImportSettings()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { PC_ROOT });
            int processed = 0;
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (!p.EndsWith(".png")) continue;

                var importer = AssetImporter.GetAtPath(p) as TextureImporter;
                if (importer == null) continue;

                bool changed = false;
                if (importer.filterMode != FilterMode.Point)
                { importer.filterMode = FilterMode.Point; changed = true; }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }

                var settings = importer.GetDefaultPlatformTextureSettings();
                if (importer.spritePixelsPerUnit != 16)
                { importer.spritePixelsPerUnit = 16; changed = true; }

                if (changed) { importer.SaveAndReimport(); processed++; }
            }
            if (processed > 0)
                Debug.Log($"[PixelCrawlerVillageBuilder] Reimportados {processed} sprites con PPU=16 Point filter.");
        }

        private static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in list) if (s.path == path) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
