using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Platformer.TopDown;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena House_Interior con tiles de Pixel Crawler.
    /// Menú: Tools > Village > Build House Interior Scene
    /// </summary>
    public static class HouseInteriorBuilder
    {
        private const string SCENE_NAME   = "House_Interior";
        private const string SCENE_PATH   = "Assets/Scenes/House_Interior.unity";
        private const string PC_ROOT      = "Assets/Sprites/PixelCrawler";
        private const string FLOOR_PATH   = PC_ROOT + "/Environment/Tilesets/Floors_Tiles.png";
        private const string WALLS_PATH   = PC_ROOT + "/Environment/Structures/Buildings/Interior/Interior_Walls_01.png";
        private const string PROPS_PATH   = PC_ROOT + "/Environment/Structures/Buildings/Interior/Interior_Props_01.png";

        // Tamaño del interior (en unidades Unity, PPU=16)
        private const float ROOM_W = 8f;
        private const float ROOM_H = 6f;

        [MenuItem("Tools/Village/Build House Interior Scene")]
        public static void BuildHouseInterior()
        {
            // Confirmar
            bool ok = EditorUtility.DisplayDialog("HouseInteriorBuilder",
                "Esto creará (o sobreescribirá) la escena:\n" + SCENE_PATH +
                "\n\n¿Continuar?", "Sí, crear", "Cancelar");
            if (!ok) return;

            // Guardar escena actual
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Crear nueva escena vacía
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara ─────────────────────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            var cam   = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 4f;
            cam.backgroundColor  = new Color(0.08f, 0.06f, 0.12f); // oscuro
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            camGo.tag = "MainCamera";

            // ── Luz 2D ambiental ───────────────────────────────────────────────
            var lightGo = new GameObject("Ambient Light");
            var light2d = lightGo.AddComponent<Light>();
            light2d.type      = LightType.Directional;
            light2d.intensity = 1f;
            light2d.color     = new Color(1f, 0.95f, 0.85f);

            // ── Suelo ─────────────────────────────────────────────────────────
            BuildFloor();

            // ── Paredes ────────────────────────────────────────────────────────
            BuildWalls();

            // ── Props / Muebles ────────────────────────────────────────────────
            BuildProps();

            // ── Puerta de salida ──────────────────────────────────────────────
            BuildExitDoor();

            // ── Punto de spawn del jugador ─────────────────────────────────────
            var spawnGo = new GameObject("PlayerSpawnPoint");
            spawnGo.transform.position = new Vector3(0f, -(ROOM_H / 2f) + 1f, 0f);
            spawnGo.tag = "Respawn";

            // ── Guardar escena ─────────────────────────────────────────────────
            string scenesDir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(scenesDir))
                Directory.CreateDirectory(scenesDir);

            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AssetDatabase.Refresh();

            // Añadir a Build Settings si no está
            AddSceneToBuildSettings(SCENE_PATH);

            EditorUtility.DisplayDialog("HouseInteriorBuilder",
                $"✅ Escena '{SCENE_NAME}' creada en:\n{SCENE_PATH}\n\n" +
                "Las puertas de la aldea ya apuntan a esta escena.\n" +
                "Presiona Play en la aldea → entra a una casa → aparecerás aquí.",
                "OK");
        }

        // ── Suelo (quad tileado) ───────────────────────────────────────────────
        private static void BuildFloor()
        {
            Sprite floorSprite = LoadSubSprite(FLOOR_PATH, 0); // primer tile

            var root = new GameObject("[Interior_Floor]");
            root.transform.position = Vector3.zero;

            int cols = Mathf.CeilToInt(ROOM_W);
            int rows = Mathf.CeilToInt(ROOM_H);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    var tile = new GameObject($"Floor_{col}_{row}");
                    tile.transform.parent = root.transform;
                    tile.transform.position = new Vector3(
                        -ROOM_W / 2f + col + 0.5f,
                        -ROOM_H / 2f + row + 0.5f,
                        0f
                    );

                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite       = floorSprite;
                    sr.sortingOrder = -100;

                    if (floorSprite == null)
                    {
                        // Fallback: quad de color
                        sr.color = new Color(0.55f, 0.40f, 0.25f);
                        tile.transform.localScale = Vector3.one;
                    }
                }
            }
        }

        // ── Paredes ───────────────────────────────────────────────────────────
        private static void BuildWalls()
        {
            Sprite wallSprite = LoadSubSprite(WALLS_PATH, 0);

            var root = new GameObject("[Interior_Walls]");

            // Pared superior (N)
            CreateWallSegment(root.transform, "Wall_N", 0f, ROOM_H / 2f + 0.5f, ROOM_W, 1f, wallSprite, 10);
            // Pared izquierda (O)
            CreateWallSegment(root.transform, "Wall_W", -ROOM_W / 2f - 0.5f, 0f, 1f, ROOM_H + 2f, wallSprite, 10);
            // Pared derecha (E)
            CreateWallSegment(root.transform, "Wall_E",  ROOM_W / 2f + 0.5f, 0f, 1f, ROOM_H + 2f, wallSprite, 10);
            // Pared inferior S — dejamos hueco en centro para la puerta
            CreateWallSegment(root.transform, "Wall_S_L", -ROOM_W / 2f + 1.25f,  -ROOM_H / 2f - 0.5f, 2.5f, 1f, wallSprite, 10);
            CreateWallSegment(root.transform, "Wall_S_R",  ROOM_W / 2f - 1.25f,  -ROOM_H / 2f - 0.5f, 2.5f, 1f, wallSprite, 10);
        }

        private static void CreateWallSegment(Transform parent, string name,
            float x, float y, float w, float h, Sprite sprite, int sortOrder)
        {
            var go = new GameObject(name);
            go.transform.parent   = parent;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(w, h, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = sprite;
            sr.sortingOrder = sortOrder;
            sr.drawMode     = SpriteDrawMode.Tiled;
            sr.size         = new Vector2(1f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);
        }

        // ── Props / Muebles ───────────────────────────────────────────────────
        private static void BuildProps()
        {
            Sprite propSprite = LoadSubSprite(PROPS_PATH, 0);

            var root = new GameObject("[Interior_Props]");

            // Cama (esquina superior izquierda)
            PlaceProp(root.transform, "Bed",        -2.5f,  1.5f, propSprite, 1.5f);
            // Mesa (centro)
            PlaceProp(root.transform, "Table",       0.5f,  0.5f, propSprite, 1f);
            // Baúl (esquina superior derecha)
            PlaceProp(root.transform, "Chest",       2.5f,  1.8f, propSprite, 1f);
            // Estante (pared izquierda)
            PlaceProp(root.transform, "Shelf",      -2.8f,  0f,   propSprite, 1f);
            // Silla (junto a la mesa)
            PlaceProp(root.transform, "Chair",       1.5f,  0f,   propSprite, 0.8f);
        }

        private static void PlaceProp(Transform parent, string name,
            float x, float y, Sprite sprite, float scale)
        {
            var go = new GameObject(name);
            go.transform.parent   = parent;
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = sprite;
            sr.sortingOrder = 500 - Mathf.RoundToInt(y * 10f);

            // Pequeño collider
            var col = go.AddComponent<BoxCollider2D>();
            col.size   = new Vector2(0.8f / scale, 0.4f / scale);
            col.offset = new Vector2(0f, -0.2f / scale);
        }

        // ── Puerta de salida ──────────────────────────────────────────────────
        private static void BuildExitDoor()
        {
            var doorGo = new GameObject("Exit_Door");
            doorGo.transform.position = new Vector3(0f, -(ROOM_H / 2f) - 0.3f, 0f);

            // Sprite de puerta (placeholder naranja)
            var sr = doorGo.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.6f, 0.35f, 0.1f, 1f);
            sr.sortingOrder = 20;
            doorGo.transform.localScale = new Vector3(1.2f, 0.3f, 1f);

            // Collider trigger
            var col = doorGo.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(1.2f, 0.5f);
            col.offset    = new Vector2(0f, 0.1f);
            col.isTrigger = true;

            // SceneTransition → regresa a Village_Scene
            var trans = doorGo.AddComponent<SceneTransition>();
            trans.targetScene   = "Village_Scene";
            trans.promptMessage = "[E] Salir de la casa";

            // Label visual
            var labelGo = new GameObject("DoorLabel");
            labelGo.transform.parent = doorGo.transform;
            labelGo.transform.localPosition = Vector3.up * 1.5f;
            labelGo.transform.localScale    = Vector3.one / 1.2f;
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static Sprite LoadSubSprite(string path, int index)
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            int count = 0;
            foreach (var a in allAssets)
            {
                if (a is Sprite sp)
                {
                    if (count == index) return sp;
                    count++;
                }
            }
            // Fallback: cargar como sprite único
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
                if (s.path == scenePath) return; // ya está

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[HouseInteriorBuilder] '{SCENE_NAME}' añadida a Build Settings.");
        }
    }
}
