using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Platformer.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Generador del Hub 2D del Bosque Ancestral con los 3 Portales Místicos.
    /// Versión 7:
    ///   - Personajes ajustados en escala (Falin 0.66x, Mía 0.56x) para proporción perfecta con el mapa.
    ///   - Indicador de selección: Diamante flotante animado (The Sims / Zelda style) en lugar del óvalo amarillo.
    ///   - Paso libre 100% en el camino adoquinado inferior derecho (abajo del portal morado).
    ///   - Muro superior en la base real de árboles y acantilados (imposible caminar sobre rocas altas).
    ///   - Capa de profundidad Y-Sorting para ocultarse y pasar por detrás del roble y arcos de portales.
    ///   - Portales 2 y 3 sólidamente bloqueados con avisos claros.
    /// Menú: Tools > Kimaya > 🏡 [3] Construir Hub 2D de Kimaya (Falin y Mia)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaHubBuilder
    {
        public const string HUB_SCENE_PATH = "Assets/Scenes/Hub.unity";
        public const string MAIN_MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";
        public const string JUMP_KING_SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaHub_Clearing_v7";

        private const string HUB_MAP_IMAGE_PATH = "Assets/Sprites/Hub/Forest_Hub_Clearing_3Portals.png";
        private const string PIXELLAB_PATH = "Assets/Sprites/PixelLab";
        private const string FALIN_DIAMOND_PATH = "Assets/Sprites/UI/Selection_Diamond_Falin.png";
        private const string MIA_DIAMOND_PATH = "Assets/Sprites/UI/Selection_Diamond_Mia.png";
        private const string OAK_TREE_SPRITE_PATH = "Assets/Sprites/Village/Art/Trees and Bushes/Tree_Emerald_1.png";

        static KimayaHubBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaHubBuilder] 🚀 Ejecutando AutoBuildCheck v7 (Diamante flotante, escala de perros & camino libre)...");
                BuildKimayaHub(false);
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        [MenuItem("Tools/Kimaya/🏡 [3] Construir Hub 2D de Kimaya (Falin y Mia)")]
        public static void BuildMenu()
        {
            BuildKimayaHub(true);
        }

        public static void BuildKimayaHub(bool showDialog)
        {
            Debug.Log("[KimayaHubBuilder] 🔨 Construyendo Hub del Claro del Bosque v7...");

            // 1. Asegurar importación de sprites
            EnsureSpriteImport(HUB_MAP_IMAGE_PATH, 32);
            EnsureSpriteImport(FALIN_DIAMOND_PATH, 32);
            EnsureSpriteImport(MIA_DIAMOND_PATH, 32);

            // 2. Crear nueva escena limpia
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Fondo: Claro del Bosque Ancestral escalado a 1.22x
            var mapGO = new GameObject("Forest_Hub_Background");
            mapGO.transform.position = Vector3.zero;
            mapGO.transform.localScale = new Vector3(1.22f, 1.22f, 1.0f);
            var mapSR = mapGO.AddComponent<SpriteRenderer>();
            mapSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HUB_MAP_IMAGE_PATH);
            mapSR.sortingOrder = -500;

            // 4. Sistema de Colisiones Perimetrales y Bloqueadores Sólidos
            var collidersRoot = new GameObject("[Hub_Colliders]");
            var collidersRB = collidersRoot.AddComponent<Rigidbody2D>();
            collidersRB.bodyType = RigidbodyType2D.Static;

            // --- ACANTILADOS IZQUIERDOS (Muros de roca perimetrales) ---
            CreateBoxCol(collidersRoot, "Cliff_Left_Upper", new Vector2(-7.2f, 2.6f), new Vector2(3.6f, 4.5f));
            CreateBoxCol(collidersRoot, "Cliff_Left_Mid", new Vector2(-7.6f, -0.6f), new Vector2(3.0f, 3.5f));
            CreateBoxCol(collidersRoot, "Cliff_Left_Lower", new Vector2(-7.0f, -3.4f), new Vector2(3.6f, 3.5f));
            CreateBoxCol(collidersRoot, "Cliff_Left_Corner", new Vector2(-5.8f, -5.2f), new Vector2(3.8f, 2.2f));

            // --- ACANTILADOS DERECHOS (Empujados hacia afuera para NO obstruir el camino inferior derecho) ---
            CreateBoxCol(collidersRoot, "Cliff_Right_Upper", new Vector2(7.4f, 2.6f), new Vector2(3.6f, 4.5f));
            CreateBoxCol(collidersRoot, "Cliff_Right_Mid", new Vector2(7.8f, -0.6f), new Vector2(2.8f, 3.5f));
            CreateBoxCol(collidersRoot, "Cliff_Right_Lower", new Vector2(8.0f, -3.4f), new Vector2(2.5f, 3.5f)); // ¡Despejado!
            CreateBoxCol(collidersRoot, "Cliff_Right_Corner", new Vector2(7.6f, -5.2f), new Vector2(2.5f, 2.2f)); // ¡Despejado!

            // --- MURO SUPERIOR EN LA BASE DE ÁRBOLES Y ROCAS (Impide caminar sobre las rocas y copas) ---
            CreateBoxCol(collidersRoot, "Cliff_Behind_P1", new Vector2(-4.94f, 1.45f), new Vector2(2.6f, 1.8f));
            CreateBoxCol(collidersRoot, "Cliff_UpperLeft_1", new Vector2(-3.3f, 2.10f), new Vector2(2.2f, 1.6f));
            CreateBoxCol(collidersRoot, "Cliff_UpperLeft_2", new Vector2(-1.8f, 2.75f), new Vector2(1.8f, 1.6f));
            CreateBoxCol(collidersRoot, "Cliff_UpperRight_1", new Vector2(1.8f, 2.75f), new Vector2(1.8f, 1.6f));
            CreateBoxCol(collidersRoot, "Cliff_UpperRight_2", new Vector2(3.3f, 2.10f), new Vector2(2.2f, 1.6f));
            CreateBoxCol(collidersRoot, "Cliff_Behind_P3", new Vector2(4.94f, 1.45f), new Vector2(2.6f, 1.8f));
            CreateBoxCol(collidersRoot, "Cliff_Right_P3", new Vector2(5.95f, 1.05f), new Vector2(1.2f, 2.0f));

            // Roca central entre portal 2 y 3
            CreateCircleCol(collidersRoot, "Boulder_TopRight", new Vector2(2.05f, 1.40f), 0.55f);

            // --- LÍMITE INFERIOR DEL MAPA ---
            CreateBoxCol(collidersRoot, "Bottom_Wall", new Vector2(0.0f, -5.8f), new Vector2(22.0f, 1.2f));

            // --- TRONCOS DE ÁRBOLES (Solo colisión pequeña a nivel de pie, ramas transitables por detrás) ---
            CreateCircleCol(collidersRoot, "Pine_BottomLeft_Base", new Vector2(-3.8f, -3.6f), 0.45f);
            CreateCircleCol(collidersRoot, "Oak_BottomRight_Trunk", new Vector2(3.8f, -2.8f), 0.45f); // Base del tronco

            // --- COLUMNAS DE PIEDRA DE LOS ARCOS ---
            // Portal Verde (P1): Columnas laterales sólidas, arco central abierto
            CreateBoxCol(collidersRoot, "P1_Column_Left", new Vector2(-5.85f, 0.98f), new Vector2(0.50f, 1.6f));
            CreateBoxCol(collidersRoot, "P1_Column_Right", new Vector2(-4.05f, 0.98f), new Vector2(0.50f, 1.6f));

            // Portal Azul (P2): Columnas laterales Y BLOQUEADOR FRONTAL SÓLIDO (Desactivado)
            CreateBoxCol(collidersRoot, "P2_Column_Left", new Vector2(-1.10f, 3.45f), new Vector2(0.50f, 1.6f));
            CreateBoxCol(collidersRoot, "P2_Column_Right", new Vector2(1.10f, 3.45f), new Vector2(0.50f, 1.6f));
            CreateBoxCol(collidersRoot, "P2_SOLID_GATE_BLOCKER", new Vector2(0.0f, 3.40f), new Vector2(2.1f, 1.4f)); // ¡SÓLIDO!

            // Portal Violeta (P3): Columnas laterales Y BLOQUEADOR FRONTAL SÓLIDO (Desactivado)
            CreateBoxCol(collidersRoot, "P3_Column_Left", new Vector2(4.05f, 0.98f), new Vector2(0.50f, 1.6f));
            CreateBoxCol(collidersRoot, "P3_Column_Right", new Vector2(5.85f, 0.98f), new Vector2(0.50f, 1.6f));
            CreateBoxCol(collidersRoot, "P3_SOLID_GATE_BLOCKER", new Vector2(4.94f, 0.95f), new Vector2(2.0f, 1.5f)); // ¡SÓLIDO!

            // 5. CAPA DE ÁRBOLES EN PRIMER PLANO (Y-Sorting para caminar por detrás y ocultarse)
            var foregroundRoot = new GameObject("[Foreground_Props]");
            var oakSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OAK_TREE_SPRITE_PATH);
            if (oakSprite != null)
            {
                var oakGO = new GameObject("Oak_Tree_Foreground");
                oakGO.transform.SetParent(foregroundRoot.transform, false);
                oakGO.transform.position = new Vector3(3.8f, -2.4f, 0f);
                oakGO.transform.localScale = new Vector3(1.35f, 1.35f, 1.0f);
                var oakSR = oakGO.AddComponent<SpriteRenderer>();
                oakSR.sprite = oakSprite;
                // Sorting order calculado a la base del tronco:
                oakSR.sortingOrder = 500 - Mathf.RoundToInt(-2.8f * 10f); // 528 -> Cuando el perro está en Y > -2.8, ¡pasa por detrás!
            }

            // 6. LOS 3 PORTALES MÍSTICOS (Triggers & Lógica)
            var portalsRoot = new GameObject("[Portales_Mision]");

            // Portal 1: Verde (Nivel 1 - Ascenso Vertical Jump King) -> ¡ACTIVO Y TRANSITABLE!
            CreatePortalTrigger(portalsRoot, "Portal_01_Verde_Ascenso",
                new Vector3(-4.94f, 0.91f, 0f),
                isLocked: false,
                targetScene: "Ascenso_Vertical",
                title: "Nivel 1: Ascenso Vertical Ancestral",
                lore: "El Gran Salto hacia la Cima - Modo Jump King",
                glowColor: new Color(0.2f, 0.95f, 0.45f, 0.9f));

            // Portal 2: Azul (Nivel 2) -> ¡DESACTIVADO CON MENSAJE!
            CreatePortalTrigger(portalsRoot, "Portal_02_Azul_Cuevas",
                new Vector3(0.0f, 2.65f, 0f),
                isLocked: true,
                targetScene: "Level_02",
                title: "Nivel 2: Cuevas del Zarzal",
                lore: "Canalización inactiva. Supera el Nivel 1 (Ascenso Vertical) primero.",
                glowColor: new Color(0.25f, 0.55f, 1f, 0.5f));

            // Portal 3: Púrpura (Nivel 3) -> ¡DESACTIVADO CON MENSAJE!
            CreatePortalTrigger(portalsRoot, "Portal_03_Purpura_Altar",
                new Vector3(4.94f, 0.20f, 0f),
                isLocked: true,
                targetScene: "Level_03",
                title: "Nivel 3: Altar de Fuego",
                lore: "Santuario Sellado. Misión bloqueada.",
                glowColor: new Color(0.85f, 0.35f, 1f, 0.5f));

            // 7. LOS DOS PROTAGONISTAS: Falin y Mía (Escala proporcionada & Diamantes flotantes)
            var duoRoot = new GameObject("Kimaya_Duo_Team");
            duoRoot.transform.position = Vector3.zero;

            var duoController = duoRoot.AddComponent<KimayaHubDuoController>();
            duoController.walkSpeed = 4.8f;
            duoController.runSpeed = 7.6f;
            duoController.companionFollowDistance = 1.35f;

            // --- FALIN (San Bernardo - Escala 0.66x proporcionada al mapa) ---
            var falinGO = new GameObject("Falin_SanBernardo");
            falinGO.tag = "Player";
            falinGO.transform.position = new Vector3(-0.75f, -1.10f, 0f);
            falinGO.transform.localScale = new Vector3(0.66f, 0.66f, 1.0f);

            var falinSR = falinGO.AddComponent<SpriteRenderer>();
            falinSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Falin/Idle/rotations/south.png");
            falinSR.sortingOrder = 500;

            // Colisión compacta solo a la altura de las patitas
            var falinCol = falinGO.AddComponent<CircleCollider2D>();
            falinCol.radius = 0.28f;
            falinCol.offset = new Vector2(0f, -0.18f);

            var falinRB = falinGO.AddComponent<Rigidbody2D>();
            falinRB.gravityScale = 0f;
            falinRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            falinRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Diamante Dorado Flotante Animado sobre la cabeza de Falin
            CreateDiamondMarker(falinGO, FALIN_DIAMOND_PATH, 1.15f);

            duoController.falinObject = falinGO;
            duoController.falinSprites = BuildDogSpriteSet("Falin", $"{PIXELLAB_PATH}/Falin");

            // --- MIA (Schnauzer - Escala 0.56x más ágil y pequeña) ---
            var miaGO = new GameObject("Mia_Schnauzer");
            miaGO.tag = "Player";
            miaGO.transform.position = new Vector3(0.75f, -1.10f, 0f);
            miaGO.transform.localScale = new Vector3(0.56f, 0.56f, 1.0f);

            var miaSR = miaGO.AddComponent<SpriteRenderer>();
            miaSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Mia/Idle/rotations/south.png");
            miaSR.sortingOrder = 500;

            // Colisión compacta solo a la altura de las patitas
            var miaCol = miaGO.AddComponent<CircleCollider2D>();
            miaCol.radius = 0.24f;
            miaCol.offset = new Vector2(0f, -0.16f);

            var miaRB = miaGO.AddComponent<Rigidbody2D>();
            miaRB.gravityScale = 0f;
            miaRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            miaRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Diamante Cian / Menta Flotante Animado sobre la cabeza de Mía
            CreateDiamondMarker(miaGO, MIA_DIAMOND_PATH, 1.15f);

            // Ignorar colisiones mutuas entre ambos perros para que nunca se bloqueen al seguirse
            Physics2D.IgnoreCollision(falinCol, miaCol, true);

            duoController.miaObject = miaGO;
            duoController.miaSprites = BuildDogSpriteSet("Mia", $"{PIXELLAB_PATH}/Mia");

            // 8. CÁMARA (Encuadre dinámico que llena la pantalla al 100%)
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.6f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.13f);
            camGO.AddComponent<AudioListener>();

            var camFollow = camGO.AddComponent<KimayaHubCameraFollow>();
            camFollow.mapRenderer = mapSR;
            camFollow.smoothSpeed = 5.5f;

            // 9. SETUP GENERAL Y HUD
            var setupGO = new GameObject("[GameSetup]");
            setupGO.AddComponent<GameData>();
            setupGO.AddComponent<SceneLoader>();
            setupGO.AddComponent<KimayaHubHUD>();

            // 10. Guardar escena
            EditorSceneManager.SaveScene(scene, HUB_SCENE_PATH);
            UpdateBuildSettings();

            Debug.Log($"[KimayaHubBuilder] ✅ Hub de Kimaya v7 creado exitosamente en: {HUB_SCENE_PATH}");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Hub de Kimaya Regenerado",
                    "¡El Hub del Claro del Bosque v7 ha sido creado!\n\n" +
                    "• Personajes escalados con proporciones perfectas al mapa.\n" +
                    "• Diamante flotante brillante sobre el perro seleccionado (estilo RPG).\n" +
                    "• Camino adoquinado inferior derecho 100% despejado y transitable.\n" +
                    "• Muros superiores ajustados (imposible caminar sobre rocas o copas altas).\n" +
                    "• Y-Sorting activado para pasar por detrás del gran roble y ocultarse.", "Aceptar");
            }
        }

        private static void CreateBoxCol(GameObject parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        private static void CreateCircleCol(GameObject parent, string name, Vector2 pos, float radius)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = radius;
        }

        private static void CreatePortalTrigger(GameObject parent, string name, Vector3 pos, bool isLocked,
            string targetScene, string title, string lore, Color glowColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.8f, 1.8f);

            var portal = go.AddComponent<KimayaHubPortal>();
            portal.isLocked = isLocked;
            portal.targetSceneName = targetScene;
            portal.destinationTitle = title;
            portal.questLore = lore;
            portal.lockedTitle = title;
            portal.lockedReason = lore;
            portal.runeGlowColor = glowColor;
            portal.interactionRadius = 2.4f;
        }

        private static void CreateDiamondMarker(GameObject dogGO, string diamondSpritePath, float yOffset)
        {
            var diamondGO = new GameObject("DiamondMarker");
            diamondGO.transform.SetParent(dogGO.transform, false);
            diamondGO.transform.localPosition = new Vector3(0f, yOffset, 0f);

            var sr = diamondGO.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(diamondSpritePath);
            sr.sortingOrder = 950; // Siempre visible por encima del personaje

            var bobber = diamondGO.AddComponent<KimayaDiamondBobber>();
            bobber.baseY = yOffset;
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

            // Power (Barking / Sneaking)
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
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureSpriteImport(string relativePath, float ppu)
        {
            var importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                bool dirty = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    dirty = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    dirty = true;
                }
                if (Math.Abs(importer.spritePixelsPerUnit - ppu) > 0.01f)
                {
                    importer.spritePixelsPerUnit = ppu;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                }
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

            AddOrSetScene(MAIN_MENU_SCENE_PATH, 0);
            AddOrSetScene(HUB_SCENE_PATH, 1);
            AddOrSetScene(JUMP_KING_SCENE_PATH, 2);

            EditorBuildSettings.scenes = currentScenes.ToArray();
        }
    }
}
