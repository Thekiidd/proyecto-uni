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
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Generador del Hub 2D Isométrico / Top-Down de Kimaya.
    /// Crea la aldea con vista cenital 2D donde Falin y Mia están juntos:
    ///   - Falin y Mia con sprites y animaciones completas en 4 direcciones de PixelLab.
    ///   - Cambio de control con [TAB] y seguimiento fiel de compañero (cooperativo).
    ///   - Gran Portal Místico de piedra hacia el Templo del Bosque (Level_01_Bosque).
    ///   - Guía Maya con lore, consejos de mecánicas y diálogo.
    ///   - Tablón de récords de Firebase Leaderboard en la plaza.
    ///   - Terreno detallado, colisiones TMX y profundidad Y-sorting.
    /// Accesible desde: Tools > Kimaya > 🏡 [3] Construir Hub 2D de Kimaya (Falin y Mia)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaHubBuilder
    {
        public const string HUB_SCENE_PATH = "Assets/Scenes/Hub.unity";
        public const string VILLAGE_SCENE_PATH = "Assets/Scenes/Village_Scene.unity";
        private const string BUILD_KEY = "KimayaHub_Build_v1";

        private const string PIXELLAB_PATH = "Assets/Sprites/PixelLab";
        private const string VILLAGE_FOLDER = "Assets/Sprites/Village";
        private const string PORTAL_PATH = "Assets/Sprites/Village/Art/Buildings/Portal_Temple_Gate.png";

        static KimayaHubBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaHubBuilder] 🚀 Ejecutando AutoBuildCheck para Hub 2D de Kimaya...");
                BuildKimayaHub(false);
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        [MenuItem("Tools/Kimaya/🏡 [3] Construir Hub 2D de Kimaya (Falin y Mia)")]
        public static void BuildMenu()
        {
            BuildKimayaHub(true);
        }

        public static void BuildBatchmode()
        {
            BuildKimayaHub(false);
        }

        public static void BuildKimayaHub(bool showDialog)
        {
            Debug.Log("[KimayaHubBuilder] 🔨 Construyendo Hub 2D Isométrico de Kimaya...");

            // 1. Configurar sprites
            EnsureAllSpriteImports();

            // 2. Crear nueva escena limpia
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Terreno Base (Beginning Fields: 640x640 px = 40x40 unidades a PPU 16)
            var groundGO = new GameObject("Village_Ground");
            groundGO.transform.position = Vector3.zero;
            var groundSR = groundGO.AddComponent<SpriteRenderer>();
            string mapSpritePath = $"{VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.png";
            groundSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(mapSpritePath);
            groundSR.sortingOrder = -1000;

            // 4. Raíz de Colisiones y Límites
            var collidersRoot = new GameObject("[Colliders]");
            var collidersRB = collidersRoot.AddComponent<Rigidbody2D>();
            collidersRB.bodyType = RigidbodyType2D.Static;

            CreateBoundaryCollider(collidersRoot, new Vector2(0, 20.5f), new Vector2(42f, 1f));
            CreateBoundaryCollider(collidersRoot, new Vector2(0, -20.5f), new Vector2(42f, 1f));
            CreateBoundaryCollider(collidersRoot, new Vector2(20.5f, 0), new Vector2(1f, 42f));
            CreateBoundaryCollider(collidersRoot, new Vector2(-20.5f, 15.5f), new Vector2(1f, 9f));
            CreateBoundaryCollider(collidersRoot, new Vector2(-20.5f, -6f), new Vector2(1f, 27f));

            // Parsear TMX para colisiones de agua, barrancos y props con Y-sorting
            var propsRoot = new GameObject("[Village_Props_Y_Sorted]");
            string tmxPath = $"{VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.tmx";
            ParseTMXColliders(tmxPath, collidersRoot, propsRoot);

            // 5. Singletons y Gestión
            var setupGO = new GameObject("[GameSetup]");
            setupGO.AddComponent<GameData>();
            setupGO.AddComponent<SceneLoader>();
            setupGO.AddComponent<DialogueManager>();

            // 6. LOS DOS PROTAGONISTAS: Falin y Mia
            var duoRoot = new GameObject("Kimaya_Duo_Team");
            duoRoot.transform.position = new Vector3(-5.0f, -2.5f, 0);

            var duoController = duoRoot.AddComponent<KimayaHubDuoController>();
            duoController.walkSpeed = 4.8f;
            duoController.runSpeed = 7.5f;
            duoController.companionFollowDistance = 1.35f;

            // --- FALIN (San Bernardo) ---
            var falinGO = new GameObject("Falin_SanBernardo");
            falinGO.tag = "Player";
            falinGO.transform.position = new Vector3(-5.2f, -2.5f, 0);

            var falinSR = falinGO.AddComponent<SpriteRenderer>();
            falinSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Falin/Idle/rotations/south.png");
            falinSR.sortingOrder = 525;

            var falinCol = falinGO.AddComponent<CircleCollider2D>();
            falinCol.radius = 0.38f;
            falinCol.offset = new Vector2(0, -0.12f);

            var falinRB = falinGO.AddComponent<Rigidbody2D>();
            falinRB.gravityScale = 0f;
            falinRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            falinRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Marcador de selección bajo Falin
            CreateSelectionMarker(falinGO, new Color(1f, 0.85f, 0.2f, 0.55f));

            duoController.falinObject = falinGO;
            duoController.falinSprites = BuildDogSpriteSet("Falin", $"{PIXELLAB_PATH}/Falin");

            // --- MIA (Schnauzer) ---
            var miaGO = new GameObject("Mia_Schnauzer");
            miaGO.tag = "Player";
            miaGO.transform.position = new Vector3(-3.8f, -2.5f, 0);

            var miaSR = miaGO.AddComponent<SpriteRenderer>();
            miaSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Mia/Idle/rotations/south.png");
            miaSR.sortingOrder = 525;

            var miaCol = miaGO.AddComponent<CircleCollider2D>();
            miaCol.radius = 0.28f;
            miaCol.offset = new Vector2(0, -0.1f);

            var miaRB = miaGO.AddComponent<Rigidbody2D>();
            miaRB.gravityScale = 0f;
            miaRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            miaRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Marcador de selección bajo Mia
            CreateSelectionMarker(miaGO, new Color(0.3f, 0.85f, 1f, 0.55f));

            duoController.miaObject = miaGO;
            duoController.miaSprites = BuildDogSpriteSet("Mia", $"{PIXELLAB_PATH}/Mia");

            // 7. CÁMARA HUB
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(-5f, -2.5f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 6.2f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.22f, 0.14f);
            camGO.AddComponent<AudioListener>();

            var camFollow = camGO.AddComponent<KimayaHubCameraFollow>();
            camFollow.minBounds = new Vector2(-12f, -14f);
            camFollow.maxBounds = new Vector2(12f, 14f);
            camFollow.smoothSpeed = 5.5f;

            // 8. PORTAL MÍSTICO HACIA EL TEMPLO DEL BOSQUE (Level_01_Bosque)
            var portalGO = new GameObject("Portal_Templo_Bosque");
            portalGO.transform.position = new Vector3(-18.0f, 9.5f, 0);

            var portalSR = portalGO.AddComponent<SpriteRenderer>();
            portalSR.sprite = LoadSprite(PORTAL_PATH) ?? LoadSprite($"{VILLAGE_FOLDER}/Art/Buildings/CityWall_Gate_1.png");
            portalSR.sortingOrder = 450;

            var portalComp = portalGO.AddComponent<KimayaHubPortal>();
            portalComp.targetSceneName = "Level_01_Bosque";
            portalComp.destinationTitle = "Templo del Bosque (Nivel 1)";
            portalComp.questLore = "Misión Principal: Rescatar a Kiara (Cooperativo)";
            portalComp.interactionRadius = 2.6f;

            var portalCol = portalGO.AddComponent<BoxCollider2D>();
            portalCol.isTrigger = true;
            portalCol.size = new Vector2(2.8f, 3.2f);
            portalCol.offset = new Vector2(0, -0.4f);

            // 9. MAYA (NPC GUÍA ESPIRITUAL) EN LA PLAZA
            var mayaGO = new GameObject("Maya_Guia_Espiritual");
            mayaGO.transform.position = new Vector3(-6.2f, -0.8f, 0);

            var mayaSR = mayaGO.AddComponent<SpriteRenderer>();
            mayaSR.sprite = LoadFirstCharacterSprite();
            mayaSR.color = new Color(0.5f, 0.95f, 1f, 0.95f);
            mayaSR.sortingOrder = 500 - Mathf.RoundToInt(-0.8f * 10f);

            var mayaCol = mayaGO.AddComponent<CircleCollider2D>();
            mayaCol.radius = 0.5f;

            var mayaComp = mayaGO.AddComponent<KimayaHubNPCMaya>();
            mayaComp.npcName = "Maya (Espíritu Guardián)";
            mayaComp.interactionRadius = 2.2f;

            // 10. TABLÓN DE RÉCORDS (FIREBASE LEADERBOARD)
            var boardGO = new GameObject("Tablon_De_Records");
            boardGO.transform.position = new Vector3(-2.2f, -0.8f, 0);

            var boardSR = boardGO.AddComponent<SpriteRenderer>();
            string signPath = $"{VILLAGE_FOLDER}/Art/Props/Sign_01.png";
            boardSR.sprite = LoadSprite(signPath) ?? LoadSprite($"{VILLAGE_FOLDER}/Art/Props/Board_01.png");
            boardSR.color = new Color(1f, 0.95f, 0.85f);
            boardSR.sortingOrder = 500 - Mathf.RoundToInt(-0.8f * 10f);

            var boardCol = boardGO.AddComponent<BoxCollider2D>();
            boardCol.size = new Vector2(1.2f, 1.2f);

            var boardComp = boardGO.AddComponent<KimayaHubBulletinBoard>();
            boardComp.interactionRadius = 2.4f;

            // 11. NPCS DE LA ALDEA (Vida y ambiente)
            var npcRoot = new GameObject("[NPCs_Aldea]");
            Sprite villagerSprite = LoadFirstCharacterSprite();

            // Granjero Tomás
            CreateVillageNPC(npcRoot, "Granjero_Tomas", villagerSprite, new Vector3(0.0f, -7.5f, 0),
                new Color(0.85f, 0.95f, 0.85f), "Granjero Tomás",
                new string[]
                {
                    "¡Falin, Mia! Qué alegría ver a la gran pareja canina en la aldea.",
                    "El templo del bosque tiene mecanismos antiguos que requieren peso y agilidad.",
                    "¡Ustedes dos hacen el equipo perfecto para rescatar a Kiara!"
                });

            // Lili (Niña en las flores)
            CreateVillageNPC(npcRoot, "Lili", villagerSprite, new Vector3(10.0f, 5.0f, 0),
                new Color(1f, 0.82f, 0.88f), "Lili",
                new string[]
                {
                    "¡Awww, Falin es tan gigante y Mia tan pequeñita y tierna! *les acaricia el pelaje*",
                    "¡Tengan mucho cuidado en el templo! Estaremos orando por ustedes aquí en el jardín."
                });

            // Guardia Bruno
            CreateVillageNPC(npcRoot, "Guardia_Bruno", villagerSprite, new Vector3(-15.2f, 8.5f, 0),
                new Color(0.85f, 0.88f, 1f), "Guardia Bruno",
                new string[]
                {
                    "¡Alto ahí, protectores! El portal místico hacia el bosque prohibido está despejado a mi izquierda.",
                    "Crucen cuando estén listos para adentrarse en el Templo de Puzles."
                });

            // 12. PUERTAS DE LAS CASAS
            BuildHouseDoors();

            // 13. HUD DE KIMAYA
            var hudGO = new GameObject("[Kimaya_HUD]");
            hudGO.AddComponent<KimayaHubHUD>();

            // 14. GUARDAR EN AMBAS RUTAS (Hub.unity y Village_Scene.unity)
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, HUB_SCENE_PATH);
            EditorSceneManager.SaveScene(scene, VILLAGE_SCENE_PATH);

            // 15. ACTUALIZAR BUILD SETTINGS
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[KimayaHubBuilder] ✅ Hub 2D de Kimaya construido y guardado en:\n  - {HUB_SCENE_PATH}\n  - {VILLAGE_SCENE_PATH}");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("🏡 Hub 2D de Kimaya Creado",
                    "¡El Hub 2D de Kimaya ha sido generado con éxito!\n\n" +
                    "🐾 Falin y Mia en la aldea con animaciones PixelLab:\n" +
                    "  • [WASD / Flechas] Caminar\n" +
                    "  • [SHIFT] Correr\n" +
                    "  • [TAB] Alternar control (el otro perro te sigue)\n" +
                    "  • [ESPACIO / Q / F] Ladrido / Poder\n" +
                    "  • [E] Interactuar\n\n" +
                    "🌀 Gran Portal al Templo del Bosque (Level_01_Bosque)\n" +
                    "🧚 Guía Maya con lore y consejos\n" +
                    "🏆 Tablón de Récords con enlace a Firebase Leaderboard Web",
                    "¡Excelente!");
            }
        }

        private static void CreateSelectionMarker(GameObject parent, Color col)
        {
            var marker = new GameObject("ActiveMarker");
            marker.transform.SetParent(parent.transform, false);
            marker.transform.localPosition = new Vector2(0, -0.32f);
            var sr = marker.AddComponent<SpriteRenderer>();
            sr.color = col;
            sr.sortingOrder = 480;

            // Textura circular sutil generada
            var tex = new Texture2D(32, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float nx = (x - 15.5f) / 15.5f;
                    float ny = (y - 7.5f) / 7.5f;
                    float d = nx * nx + ny * ny;
                    tex.SetPixel(x, y, d <= 1f ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            var sp = Sprite.Create(tex, new Rect(0, 0, 32, 16), new Vector2(0.5f, 0.5f), 16f);
            sr.sprite = sp;
        }

        private static DogSpriteSet BuildDogSpriteSet(string name, string folder)
        {
            var set = new DogSpriteSet();
            set.dogName = name;
            set.defaultPortrait = LoadSprite($"{folder}/Idle/rotations/south.png");

            // Idle
            set.idleSouth = LoadFrames($"{folder}/Idle/animations/idle/south", 8);
            set.idleNorth = LoadFrames($"{folder}/Idle/animations/idle/north", 8);
            set.idleEast  = LoadFrames($"{folder}/Idle/animations/idle/east", 8);
            set.idleWest  = LoadFrames($"{folder}/Idle/animations/idle/west", 8);

            // Walking
            set.walkSouth = LoadFrames($"{folder}/Idle/animations/walking/south", 8);
            set.walkNorth = LoadFrames($"{folder}/Idle/animations/walking/north", 8);
            set.walkEast  = LoadFrames($"{folder}/Idle/animations/walking/east", 8);
            set.walkWest  = LoadFrames($"{folder}/Idle/animations/walking/west", 8);

            // Running
            set.runSouth  = LoadFrames($"{folder}/Idle/animations/running/south", 8);
            set.runNorth  = LoadFrames($"{folder}/Idle/animations/running/north", 8);
            set.runEast   = LoadFrames($"{folder}/Idle/animations/running/east", 8);
            set.runWest   = LoadFrames($"{folder}/Idle/animations/running/west", 8);

            // Power (Barking o Sneaking)
            set.powerSouth = LoadFrames($"{folder}/Idle/animations/barking/south", 6);
            set.powerNorth = LoadFrames($"{folder}/Idle/animations/barking/north", 6);
            set.powerEast  = LoadFrames($"{folder}/Idle/animations/barking/east", 6);
            set.powerWest  = LoadFrames($"{folder}/Idle/animations/barking/west", 6);

            return set;
        }

        private static Sprite[] LoadFrames(string folder, int count)
        {
            var list = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                string path = $"{folder}/frame_{i:D3}.png";
                var sp = LoadSprite(path);
                if (sp != null) list.Add(sp);
            }
            return list.ToArray();
        }

        private static Sprite LoadSprite(string path)
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sp == null && File.Exists(path))
            {
                var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.filterMode = FilterMode.Point;
                    ti.spritePixelsPerUnit = path.Contains("PixelLab") ? 32f : 16f;
                    ti.textureCompression = TextureImporterCompression.Uncompressed;
                    ti.alphaIsTransparency = true;
                    ti.SaveAndReimport();
                    sp = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                }
            }
            return sp;
        }

        private static void EnsureAllSpriteImports()
        {
            VillageAssetImporter.ProcessVillageAssets(false);
        }

        private static Sprite LoadFirstCharacterSprite()
        {
            string charFolder = $"{VILLAGE_FOLDER}/Art/Characters";
            if (!Directory.Exists(charFolder)) return null;
            string[] pngs = Directory.GetFiles(charFolder, "*.png");
            if (pngs.Length > 0)
            {
                string p = pngs[0].Replace('\\', '/');
                return AssetDatabase.LoadAssetAtPath<Sprite>(p);
            }
            return null;
        }

        private static void CreateBoundaryCollider(GameObject parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject($"Wall_{pos.x}_{pos.y}");
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateVillageNPC(GameObject parent, string name, Sprite sprite, Vector3 pos, Color tint, string npcName, string[] lines)
        {
            var npcGO = new GameObject(name);
            npcGO.transform.SetParent(parent.transform, false);
            npcGO.transform.position = pos;

            var sr = npcGO.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = tint;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);

            var col = npcGO.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;

            var npcComp = npcGO.AddComponent<NPCInteractable>();
            npcComp.dialogue = new DialogueData
            {
                npcName = npcName,
                lines = lines
            };
        }

        private static void BuildHouseDoors()
        {
            var doorsRoot = new GameObject("[House_Doors]");
            Vector3[] doorPositions = new Vector3[]
            {
                new Vector3(-7.5f,  3.5f, 0f),
                new Vector3( 5.5f,  3.5f, 0f),
                new Vector3(-7.5f, -5.5f, 0f),
                new Vector3( 5.5f, -5.5f, 0f),
            };
            string[] doorNames = new string[] { "Puerta_Casa1", "Puerta_Casa2", "Puerta_Casa3", "Puerta_Casa4" };

            for (int i = 0; i < doorPositions.Length; i++)
            {
                var door = new GameObject(doorNames[i]);
                door.transform.SetParent(doorsRoot.transform, false);
                door.transform.position = doorPositions[i];

                var doorCol = door.AddComponent<BoxCollider2D>();
                doorCol.size = new Vector2(1.2f, 0.5f);
                doorCol.isTrigger = true;

                var trans = door.AddComponent<SceneTransition>();
                trans.targetScene = "House_Interior";
                trans.promptMessage = "[E] Entrar a la casa";
                trans.fadeDuration = 0.4f;
            }
        }

        private static void UpdateBuildSettings()
        {
            var currentScenes = EditorBuildSettings.scenes.ToList();

            void AddOrSetScene(string path, int index)
            {
                int existingIdx = currentScenes.FindIndex(s => s.path == path);
                if (existingIdx >= 0)
                {
                    var scene = currentScenes[existingIdx];
                    currentScenes.RemoveAt(existingIdx);
                    currentScenes.Insert(Mathf.Clamp(index, 0, currentScenes.Count), scene);
                }
                else if (File.Exists(path))
                {
                    currentScenes.Insert(Mathf.Clamp(index, 0, currentScenes.Count), new EditorBuildSettingsScene(path, true));
                }
            }

            AddOrSetScene("Assets/Scenes/MainMenu.unity", 0);
            AddOrSetScene(HUB_SCENE_PATH, 1);
            AddOrSetScene(VILLAGE_SCENE_PATH, 2);
            AddOrSetScene("Assets/Scenes/Level_01_Bosque.unity", 3);

            EditorBuildSettings.scenes = currentScenes.ToArray();
        }

        private static bool IsStairsTile(int gid)
        {
            if (gid >= 1337 && gid <= 1341) return true;
            if (gid == 1466 || gid == 1467) return true;
            if (gid >= 2105 && gid <= 2109) return true;
            if (gid >= 2169 && gid <= 2173) return true;
            return false;
        }

        private static void ParseTMXColliders(string tmxRelativePath, GameObject collidersRoot, GameObject propsRoot)
        {
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), tmxRelativePath);
            if (!File.Exists(fullPath)) return;

            try
            {
                XDocument doc = XDocument.Load(fullPath);
                XElement root = doc.Root;
                if (root == null) return;

                var waterLayer = root.Elements("layer").FirstOrDefault(e => (string)e.Attribute("name") == "Water");
                if (waterLayer != null)
                {
                    var waterRoot = new GameObject("Water_Colliders");
                    waterRoot.transform.SetParent(collidersRoot.transform, false);
                    CreateMergedLayerColliders(waterLayer, waterRoot, gid => gid != 0 && gid != 5211);
                }

                var cliffLayer = root.Elements("layer").FirstOrDefault(e => (string)e.Attribute("name") == "RockSlopes_Auto");
                if (cliffLayer != null)
                {
                    var cliffRoot = new GameObject("Cliff_Colliders");
                    cliffRoot.transform.SetParent(collidersRoot.transform, false);
                    CreateMergedLayerColliders(cliffLayer, cliffRoot, gid => gid != 0 && !IsStairsTile(gid));
                }

                var objGroup = root.Elements("objectgroup").FirstOrDefault(e => (string)e.Attribute("name") == "Object Layer 1");
                if (objGroup != null)
                {
                    var objsRoot = new GameObject("Objects_Colliders");
                    objsRoot.transform.SetParent(collidersRoot.transform, false);

                    foreach (var obj in objGroup.Elements("object"))
                    {
                        float x = float.Parse((string)obj.Attribute("x") ?? "0", CultureInfo.InvariantCulture);
                        float y = float.Parse((string)obj.Attribute("y") ?? "0", CultureInfo.InvariantCulture);
                        float w = float.Parse((string)obj.Attribute("width") ?? "0", CultureInfo.InvariantCulture);
                        float h = float.Parse((string)obj.Attribute("height") ?? "0", CultureInfo.InvariantCulture);

                        float unityCenterX = (x + w * 0.5f) / 16f - 20f;
                        float bottomY = 20f - y / 16f;
                        float unityCenterY = 20f - (y - h * 0.5f) / 16f;

                        // Si es árbol o casa grande, agregar colisión inferior
                        if (w >= 32f && h >= 32f)
                        {
                            var colGO = new GameObject($"ObjCol_{unityCenterX:F1}_{bottomY:F1}");
                            colGO.transform.SetParent(objsRoot.transform, false);
                            colGO.transform.position = new Vector3(unityCenterX, bottomY + 0.35f, 0);
                            var col = colGO.AddComponent<BoxCollider2D>();
                            col.size = new Vector2(w / 16f * 0.7f, 0.7f);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[KimayaHubBuilder] Error parseando TMX: {ex.Message}");
            }
        }

        private static void CreateMergedLayerColliders(XElement layerElem, GameObject parent, Func<int, bool> isSolidPredicate)
        {
            var dataElem = layerElem.Element("data");
            if (dataElem == null) return;

            int[] gids = dataElem.Value.Split(new char[] { ',', '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(s => int.Parse(s.Trim()))
                                      .ToArray();

            int width = int.Parse((string)layerElem.Attribute("width") ?? "40");
            int height = int.Parse((string)layerElem.Attribute("height") ?? "40");

            bool[,] solid = new bool[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    solid[r, c] = isSolidPredicate(gids[r * width + c]);

            bool[,] visited = new bool[height, width];
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (solid[r, c] && !visited[r, c])
                    {
                        int spanX = 0;
                        while (c + spanX < width && solid[r, c + spanX] && !visited[r, c + spanX]) spanX++;

                        int spanY = 1;
                        bool canExpand = true;
                        while (r + spanY < height && canExpand)
                        {
                            for (int k = 0; k < spanX; k++)
                            {
                                if (!solid[r + spanY, c + k] || visited[r + spanY, c + k])
                                {
                                    canExpand = false;
                                    break;
                                }
                            }
                            if (canExpand) spanY++;
                        }

                        for (int dr = 0; dr < spanY; dr++)
                            for (int dc = 0; dc < spanX; dc++)
                                visited[r + dr, c + dc] = true;

                        float unityX = (c + spanX * 0.5f) - 20f;
                        float unityY = 20f - (r + spanY * 0.5f);

                        var colGO = new GameObject($"Merged_{c}_{r}_{spanX}x{spanY}");
                        colGO.transform.SetParent(parent.transform, false);
                        colGO.transform.position = new Vector3(unityX, unityY, 0);

                        var box = colGO.AddComponent<BoxCollider2D>();
                        box.size = new Vector2(spanX, spanY);
                    }
                }
            }
        }
    }
}
