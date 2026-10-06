using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Kimaya;
using TMPro;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena Level_01_Bosque — el nivel principal de Kimaya.
    /// Estilo: plataformas 2D lateral al estilo Fireboy & Watergirl.
    ///
    /// Layout:
    ///   Zona 1 — Tutorial con Maya
    ///   Zona 2 — Plataformas con Falin (empujar caja)
    ///   Zona 3 — Túnel estrecho de Mia
    ///   Zona 4 — Switch de colores + puerta
    ///   Zona 5 — Caminos ocultos de Mia
    ///   Zona 6 — Enemigos del Zarzal
    ///   Zona 7 — Puzzle final + Rescate de Kiara
    /// </summary>
    public static class Level01ForestBuilder
    {
        private const string SCENE_PATH   = "Assets/Scenes/Level_01_Bosque.unity";
        private const string TERRAIN_PATH = "Assets/Sprites/PixelAdventure/Terrain/Terrain (16x16) 1.png";
        private const string FALIN_SPRITE = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-run.png";
        private const string MIA_SPRITE   = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-run.png";
        private const string CHECKPOINT_SPRITE = "Assets/Sprites/PixelAdventure/Items/Checkpoints/Checkpoint/Checkpoint (No Flag).png";
        private const string FRUIT_PATH   = "Assets/Sprites/PixelAdventure/Items/Fruits/Apple.png";

        private static readonly int GROUND_LAYER  = LayerMask.NameToLayer("Default");
        private static readonly int PLATFORM_LAYER = LayerMask.NameToLayer("Default");

        [MenuItem("Tools/Kimaya/Build Level 01 - Bosque del Eco")]
        public static void Build()
        {
            bool ok = EditorUtility.DisplayDialog("Level01ForestBuilder",
                "¿Construir 'Level_01_Bosque'?\n\n" +
                "Crea un nivel de plataformas lateral completo con:\n" +
                "• Tutorial de Maya\n• 3 zonas de puzzle (Falin + Mia)\n" +
                "• Enemigos Zarzal Sombrío\n• Puntos de control\n• Zona de victoria\n\n" +
                "¡Al estilo Fireboy & Watergirl!", "¡Construir!", "Cancelar");
            if (!ok) return;

            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara ─────────────────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(8f, 4f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic      = true;
            cam.orthographicSize  = 6f;
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.backgroundColor   = new Color(0.08f, 0.15f, 0.08f);
            camGo.AddComponent<AudioListener>();
            var camCtrl = camGo.AddComponent<KimayaCameraController>();
            camCtrl.smoothSpeed = 5f;
            camCtrl.offset      = new Vector3(0f, 2f, -10f);
            camCtrl.useBounds   = true;
            camCtrl.minX = -2f; camCtrl.maxX = 140f;
            camCtrl.minY = -5f; camCtrl.maxY = 30f;

            // ── Nivel ──────────────────────────────────────────────────────
            BuildLevel(camCtrl);

            // ── UI y Managers ──────────────────────────────────────────────
            BuildUI();
            BuildManagers();

            // ── Guardar ────────────────────────────────────────────────────
            string dir = Path.GetDirectoryName(SCENE_PATH);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            AddToBuildSettings(SCENE_PATH);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("✅ Level_01_Bosque creado",
                "¡Escena construida con éxito!\n\n" +
                "Controles en el nivel:\n" +
                "  Falin: WASD + SHIFT (salto impulsado)\n" +
                "  Mia: Flechas + Q (olfato)\n" +
                "  TAB: cambiar de personaje\n" +
                "  ESC: pausa\n\n" +
                "Presiona Play para probar.", "OK");
        }

        // ── Construcción del nivel ──────────────────────────────────────────
        private static void BuildLevel(KimayaCameraController camCtrl)
        {
            var levelRoot = new GameObject("[Level]");
            Sprite terrain  = LoadSprite(TERRAIN_PATH);
            Sprite falinSpr = LoadSprite(FALIN_SPRITE);
            Sprite miaSpr   = LoadSprite(MIA_SPRITE);

            // ─── ZONA 1: TUTORIAL (x=0..20) ────────────────────────────────
            // Suelo inicial
            CreatePlatform(levelRoot.transform, "Suelo_Inicio", new Vector2(10f, -0.5f), new Vector2(20f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));
            // Pared izquierda
            CreatePlatform(levelRoot.transform, "Pared_Izq", new Vector2(-0.5f, 4f), new Vector2(1f, 10f),
                terrain, new Color(0.25f, 0.4f, 0.2f));

            // Maya (hada tutorial) — trigger al entrar
            CreateMaya(levelRoot.transform, new Vector3(6f, 1.5f, 0f));

            // ─── ZONA 2: PLATAFORMAS + CAJA DE FALIN (x=20..50) ────────────
            // Foso
            CreatePlatform(levelRoot.transform, "Suelo_Z2_L", new Vector2(25f, -0.5f), new Vector2(10f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));
            CreatePlatform(levelRoot.transform, "Suelo_Z2_R", new Vector2(44f, -0.5f), new Vector2(10f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));
            // Foso de raíces (peligro)
            CreateHazardFloor(levelRoot.transform, new Vector2(35f, -0.5f), new Vector2(8f, 1f));

            // Plataformas flotantes a distintas alturas
            CreatePlatform(levelRoot.transform, "Plat_A", new Vector2(28f, 2.5f), new Vector2(3f, 0.5f),
                terrain, new Color(0.4f, 0.6f, 0.25f));
            CreatePlatform(levelRoot.transform, "Plat_B", new Vector2(33f, 4.5f), new Vector2(3f, 0.5f),
                terrain, new Color(0.4f, 0.6f, 0.25f));
            CreatePlatform(levelRoot.transform, "Plat_C", new Vector2(38f, 2.5f), new Vector2(3f, 0.5f),
                terrain, new Color(0.4f, 0.6f, 0.25f));

            // Caja pesada que Falin empuja para cruzar el foso
            CreatePushableBox(levelRoot.transform, new Vector3(24f, 1f, 0f));

            // Checkpoint 1
            CreateCheckpoint(levelRoot.transform, new Vector3(45f, 1f, 0f));

            // ─── ZONA 3: TÚNEL ESTRECHO DE MIA (x=50..75) ──────────────────
            CreatePlatform(levelRoot.transform, "Suelo_Z3", new Vector2(62f, -0.5f), new Vector2(24f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));

            // Pared con hueco solo para Mia
            CreateSmallTunnel(levelRoot.transform, new Vector3(62f, 1.2f, 0f));

            // Plataformas altas para Falin (ruta alternativa con salto)
            CreatePlatform(levelRoot.transform, "Plat_Falin_1", new Vector2(57f, 3f), new Vector2(3f, 0.5f),
                terrain, new Color(0.35f, 0.55f, 0.2f));
            CreatePlatform(levelRoot.transform, "Plat_Falin_2", new Vector2(62f, 5.5f), new Vector2(4f, 0.5f),
                terrain, new Color(0.35f, 0.55f, 0.2f));
            CreatePlatform(levelRoot.transform, "Plat_Falin_3", new Vector2(68f, 3f), new Vector2(3f, 0.5f),
                terrain, new Color(0.35f, 0.55f, 0.2f));

            // Enemigo en la zona 3
            CreateEnemy(levelRoot.transform, new Vector3(68f, 1f, 0f), 5f);

            // Checkpoint 2
            CreateCheckpoint(levelRoot.transform, new Vector3(74f, 1f, 0f));

            // ─── ZONA 4: SWITCH DE COLORES + PUERTA (x=75..95) ─────────────
            CreatePlatform(levelRoot.transform, "Suelo_Z4", new Vector2(85f, -0.5f), new Vector2(20f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));

            // Puerta verde bloqueando el paso
            CreateColoredDoor(levelRoot.transform, new Vector3(88f, 2f, 0f), "green");

            // Switch que Falin activa empujando una caja sobre él
            CreateSwitchWithBox(levelRoot.transform, new Vector3(78f, 1f, 0f), "green");

            // Plataformas secundarias
            CreatePlatform(levelRoot.transform, "Plat_Z4_1", new Vector2(80f, 3f), new Vector2(3f, 0.5f),
                terrain, new Color(0.35f, 0.55f, 0.2f));
            CreatePlatform(levelRoot.transform, "Plat_Z4_2", new Vector2(85f, 5f), new Vector2(3f, 0.5f),
                terrain, new Color(0.35f, 0.55f, 0.2f));

            // Checkpoint 3
            CreateCheckpoint(levelRoot.transform, new Vector3(93f, 1f, 0f));

            // ─── ZONA 5: CAMINOS OCULTOS DE MIA (x=95..115) ────────────────
            CreatePlatform(levelRoot.transform, "Suelo_Z5", new Vector2(105f, -0.5f), new Vector2(20f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));

            // Camino oculto 1 (plataforma invisible)
            CreateHiddenPath(levelRoot.transform, new Vector3(100f, 2f, 0f), new Vector2(5f, 0.5f));
            CreateHiddenPath(levelRoot.transform, new Vector3(107f, 4f, 0f), new Vector2(4f, 0.5f));
            CreateHiddenPath(levelRoot.transform, new Vector3(113f, 2.5f, 0f), new Vector2(5f, 0.5f));

            // Varios enemigos
            CreateEnemy(levelRoot.transform, new Vector3(102f, 1f, 0f), 4f);
            CreateEnemy(levelRoot.transform, new Vector3(110f, 1f, 0f), 3f);

            // ─── ZONA 6: PUZZLE FINAL + RESCATE (x=115..135) ───────────────
            CreatePlatform(levelRoot.transform, "Suelo_Final", new Vector2(125f, -0.5f), new Vector2(22f, 1f),
                terrain, new Color(0.3f, 0.5f, 0.2f));

            // Puzzle: dos switches que abren la puerta final
            CreatePressurePlate(levelRoot.transform, new Vector3(118f, 0.3f, 0f), "purple");
            CreatePressurePlate(levelRoot.transform, new Vector3(128f, 0.3f, 0f), "purple");
            CreateColoredDoor(levelRoot.transform, new Vector3(132f, 2.5f, 0f), "purple");

            // Plataformas del puzzle final
            CreatePlatform(levelRoot.transform, "Plat_Final_1", new Vector2(120f, 3f), new Vector2(3f, 0.5f),
                terrain, new Color(0.4f, 0.6f, 0.25f));
            CreatePlatform(levelRoot.transform, "Plat_Final_2", new Vector2(126f, 5f), new Vector2(3f, 0.5f),
                terrain, new Color(0.4f, 0.6f, 0.25f));

            // Cajas para los switches
            CreatePushableBox(levelRoot.transform, new Vector3(116f, 1f, 0f));
            CreatePushableBox(levelRoot.transform, new Vector3(131f, 1f, 0f));

            // Plataforma de la victoria (Kiara)
            CreatePlatform(levelRoot.transform, "Plataforma_Kiara", new Vector2(133f, 3f), new Vector2(5f, 0.5f),
                terrain, new Color(0.5f, 0.8f, 0.3f));
            CreateVictoryZone(levelRoot.transform, new Vector3(133f, 4.5f, 0f));

            // Maya final (diálogo de victoria si llegan sin activar zona)
            CreateMayaFinal(levelRoot.transform, new Vector3(130f, 1.5f, 0f));

            // ─── Decoración de árboles/fondo ───────────────────────────────
            BuildForestBackground(levelRoot.transform);

            // ─── PERSONAJES ────────────────────────────────────────────────
            BuildPlayers(levelRoot.transform, falinSpr, miaSpr, camCtrl);
        }

        // ── Factories ──────────────────────────────────────────────────────
        private static GameObject CreatePlatform(Transform parent, string name,
            Vector2 pos, Vector2 size, Sprite spr, Color color)
        {
            var go = new GameObject(name);
            go.transform.parent   = parent;
            go.transform.position = pos;
            go.layer              = GROUND_LAYER;
            go.tag                = "Ground";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.color        = color;
            sr.drawMode     = SpriteDrawMode.Tiled;
            sr.size         = size;
            sr.sortingOrder = -10;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            return go;
        }

        private static void CreateHazardFloor(Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Hazard_Raices");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.25f, 0.1f, 0.35f);
            sr.sortingOrder = -5;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(1f, 1f);
            col.isTrigger = true;
            go.tag        = "Hazard";

            // Script de daño
            go.AddComponent<HazardZone>();
        }

        private static void CreateMaya(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Maya_Tutorial");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.7f, 0.4f, 1f);
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * 0.8f;

            var col = go.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(3f, 4f);
            col.isTrigger = true;

            var dlg = go.AddComponent<MayaDialogue>();
            dlg.speakerName = "Maya";
            dlg.lines = new[] {
                "¡Por fin llegan! Este es el Bosque del Eco.",
                "Kiara fue arrastrada por las raíces del Zarzal Sombrío. Está más adentro del bosque.",
                "Para llegar a ella, deben trabajar juntas.",
                "Usa TAB para cambiar entre Falin y Mia.",
                "Falin puede mover objetos pesados y dar saltos impulsados (mantén SHIFT + W).",
                "Mia puede pasar por espacios estrechos y usar su olfato (presiona Q) para revelar caminos ocultos.",
                "¡Tengan cuidado con las raíces oscuras — si las tocan perderán energía!",
                "¡Yo volaré adelante para guiarlas! ¡Buena suerte!"
            };
        }

        private static void CreateMayaFinal(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Maya_Final");
            go.transform.parent   = parent;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.8f;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.7f, 0.4f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 3f); col.isTrigger = true;

            var dlg = go.AddComponent<MayaDialogue>();
            dlg.speakerName = "Maya";
            dlg.lines = new[] {
                "¡Lo lograron! Las dos juntas rompieron el hechizo del Zarzal Sombrío.",
                "Kiara está ahí, en la plataforma. Subamos a rescatarla.",
                "El círculo de hadas vuelve a brillar... pero no del todo.",
                "El camino de regreso a casa aún no está abierto. Pero eso es para otro día.",
                "Por ahora, ¡las tres hermanas están juntas de nuevo!"
            };
            dlg.onlyOnce = true;
        }

        private static void CreatePushableBox(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Caja_Pesada");
            go.transform.parent   = parent;
            go.transform.position = pos;
            go.tag = "Pushable";

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.6f, 0.42f, 0.25f);
            sr.sortingOrder = 5;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale   = 3f;
            rb.freezeRotation = true;
            rb.mass           = 5f;

            go.AddComponent<PushableObject>().pushForce = 3.5f;
        }

        private static void CreateSmallTunnel(Transform parent, Vector3 pos)
        {
            var root = new GameObject("Tunel_Estrecho");
            root.transform.parent   = parent;
            root.transform.position = pos;

            // Pared con hueco
            var wallTop = new GameObject("Pared_Top");
            wallTop.transform.parent   = root.transform;
            wallTop.transform.position = pos + new Vector3(0f, 1.5f, 0f);
            var srT = wallTop.AddComponent<SpriteRenderer>();
            srT.color = new Color(0.3f, 0.22f, 0.15f); srT.sortingOrder = 5;
            wallTop.transform.localScale = new Vector3(1f, 2f, 1f);
            wallTop.AddComponent<BoxCollider2D>();

            var wallBot = new GameObject("Pared_Bot");
            wallBot.transform.parent   = root.transform;
            wallBot.transform.position = pos + new Vector3(0f, -0.8f, 0f);
            var srB = wallBot.AddComponent<SpriteRenderer>();
            srB.color = new Color(0.3f, 0.22f, 0.15f); srB.sortingOrder = 5;
            wallBot.transform.localScale = new Vector3(1f, 0.8f, 1f);
            wallBot.AddComponent<BoxCollider2D>();

            // Trigger del túnel
            var trigger = new GameObject("Tunnel_Trigger");
            trigger.transform.parent   = root.transform;
            trigger.transform.position = pos + new Vector3(0f, 0.35f, 0f);
            var col = trigger.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(1.2f, 0.8f);
            col.isTrigger = true;
            trigger.AddComponent<SmallTunnel>();

            // Letrero visual
            var label = new GameObject("Letrero_Solo_Mia");
            label.transform.parent   = root.transform;
            label.transform.position = pos + new Vector3(0f, 2.8f, 0f);
            var srL = label.AddComponent<SpriteRenderer>();
            srL.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            srL.sortingOrder = 20;
            label.transform.localScale = Vector3.one * 0.5f;
        }

        private static void CreateColoredDoor(Transform parent, Vector3 pos, string color)
        {
            var go = new GameObject($"Puerta_{color}");
            go.transform.parent   = parent;
            go.transform.position = pos;

            Color doorColor = color == "green" ?
                new Color(0.2f, 0.8f, 0.3f) : new Color(0.6f, 0.2f, 0.9f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = doorColor;
            sr.sortingOrder = 8;
            go.transform.localScale = new Vector3(1f, 4f, 1f);

            go.AddComponent<BoxCollider2D>();

            var door = go.AddComponent<ColoredDoor>();
            door.doorColor  = color;
            door.openOffset = new Vector3(0f, -4.5f, 0f);
            door.openSpeed  = 4f;
        }

        private static void CreateSwitchWithBox(Transform parent, Vector3 pos, string color)
        {
            var sw = new GameObject($"Switch_{color}");
            sw.transform.parent   = parent;
            sw.transform.position = pos;

            Color sColor = color == "green" ? new Color(0.3f, 0.9f, 0.3f, 0.7f)
                                            : new Color(0.7f, 0.3f, 0.9f, 0.7f);
            var sr = sw.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.8f, 0.3f, 0.2f); sr.sortingOrder = 5;
            sw.transform.localScale = new Vector3(1.5f, 0.2f, 1f);

            var col = sw.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f); col.isTrigger = true;

            var s = sw.AddComponent<Switch>();
            s.switchColor = color;
            s.switchType  = Switch.SwitchType.PressurePlate;
            s.onColor     = sColor;
        }

        private static void CreatePressurePlate(Transform parent, Vector3 pos, string color)
            => CreateSwitchWithBox(parent, pos, color);

        private static void CreateHiddenPath(Transform parent, Vector3 pos, Vector2 size)
        {
            var go = new GameObject("CaminoOculto");
            go.transform.parent   = parent;
            go.transform.position = pos;
            go.layer = PLATFORM_LAYER;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.3f, 0.8f, 0.4f, 0f); // invisible por defecto
            sr.sortingOrder = -5;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size    = new Vector2(1f, 1f);
            col.enabled = false;

            var hp = go.AddComponent<HiddenPath>();
            hp.enableColliderWhenRevealed = true;
        }

        private static void CreateEnemy(Transform parent, Vector3 pos, float patrolDist)
        {
            var go = new GameObject("Zarzal_Enemy");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color        = new Color(0.25f, 0.1f, 0.35f);
            sr.sortingOrder = 6;
            go.transform.localScale = Vector3.one * 0.8f;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size      = new Vector2(0.6f, 0.8f);
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType    = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var ep = go.AddComponent<EnemyPatrol>();
            ep.patrolDistance = patrolDist;
            ep.moveSpeed      = 1.5f;
        }

        private static void CreateCheckpoint(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Checkpoint");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite  = LoadSprite(CHECKPOINT_SPRITE);
            sr.color   = new Color(0.9f, 0.9f, 0.9f);
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * 1.5f;

            var col = go.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(0.5f, 1.2f);
            col.isTrigger = true;
            go.AddComponent<Checkpoint>();
        }

        private static void CreateVictoryZone(Transform parent, Vector3 pos)
        {
            var go = new GameObject("Zona_Victoria_Kiara");
            go.transform.parent   = parent;
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.9f, 0.3f, 0.4f);
            sr.sortingOrder = 15;
            go.transform.localScale = new Vector3(3f, 2f, 1f);

            var col = go.AddComponent<BoxCollider2D>();
            col.size      = new Vector2(1f, 1f);
            col.isTrigger = true;

            go.AddComponent<VictoryZone>();
        }

        private static void BuildForestBackground(Transform parent)
        {
            // Árboles de fondo simples (coloreados)
            Color treeColor1 = new Color(0.15f, 0.35f, 0.12f, 0.6f);
            Color treeColor2 = new Color(0.1f, 0.28f, 0.1f, 0.5f);

            float[] xPositions = { 5f, 15f, 25f, 38f, 52f, 65f, 78f, 90f, 103f, 115f, 128f };
            for (int i = 0; i < xPositions.Length; i++)
            {
                var tree = new GameObject($"Arbol_BG_{i}");
                tree.transform.parent   = parent;
                tree.transform.position = new Vector3(xPositions[i], 5f + (i % 3) * 1.5f, 1f);
                tree.transform.localScale = new Vector3(2f + (i % 2), 4f + (i % 3), 1f);

                var sr = tree.AddComponent<SpriteRenderer>();
                sr.color        = i % 2 == 0 ? treeColor1 : treeColor2;
                sr.sortingOrder = -50;
            }
        }

        private static void BuildPlayers(Transform parent,
            Sprite falinSpr, Sprite miaSpr, KimayaCameraController camCtrl)
        {
            // ── FALIN ──────────────────────────────────────────────────────
            var falin = new GameObject("Falin");
            falin.transform.parent   = parent;
            falin.transform.position = new Vector3(3f, 2f, 0f);
            falin.tag = "Player";

            var falinSR = falin.AddComponent<SpriteRenderer>();
            falinSR.sprite       = falinSpr;
            falinSR.color        = new Color(0.9f, 0.7f, 0.3f);
            falinSR.sortingOrder = 50;
            falin.transform.localScale = Vector3.one * 1.2f;

            var falinRb = falin.AddComponent<Rigidbody2D>();
            falinRb.gravityScale   = 3f;
            falinRb.freezeRotation = true;
            falinRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var falinCol = falin.AddComponent<CapsuleCollider2D>();
            falinCol.size   = new Vector2(0.55f, 0.85f);
            falinCol.offset = new Vector2(0f, 0f);

            var falinPC = falin.AddComponent<PlayerController2D>();
            falinPC.moveSpeed    = 5.5f;
            falinPC.jumpForce    = 11f;
            falinPC.gravityScale = 3f;
            falinPC.leftKey      = KeyCode.A;
            falinPC.rightKey     = KeyCode.D;
            falinPC.jumpKey      = KeyCode.W;
            falinPC.groundLayer  = LayerMask.GetMask("Default");

            var falinAbil = falin.AddComponent<FalinAbilities>();
            falinAbil.chargeKey       = KeyCode.LeftShift;
            falinAbil.poweredJumpForce = 18f;
            falinAbil.pushableLayer   = LayerMask.GetMask("Default");

            // ── MIA ────────────────────────────────────────────────────────
            var mia = new GameObject("Mia");
            mia.transform.parent   = parent;
            mia.transform.position = new Vector3(5f, 2f, 0f);
            mia.tag = "Player";

            var miaSR = mia.AddComponent<SpriteRenderer>();
            miaSR.sprite       = miaSpr;
            miaSR.color        = new Color(0.5f, 0.5f, 0.5f, 0.75f); // inactiva al inicio
            miaSR.sortingOrder = 50;
            mia.transform.localScale = Vector3.one;

            var miaRb = mia.AddComponent<Rigidbody2D>();
            miaRb.gravityScale   = 3f;
            miaRb.freezeRotation = true;
            miaRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var miaCol = mia.AddComponent<CapsuleCollider2D>();
            miaCol.size   = new Vector2(0.45f, 0.7f);
            miaCol.offset = new Vector2(0f, 0f);

            var miaPC = mia.AddComponent<PlayerController2D>();
            miaPC.moveSpeed    = 4.5f;
            miaPC.jumpForce    = 9f;
            miaPC.gravityScale = 3f;
            miaPC.leftKey      = KeyCode.LeftArrow;
            miaPC.rightKey     = KeyCode.RightArrow;
            miaPC.jumpKey      = KeyCode.UpArrow;
            miaPC.groundLayer  = LayerMask.GetMask("Default");
            miaPC.isActive     = false; // Falin empieza activa

            var miaAbil = mia.AddComponent<MiaAbilities>();
            miaAbil.smellKey   = KeyCode.Q;
            miaAbil.hiddenLayer = LayerMask.GetMask("Default");

            // ── Character Switcher ─────────────────────────────────────────
            var switcherGo = new GameObject("CharacterSwitcher");
            switcherGo.transform.parent = parent;
            var switcher = switcherGo.AddComponent<CharacterSwitchController>();
            switcher.falin     = falinPC;
            switcher.mia       = miaPC;
            switcher.switchKey = KeyCode.Tab;

            // Vincular cámara a Falin al inicio
            camCtrl.target = falin.transform;

            // ── Respawn Manager ────────────────────────────────────────────
            var respawnGo = new GameObject("RespawnManager");
            respawnGo.transform.parent = parent;
            var respawn = respawnGo.AddComponent<RespawnManager>();
            respawn.falin    = falinPC;
            respawn.mia      = miaPC;
            respawn.switcher = switcher;
            respawn.fallDeathY = -8f;
            respawn.maxLives   = 3;
        }

        private static void BuildUI()
        {
            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

            // HUD container
            var hud = new GameObject("HUD");
            hud.transform.SetParent(canvasGo.transform, false);

            // Panel superior izquierdo — info de personaje y vidas
            CreateUILabel(hud.transform, "LabelPersonaje", "▶ Falin",
                new Vector2(10, -10), new Vector2(200, 40), TextAnchor.UpperLeft,
                new Color(0.9f, 0.7f, 0.3f), 22);

            CreateUILabel(hud.transform, "LabelVidas", "❤ 3",
                new Vector2(10, -55), new Vector2(120, 35), TextAnchor.UpperLeft,
                new Color(1f, 0.4f, 0.4f), 20);

            // Panel de controles (esquina superior derecha)
            CreateUILabel(hud.transform, "LabelControles",
                "TAB: Cambiar  |  WASD: Falin  |  ←→↑: Mia  |  SHIFT+W: Salto fuerte  |  Q: Olfato  |  ESC: Pausa",
                new Vector2(-10, -10), new Vector2(700, 30), TextAnchor.UpperRight,
                new Color(1f, 1f, 1f, 0.5f), 12);

            // Daño flash
            var flashGo = new GameObject("DamageFlash");
            flashGo.transform.SetParent(canvasGo.transform, false);
            var flashImg = flashGo.AddComponent<Image>();
            flashImg.color = Color.clear;
            var flashRT = flashGo.GetComponent<RectTransform>();
            flashRT.anchorMin = Vector2.zero; flashRT.anchorMax = Vector2.one;
            flashRT.offsetMin = flashRT.offsetMax = Vector2.zero;

            // GameManager
            var gmGo = new GameObject("KimayaGameManager");
            var gm = gmGo.AddComponent<KimayaGameManager>();
            gm.hud              = hud;
            gm.damageFlashImage = flashImg;
            gm.mainMenuScene    = "MainMenu";
            gm.nextLevelScene   = "Ending";
        }

        private static void CreateUILabel(Transform parent, string name, string text,
            Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor anchor, Color color, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<Text>();
            label.text      = text;
            label.color     = color;
            label.fontSize  = fontSize;
            label.alignment = anchor;
            label.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin      = new Vector2(anchor == TextAnchor.UpperRight ? 1 : 0, 1);
            rt.anchorMax      = new Vector2(anchor == TextAnchor.UpperRight ? 1 : 0, 1);
            rt.pivot          = new Vector2(anchor == TextAnchor.UpperRight ? 1 : 0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta      = sizeDelta;
        }

        private static void BuildManagers()
        {
            // Physics2D gravity
            Physics2D.gravity = new Vector2(0f, -20f);
        }

        // ── Helpers ──────────────────────────────────────────────────────────
        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                var all = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in all) if (a is Sprite sp) return sp;
            }
            return s;
        }

        private static void AddToBuildSettings(string path)
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in list) if (s.path == path) return;
            list.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }

    /// <summary>Zona de peligro — llama a TakeDamage al tocarla.</summary>
    public class HazardZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            RespawnManager.Instance?.TakeDamage();
        }
    }
}
