using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Kimaya;

namespace Platformer.EditorTools
{
    public struct KimayaPlatformDef
    {
        public string name;
        public float centerX;
        public float centerY;
        public float width;
        public float height;
        public SurfaceType surfaceType;

        public KimayaPlatformDef(string name, float centerX, float centerY, float width, float height, SurfaceType surfaceType = SurfaceType.Normal)
        {
            this.name = name;
            this.centerX = centerX;
            this.centerY = centerY;
            this.width = width;
            this.height = height;
            this.surfaceType = surfaceType;
        }
    }

    /// <summary>
    /// Generador automático de la escena de Ascenso Vertical estilo Jump King (5 Pisos Icónicos de Kimaya).
    /// Basado en la progresión oficial del mapa mundial de Jump King y generado con PixelLab:
    /// - Piso 1 (Y: 0-10): El Bosque Profundo y la Entrada a las Ruinas
    /// - Piso 2 (Y: 10-20): La Fortaleza y Muros de Piedra Ancestrales
    /// - Piso 3 (Y: 20-30): Los Acueductos y el Campanario Olvidado
    /// - Piso 4 (Y: 30-40): La Aguja de Hielo y Cristales Celestiales (Física resbaladiza de Hielo)
    /// - Piso 5 (Y: 40-50): La Cumbre del Templo al Atardecer (Altar de Kiara / Rescate)
    /// Menú: Tools > Kimaya > 🦘 [4] Construir Modo Jump King (Ascenso Vertical)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaJumpKingBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaJumpKing_Build_v15_5Floors";

