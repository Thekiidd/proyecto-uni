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
    /// Utiliza el mapa en pixel art con el círculo de runas central y los tres portales:
    ///   - Portal 1 (Izquierdo / Verde): ACTIVO -> Nivel 1: Ascenso Vertical (Jump King)
    ///   - Portal 2 (Central / Azul): DESACTIVADO -> Requiere completar Nivel 1
    ///   - Portal 3 (Derecho / Violeta): DESACTIVADO -> Santuario Sellado
    ///   - Colisiones perimetrales completas (acantilados, árboles, rocas y arcos)
    ///   - Dúo de perros (Falin y Mía) con animaciones 4D de PixelLab y cambio con [TAB o Q]
    ///   - HUD con misiones, indicadores y menú.
    ///   - Cámara adaptativa que llena la pantalla al 100% sin bordes vacíos.
    /// Menú: Tools > Kimaya > 🏡 [3] Construir Hub 2D de Kimaya (Falin y Mia)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaHubBuilder
    {
        public const string HUB_SCENE_PATH = "Assets/Scenes/Hub.unity";
        public const string MAIN_MENU_SCENE_PATH = "Assets/Scenes/MainMenu.unity";
        public const string JUMP_KING_SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaHub_Clearing_v5";

        private const string HUB_MAP_IMAGE_PATH = "Assets/Sprites/Hub/Forest_Hub_Clearing_3Portals.png";
        private const string PIXELLAB_PATH = "Assets/Sprites/PixelLab";

        static KimayaHubBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaHubBuilder] 🚀 Ejecutando AutoBuildCheck v5 para Hub de 3 Portales...");
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
            Debug.Log("[KimayaHubBuilder] 🔨 Construyendo Hub del Claro del Bosque con 3 Portales (v5)...");

            // 1. Asegurar importación del sprite del Hub
            EnsureSpriteImport(HUB_MAP_IMAGE_PATH, 32);

            // 2. Crear nueva escena limpia
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Fondo: Claro del Bosque Ancestral escalado a 1.22x para cubrir 100% de la pantalla
            var mapGO = new GameObject("Forest_Hub_Background");
            mapGO.transform.position = Vector3.zero;
            mapGO.transform.localScale = new Vector3(1.22f, 1.22f, 1.0f);
            var mapSR = mapGO.AddComponent<SpriteRenderer>();
            mapSR.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HUB_MAP_IMAGE_PATH);
            mapSR.sortingOrder = -500;

            // 4. Sistema de Colisiones Perimetrales y Obstáculos
            var collidersRoot = new GameObject("[Hub_Colliders]");
            var collidersRB = collidersRoot.AddComponent<Rigidbody2D>();
            collidersRB.bodyType = RigidbodyType2D.Static;

            // --- Acantilados Izquierdos (Rocas de la montaña) ---
            CreateBoxCol(collidersRoot, "Cliff_Left_Top", new Vector2(-7.1f, 3.3f), new Vector2(4.5f, 6.0f));
            CreateBoxCol(collidersRoot, "Cliff_Left_Mid", new Vector2(-7.7f, -0.7f), new Vector2(3.4f, 4.0f));
            CreateBoxCol(collidersRoot, "Cliff_Left_Bottom", new Vector2(-6.3f, -4.6f), new Vector2(5.2f, 4.2f));

            // --- Acantilados Derechos (Rocas de la montaña) ---
            CreateBoxCol(collidersRoot, "Cliff_Right_Top", new Vector2(7.1f, 3.3f), new Vector2(4.5f, 6.0f));
            CreateBoxCol(collidersRoot, "Cliff_Right_Mid", new Vector2(7.7f, -0.7f), new Vector2(3.4f, 4.0f));
            CreateBoxCol(collidersRoot, "Cliff_Right_Bottom", new Vector2(6.3f, -4.6f), new Vector2(5.2f, 4.2f));

            // --- Muro Superior y Bosque Alto (Entre y sobre los portales) ---
            CreateBoxCol(collidersRoot, "Cliff_Top_LeftGap", new Vector2(-2.6f, 4.4f), new Vector2(3.3f, 3.4f));
            CreateBoxCol(collidersRoot, "Cliff_Top_RightGap", new Vector2(2.6f, 4.4f), new Vector2(3.3f, 3.4f));
            CreateBoxCol(collidersRoot, "Cliff_Top_Ceiling", new Vector2(0.0f, 5.3f), new Vector2(4.8f, 1.8f));

            // --- Límite Inferior (Salida del sendero) ---
            CreateBoxCol(collidersRoot, "Path_Bottom_Fence", new Vector2(0.0f, -5.9f), new Vector2(20.0f, 1.5f));

            // --- Obstáculos en el Claro ---
            CreateBoxCol(collidersRoot, "Pines_BottomLeft", new Vector2(-3.4f, -3.5f), new Vector2(2.2f, 2.2f));
            CreateBoxCol(collidersRoot, "OakTree_BottomRight", new Vector2(4.0f, -3.0f), new Vector2(2.7f, 2.7f));
            CreateBoxCol(collidersRoot, "RockBush_TopRight", new Vector2(2.3f, 1.6f), new Vector2(1.5f, 1.0f));
            CreateBoxCol(collidersRoot, "Log_CenterLeft", new Vector2(-2.7f, -1.5f), new Vector2(1.4f, 0.8f));

            // --- Columnas y Cornisas de Piedra de los 3 Portales ---
            // Portal Verde (Izquierda: X=-4.94, Y=0.91)
            CreateBoxCol(collidersRoot, "P1_Post_L", new Vector2(-5.80f, 0.98f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P1_Post_R", new Vector2(-4.08f, 0.98f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P1_Arch", new Vector2(-4.94f, 2.20f), new Vector2(2.2f, 0.6f));

            // Portal Azul (Centro: X=0.0, Y=3.48)
            CreateBoxCol(collidersRoot, "P2_Post_L", new Vector2(-1.04f, 3.54f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P2_Post_R", new Vector2(1.04f, 3.54f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P2_Arch", new Vector2(0.0f, 4.70f), new Vector2(2.2f, 0.6f));

            // Portal Violeta (Derecha: X=4.94, Y=0.91)
            CreateBoxCol(collidersRoot, "P3_Post_L", new Vector2(4.08f, 0.98f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P3_Post_R", new Vector2(5.80f, 0.98f), new Vector2(0.55f, 1.7f));
            CreateBoxCol(collidersRoot, "P3_Arch", new Vector2(4.94f, 2.20f), new Vector2(2.2f, 0.6f));

            // 5. LOS 3 PORTALES MÍSTICOS (Triggers & Lógica)
            var portalsRoot = new GameObject("[Portales_Mision]");

            // Portal 1: Verde (Nivel 1 - Ascenso Vertical Jump King) -> ¡ACTIVO!
            CreatePortalTrigger(portalsRoot, "Portal_01_Verde_Ascenso",
                new Vector3(-4.94f, 0.91f, 0f),
                isLocked: false,
                targetScene: "Ascenso_Vertical",
                title: "Nivel 1: Ascenso Vertical Ancestral",
                lore: "El Gran Salto hacia la Cima - Modo Jump King",
                glowColor: new Color(0.2f, 0.95f, 0.45f, 0.9f));

            // Portal 2: Azul (Nivel 2) -> ¡DESACTIVADO!
            CreatePortalTrigger(portalsRoot, "Portal_02_Azul_Cuevas",
                new Vector3(0.0f, 3.48f, 0f),
                isLocked: true,
                targetScene: "Level_02",
                title: "Nivel 2: Cuevas del Zarzal",
                lore: "Canalización inactiva. Supera el Nivel 1 (Ascenso Vertical) primero.",
                glowColor: new Color(0.25f, 0.55f, 1f, 0.5f));

            // Portal 3: Púrpura (Nivel 3) -> ¡DESACTIVADO!
            CreatePortalTrigger(portalsRoot, "Portal_03_Purpura_Altar",
                new Vector3(4.94f, 0.91f, 0f),
                isLocked: true,
                targetScene: "Level_03",
                title: "Nivel 3: Altar de Fuego",
                lore: "Santuario Sellado. Misión bloqueada.",
                glowColor: new Color(0.85f, 0.35f, 1f, 0.5f));

            // 6. LOS DOS PROTAGONISTAS: Falin y Mía
            var duoRoot = new GameObject("Kimaya_Duo_Team");
            duoRoot.transform.position = Vector3.zero;

            var duoController = duoRoot.AddComponent<KimayaHubDuoController>();
            duoController.walkSpeed = 4.8f;
            duoController.runSpeed = 7.5f;
            duoController.companionFollowDistance = 1.35f;

            // --- FALIN (San Bernardo) ---
            var falinGO = new GameObject("Falin_SanBernardo");
            falinGO.tag = "Player";
            falinGO.transform.position = new Vector3(-0.75f, -1.10f, 0f);

            var falinSR = falinGO.AddComponent<SpriteRenderer>();
            falinSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Falin/Idle/rotations/south.png");
            falinSR.sortingOrder = 500;

            var falinCol = falinGO.AddComponent<CircleCollider2D>();
            falinCol.radius = 0.32f;
            falinCol.offset = new Vector2(0f, -0.15f);

            var falinRB = falinGO.AddComponent<Rigidbody2D>();
            falinRB.gravityScale = 0f;
            falinRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            falinRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CreateSelectionMarker(falinGO, new Color(1f, 0.85f, 0.2f, 0.55f));

            duoController.falinObject = falinGO;
            duoController.falinSprites = BuildDogSpriteSet("Falin", $"{PIXELLAB_PATH}/Falin");

            // --- MIA (Schnauzer) ---
            var miaGO = new GameObject("Mia_Schnauzer");
            miaGO.tag = "Player";
            miaGO.transform.position = new Vector3(0.75f, -1.10f, 0f);

            var miaSR = miaGO.AddComponent<SpriteRenderer>();
            miaSR.sprite = LoadSprite($"{PIXELLAB_PATH}/Mia/Idle/rotations/south.png");
            miaSR.sortingOrder = 500;

            var miaCol = miaGO.AddComponent<CircleCollider2D>();
            miaCol.radius = 0.26f;
            miaCol.offset = new Vector2(0f, -0.12f);

            var miaRB = miaGO.AddComponent<Rigidbody2D>();
            miaRB.gravityScale = 0f;
            miaRB.constraints = RigidbodyConstraints2D.FreezeRotation;
            miaRB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CreateSelectionMarker(miaGO, new Color(0.3f, 0.85f, 1f, 0.55f));

            // Ignorar colisiones mutuas entre ambos perros para que nunca se bloqueen al seguirse
            Physics2D.IgnoreCollision(falinCol, miaCol, true);

            duoController.miaObject = miaGO;
            duoController.miaSprites = BuildDogSpriteSet("Mia", $"{PIXELLAB_PATH}/Mia");

            // 7. CÁMARA (Encuadre perfecto y dinámico)
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 4.6f; // Altura 9.2m, Ancho 16:9 = 16.35m (cubierto al 100% por el mapa de 18.3m)
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.09f, 0.10f, 0.13f); // Fondo roca oscura profunda
            camGO.AddComponent<AudioListener>();

            var camFollow = camGO.AddComponent<KimayaHubCameraFollow>();
            camFollow.mapRenderer = mapSR;
            camFollow.smoothSpeed = 5.5f;

            // 8. SETUP GENERAL Y HUD
            var setupGO = new GameObject("[GameSetup]");
            setupGO.AddComponent<GameData>();
            setupGO.AddComponent<SceneLoader>();
            setupGO.AddComponent<KimayaHubHUD>();

            // 9. Guardar escena
            EditorSceneManager.SaveScene(scene, HUB_SCENE_PATH);
            UpdateBuildSettings();

            Debug.Log($"[KimayaHubBuilder] ✅ Hub de Kimaya v5 creado exitosamente en: {HUB_SCENE_PATH}");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Hub de Kimaya Creado",
                    "¡El Hub del Claro del Bosque ha sido regenerado!\n\n" +
                    "• Imagen 100% ajustada sin bordes verdes vacíos.\n" +
                    "• Sistema de compañero recíproco activo (Falin te sigue si usas a Mía, Mía te sigue si usas a Falin).\n" +
                    "• Controles con WASD / Flechas / Shift / TAB.\n" +
                    "• Portal Verde (Izquierdo) ACTIVO hacia Ascenso Vertical.", "Aceptar");
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

        private static void CreatePortalTrigger(GameObject parent, string name, Vector3 pos, bool isLocked,
            string targetScene, string title, string lore, Color glowColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 1.8f);

            var portal = go.AddComponent<KimayaHubPortal>();
            portal.isLocked = isLocked;
            portal.targetSceneName = targetScene;
            portal.destinationTitle = title;
            portal.questLore = lore;
            portal.lockedTitle = title;
            portal.lockedReason = lore;
            portal.runeGlowColor = glowColor;
            portal.interactionRadius = 2.2f;
        }

        private static void CreateSelectionMarker(GameObject dogGO, Color markerColor)
        {
            var markerGO = new GameObject("ActiveMarker");
            markerGO.transform.SetParent(dogGO.transform, false);
            markerGO.transform.localPosition = new Vector3(0f, -0.32f, 0f);

            var sr = markerGO.AddComponent<SpriteRenderer>();
            sr.color = markerColor;
            sr.sortingOrder = 490;

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
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 16), new Vector2(0.5f, 0.5f), 16f);
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
