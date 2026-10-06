using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Platformer.TopDown;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye escenas de interiores de casas usando tiles de Pixel Crawler.
    /// Menú: Tools > Village > Build House Interior Scene
    ///
    /// Crea 3 variantes de interior:
    ///   House_Interior          — casa estándar de aldeano
    ///   House_Interior_Tavern   — interior de taberna con barras y mesas
    ///   House_Interior_Merchant — tienda de mercader
    /// </summary>
    public static class HouseInteriorBuilder
    {
        private const string PC_ROOT   = "Assets/Sprites/PixelCrawler";
        private const string FLOOR_TS  = PC_ROOT + "/Environment/Tilesets/Floors_Tiles.png";
        private const string WALL_INT  = PC_ROOT + "/Environment/Structures/Buildings/Interior/Interior_Walls_01.png";
        private const string PROPS_INT = PC_ROOT + "/Environment/Structures/Buildings/Interior/Interior_Props_01.png";
        private const string FURN_TS   = PC_ROOT + "/Environment/Props/Static/Furniture.png";
        private const string FARM_TS   = PC_ROOT + "/Environment/Props/Static/Farm.png";

        private const float ROOM_W = 10f;
        private const float ROOM_H =  7f;

        [MenuItem("Tools/Village/Build House Interior Scene")]
        public static void BuildStandardHouse()
            => BuildInterior("House_Interior", "Assets/Scenes/House_Interior.unity",
                InteriorType.Standard, "Village_Scene");

        [MenuItem("Tools/Village/Build Tavern Interior Scene")]
        public static void BuildTavern()
            => BuildInterior("House_Interior_Tavern", "Assets/Scenes/House_Interior_Tavern.unity",
                InteriorType.Tavern, "PixelCrawler_Village");

        [MenuItem("Tools/Village/Build Merchant Interior Scene")]
        public static void BuildMerchant()
            => BuildInterior("House_Interior_Merchant", "Assets/Scenes/House_Interior_Merchant.unity",
                InteriorType.Merchant, "PixelCrawler_Village");

        private enum InteriorType { Standard, Tavern, Merchant }

        private static void BuildInterior(string sceneName, string scenePath,
            InteriorType type, string returnScene)
        {
            bool ok = EditorUtility.DisplayDialog("HouseInteriorBuilder",
                $"Crear '{sceneName}'?\n\nUsa los tiles de Pixel Crawler para el interior.\nSalida: → {returnScene}",
                "Sí, crear", "Cancelar");
            if (!ok) return;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 4.5f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.06f, 0.05f, 0.1f);
            camGo.AddComponent<AudioListener>();

            // Luz ambiente
            var lightGo = new GameObject("Ambient");
            var lt = lightGo.AddComponent<Light>();
            lt.type      = LightType.Directional;
            lt.intensity = 0.85f;
            lt.color     = new Color(1f, 0.92f, 0.78f);

            // Suelo
            BuildFloor(type);

            // Paredes
            BuildWalls(returnScene);

            // Muebles según tipo
            switch (type)
            {
                case InteriorType.Standard:  BuildStandardFurniture();  break;
                case InteriorType.Tavern:    BuildTavernFurniture();    break;
                case InteriorType.Merchant:  BuildMerchantFurniture();  break;
            }

            // NPC interior (dueño de la casa)
            BuildInteriorNPC(type);

            // Spawn del jugador
            var spawn = new GameObject("PlayerSpawnPoint");
            spawn.transform.position = new Vector3(0f, -(ROOM_H / 2f) + 1.2f, 0f);
            spawn.tag = "Respawn";

            // Guardar
            string dir = Path.GetDirectoryName(scenePath);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, scenePath);
            AddToBuildSettings(scenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Interior Creado",
                $"✅ '{sceneName}' creado.\n\nPuerta de salida → {returnScene}\n\n" +
                "Las casas de la aldea ya tienen triggers de entrada configurados.\n" +
                "Presiona Play → entra a una casa → aparecerás aquí.", "OK");
        }

        // ── Suelo ─────────────────────────────────────────────────────────────
        private static void BuildFloor(InteriorType type)
        {
            Sprite spr = LoadSprite(FLOOR_TS, type == InteriorType.Tavern ? 3 : 0);
            Color c = type switch {
                InteriorType.Tavern   => new Color(0.6f, 0.4f, 0.25f),
                InteriorType.Merchant => new Color(0.45f, 0.45f, 0.45f),
                _                    => new Color(0.65f, 0.5f, 0.35f)
            };

            var root = new GameObject("[Floor]");
            int cols = Mathf.CeilToInt(ROOM_W);
            int rows = Mathf.CeilToInt(ROOM_H);

            for (int r = 0; r < rows; r++)
            {
                for (int c2 = 0; c2 < cols; c2++)
                {
                    var tile = new GameObject($"F_{c2}_{r}");
                    tile.transform.parent   = root.transform;
                    tile.transform.position = new Vector3(
                        -ROOM_W / 2f + c2 + 0.5f,
                        -ROOM_H / 2f + r + 0.5f, 0f);
                    var sr = tile.AddComponent<SpriteRenderer>();
                    sr.sprite       = spr;
                    sr.color        = c;
                    sr.sortingOrder = -100;
                }
            }
        }

        // ── Paredes ─────────────────────────────────────────────────────────
        private static void BuildWalls(string returnScene)
        {
            Sprite wallSpr = LoadSprite(WALL_INT, 0);
            var root = new GameObject("[Walls]");

            // Norte
            CreateWallSegment(root.transform, "Wall_N",
                new Vector2(0f, ROOM_H / 2f + 0.5f), new Vector2(ROOM_W + 2f, 1f), wallSpr,
                new Color(0.45f, 0.38f, 0.3f), 15, true);
            // Oeste
            CreateWallSegment(root.transform, "Wall_W",
                new Vector2(-ROOM_W / 2f - 0.5f, 0f), new Vector2(1f, ROOM_H + 2f), wallSpr,
                new Color(0.45f, 0.38f, 0.3f), 15, true);
            // Este
            CreateWallSegment(root.transform, "Wall_E",
                new Vector2(ROOM_W / 2f + 0.5f, 0f), new Vector2(1f, ROOM_H + 2f), wallSpr,
                new Color(0.45f, 0.38f, 0.3f), 15, true);
            // Sur izquierda (deja hueco de 2u para puerta)
            CreateWallSegment(root.transform, "Wall_S_L",
                new Vector2(-ROOM_W / 2f + 1.5f, -ROOM_H / 2f - 0.5f), new Vector2(3f, 1f), wallSpr,
                new Color(0.45f, 0.38f, 0.3f), 15, true);
            // Sur derecha
            CreateWallSegment(root.transform, "Wall_S_R",
                new Vector2(ROOM_W / 2f - 1.5f, -ROOM_H / 2f - 0.5f), new Vector2(3f, 1f), wallSpr,
                new Color(0.45f, 0.38f, 0.3f), 15, true);

            // Puerta de salida
            var door = new GameObject("Exit_Door");
            door.transform.position = new Vector3(0f, -(ROOM_H / 2f) - 0.15f, 0f);
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.size      = new Vector2(2f, 0.6f);
            doorCol.isTrigger = true;

            var trans = door.AddComponent<SceneTransition>();
            trans.targetScene   = returnScene;
            trans.promptMessage = "[E] Salir";
            trans.fadeDuration  = 0.4f;

            // Marca visual de la puerta
            var doorVis = new GameObject("Door_Visual");
            doorVis.transform.parent   = door.transform;
            doorVis.transform.localPosition = Vector3.zero;
            var dvSR = doorVis.AddComponent<SpriteRenderer>();
            dvSR.color        = new Color(0.55f, 0.32f, 0.12f);
            dvSR.sortingOrder = 20;
            doorVis.transform.localScale = new Vector3(2f, 0.25f, 1f);
        }

        private static void CreateWallSegment(Transform parent, string name,
            Vector2 pos, Vector2 size, Sprite spr, Color color, int order, bool addCollider)
        {
            var go = new GameObject(name);
            go.transform.parent   = parent;
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.color        = color;
            sr.sortingOrder = order;

            if (addCollider)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
            }
        }

        // ── Muebles: Casa Estándar ──────────────────────────────────────────
        private static void BuildStandardFurniture()
        {
            var root = new GameObject("[Furniture]");
            Sprite propSpr = LoadSprite(PROPS_INT, 0);
            Sprite furnSpr = LoadSprite(FURN_TS, 0);

            // Cama (esquina noroeste)
            PlaceProp(root.transform, "Cama", propSpr, new Color(0.6f, 0.5f, 0.7f),
                new Vector3(-3.5f, 2f, 0f), 1.5f,
                new[] {"Una cama cómoda de madera con sábanas de lino.", "La almohada tiene bordados de flores."});

            // Armario (pared norte)
            PlaceProp(root.transform, "Armario", furnSpr, new Color(0.55f, 0.4f, 0.25f),
                new Vector3(1f, 2.5f, 0f), 1.2f,
                new[] {"Un armario de roble con grabados de animales.", "Huele a lavanda — las hojas secas dentro ahuyentan las polillas."});

            // Mesa central con silla
            PlaceProp(root.transform, "Mesa", propSpr, new Color(0.7f, 0.5f, 0.3f),
                new Vector3(-0.5f, 0.3f, 0f), 1.2f,
                new[] {"Una mesa de madera con una vela encendida.", "Hay un libro abierto sobre la mesa: 'Crónicas del Bosque Eterno'."});

            PlaceProp(root.transform, "Silla", furnSpr, new Color(0.6f, 0.45f, 0.28f),
                new Vector3(1f, -0.2f, 0f), 0.9f,
                new[] {"Una silla sencilla con un cojín bordado."});

            // Estante (pared oeste)
            PlaceProp(root.transform, "Estante", propSpr, new Color(0.55f, 0.4f, 0.28f),
                new Vector3(-4f, 0.5f, 0f), 1f,
                new[] {"Un estante lleno de jarras, libros y curiosidades.", "Hay un cristal pequeño en el estante. Ya no brilla, pero lo conservan como recuerdo."});

            // Baúl (esquina sureste)
            PlaceProp(root.transform, "Baúl", propSpr, new Color(0.5f, 0.35f, 0.2f),
                new Vector3(3.5f, -1.5f, 0f), 1f,
                new[] {"Un baúl de viaje con cerraduras de bronce.", "Está cerrado con llave. Se escucha algo moviéndose adentro..."});

            // Chimenea (pared norte)
            PlaceProp(root.transform, "Chimenea", furnSpr, new Color(0.7f, 0.3f, 0.15f),
                new Vector3(3.5f, 2.2f, 0f), 1.5f,
                new[] {"Una pequeña chimenea de piedra. El fuego chisporrotea cálidamente.", "Hay una olla colgada sobre el fuego. Huele a estofado de verduras."});

            // Alfombra
            PlaceProp(root.transform, "Alfombra", propSpr, new Color(0.7f, 0.3f, 0.4f),
                new Vector3(-0.5f, -0.3f, 0f), 2f,
                new[] {"Una alfombra tejida a mano con patrones geométricos tradicionales."});
        }

        // ── Muebles: Taberna ───────────────────────────────────────────────
        private static void BuildTavernFurniture()
        {
            var root = new GameObject("[Furniture_Tavern]");
            Sprite propSpr = LoadSprite(PROPS_INT, 0);
            Sprite furnSpr = LoadSprite(FURN_TS, 0);

            // Barra del bar (pared norte)
            PlaceProp(root.transform, "Barra_Norte", propSpr, new Color(0.6f, 0.4f, 0.2f),
                new Vector3(0f, 2.5f, 0f), new Vector3(8f, 1f, 1f),
                new[] {"La larga barra de madera de roble. Encima hay filas de jarras y botellas.",
                       "El barman dice que tiene cervezas de 12 regiones distintas. ¡Y se nota!"});

            // Mesas de taberna (3)
            for (int i = 0; i < 3; i++)
            {
                float xPos = -3f + i * 3f;
                PlaceProp(root.transform, $"Mesa_Taberna_{i}", propSpr, new Color(0.65f, 0.45f, 0.25f),
                    new Vector3(xPos, -0.5f, 0f), 1.2f,
                    new[] {
                        "Una mesa redonda de taberna, marcada por años de uso.",
                        "Hay unas monedas olvidadas debajo."
                    });
                PlaceProp(root.transform, $"Silla_T_{i}_A", furnSpr, new Color(0.55f, 0.38f, 0.22f),
                    new Vector3(xPos - 0.7f, -1.2f, 0f), 0.8f, null);
                PlaceProp(root.transform, $"Silla_T_{i}_B", furnSpr, new Color(0.55f, 0.38f, 0.22f),
                    new Vector3(xPos + 0.7f, -1.2f, 0f), 0.8f, null);
            }

            // Barril de bebidas (lateral)
            PlaceProp(root.transform, "Barriles", propSpr, new Color(0.5f, 0.35f, 0.2f),
                new Vector3(-4f, 1.5f, 0f), 1.2f,
                new[] {"Una pila de barriles de cerveza y vino. Algunos todavía gotean.",
                       "El barril más grande tiene la marca del Gremio de Cerveceros del Norte."});

            // Chimenea taberna
            PlaceProp(root.transform, "Chimenea_Grande", furnSpr, new Color(0.65f, 0.28f, 0.12f),
                new Vector3(4f, 1.8f, 0f), 2f,
                new[] {"Una gran chimenea de piedra. El calor se siente hasta la puerta.",
                       "Hay un ciervo disecado sobre la repisa. El barman dice que lo cazó solo. Nadie le cree."});

            // Tablón de avisos
            PlaceProp(root.transform, "Tablón_Avisos", propSpr, new Color(0.65f, 0.5f, 0.3f),
                new Vector3(-4f, 2.5f, 0f), 1f,
                new[] {
                    "TABLÓN DE MISIONES:\n• Se busca: al ladrón de los cristales.\n• Recompensa: 500 monedas de oro.\n• Contactar: Sabio Eldor.",
                    "AVISO: La taberna cierra a medianoche. Los que queden dormidos amanecen barriendo.",
                    "OFERTA: Habitación + desayuno = 5 monedas. Solo si no traes mascotas... a menos que sea un perro heroico."
                });
        }

        // ── Muebles: Tienda de Mercader ────────────────────────────────────
        private static void BuildMerchantFurniture()
        {
            var root = new GameObject("[Furniture_Merchant]");
            Sprite propSpr = LoadSprite(PROPS_INT, 0);
            Sprite furnSpr = LoadSprite(FURN_TS, 0);
            Sprite farmSpr = LoadSprite(FARM_TS, 0);

            // Mostrador (pared norte)
            PlaceProp(root.transform, "Mostrador", propSpr, new Color(0.6f, 0.45f, 0.28f),
                new Vector3(0f, 2.2f, 0f), new Vector3(7f, 0.8f, 1f),
                new[] {"El mostrador de madera del mercader, lleno de artículos de todo el reino.",
                       "Hay un libro de cuentas abierto. La última entrada dice: '47 monedas — destino: desconocido'."});

            // Estantes de mercancía (paredes este y oeste)
            for (int i = 0; i < 4; i++)
            {
                float y = -1f + i * 1f;
                PlaceProp(root.transform, $"Estante_O_{i}", propSpr, new Color(0.55f, 0.4f, 0.25f),
                    new Vector3(-4f, y, 0f), 0.9f,
                    new[] {"Estante con jarras, telas y artículos de viaje.", "Hay un mapa enrollado. El precio no se ve claramente."});
                PlaceProp(root.transform, $"Estante_E_{i}", propSpr, new Color(0.55f, 0.4f, 0.25f),
                    new Vector3(4f, y, 0f), 0.9f,
                    new[] {"Estante con armas, herramientas y objetos mágicos menores.", "Una lupa de cristal con montura de plata. ¡Muy cara!"});
            }

            // Sacos y cajas
            PlaceProp(root.transform, "Sacos", farmSpr, new Color(0.7f, 0.6f, 0.4f),
                new Vector3(-2.5f, -2f, 0f), 1.2f,
                new[] {"Sacos de especias, granos y materiales de construcción.",
                       "Uno de los sacos tiene un extraño brillo azulado. ¿Polvo de cristal?"});

            PlaceProp(root.transform, "Cofre_Mercader", propSpr, new Color(0.7f, 0.55f, 0.2f),
                new Vector3(2.5f, -2f, 0f), 1f,
                new[] {"Un cofre decorado con monedas incrustadas.",
                       "Está cerrado con tres candados. El mercader guarda sus mejores artículos aquí."});

            // Linterna decorativa
            PlaceProp(root.transform, "Linterna", furnSpr, new Color(1f, 0.8f, 0.2f),
                new Vector3(0f, 2.8f, 0f), 0.6f,
                new[] {"Una linterna de cristal que ilumina la tienda con luz cálida."});
        }

        // ── NPC interior ────────────────────────────────────────────────────
        private static void BuildInteriorNPC(InteriorType type)
        {
            string spritePath = type switch {
                InteriorType.Tavern   => PC_ROOT + "/Entities/Npc's/Citizen_F/Tavern_A/Idle/Idle_Side-Sheet.png",
                InteriorType.Merchant => PC_ROOT + "/Entities/Npc's/Rogue/Idle/Idle-Sheet.png",
                _                    => PC_ROOT + "/Entities/Npc's/Citizen_F/Peasant_A/Idle/Idle-Sheet.png"
            };

            string npcName = type switch {
                InteriorType.Tavern   => "Tabernera Elena",
                InteriorType.Merchant => "Asistente del Mercader",
                _                    => "Dueño de Casa"
            };

            string[] dialogLines = type switch {
                InteriorType.Tavern => new[] {
                    "¡Bienvenido a 'La Linterna Dorada'! ¿Te sirvo algo?",
                    "Tenemos cerveza de cebada, vino del norte y... agua, si prefieres.",
                    "¿Buscas información? Aquí pasan todos los viajeros del reino. He escuchado muchas cosas.",
                    "El viajero de anoche dejó este mapa en la barra. Quizá te sea útil..."
                },
                InteriorType.Merchant => new[] {
                    "¡El jefe no está ahora mismo! ¿En qué puedo ayudarte?",
                    "Tenemos los mejores mapas del reino. Actualizados cada semana.",
                    "Si buscas el cristal... el jefe dice que no sabe nada. Pero yo vi algo brillante en esa caja.",
                    "Los precios son fijos. Bueno... todo es negociable si traes información valiosa."
                },
                _ => new[] {
                    "¡Ah, un visitante! Esta es mi casa. Hace frío afuera, ¿verdad?",
                    "Llevo aquí toda mi vida. Esta aldea es el mejor lugar del mundo, aunque ahora está oscura sin los cristales.",
                    "¿Ves ese cristal roto en el estante? Era de mi abuela. Lloramos cuando dejó de brillar.",
                    "Si necesitas descansar, eres bienvenido. Los viajeros siempre tienen un rincón aquí."
                }
            };

            var go = new GameObject(npcName.Replace(" ", "_"));
            go.transform.position = type switch {
                InteriorType.Tavern   => new Vector3(0f, 1.5f, 0f),
                InteriorType.Merchant => new Vector3(0f, 1.5f, 0f),
                _                    => new Vector3(-2f, 0f, 0f)
            };
            go.transform.localScale = Vector3.one * 2f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = LoadSprite(spritePath, 0);
            sr.sortingOrder = 500;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType      = RigidbodyType2D.Kinematic;
            rb.gravityScale  = 0f;
            rb.freezeRotation = true;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.offset = new Vector2(0f, -0.25f);

            var npc = go.AddComponent<VillageNPC>();
            npc.npcDisplayName      = npcName;
            npc.patrolDistance      = type == InteriorType.Tavern ? 2f : 0.5f;
            npc.moveSpeed           = 0.6f;
            npc.inlineDialogueLines = dialogLines;
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static void PlaceProp(Transform parent, string name, Sprite spr,
            Color color, Vector3 pos, float scale, string[] interactLines)
        {
            var s = new Vector3(scale, scale, 1f);
            PlaceProp(parent, name, spr, color, pos, s, interactLines);
        }

        private static void PlaceProp(Transform parent, string name, Sprite spr,
            Color color, Vector3 pos, Vector3 scale, string[] interactLines)
        {
            var go = new GameObject(name);
            go.transform.parent     = parent;
            go.transform.position   = pos;
            go.transform.localScale = scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.color        = color;
            sr.sortingOrder = 500 - Mathf.RoundToInt(pos.y * 10f);

            if (spr == null) sr.color = color;

            // Collider
            var col = go.AddComponent<BoxCollider2D>();
            col.size   = new Vector2(0.9f / scale.x, 0.4f / scale.y);
            col.offset = new Vector2(0f, -0.25f / scale.y);

            // Interactable con diálogo
            if (interactLines != null && interactLines.Length > 0)
            {
                var npc = go.AddComponent<VillageNPC>();
                npc.npcDisplayName      = name;
                npc.patrolDistance      = 0f;
                npc.inlineDialogueLines = interactLines;
                npc.interactRadius      = 1.2f;
            }
        }

        private static Sprite LoadSprite(string path, int index)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            int count = 0;
            foreach (var a in all)
            {
                if (a is Sprite sp) { if (count == index) return sp; count++; }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
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