        static KimayaJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaJumpKingBuilder] 🚀 Ejecutando AutoBuildCheck v15 para Jump King (5 Pisos Icónicos PixelLab)...");
                BuildJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        private static readonly string[] JUMP_KING_SCREEN_PATHS = new string[]
        {
            "Assets/Sprites/Backgrounds/JumpKing_Screen0_ForestBase.png",
            "Assets/Sprites/Backgrounds/JumpKing_Screen1_Fortress.png",
            "Assets/Sprites/Backgrounds/JumpKing_Screen2_Aqueduct.png",
            "Assets/Sprites/Backgrounds/JumpKing_Screen3_SnowPeak.png",
            "Assets/Sprites/Backgrounds/JumpKing_Screen4_SunsetSummit.png"
        };

        // Coordenadas matemáticas exactas calculadas directamente desde las imágenes generadas (384x216 px, PPU 21.6f)
        public static readonly KimayaPlatformDef[][] JumpKing5Floors = new KimayaPlatformDef[][]
        {
            // Piso 1: El Bosque Profundo y la Entrada a las Ruinas (Y: 0 a 10)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P0_Ground_ForestFloor", 0.000f, 0.486f, 17.778f, 0.972f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Right_LowStoneStep", 5.463f, 2.361f, 4.630f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Center_LBlockLedge", -0.324f, 4.213f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Left_HighWallBracket", -5.417f, 6.065f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Top_ExitLedge", -0.556f, 7.917f, 3.241f, 0.926f, SurfaceType.Normal),
            },
            // Piso 2: La Fortaleza y Muros de Piedra Ancestrales (Y: 10 a 20)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P1_Entry_CatchLedge", -0.556f, 10.972f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_Right_WallBattlement", 6.157f, 12.824f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_Center_LBlockPlatform", 0.139f, 14.676f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_Left_FortressBracket", -5.648f, 16.528f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_Top_ExitShelf", 4.769f, 18.380f, 3.704f, 0.926f, SurfaceType.Normal),
            },
            // Piso 3: Los Acueductos y el Campanario Olvidado (Y: 20 a 30)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P2_Entry_AqueductPier", 4.884f, 20.972f, 4.398f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_Center_CanalArch", -0.556f, 22.824f, 4.630f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_Left_SpillwayLedge", -5.880f, 24.676f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_Center_HighGirder", 0.370f, 26.528f, 4.630f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_Top_ExitSlab", -5.417f, 28.380f, 3.704f, 0.926f, SurfaceType.Normal),
            },
            // Piso 4: La Aguja de Hielo y Cristales Celestiales (Y: 30 a 40) - ¡Superficie resbaladiza de Hielo!
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P3_Entry_SnowyLanding", -5.301f, 30.972f, 4.398f, 0.926f, SurfaceType.Ice),
                new KimayaPlatformDef("P3_Mid_FrostShelf", 1.065f, 32.824f, 4.167f, 0.926f, SurfaceType.Ice),
                new KimayaPlatformDef("P3_Right_IcePillar", 5.926f, 34.676f, 4.167f, 0.926f, SurfaceType.Ice),
                new KimayaPlatformDef("P3_Center_GlacierLedge", -0.324f, 36.528f, 4.167f, 0.926f, SurfaceType.Ice),
                new KimayaPlatformDef("P3_Top_ExitPerch", -0.324f, 38.380f, 3.704f, 0.926f, SurfaceType.Ice),
            },
            // Piso 5: La Cumbre del Templo al Atardecer (Altar de Kiara) (Y: 40 a 50)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P4_Entry_SanctumSteps", -0.208f, 40.972f, 4.398f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P4_Left_TowerBattlement", -5.648f, 42.824f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P4_Right_TowerBattlement", 5.694f, 42.824f, 4.167f, 0.926f, SurfaceType.Normal),
                new KimayaPlatformDef("P4_GRAND_SUMMIT_ALTAR", 0.023f, 45.370f, 10.417f, 1.389f, SurfaceType.Normal),
            },
        };

        [MenuItem("Tools/Kimaya/🦘 [4] Construir Modo Jump King (Ascenso Vertical)")]
        public static void BuildMenu()
        {
            BuildJumpKingScene();
        }

        public static void BuildJumpKingScene()
        {
            Debug.Log("[KimayaJumpKingBuilder] 🚀 Iniciando construcción de escena Jump King v15 (5 Pisos Icónicos)...");

            // 1. Asegurar importación de los 5 fondos generados con PixelLab (384x216 px, PPU = 21.6f -> 17.777f x 10f)
            foreach (var path in JUMP_KING_SCREEN_PATHS)
            {
                EnsureTextureIsSprite(path, 21.6f);
            }

            // Crear escena nueva
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cargar sprites
            Sprite falinSprite = LoadAnySprite(FALIN_SPRITE_PATH);
            Sprite miaSprite = LoadAnySprite(MIA_SPRITE_PATH);
            Sprite altarSprite = LoadAnySprite(ALTAR_SPRITE_PATH);

            // Layer Ground
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");

            // 2. Jugador (Falin / Mía en vista lateral)
            GameObject playerGo = new GameObject("Player_JumpKing");
            playerGo.transform.position = new Vector3(-5.0f, 1.8f, 0f); // Spawnea en el suelo del Bosque

            var sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = falinSprite != null ? falinSprite : miaSprite;
            sr.sortingOrder = 20;

            var rb = playerGo.GetComponent<Rigidbody2D>();
            if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            var col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.0f, 0.75f);
            col.offset = new Vector2(0f, 0.38f);
            var pMat = new PhysicsMaterial2D("PlayerMat") { friction = 0.2f, bounciness = 0f };
            col.sharedMaterial = pMat;

            var jumpCtrl = playerGo.AddComponent<KimayaJumpKingController>();
            jumpCtrl.spriteRenderer = sr;
            jumpCtrl.character = KimayaJumpKingController.CharacterType.Falin;
            jumpCtrl.enableHighFallStun = false; // Solo saltar y escalar, sin aturdimiento

            // Animador Jump King con poses reales de carga, estiramiento, caída y splat
            var anim = playerGo.AddComponent<KimayaJumpKingAnimator>();
            anim.falinIdleFrames = LoadFrames("Assets/Sprites/PixelLab/Falin/Idle/animations/idle/east");
            anim.falinWalkFrames = LoadFrames("Assets/Sprites/PixelLab/Falin/Idle/animations/walking/east");
            anim.falinChargeProgress = new Sprite[]
            {
                LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/charge_0.png"),
                LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/charge_1.png"),
                LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/charge_2.png")
            };
            anim.falinJumpStretch = LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/jump_stretch.png");
            anim.falinFall = LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/fall.png");
            anim.falinSplat = LoadSprite("Assets/Sprites/PixelLab/Falin/JumpKing/splat.png");

            anim.miaIdleFrames = LoadFrames("Assets/Sprites/PixelLab/Mia/Idle/animations/idle/east");
            anim.miaWalkFrames = LoadFrames("Assets/Sprites/PixelLab/Mia/Idle/animations/walking/east");
            anim.miaChargeProgress = new Sprite[]
            {
                LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/charge_0.png"),
                LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/charge_1.png"),
                LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/charge_2.png")
            };
            anim.miaJumpStretch = LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/jump_stretch.png");
            anim.miaFall = LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/fall.png");
            anim.miaSplat = LoadSprite("Assets/Sprites/PixelLab/Mia/JumpKing/splat.png");

            // 3. Cámara Jump King (Pantalla por pantalla con altura de 10 unidades, ancho 17.777f)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f; // Altura visible = 10 unidades
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 5f, -10f);

            var screenCam = camGo.AddComponent<JumpKingScreenCamera>();
            screenCam.target = playerGo.transform;
            screenCam.screenHeight = 10f;
            screenCam.screenWidth = 17.777f;
            screenCam.baseWorldY = 0f;
            screenCam.transitionSpeed = 15f;

            camGo.AddComponent<AudioListener>();

            // 4. Paredes Verticales de Contención (5 Pisos = 50 unidades verticales de Y: 0 a 50)
            GameObject wallsRoot = new GameObject("BoundaryWalls");
            CreateWall(wallsRoot.transform, -8.888f, 25f, 0.8f, 52f, groundLayer); // Pared izquierda
            CreateWall(wallsRoot.transform, 8.888f, 25f, 0.8f, 52f, groundLayer);  // Pared derecha
            CreateWall(wallsRoot.transform, 0f, 0.2f, 18.5f, 1f, groundLayer);      // Suelo base debajo de Piso 1
            CreateWall(wallsRoot.transform, 0f, 50.5f, 18.5f, 1f, groundLayer);     // Techo cerrado sobre Piso 5

            // 5. Fondos Pixel Art Ilustrados por Piso (5 Fondos de Biomas Distintos)
            GameObject bgRoot = new GameObject("Backgrounds");
            for (int i = 0; i < JUMP_KING_SCREEN_PATHS.Length; i++)
            {
                Sprite bgSprite = LoadAnySprite(JUMP_KING_SCREEN_PATHS[i]);
                CreateScreenBackground(bgRoot.transform, i, bgSprite, $"BG_Piso_{i + 1}");
            }

            // 6. Colisionadores Físicos en las Plataformas Ilustradas (Calculados al píxel)
            GameObject platformsRoot = new GameObject("Platforms");
            for (int r = 0; r < JumpKing5Floors.Length; r++)
            {
                GameObject roomFolder = new GameObject($"Piso_{r + 1}_Colliders");
                roomFolder.transform.SetParent(platformsRoot.transform);
                foreach (var p in JumpKing5Floors[r])
                {
                    CreatePlatformCollider(roomFolder.transform, p.centerX, p.centerY, p.width, p.height, groundLayer, p.name, p.surfaceType);
                }
            }

            // 7. Altar Sagrado de Victoria en la Cumbre al Atardecer (Piso 5, Y = 46.2f)
            GameObject altarGo = new GameObject("Altar_Rescate");
            altarGo.transform.position = new Vector3(0.0f, 46.2f, 0f);
            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite;
            altarSr.sortingOrder = 5;
            altarGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            // Compañera Kiara esperando rescate en el altar
            GameObject rescuedGo = new GameObject("Rescued_Companion");
            rescuedGo.transform.position = new Vector3(1.2f, 46.1f, 0f);
            rescuedGo.transform.localScale = new Vector3(-0.9f, 0.9f, 1f); // Mirando hacia el héroe
            var rescuedSr = rescuedGo.AddComponent<SpriteRenderer>();
            rescuedSr.sprite = miaSprite != null ? miaSprite : falinSprite;
            rescuedSr.sortingOrder = 6;

            // 8. UI / Canvas de Jump King
            GameObject canvasGo = new GameObject("JumpKing_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Manager de partida
            var manager = canvasGo.AddComponent<KimayaJumpKingManager>();
            manager.playerController = jumpCtrl;
            manager.screenCamera = screenCam;
            manager.falinSprite = falinSprite;
            manager.miaSprite = miaSprite;
            manager.finalScreenIndex = 4; // 5 Pisos (0 a 4)

            // Panel Superior HUD
            GameObject topHud = CreateUIPanel(canvasGo.transform, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(1400, 65));
            var topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.04f, 0.07f, 0.14f, 0.90f);

            // Texto Piso
            manager.screenText = CreateUIText(topHud.transform, "Txt_Piso", "Piso 1 / 5: El Bosque Profundo y la Entrada a las Ruinas", 24, TextAnchor.MiddleLeft, new Vector2(-460, 0), new Vector2(450, 50));
            manager.screenText.color = new Color(1f, 0.9f, 0.35f);

            // Texto Personaje
            manager.characterNameText = CreateUIText(topHud.transform, "Txt_Char", "🐾 Falin (Fuerza) | [TAB] Mía", 23, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(380, 50));
            manager.characterNameText.color = new Color(1f, 0.75f, 0.2f);

            // Texto Stats
            manager.statsText = CreateUIText(topHud.transform, "Txt_Stats", "⏱️ 00:00  |  🦘 Saltos: 0  |  💥 Caídas: 0", 22, TextAnchor.MiddleRight, new Vector2(460, 0), new Vector2(420, 50));
            manager.statsText.color = Color.white;

            // Barra de Carga de Salto
            GameObject chargeBarGo = CreateUIPanel(canvasGo.transform, "ChargeBar_Panel", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 58), new Vector2(440, 24));
            var slider = chargeBarGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            manager.chargeSlider = slider;

            var barBg = chargeBarGo.AddComponent<Image>();
            barBg.color = new Color(0.08f, 0.08f, 0.12f, 0.9f);

            GameObject fillGo = CreateUIPanel(chargeBarGo.transform, "Fill", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.3f);
            slider.fillRect = fillGo.GetComponent<RectTransform>();
            manager.chargeFillImage = fillImg;

            // Texto de Controles
            Text controlsText = CreateUIText(canvasGo.transform, "Txt_Controls", "Mantén [ESPACIO] para cargar salto • [A / D] Dirección • [TAB] Alternar Falin / Mía", 18, TextAnchor.MiddleCenter, new Vector2(0, 22), new Vector2(1100, 28));
            controlsText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controlsText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controlsText.color = new Color(0.75f, 0.85f, 0.95f, 0.85f);

            // Panel de Victoria
            GameObject victoryGo = CreateUIPanel(canvasGo.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(750, 420));
            var vicBg = victoryGo.AddComponent<Image>();
            vicBg.color = new Color(0.06f, 0.15f, 0.25f, 0.95f);

            Text vicTitle = CreateUIText(victoryGo.transform, "Txt_VicTitle", "🏆 ¡RESCATE EN LA CUMBRE AL ATARDECER!", 36, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(700, 70));
            vicTitle.color = new Color(1f, 0.85f, 0.2f);

            Text vicDesc = CreateUIText(victoryGo.transform, "Txt_VicDesc", "¡Has conquistado los 5 Pisos del Ascenso Sagrado!\nLlegaste al Altar de la Cumbre y rescataste a Kiara.\n\nFalin y Mía han demostrado valor, destreza y compañerismo.", 24, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(650, 130));
            vicDesc.color = Color.white;

            manager.victoryPanel = victoryGo;
            victoryGo.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[KimayaJumpKingBuilder] ✅ Escena Jump King v15 (5 Pisos Icónicos) creada exitosamente en {SCENE_PATH}");

            RegisterSceneInBuildSettings(SCENE_PATH);
        }

        private static void EnsureTextureIsSprite(string path, float ppu = 21.6f)
        {
            if (!File.Exists(path)) return;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    changed = true;
                }
                if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.05f)
                {
                    importer.spritePixelsPerUnit = ppu;
                    changed = true;
                }
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadAnySprite(string path)
        {
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                UnityEngine.Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in all)
                {
                    if (a is Sprite sp) return sp;
                }
            }
            return s;
        }

        private static void CreateScreenBackground(Transform parent, int screenIndex, Sprite sprite, string name)
        {
            if (sprite == null) return;
            GameObject bgGo = new GameObject(name);
            bgGo.transform.SetParent(parent);
            // Cada pantalla mide 10 unidades de alto, centro Y = screenIndex * 10 + 5
            bgGo.transform.position = new Vector3(0f, screenIndex * 10f + 5f, 0f);

            var sr = bgGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
        }

        private static void CreatePlatformCollider(Transform parent, float x, float y, float w, float h, int layer, string name, SurfaceType surfaceType)
        {
            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.layer = layer;
            platGo.transform.position = new Vector3(x, y, 0f);

            var col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);

            float friction = (surfaceType == SurfaceType.Ice) ? 0.05f : 0.5f;
            var pMat = new PhysicsMaterial2D($"PlatMat_{surfaceType}") { friction = friction, bounciness = 0f };
            col.sharedMaterial = pMat;

            // Componente de superficie Jump King
            var surface = platGo.AddComponent<KimayaSurface>();
            surface.surfaceType = surfaceType;
        }

        private static void CreateWall(Transform parent, float x, float y, float w, float h, int layer)
        {
            GameObject wallGo = new GameObject($"Wall_{x}_{y}");
            wallGo.transform.SetParent(parent);
            wallGo.layer = layer;
            wallGo.transform.position = new Vector3(x, y, 0f);

            var col = wallGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            var pMat = new PhysicsMaterial2D("WallMat") { friction = 0.1f, bounciness = 0f };
            col.sharedMaterial = pMat;
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        private static Text CreateUIText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var txt = go.AddComponent<Text>();
            txt.text = text;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.color = Color.white;
            return txt;
        }

        private static Sprite[] LoadFrames(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new Sprite[0];
            string[] files = Directory.GetFiles(folderPath, "*.png");
            Array.Sort(files);
            Sprite[] sprites = new Sprite[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(files[i].Replace("\\", "/"));
            }
            return sprites;
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath) return;
            }

            Array.Resize(ref scenes, scenes.Length + 1);
            scenes[scenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = scenes;
        }
    }
}
