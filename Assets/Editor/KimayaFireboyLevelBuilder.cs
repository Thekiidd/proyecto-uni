using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye el Nivel 01 en pantalla única estilo Fireboy & Watergirl:
    ///   - Cámara fija que abarca todo el templo de puzle en 16:9.
    ///   - Balancines (Seesaws) interactivos con física 2D real (HingeJoint2D).
    ///   - Cajas empujables con masa y física.
    ///   - Placas de presión animadas que se hunden y encienden.
    ///   - Cristales coleccionables flotantes.
    ///   - Puertas rúnicas de salida y Cronómetro de Speedrun arriba.
    /// Menú: Tools > Kimaya > Build Level 01 - Estilo Fireboy
    /// </summary>
    public static class KimayaFireboyLevelBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Level_01_Bosque.unity";
        private const string FALIN_SPR  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-idle.png";

        [MenuItem("Tools/Kimaya/Build Level 01 - Estilo Fireboy")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Cámara Fija 16:9
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 7.6f; // Encuadre perfecto para toda la sala
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.1f, 0.06f);
            camGo.AddComponent<AudioListener>();

            // 2. Fondo del templo
            BuildBackground();

            // 3. Estructura y Plataformas (Marco y Pisos)
            var levelRoot = new GameObject("TempleStructure");
            BuildTempleChamber(levelRoot.transform);

            // 4. Mecanismos (Balancines / Seesaws)
            BuildSeesaws(levelRoot.transform);

            // 5. Cajas Empujables
            BuildCrates(levelRoot.transform);

            // 6. Placas de Presión y Puertas
            BuildPuzzlesAndDoors(levelRoot.transform);

            // 7. Coleccionables (Gemas)
            BuildGems(levelRoot.transform);

            // 8. Jugador (Falin)
            BuildPlayer(levelRoot.transform);

            // 9. UI & Cronómetro (HUD)
            BuildHUD();

            // 10. Managers (GameManager, Audio, Firebase)
            BuildManagers();

            SaveScene(scene, SCENE_PATH);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("✅ Nivel 1 Fireboy Creado",
                    "¡Level_01_Bosque.unity listo con el nuevo diseño!\n\n" +
                    "🎮 Características incluidas:\n" +
                    "  • Pantalla completa fija estilo Fireboy & Watergirl.\n" +
                    "  • Balancín con física 2D que se inclina con tu peso.\n" +
                    "  • Caja de madera que puedes empujar libremente.\n" +
                    "  • Placa de presión animada que se hunde y activa la puerta.\n" +
                    "  • Cronómetro de Speedrun arriba al centro.\n\n" +
                    "¡Dale al Play (▶) para probarlo!", "OK");
            }
            Debug.Log("[KimayaFireboyLevelBuilder] Nivel 1 Fireboy construido y guardado con éxito en: " + SCENE_PATH);
        }

        private static void BuildBackground()
        {
            var bgGo = new GameObject("Temple_BG");
            bgGo.transform.position = new Vector3(0f, 0f, 5f);
            var sr = bgGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -100;

            string bgPath = "Assets/Sprites/Tiles/Kimaya_Level_Background.jpg";
            var importer = AssetImporter.GetAtPath(bgPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }

            var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgPath);
            if (bgSprite != null)
            {
                sr.sprite = bgSprite;
                bgGo.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            }
            else
            {
                sr.color = new Color(0.08f, 0.16f, 0.1f);
                bgGo.transform.localScale = new Vector3(28f, 16f, 1f);
            }
        }

        private static void BuildTempleChamber(Transform parent)
        {
            Color stoneColor = new Color(0.25f, 0.38f, 0.22f);

            // Marco exterior (Paredes, Suelo y Techo)
            CreateSolidBlock(parent, "Ceiling",    new Vector3(0f, 7.3f, 0f),    new Vector3(27f, 1.2f, 1f), stoneColor);
            CreateSolidBlock(parent, "Floor",      new Vector3(0f, -7.3f, 0f),   new Vector3(27f, 1.2f, 1f), stoneColor);
            CreateSolidBlock(parent, "LeftWall",   new Vector3(-12.8f, 0f, 0f),  new Vector3(1.2f, 15.5f, 1f), stoneColor);
            CreateSolidBlock(parent, "RightWall",  new Vector3(12.8f, 0f, 0f),   new Vector3(1.2f, 15.5f, 1f), stoneColor);

            // Piso Intermedio (Y = 0f)
            CreateSolidBlock(parent, "MidFloor_Left",  new Vector3(-9.2f, 0f, 0f), new Vector3(6.2f, 0.7f, 1f), stoneColor);
            CreateSolidBlock(parent, "MidFloor_Right", new Vector3(9.2f, 0f, 0f),  new Vector3(6.2f, 0.7f, 1f), stoneColor);

            // Piso Superior (Ledges altos Y = 4.2f)
            CreateSolidBlock(parent, "TopLedge_Left",  new Vector3(-6.5f, 4.2f, 0f), new Vector3(6f, 0.6f, 1f), stoneColor);
            CreateSolidBlock(parent, "TopLedge_Right", new Vector3(6.5f, 4.2f, 0f),  new Vector3(6f, 0.6f, 1f), stoneColor);

            // Plataformas flotantes centrales superiores
            CreateSolidBlock(parent, "FloatPlat_1", new Vector3(-3.2f, 5.4f, 0f), new Vector3(2.2f, 0.45f, 1f), stoneColor);
            CreateSolidBlock(parent, "FloatPlat_2", new Vector3(0f, 5.0f, 0f),    new Vector3(2.2f, 0.45f, 1f), stoneColor);
            CreateSolidBlock(parent, "FloatPlat_3", new Vector3(3.2f, 5.4f, 0f),  new Vector3(2.2f, 0.45f, 1f), stoneColor);

            // Piso Inferior (Y = -3.8f)
            CreateSolidBlock(parent, "LowDivider_Left", new Vector3(-6.5f, -3.6f, 0f), new Vector3(4.5f, 0.6f, 1f), stoneColor);
            CreateSolidBlock(parent, "LowWall_Vertical", new Vector3(-4.5f, -5.2f, 0f), new Vector3(0.6f, 3.8f, 1f), stoneColor);
        }

        private static void BuildSeesaws(Transform parent)
        {
            // ── Balancín Central (Piso Intermedio) ─────────────────────
            var seesaw1 = new GameObject("Seesaw_Central");
            seesaw1.transform.parent = parent;
            seesaw1.transform.position = new Vector3(0f, 0.1f, 0f);

            // Pivote / Base triangular estática
            var fulcrum1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fulcrum1.name = "Fulcrum";
            fulcrum1.transform.parent = seesaw1.transform;
            fulcrum1.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            fulcrum1.transform.localScale = new Vector3(0.7f, 0.8f, 0.5f);
            fulcrum1.GetComponent<MeshRenderer>().sharedMaterial = CreateColorMat(new Color(0.35f, 0.32f, 0.25f));
            Object.DestroyImmediate(fulcrum1.GetComponent<Collider>());

            // Barra basculante móvil
            var beam1 = new GameObject("Seesaw_Beam");
            beam1.transform.parent = seesaw1.transform;
            beam1.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            beam1.layer = LayerMask.NameToLayer("Default");

            var sr = beam1.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.48f, 0.38f, 0.22f);
            beam1.transform.localScale = new Vector3(7.2f, 0.35f, 1f);

            var col = beam1.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var seesawComp = beam1.AddComponent<KimayaSeesaw>();
            seesawComp.maxAngle = 26f;

            // ── Balancín Inferior (Piso Bajo) ──────────────────────────
            var seesaw2 = new GameObject("Seesaw_Inferior");
            seesaw2.transform.parent = parent;
            seesaw2.transform.position = new Vector3(3.5f, -5.4f, 0f);

            var fulcrum2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fulcrum2.name = "Fulcrum";
            fulcrum2.transform.parent = seesaw2.transform;
            fulcrum2.transform.localPosition = new Vector3(0f, -0.4f, 0f);
            fulcrum2.transform.localScale = new Vector3(0.7f, 0.8f, 0.5f);
            fulcrum2.GetComponent<MeshRenderer>().sharedMaterial = CreateColorMat(new Color(0.35f, 0.32f, 0.25f));
            Object.DestroyImmediate(fulcrum2.GetComponent<Collider>());

            var beam2 = new GameObject("Seesaw_Beam");
            beam2.transform.parent = seesaw2.transform;
            beam2.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            beam2.layer = LayerMask.NameToLayer("Default");

            var sr2 = beam2.AddComponent<SpriteRenderer>();
            sr2.color = new Color(0.48f, 0.38f, 0.22f);
            beam2.transform.localScale = new Vector3(6.5f, 0.35f, 1f);

            var col2 = beam2.AddComponent<BoxCollider2D>();
            col2.size = Vector2.one;

            var seesawComp2 = beam2.AddComponent<KimayaSeesaw>();
            seesawComp2.maxAngle = 24f;
        }

        private static void BuildCrates(Transform parent)
        {
            // Caja empujable en el piso intermedio
            var crate = new GameObject("Caja_Madera");
            crate.transform.parent = parent;
            crate.transform.position = new Vector3(-7f, 0.85f, 0f);
            crate.tag = "Pushable";
            crate.layer = LayerMask.NameToLayer("Default");

            var sr = crate.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.6f, 0.45f, 0.25f);
            crate.transform.localScale = new Vector3(1.1f, 1.1f, 1f);

            var col = crate.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var rb = crate.AddComponent<Rigidbody2D>();
            rb.mass = 2.5f;
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            crate.AddComponent<PushableObject>();
        }

        private static void BuildPuzzlesAndDoors(Transform parent)
        {
            // Placa de presión animada (verde)
            var plateGo = new GameObject("Placa_Verde");
            plateGo.transform.parent = parent;
            plateGo.transform.position = new Vector3(-4.2f, 0.45f, 0f);

            // Base de la placa
            var baseGo = new GameObject("Base");
            baseGo.transform.parent = plateGo.transform;
            baseGo.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            var baseSr = baseGo.AddComponent<SpriteRenderer>();
            baseSr.color = new Color(0.2f, 0.25f, 0.2f);
            baseGo.transform.localScale = new Vector3(1.3f, 0.15f, 1f);

            // Tapa móvil
            var topGo = new GameObject("PlateTop");
            topGo.transform.parent = plateGo.transform;
            topGo.transform.localPosition = Vector3.zero;
            var topSr = topGo.AddComponent<SpriteRenderer>();
            topSr.color = new Color(0.35f, 0.5f, 0.35f);
            topGo.transform.localScale = new Vector3(1.1f, 0.2f, 1f);

            // Trigger colisión
            var triggerCol = plateGo.AddComponent<BoxCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector2(1.1f, 0.4f);
            triggerCol.offset = new Vector2(0f, 0.1f);

            var plateComp = plateGo.AddComponent<KimayaPressurePlate>();
            plateComp.plateTop = topGo.transform;
            plateComp.plateColor = "green";
            plateComp.unpressedColor = new Color(0.35f, 0.5f, 0.35f);
            plateComp.pressedColor   = new Color(0.2f, 1f, 0.35f);

            // Puerta rúnica verde en el piso inferior
            var doorGo = new GameObject("Puerta_Verde");
            doorGo.transform.parent = parent;
            doorGo.transform.position = new Vector3(11.8f, -2.6f, 0f);
            doorGo.transform.localScale = new Vector3(0.8f, 2.8f, 1f);

            var doorSr = doorGo.AddComponent<SpriteRenderer>();
            doorSr.color = new Color(0.2f, 0.8f, 0.35f);

            doorGo.AddComponent<BoxCollider2D>();
            var doorComp = doorGo.AddComponent<ColoredDoor>();
            doorComp.doorColor = "green";
            doorComp.openOffset = new Vector3(0f, 3.2f, 0f);
            doorComp.openSpeed = 4f;

            // Puerta de salida final (Victoria) en la esquina inferior derecha
            var exitDoorGo = new GameObject("Puerta_Victoria");
            exitDoorGo.transform.parent = parent;
            exitDoorGo.transform.position = new Vector3(11.8f, -5.7f, 0f);
            exitDoorGo.transform.localScale = new Vector3(0.9f, 2.2f, 1f);

            var exitSr = exitDoorGo.AddComponent<SpriteRenderer>();
            exitSr.color = new Color(0.7f, 0.3f, 0.9f);

            var exitCol = exitDoorGo.AddComponent<BoxCollider2D>();
            exitCol.isTrigger = true;
            var goalDoor = exitDoorGo.AddComponent<KimayaGoalDoor>();
            goalDoor.nextScene = "MainMenu";
            goalDoor.requireAllGems = false;
        }

        private static void BuildGems(Transform parent)
        {
            Vector3[] greenGemPositions = {
                new Vector3(-3.2f, 6.2f, 0f),
                new Vector3(0f, 5.8f, 0f),
                new Vector3(3.2f, 6.2f, 0f),
                new Vector3(-9.5f, 1.2f, 0f)
            };

            for (int i = 0; i < greenGemPositions.Length; i++)
            {
                var gem = new GameObject($"Gema_Verde_{i}");
                gem.transform.parent = parent;
                gem.transform.position = greenGemPositions[i];
                gem.transform.localScale = Vector3.one * 0.5f;

                var sr = gem.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.2f, 1f, 0.4f);

                var col = gem.AddComponent<CircleCollider2D>();
                col.isTrigger = true;

                var comp = gem.AddComponent<KimayaGem>();
                comp.gemType = KimayaGem.GemType.Green;
                comp.gemColor = new Color(0.2f, 1f, 0.4f);
            }

            Vector3[] purpleGemPositions = {
                new Vector3(6.5f, 5.0f, 0f),
                new Vector3(9.5f, 1.2f, 0f),
                new Vector3(7.5f, -4.2f, 0f)
            };

            for (int i = 0; i < purpleGemPositions.Length; i++)
            {
                var gem = new GameObject($"Gema_Morada_{i}");
                gem.transform.parent = parent;
                gem.transform.position = purpleGemPositions[i];
                gem.transform.localScale = Vector3.one * 0.5f;

                var sr = gem.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.8f, 0.3f, 1f);

                var col = gem.AddComponent<CircleCollider2D>();
                col.isTrigger = true;

                var comp = gem.AddComponent<KimayaGem>();
                comp.gemType = KimayaGem.GemType.Purple;
                comp.gemColor = new Color(0.8f, 0.3f, 1f);
            }
        }

        private static void BuildPlayer(Transform parent)
        {
            var player = new GameObject("PlayerDog");
            player.transform.parent = parent;
            player.transform.position = new Vector3(-10.5f, 0.85f, 0f);
            player.tag = "Player";
            player.layer = LayerMask.NameToLayer("Default");

            var sr = player.AddComponent<SpriteRenderer>();
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(FALIN_SPR);
            if (spr != null) sr.sprite = spr;
            sr.color = new Color(0.9f, 0.75f, 0.3f);
            sr.sortingOrder = 50;
            player.transform.localScale = Vector3.one * 0.8f;

            var rb = player.AddComponent<Rigidbody2D>();
            rb.mass = 1.5f;
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = player.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.6f, 0.9f);

            var pc = player.AddComponent<PlayerController2D>();
            pc.moveSpeed = 6f;
            pc.jumpForce = 12f;
            pc.gravityScale = 3f;
            pc.groundLayer = LayerMask.GetMask("Default");

            player.AddComponent<FalinAbilities>();
        }

        private static void BuildHUD()
        {
            var cvGo = new GameObject("Canvas_HUD");
            var cv = cvGo.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cvGo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            cvGo.AddComponent<GraphicRaycaster>();

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // Marco del Cronómetro arriba al centro
            var timerBg = new GameObject("Timer_Banner");
            timerBg.transform.SetParent(cvGo.transform, false);
            var timerBgImg = timerBg.AddComponent<Image>();
            timerBgImg.color = new Color(0.12f, 0.08f, 0.04f, 0.85f);
            timerBgImg.raycastTarget = false;
            var bgRT = timerBg.GetComponent<RectTransform>();
            bgRT.anchorMin = bgRT.anchorMax = bgRT.pivot = new Vector2(0.5f, 1f);
            bgRT.anchoredPosition = new Vector2(0f, -15f);
            bgRT.sizeDelta = new Vector2(260f, 65f);

            var timerTxtGo = new GameObject("TimerText");
            timerTxtGo.transform.SetParent(timerBg.transform, false);
            var t = timerTxtGo.AddComponent<Text>();
            t.text = "00:00.00";
            t.color = new Color(1f, 0.9f, 0.5f);
            t.fontSize = 32;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.raycastTarget = false;
            var tRT = timerTxtGo.GetComponent<RectTransform>();
            tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one;
            tRT.offsetMin = tRT.offsetMax = Vector2.zero;

            cvGo.AddComponent<KimayaTimer>().timerText = t;

            // Controles Hint
            var hintGo = new GameObject("ControlsHint");
            hintGo.transform.SetParent(cvGo.transform, false);
            var ht = hintGo.AddComponent<Text>();
            ht.text = "WASD / Flechas: Mover y Saltar  |  Empuja la caja a la placa verde para abrir la compuerta  |  ESC: Pausa";
            ht.color = new Color(1f, 1f, 1f, 0.45f);
            ht.fontSize = 16;
            ht.alignment = TextAnchor.MiddleCenter;
            ht.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ht.raycastTarget = false;
            var hRT = hintGo.GetComponent<RectTransform>();
            hRT.anchorMin = hRT.anchorMax = hRT.pivot = new Vector2(0.5f, 0f);
            hRT.anchoredPosition = new Vector2(0f, 20f);
            hRT.sizeDelta = new Vector2(1000f, 35f);
        }

        private static void BuildManagers()
        {
            var mgr = new GameObject("GameManager");
            mgr.AddComponent<KimayaGameManager>();
            mgr.AddComponent<KimayaAudio>();
            mgr.AddComponent<FirebaseLeaderboardManager>();
        }

        private static void CreateSolidBlock(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.parent = parent;
            go.transform.position = pos;
            go.transform.localScale = size;
            go.layer = LayerMask.NameToLayer("Default");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = color;
            sr.sortingOrder = 5;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
        }

        private static Material CreateColorMat(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, path);
            AssetDatabase.Refresh();
        }
    }
}
