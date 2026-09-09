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
        private const string BUILD_KEY = "VillageScene_Build_v4";

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

            // 7. Parsear TMX para generar colisiones automáticas de Agua, Barrancos (excluyendo escaleras) y Objetos
            string tmxPath = $"{VillageAssetImporter.VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.tmx";
            ParseTMXColliders(tmxPath, collidersRoot);

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
            playerCol.radius = 0.45f;
            playerCol.offset = new Vector2(0, 0.2f);

            var playerRB = playerGO.AddComponent<Rigidbody2D>();
            playerRB.gravityScale = 0f;
            playerRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var dogController = playerGO.AddComponent<TopDownDogController>();
            dogController.spriteRenderer = playerSR;

            // Target de la cámara al jugador
            camFollow.target = playerGO.transform;

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
                    "• Escaleras de piedra 100% transitables: ya puedes subir y bajar libremente.\n" +
                    "• NPCs reubicados en lugares naturales del suelo y caminos:\n" +
                    "   - Sabio Eldor: en el camino de la plaza central.\n" +
                    "   - Granjero Tomás y San Bernardo: en el camino frente a la granja.\n" +
                    "   - Lili: en el campo de flores frente a la casa este.\n" +
                    "   - Guardia Bruno: custodiando el portón oeste.\n" +
                    "• Puertas de casas y pozo examinables con [E].\n\n" +
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

        private static void ParseTMXColliders(string tmxRelativePath, GameObject collidersRoot)
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

                // 3. Colisiones de Objetos (Casas, árboles, rocas, pozo, cercas)
                var objGroup = root.Elements("objectgroup").FirstOrDefault(e => (string)e.Attribute("name") == "Object Layer 1");
                if (objGroup != null)
                {
                    var objsRoot = new GameObject("Objects_Colliders");
                    objsRoot.transform.SetParent(collidersRoot.transform, false);

                    foreach (var obj in objGroup.Elements("object"))
                    {
                        int gid = (int?)obj.Attribute("gid") ?? 0;
                        float x = float.Parse((string)obj.Attribute("x") ?? "0", CultureInfo.InvariantCulture);
                        float y = float.Parse((string)obj.Attribute("y") ?? "0", CultureInfo.InvariantCulture);
                        float w = float.Parse((string)obj.Attribute("width") ?? "0", CultureInfo.InvariantCulture);
                        float h = float.Parse((string)obj.Attribute("height") ?? "0", CultureInfo.InvariantCulture);

                        // En Tiled para tile objects (x, y) es la esquina inferior izquierda
                        float unityCenterX = (x + w * 0.5f) / 16f - 20f;
                        float unityCenterY = 20f - (y - h * 0.5f) / 16f;

                        // Casas (GIDs 477..480): colisión sólida en la base
                        if (gid >= 477 && gid <= 480)
                        {
                            CreateHouseBaseAndDoor(objsRoot, gid, unityCenterX, unityCenterY, w / 16f, h / 16f);
                        }
                        // Árboles grandes (GIDs 514..517): colisión solo en el tronco
                        else if (gid >= 514 && gid <= 517)
                        {
                            var treeGO = new GameObject($"Tree_Col_{unityCenterX:F1}_{unityCenterY:F1}");
                            treeGO.transform.SetParent(objsRoot.transform, false);
                            float trunkY = 20f - (y - 8f) / 16f;
                            treeGO.transform.position = new Vector3(unityCenterX, trunkY, 0);
                            var col = treeGO.AddComponent<CircleCollider2D>();
                            col.radius = 0.35f;
                        }
                        // Rocas (GIDs 502..506): colisión circular en la base
                        else if (gid >= 502 && gid <= 506)
                        {
                            var rockGO = new GameObject($"Rock_Col_{unityCenterX:F1}_{unityCenterY:F1}");
                            rockGO.transform.SetParent(objsRoot.transform, false);
                            float rockBaseY = 20f - (y - h * 0.35f) / 16f;
                            rockGO.transform.position = new Vector3(unityCenterX, rockBaseY, 0);
                            var col = rockGO.AddComponent<CircleCollider2D>();
                            col.radius = Mathf.Min(w, h) / 32f * 0.65f;
                        }
                        // Pozo de agua (GID 501)
                        else if (gid == 501)
                        {
                            var wellGO = new GameObject("Well_Col");
                            wellGO.transform.SetParent(objsRoot.transform, false);
                            wellGO.transform.position = new Vector3(unityCenterX, unityCenterY, 0);
                            var col = wellGO.AddComponent<CircleCollider2D>();
                            col.radius = 0.85f;

                            // Puerta / Inspección interactiva del pozo
                            var doorComp = wellGO.AddComponent<DoorInteractable>();
                            doorComp.houseName = "Pozo Antiguo de la Aldea";
                            doorComp.doorMessage = "El agua del pozo es pura y cristalina. Un susurro mágico parece desearte suerte en tu misión.";
                            doorComp.interactRadius = 2.0f;
                        }
                        // Cercas y cajas (GIDs 484, 485, 487, 498, etc.)
                        else if (gid == 484 || gid == 485 || gid == 487 || gid == 498)
                        {
                            var propGO = new GameObject($"Prop_Col_{unityCenterX:F1}_{unityCenterY:F1}");
                            propGO.transform.SetParent(objsRoot.transform, false);
                            float propBaseY = 20f - (y - h * 0.3f) / 16f;
                            propGO.transform.position = new Vector3(unityCenterX, propBaseY, 0);
                            var col = propGO.AddComponent<BoxCollider2D>();
                            col.size = new Vector2(w / 16f * 0.85f, h / 16f * 0.5f);
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

        private static void CreateHouseBaseAndDoor(GameObject parent, int gid, float centerX, float centerY, float w, float h)
        {
            string houseName = "Casa";
            string doorMessage = "Puerta de la aldea.";
            Vector2 colSize = new Vector2(w * 0.85f, h * 0.45f);
            Vector2 colCenter = new Vector2(centerX, centerY - h * 0.15f);
            Vector2 doorPos = new Vector2(centerX, centerY - h * 0.48f);

            switch (gid)
            {
                case 480: // Casa 1: Noroeste (Casa del Sabio Eldor)
                    houseName = "Casa del Sabio Eldor";
                    doorMessage = "Hogar del Sabio Eldor. La puerta tiene símbolos arcanos y un suave brillo mágico.";
                    break;
                case 479: // Casa 2: Mansión Norte
                    houseName = "Gran Mansión del Norte";
                    doorMessage = "Residencia principal del pueblo. Las ventanas miran hacia las montañas nevadas.";
                    break;
                case 478: // Casa 3: Granja de Trigo (Sureste)
                    houseName = "Granja de Trigo";
                    doorMessage = "Granja comunitaria del pueblo. Hay costales de semillas y trigo apilados.";
                    break;
                case 477: // Casa 4: Casa de Lili (Noreste)
                    houseName = "Hogar de Lili y Familia";
                    doorMessage = "Pequeña y acogedora casa de campo. En la repisa hay flores silvestres recién cortadas.";
                    break;
            }

            // Collider sólido en la pared/base
            var houseColGO = new GameObject($"HouseBase_{houseName.Replace(" ", "_")}");
            houseColGO.transform.SetParent(parent.transform, false);
            houseColGO.transform.position = new Vector3(colCenter.x, colCenter.y, 0);
            var col = houseColGO.AddComponent<BoxCollider2D>();
            col.size = colSize;

            // Trigger interactivo en la puerta
            var doorGO = new GameObject($"Door_{houseName.Replace(" ", "_")}");
            doorGO.transform.SetParent(parent.transform, false);
            doorGO.transform.position = new Vector3(doorPos.x, doorPos.y, 0);
            var doorCol = doorGO.AddComponent<BoxCollider2D>();
            doorCol.isTrigger = true;
            doorCol.size = new Vector2(1.5f, 1.2f);
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
