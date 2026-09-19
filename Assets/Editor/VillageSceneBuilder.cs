using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Platformer.Core;
using Platformer.Dialogue;
using Platformer.TopDown;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class VillageSceneBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Village_Scene.unity";
        private const string BUILD_KEY = "VillageScene_Build_v6";

        static VillageSceneBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!File.Exists(SCENE_PATH) || !EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[VillageSceneBuilder] Construyendo Village_Scene con escaleras transitables y NPCs en lugares naturales...");
                BuildVillageScene(false);
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        [MenuItem("Tools/Village/🏡 Construir Escena del Pueblo (Village_Scene)")]
        public static void BuildSceneMenu()
        {
            BuildVillageScene(true);
        }

        public static void BuildVillageScene(bool showDialog)
        {
            // 1. Asegurar configuración de sprites (PPU 16, Point filter, etc.)
            VillageAssetImporter.ProcessVillageAssets(false);

            // 2. Crear nueva escena limpia
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Cámara Top-Down
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-5f, -2.5f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.0f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.18f, 0.28f, 0.18f);
            camGO.AddComponent<AudioListener>();
            var camFollow = camGO.AddComponent<TopDownCameraFollow>();
            camFollow.minBounds = new Vector2(-10f, -14f);
            camFollow.maxBounds = new Vector2(10f, 14f);
            camFollow.smoothSpeed = 6f;

            // 4. Singletons y Gestores
            var setupGO = new GameObject("[GameSetup]");
            setupGO.AddComponent<GameData>();
            setupGO.AddComponent<SceneLoader>();
            setupGO.AddComponent<DialogueManager>();

            // 5. Terreno Base del Pueblo (640x640 px = 40x40 Unity units a PPU=16)
            string mapSpritePath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.png";
            var mapSprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapSpritePath);

            var groundGO = new GameObject("Village_Ground");
            groundGO.transform.position = Vector3.zero;
            var groundSR = groundGO.AddComponent<SpriteRenderer>();
            groundSR.sprite = mapSprite;
            // Sorting order muy bajo para que el fondo SIEMPRE esté detrás de todo (el perro usa 300-700)
            groundSR.sortingOrder = -1000;

            // 6. Raíz de Colisiones
            var collidersRoot = new GameObject("[Colliders]");
            var collidersRB = collidersRoot.AddComponent<Rigidbody2D>();
            collidersRB.bodyType = RigidbodyType2D.Static;

            // Límites del mapa (40x40 unidades, centrado en 0,0: X de -20 a 20, Y de -20 a 20)
            // Pared Norte
            CreateBoundaryCollider(collidersRoot, new Vector2(0, 20.5f), new Vector2(42f, 1f));
            // Pared Sur
            CreateBoundaryCollider(collidersRoot, new Vector2(0, -20.5f), new Vector2(42f, 1f));
            // Pared Este
            CreateBoundaryCollider(collidersRoot, new Vector2(20.5f, 0), new Vector2(1f, 42f));
            // Pared Oeste (dividida para dejar paso al camino de salida en Y de 8 a 11)
            CreateBoundaryCollider(collidersRoot, new Vector2(-20.5f, 15.5f), new Vector2(1f, 9f));
            CreateBoundaryCollider(collidersRoot, new Vector2(-20.5f, -6f), new Vector2(1f, 27f));

            // 7. Parsear TMX para generar colisiones automáticas de Agua, Barrancos (excluyendo escaleras) y Objetos con Y-Sorting
            var propsRoot = new GameObject("[Village_Props_Y_Sorted]");
            string tmxPath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.tmx";
            ParseTMXColliders(tmxPath, collidersRoot, propsRoot);

            // 8. Portón de Salida a la Aventura (en el camino oeste, Y = 9.5)
            var gateGO = new GameObject("Village_Gate_Exit");
            gateGO.transform.position = new Vector3(-18.0f, 9.5f, 0);
            var gateSR = gateGO.AddComponent<SpriteRenderer>();
            string gateSpritePath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Art/Buildings/CityWall_Gate_1.png";
            gateSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(gateSpritePath);
            gateSR.sortingOrder = 450; // Delante del fondo, perro pasa a través

            // Trigger de salida al Nivel 1 en el arco del portón
            var gateTriggerGO = new GameObject("GateTrigger");
            gateTriggerGO.transform.SetParent(gateGO.transform, false);
            gateTriggerGO.transform.localPosition = new Vector3(0, -0.5f, 0);
            var gateTriggerCol = gateTriggerGO.AddComponent<BoxCollider2D>();
            gateTriggerCol.isTrigger = true;
            gateTriggerCol.size = new Vector2(2.5f, 3.0f);
            var gateTriggerComp = gateTriggerGO.AddComponent<VillageGateTrigger>();
            gateTriggerComp.targetLevelName = "Level_01";
            gateTriggerComp.interactRadius = 2.5f;

            // 9. NPCs y Habitantes ubicados en lugares naturales del suelo / caminos
            var npcRoot = new GameObject("[NPCs]");
            Sprite villagerSprite = LoadFirstCharacterSprite();

            // NPC 1: Sabio Eldor (En la plaza central, camino de tierra despejado entre la mesa de madera y las escaleras)
            CreateNPC(npcRoot, "Sabio_Eldor", villagerSprite, new Vector3(-6.0f, -1.0f, 0), new Color(0.95f, 0.88f, 0.75f),
                npcName: "Sabio Eldor",
                lines: new string[]
                {
                    "¡Ah, fiel guardián canino! Me alegra tanto que hayas venido.",
                    "Una extraña sombra azotó el reino anoche. Los Cristales de Luz fueron robados y dispersados a través del bosque.",
                    "Los aldeanos están asustados, pero yo sé que tu valentía y rapidez pueden salvarnos.",
                    "Toma estos 5 diamantes que recolectamos para ayudarte en tu camino.",
                    "¡Sube por las escaleras de piedra de la plaza o sigue el camino al portón del oeste para salir a la aventura!"
                },
                rewardCoins: 5);

            // NPC 2: Lili (la Niña, en el claro abierto con flores frente a la casa este)
            CreateNPC(npcRoot, "Lili", villagerSprite, new Vector3(10.0f, 5.0f, 0), new Color(1f, 0.82f, 0.88f),
                npcName: "Lili",
                lines: new string[]
                {
                    "¡Hola perrito lindo! *te acaricia suavemente las orejitas*",
                    "¡Eres tan suave y veloz! Mi abuelo me contó que puedes saltar sobre cualquier obstáculo.",
                    "Ten mucho cuidado en el bosque. Hay slimes verdes, pero si saltas encima de ellos los vencerás.",
                    "¡Estaré esperándote aquí en el jardín con las flores para darte más caricias y premios cuando regreses!"
                });

            // NPC 3: Granjero Tomás (En el camino empedrado frente a la cerca de su granja)
            CreateNPC(npcRoot, "Granjero_Tomas", villagerSprite, new Vector3(0.0f, -7.5f, 0), new Color(0.85f, 0.95f, 0.85f),
                npcName: "Granjero Tomás",
                lines: new string[]
                {
                    "¡Hola amiguito! Cuidando la cosecha de trigo como todos los días.",
                    "Si caes al vacío en los bosques o montañas, mantén la calma y calcula bien el impulso de tus patitas.",
                    "¡Todo el pueblo cree en ti, noble guardián!"
                });

            // Mascota amiga: San Bernardo durmiendo plácidamente sobre el pasto junto a la cerca de la granja
            var sleepingDogSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Dogs/Saint-Bernard/Saint-Bernard-lying-down.png");
            if (sleepingDogSprite != null)
            {
                var dogFriend = new GameObject("Perro_Amigo_Dormilon");
                dogFriend.transform.SetParent(npcRoot.transform, false);
                dogFriend.transform.position = new Vector3(0.0f, -6.0f, 0);

                var sr = dogFriend.AddComponent<SpriteRenderer>();
                sr.sprite = sleepingDogSprite;
                sr.color = Color.white;
                sr.sortingOrder = 500 - Mathf.RoundToInt(-6.0f * 10f);

                var col = dogFriend.AddComponent<CircleCollider2D>();
                col.radius = 0.6f;

                var npcComp = dogFriend.AddComponent<NPCInteractable>();
                npcComp.dialogue = new DialogueData
                {
                    npcName = "San Bernardo Amigo",
                    lines = new string[]
                    {
                        "*Zzz... El gran San Bernardo duerme plácidamente bajo el sol en el pasto*",
                        "*Abre un ojo perezosamente, mueve la cola amistosamente dos veces y vuelve a roncar felizmente*"
                    }
                };
            }

            // NPC 4: Capitán Bruno (Guardia apostado al costado del Portón de salida)
            CreateNPC(npcRoot, "Guardia_Bruno", villagerSprite, new Vector3(-15.5f, 8.5f, 0), new Color(0.85f, 0.88f, 1f),
                npcName: "Guardia Bruno",
                lines: new string[]
                {
                    "¡Saludos, guardián canino! El camino al bosque está lleno de peligros y plataformas.",
                    "Asegúrate de haber hablado con el Sabio Eldor en la plaza antes de partir.",
                    "¡Cruza el arco del portón a mi izquierda y presiona E cuando estés listo para partir al Bosque!"
                });

            // 10. EL JUGADOR: El Perro en el centro de la plaza sobre el camino
            var playerGO = new GameObject("PlayerDog");
            playerGO.tag = "Player";
            playerGO.transform.position = new Vector3(-5.0f, -2.5f, 0);

            var playerSR = playerGO.AddComponent<SpriteRenderer>();
            playerSR.color = Color.white;
            playerSR.sortingOrder = 500;

            var playerCol = playerGO.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.32f;
            playerCol.offset = new Vector2(0, -0.15f);

            var playerRB = playerGO.AddComponent<Rigidbody2D>();
            playerRB.gravityScale = 0f;
            playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var dogController = playerGO.AddComponent<TopDownDogController>();
            dogController.spriteRenderer = playerSR;

            // Target de la cámara al jugador
            camFollow.target = playerGO.transform;

            // 12. PUERTAS DE CASAS → SceneTransition a House_Interior
            var doorsRoot = new GameObject("[House_Doors]");
            // Posiciones aproximadas de las 4 casas del mapa (Bottom-center de cada puerta)
            // Ajustadas a las coordenadas Tiled → Unity del mapa Beginning Fields
            Vector3[] doorPositions = new Vector3[]
            {
                new Vector3(-7.5f,  3.5f, 0f),   // Casa 1 (noroeste)
                new Vector3( 5.5f,  3.5f, 0f),   // Casa 2 (noreste)
                new Vector3(-7.5f, -5.5f, 0f),   // Casa 3 (suroeste)
                new Vector3( 5.5f, -5.5f, 0f),   // Casa 4 (sureste)
            };
            string[] doorNames = new string[] { "Puerta_Casa1", "Puerta_Casa2", "Puerta_Casa3", "Puerta_Casa4" };

            for (int i = 0; i < doorPositions.Length; i++)
            {
                var door = new GameObject(doorNames[i]);
                door.transform.SetParent(doorsRoot.transform, false);
                door.transform.position = doorPositions[i];

                // Visualización: quad semi-transparente verde
                var doorSR = door.AddComponent<SpriteRenderer>();
                doorSR.color        = new Color(0.2f, 0.9f, 0.3f, 0.35f);
                doorSR.sortingOrder = 600;

                // Collider trigger
                var doorCol = door.AddComponent<BoxCollider2D>();
                doorCol.size      = new Vector2(1.2f, 0.5f);
                doorCol.offset    = new Vector2(0f, 0f);
                doorCol.isTrigger = true;

                // SceneTransition → House_Interior
                var trans = door.AddComponent<SceneTransition>();
                trans.targetScene   = "House_Interior";
                trans.promptMessage = "[E] Entrar a la casa";
                trans.fadeDuration  = 0.4f;
            }

            // 11. Guardar Escena
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);

            // Asegurar que Village_Scene está en Build Settings en index 1 (después de MainMenu)
            AddSceneToBuildSettings(SCENE_PATH, 1);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VillageSceneBuilder] ✅ Escena del pueblo construida exitosamente en {SCENE_PATH}");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Aldea Actualizada",
                    "¡La aldea (Village_Scene) ha sido reconstruida con éxito!\n\n" +
                    "• 4 Puertas de casas activas: acércate y presiona [E] para entrar.\n" +
                    "• NPCs originales (Sabio Eldor, Lili, Granjero Tomás, Guardia Bruno).\n" +
                    "• Para añadir NPCs aldeanos de Pixel Crawler usa:\n" +
                    "  Tools > Village > Spawn Village NPCs\n" +
                    "• Para construir el interior: \n" +
                    "  Tools > Village > Build House Interior Scene\n\n" +
                    "¡Presiona Play para probarlo!",
                    "¡Excelente!");
            }
        }

        private static bool IsStairsTile(int gid)
        {
            // Azulejos de escaleras en RockSlopes_Auto que deben ser transitables:
            // 1. Escaleras de piedra (Noroeste fila 6, Centro fila 17, Sureste fila 31)
            if (gid >= 1337 && gid <= 1341) return true;
            if (gid == 1466 || gid == 1467) return true;
            // 2. Rampa/escalera de tierra este (filas 19-20, columnas 32-36)
            if (gid >= 2105 && gid <= 2109) return true;
            if (gid >= 2169 && gid <= 2173) return true;
            return false;
        }

        private static Sprite LoadVillageSprite(string subPath)
        {
            string fullPath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Art/{subPath}";
            return AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
        }

        private static void ParseTMXColliders(string tmxRelativePath, GameObject collidersRoot, GameObject propsRoot)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), tmxRelativePath);
            if (!File.Exists(fullPath))
            {
                Debug.LogWarning($"[VillageSceneBuilder] TMX no encontrado en {fullPath}, usando colisiones básicas.");
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(fullPath);
                XElement root = doc.Root;
                if (root == null) return;

                // 1. Colisiones de Agua (Capa Water: tiles != 0 y != 5211)
                var waterLayer = root.Elements("layer").FirstOrDefault(e => (string)e.Attribute("name") == "Water");
                if (waterLayer != null)
                {
                    var waterRoot = new GameObject("Water_Colliders");
                    waterRoot.transform.SetParent(collidersRoot.transform, false);
                    CreateMergedLayerColliders(waterLayer, waterRoot, gid => gid != 0 && gid != 5211);
                }

                // 2. Colisiones de Barrancos (Capa RockSlopes_Auto: tiles != 0 Y NO escaleras)
                var cliffLayer = root.Elements("layer").FirstOrDefault(e => (string)e.Attribute("name") == "RockSlopes_Auto");
                if (cliffLayer != null)
                {
                    var cliffRoot = new GameObject("Cliff_Colliders");
                    cliffRoot.transform.SetParent(collidersRoot.transform, false);
                    CreateMergedLayerColliders(cliffLayer, cliffRoot, gid => gid != 0 && !IsStairsTile(gid));
                }

                // 3. Colisiones de Objetos y Sprites con Profundidad Y-Sorting
                var objGroup = root.Elements("objectgroup").FirstOrDefault(e => (string)e.Attribute("name") == "Object Layer 1");
                if (objGroup != null)
                {
                    var objsRoot = new GameObject("Objects_Colliders");
                    objsRoot.transform.SetParent(collidersRoot.transform, false);

                    bool campfireColliderCreated = false;

                    foreach (var obj in objGroup.Elements("object"))
                    {
                        int gid = (int?)obj.Attribute("gid") ?? 0;
                        float x = float.Parse((string)obj.Attribute("x") ?? "0", CultureInfo.InvariantCulture);
                        float y = float.Parse((string)obj.Attribute("y") ?? "0", CultureInfo.InvariantCulture);
                        float w = float.Parse((string)obj.Attribute("width") ?? "0", CultureInfo.InvariantCulture);
                        float h = float.Parse((string)obj.Attribute("height") ?? "0", CultureInfo.InvariantCulture);

                        // En Tiled para tile objects (x, y) es la esquina inferior izquierda
                        float unityCenterX = (x + w * 0.5f) / 16f - 20f;
                        float bottomY = 20f - y / 16f;
                        float unityCenterY = 20f - (y - h * 0.5f) / 16f;
                        int sortOrder = 500 - Mathf.RoundToInt(bottomY * 10f);

                        // --- 1. CASAS (GIDs 477..480) ---
                        if (gid >= 477 && gid <= 480)
                        {
                            CreateHouseBaseAndDoor(objsRoot, propsRoot, gid, unityCenterX, bottomY, w / 16f, h / 16f);
                        }
                        // --- 2. ÁRBOLES GRANDES (GIDs 514..517) ---
                        else if (gid >= 514 && gid <= 517)
                        {
                            int treeIdx = gid - 513; // 1..4
                            var treeSprite = LoadVillageSprite($"Trees and Bushes/Tree_Emerald_{treeIdx}.png");
                            if (treeSprite != null)
                            {
                                var treeVis = new GameObject($"Tree_Vis_{treeIdx}_{unityCenterX:F1}_{bottomY:F1}");
                                treeVis.transform.SetParent(propsRoot.transform, false);
                                treeVis.transform.position = new Vector3(unityCenterX, bottomY, 0);
                                var sr = treeVis.AddComponent<SpriteRenderer>();
                                sr.sprite = treeSprite;
                                sr.sortingOrder = sortOrder;
                            }

                            // Colisión sólida únicamente en el tronco para permitir pasar por detrás de la copa o por delante
                            var treeColGO = new GameObject($"Tree_Col_{unityCenterX:F1}_{bottomY:F1}");
                            treeColGO.transform.SetParent(objsRoot.transform, false);
                            float trunkY = bottomY + 0.45f;
                            treeColGO.transform.position = new Vector3(unityCenterX, trunkY, 0);
                            var col = treeColGO.AddComponent<CircleCollider2D>();
                            col.radius = 0.35f;
                        }
                        // --- 3. ARBUSTOS Y PINOS (GIDs 507..513) ---
                        else if (gid >= 507 && gid <= 513)
                        {
                            int bushIdx = gid - 506; // 1..7
                            var bushSprite = LoadVillageSprite($"Trees and Bushes/Bush_Emerald_{bushIdx}.png");
                            if (bushSprite != null)
                            {
                                var bushVis = new GameObject($"Bush_Vis_{bushIdx}_{unityCenterX:F1}_{bottomY:F1}");
                                bushVis.transform.SetParent(propsRoot.transform, false);
                                bushVis.transform.position = new Vector3(unityCenterX, bottomY, 0);
                                var sr = bushVis.AddComponent<SpriteRenderer>();
                                sr.sprite = bushSprite;
                                sr.sortingOrder = sortOrder;
                            }

                            var bushColGO = new GameObject($"Bush_Col_{unityCenterX:F1}_{bottomY:F1}");
                            bushColGO.transform.SetParent(objsRoot.transform, false);

                            if (gid == 507 || gid == 508)
                            {
                                bushColGO.transform.position = new Vector3(unityCenterX, bottomY + 0.3f, 0);
                                var col = bushColGO.AddComponent<BoxCollider2D>();
                                col.size = new Vector2(w / 16f * 0.75f, 0.45f);
                            }
                            else
                            {
                                bushColGO.transform.position = new Vector3(unityCenterX, bottomY + 0.3f, 0);
                                var col = bushColGO.AddComponent<CircleCollider2D>();
                                col.radius = Mathf.Max(0.28f, Mathf.Min(w, h) / 32f * 0.75f);
                            }
                        }
                        // --- 4. POSTES DE LUZ (GID 490) ---
                        else if (gid == 490)
                        {
                            var postSprite = LoadVillageSprite("Props/LampPost_3.png");
                            if (postSprite != null)
                            {
                                var postVis = new GameObject($"LampPost_Vis_{unityCenterX:F1}_{bottomY:F1}");
                                postVis.transform.SetParent(propsRoot.transform, false);
                                postVis.transform.position = new Vector3(unityCenterX, bottomY, 0);
                                var sr = postVis.AddComponent<SpriteRenderer>();
                                sr.sprite = postSprite;
                                sr.sortingOrder = sortOrder;
                            }

                            var postColGO = new GameObject($"LampPost_Col_{unityCenterX:F1}_{bottomY:F1}");
                            postColGO.transform.SetParent(objsRoot.transform, false);
                            postColGO.transform.position = new Vector3(unityCenterX, bottomY + 0.25f, 0);
                            var col = postColGO.AddComponent<BoxCollider2D>();
                            col.size = new Vector2(0.45f, 0.45f);
                        }
                        // --- 5. ESTANDARTES MORADOS (GID 495) ---
                        else if (gid == 495)
                        {
                            var bannerSprite = LoadVillageSprite("Props/Banner_Stick_1_Purple.png");
                            if (bannerSprite != null)
                            {
                                var bannerVis = new GameObject($"Banner_Vis_{unityCenterX:F1}_{bottomY:F1}");
                                bannerVis.transform.SetParent(propsRoot.transform, false);
                                bannerVis.transform.position = new Vector3(unityCenterX, bottomY, 0);
                                var sr = bannerVis.AddComponent<SpriteRenderer>();
                                sr.sprite = bannerSprite;
                                sr.sortingOrder = sortOrder;
                            }

                            var bannerColGO = new GameObject($"Banner_Col_{unityCenterX:F1}_{bottomY:F1}");
                            bannerColGO.transform.SetParent(objsRoot.transform, false);
                            bannerColGO.transform.position = new Vector3(unityCenterX, bottomY + 0.25f, 0);
                            var col = bannerColGO.AddComponent<BoxCollider2D>();
                            col.size = new Vector2(0.45f, 0.45f);
                        }
                        // --- 6. ROCAS (GIDs 502..506) ---
                        else if (gid >= 502 && gid <= 506)
                        {
                            int rockNum = 1;
                            if (gid == 502) rockNum = 1;
                            else if (gid == 503) rockNum = 2;
                            else if (gid == 504) rockNum = 4;
                            else if (gid == 505) rockNum = 6;
                            else if (gid == 506) rockNum = 9;

                            var rockSprite = LoadVillageSprite($"Rocks/Rock_Brown_{rockNum}.png");
                            if (rockSprite != null)
                            {
                                var rockVis = new GameObject($"Rock_Vis_{unityCenterX:F1}_{unityCenterY:F1}");
                                rockVis.transform.SetParent(propsRoot.transform, false);
                                rockVis.transform.position = new Vector3(unityCenterX, unityCenterY, 0);
                                var sr = rockVis.AddComponent<SpriteRenderer>();
                                sr.sprite = rockSprite;
                                sr.sortingOrder = sortOrder;
                            }

                            var rockColGO = new GameObject($"Rock_Col_{unityCenterX:F1}_{unityCenterY:F1}");
                            rockColGO.transform.SetParent(objsRoot.transform, false);
                            float rockBaseY = bottomY + Mathf.Min(w, h) / 32f * 0.5f;
                            rockColGO.transform.position = new Vector3(unityCenterX, rockBaseY, 0);
                            var col = rockColGO.AddComponent<CircleCollider2D>();
                            col.radius = Mathf.Max(0.25f, Mathf.Min(w, h) / 32f * 0.7f);
                        }
                        // --- 7. ACCESORIOS Y PROPS DE ALDEA (Bancas, Cajas, Carteleras, Trigo, Barriles, Mesa) ---
                        else if (gid >= 484 && gid <= 501)
                        {
                            string propFile = null;
                            bool isBoxCol = true;
                            Vector2 colSize = new Vector2(w / 16f * 0.85f, h / 16f * 0.6f);
                            float colOffsetY = h / 32f;
                            float circleRad = 0.35f;

                            switch (gid)
                            {
                                case 484: propFile = "Props/Bench_1.png"; break;
                                case 485: propFile = "Props/Bench_3.png"; break;
                                case 486: propFile = "Props/BulletinBoard_1.png"; colSize = new Vector2(w / 16f * 0.85f, 0.5f); colOffsetY = 0.25f; break;
                                case 487: propFile = "Props/Chopped_Tree_1.png"; isBoxCol = false; circleRad = 0.45f; colOffsetY = 0.4f; break;
                                case 488: propFile = "Props/Crate_Large_Empty.png"; break;
                                case 489: propFile = "Props/Crate_Medium_Closed.png"; break;
                                case 491: propFile = "Props/Plant_2.png"; break; // planta decorativa
                                case 492: propFile = "Props/Sack_3.png"; isBoxCol = false; circleRad = 0.35f; colOffsetY = 0.25f; break;
                                case 493: propFile = "Props/Sign_1.png"; colSize = new Vector2(0.5f, 0.4f); colOffsetY = 0.25f; break;
                                case 496: propFile = "Props/Crate_Water_1.png"; break;
                                case 498: propFile = "Props/HayStack_2.png"; colSize = new Vector2(w / 16f * 0.85f, 0.7f); colOffsetY = 0.35f; break;
                                case 499: propFile = "Props/Barrel_Small_Empty.png"; isBoxCol = false; circleRad = 0.4f; colOffsetY = 0.35f; break;
                                case 501: propFile = "Props/Table_Medium_1.png"; break;
                            }

                            if (!string.IsNullOrEmpty(propFile))
                            {
                                var propSprite = LoadVillageSprite(propFile);
                                if (propSprite != null)
                                {
                                    var propVis = new GameObject($"Prop_Vis_{gid}_{unityCenterX:F1}_{bottomY:F1}");
                                    propVis.transform.SetParent(propsRoot.transform, false);
                                    propVis.transform.position = new Vector3(unityCenterX, bottomY, 0);
                                    var sr = propVis.AddComponent<SpriteRenderer>();
                                    sr.sprite = propSprite;
                                    sr.sortingOrder = sortOrder;
                                }
                            }

                            // Para plantas decorativas pequeñas (491), dejamos que el perro camine sobre ellas
                            if (gid != 491)
                            {
                                var propColGO = new GameObject($"Prop_Col_{gid}_{unityCenterX:F1}_{bottomY:F1}");
                                propColGO.transform.SetParent(objsRoot.transform, false);
                                propColGO.transform.position = new Vector3(unityCenterX, bottomY + colOffsetY, 0);

                                if (isBoxCol)
                                {
                                    var col = propColGO.AddComponent<BoxCollider2D>();
                                    col.size = colSize;
                                }
                                else
                                {
                                    var col = propColGO.AddComponent<CircleCollider2D>();
                                    col.radius = circleRad;
                                }
                            }
                        }
                        // --- 8. FOGATA (GIDs 5770, 5771, 5786, 5787) ---
                        else if ((gid == 5770 || gid == 5771 || gid == 5786 || gid == 5787) && !campfireColliderCreated)
                        {
                            campfireColliderCreated = true;
                            var fireColGO = new GameObject("Campfire_Col");
                            fireColGO.transform.SetParent(objsRoot.transform, false);
                            fireColGO.transform.position = new Vector3(-3.5f, -10.5f, 0);
                            var col = fireColGO.AddComponent<CircleCollider2D>();
                            col.radius = 0.85f;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[VillageSceneBuilder] Error parseando colisiones TMX: {ex}");
            }
        }

        private static void CreateMergedLayerColliders(XElement layerElement, GameObject parent, Func<int, bool> tileFilter)
        {
            var dataElem = layerElement.Element("data");
            if (dataElem == null) return;

            string raw = dataElem.Value.Replace("\r", "").Replace("\n", "").Trim();
            string[] tokens = raw.Split(',');
            if (tokens.Length < 1600) return;

            var cellsByRow = new Dictionary<int, List<int>>();
            for (int i = 0; i < tokens.Length; i++)
            {
                if (int.TryParse(tokens[i], out int gid) && tileFilter(gid))
                {
                    int c = i % 40;
                    int r = i / 40;
                    if (!cellsByRow.ContainsKey(r)) cellsByRow[r] = new List<int>();
                    cellsByRow[r].Add(c);
                }
            }

            // Agrupar en tramos horizontales continuos para minimizar el número de colliders
            foreach (var kvp in cellsByRow.OrderBy(k => k.Key))
            {
                int r = kvp.Key;
                var cols = kvp.Value.OrderBy(c => c).ToList();
                int startC = cols[0];
                int prevC = cols[0];

                for (int i = 1; i < cols.Count; i++)
                {
                    int c = cols[i];
                    if (c == prevC + 1)
                    {
                        prevC = c;
                    }
                    else
                    {
                        CreateSpanCollider(parent, startC, prevC, r);
                        startC = c;
                        prevC = c;
                    }
                }
                CreateSpanCollider(parent, startC, prevC, r);
            }
        }

        private static void CreateSpanCollider(GameObject parent, int startCol, int endCol, int row)
        {
            float width = endCol - startCol + 1;
            float centerX = -20f + (startCol + endCol + 1) * 0.5f;
            float centerY = 20f - row - 0.5f;

            var spanGO = new GameObject($"Span_r{row}_c{startCol}_{endCol}");
            spanGO.transform.SetParent(parent.transform, false);
            spanGO.transform.position = new Vector3(centerX, centerY, 0);

            var col = spanGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 1.0f);
        }

        private static void CreateHouseBaseAndDoor(GameObject collidersRoot, GameObject propsRoot, int gid, float centerX, float bottomY, float w, float h)
        {
            string houseName = "Casa";
            string doorMessage = "Puerta de la aldea.";
            string spriteSubPath = "";

            switch (gid)
            {
                case 480: // Casa 1: Noroeste (Casa del Sabio Eldor)
                    houseName = "Casa del Sabio Eldor";
                    doorMessage = "Hogar del Sabio Eldor. La puerta tiene símbolos arcanos y un suave brillo mágico.";
                    spriteSubPath = "Buildings/House_Hay_4_Purple.png";
                    break;
                case 479: // Casa 2: Mansión Norte
                    houseName = "Gran Mansión del Norte";
                    doorMessage = "Residencia principal del pueblo. Las ventanas miran hacia las montañas nevadas.";
                    spriteSubPath = "Buildings/House_Hay_3.png";
                    break;
                case 478: // Casa 3: Granja de Trigo (Sureste)
                    houseName = "Granja de Trigo";
                    doorMessage = "Granja comunitaria del pueblo. Hay costales de semillas y trigo apilados.";
                    spriteSubPath = "Buildings/House_Hay_2.png";
                    break;
                case 477: // Casa 4: Casa de Lili (Noreste)
                    houseName = "Hogar de Lili y Familia";
                    doorMessage = "Pequeña y acogedora casa de campo. En la repisa hay flores silvestres recién cortadas.";
                    spriteSubPath = "Buildings/House_Hay_1.png";
                    break;
            }

            int sortOrder = 500 - Mathf.RoundToInt(bottomY * 10f);

            // 1. SpriteRenderer para Y-Sorting de la casa (delante o detrás del perro según la posición Y)
            if (!string.IsNullOrEmpty(spriteSubPath))
            {
                var houseSprite = LoadVillageSprite(spriteSubPath);
                if (houseSprite != null)
                {
                    var houseVisGO = new GameObject($"House_Visual_{houseName.Replace(" ", "_")}");
                    houseVisGO.transform.SetParent(propsRoot.transform, false);
                    houseVisGO.transform.position = new Vector3(centerX, bottomY, 0);
                    var sr = houseVisGO.AddComponent<SpriteRenderer>();
                    sr.sprite = houseSprite;
                    sr.sortingOrder = sortOrder;
                }
            }

            // 2. Colisión sólida que cubre el techo y las paredes (evita que el perro camine sobre el techo o traspase las paredes)
            var houseColGO = new GameObject($"HouseBase_{houseName.Replace(" ", "_")}");
            houseColGO.transform.SetParent(collidersRoot.transform, false);
            float colH = h * 0.78f;
            float colW = w * 0.90f;
            houseColGO.transform.position = new Vector3(centerX, bottomY + colH * 0.5f + 0.15f, 0);
            var col = houseColGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(colW, colH);

            // 3. Trigger interactivo en la puerta (en la base inferior de la casa)
            var doorGO = new GameObject($"Door_{houseName.Replace(" ", "_")}");
            doorGO.transform.SetParent(collidersRoot.transform, false);
            doorGO.transform.position = new Vector3(centerX, bottomY + 0.3f, 0);
            var doorCol = doorGO.AddComponent<BoxCollider2D>();
            doorCol.isTrigger = true;
            doorCol.size = new Vector2(1.5f, 1.0f);
            var doorComp = doorGO.AddComponent<DoorInteractable>();
            doorComp.houseName = houseName;
            doorComp.doorMessage = doorMessage;
            doorComp.interactRadius = 2.0f;
        }

        private static GameObject CreateNPC(GameObject parent, string name, Sprite sprite, Vector3 pos, Color tint, string npcName, string[] lines, int rewardCoins = 0)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.offset = new Vector2(0, 0.3f);

            var npc = go.AddComponent<NPCInteractable>();
            npc.dialogue = new DialogueData
            {
                npcName = npcName,
                lines = lines,
                rewardCoins = rewardCoins
            };
            npc.interactRadius = 2.2f;

            return go;
        }

        private static void CreateBoundaryCollider(GameObject parent, Vector2 pos, Vector2 size)
        {
            var wall = new GameObject("BoundaryWall");
            wall.transform.SetParent(parent.transform, false);
            wall.transform.position = new Vector3(pos.x, pos.y, 0);
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static Sprite LoadFirstCharacterSprite()
        {
            string charPath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Art/Characters/Main Character/Character_Idle.png";
            var all = AssetDatabase.LoadAllAssetsAtPath(charPath);
            foreach (var a in all)
            {
                if (a is Sprite s && s.name.Contains("_0_0")) return s;
            }
            foreach (var a in all)
            {
                if (a is Sprite s) return s;
            }
            return null;
        }

        private static void AddSceneToBuildSettings(string path, int insertIndex = -1)
        {
            if (!File.Exists(path)) return;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            var newScene = new EditorBuildSettingsScene(path, true);
            if (insertIndex >= 0 && insertIndex <= scenes.Count)
                scenes.Insert(insertIndex, newScene);
            else
                scenes.Add(newScene);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
