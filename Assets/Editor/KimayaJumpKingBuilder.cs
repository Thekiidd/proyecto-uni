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

        public KimayaPlatformDef(string name, float centerX, float centerY, float width, float height)
        {
            this.name = name;
            this.centerX = centerX;
            this.centerY = centerY;
            this.width = width;
            this.height = height;
        }
    }

    /// <summary>
    /// Generador automático de la escena de Ascenso Vertical estilo Jump King (10 Salas del Templo Ancestral).
    /// El nivel utiliza fondos ilustrados completos en pixel art 100% congruentes donde las plataformas
    /// de piedra tallada, musgo y mármol YA ESTÁN ILUSTRADAS y tienen colisionadores físicos exactos al píxel:
    /// - Sala 1 (Y: 0-10): El Gran Atrio y las Puertas del Templo
    /// - Sala 2 (Y: 10-20): El Salón de las Columnas Olvidadas
    /// - Sala 3 (Y: 20-30): La Cámara de los Glifos Rúnicos
    /// - Sala 4 (Y: 30-40): El Pozo Vertical del Silencio
    /// - Sala 5 (Y: 40-50): El Gran Acueducto de Piedra
    /// - Sala 6 (Y: 50-60): La Cripta de los Cristales Cyan
    /// - Sala 7 (Y: 60-70): El Balcón de los Vientos
    /// - Sala 8 (Y: 70-80): El Campanario Ruinoso
    /// - Sala 9 (Y: 80-90): La Escalera Celestial
    /// - Sala 10 (Y: 90-100): El Sagrario de la Cumbre (Altar de la Luz)
    /// Menú: Tools > Kimaya > 🦘 [4] Construir Modo Jump King (Ascenso Vertical)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaJumpKingBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaJumpKing_Build_v13";

        static KimayaJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaJumpKingBuilder] 🚀 Ejecutando AutoBuildCheck v13 para Jump King (10 Salas del Templo)...");
                BuildJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        private static readonly string[] TEMPLE_SCREEN_PATHS = new string[]
        {
            "Assets/Sprites/Backgrounds/Temple_Screen0.png",
            "Assets/Sprites/Backgrounds/Temple_Screen1.png",
            "Assets/Sprites/Backgrounds/Temple_Screen2.png",
            "Assets/Sprites/Backgrounds/Temple_Screen3.png",
            "Assets/Sprites/Backgrounds/Temple_Screen4.png",
            "Assets/Sprites/Backgrounds/Temple_Screen5.png",
            "Assets/Sprites/Backgrounds/Temple_Screen6.png",
            "Assets/Sprites/Backgrounds/Temple_Screen7.png",
            "Assets/Sprites/Backgrounds/Temple_Screen8.png",
            "Assets/Sprites/Backgrounds/Temple_Screen9.png"
        };

        // Coordenadas matemáticas exactas calculadas directamente desde las imágenes generadas (768x432 px, PPU 43.2)
        public static readonly KimayaPlatformDef[][] TempleRooms = new KimayaPlatformDef[][]
        {
            // Sala 1: El Gran Atrio y las Puertas del Templo (Piso 1, Y: 0 a 10)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P0_BaseFloor", 0.000f, 0.417f, 17.778f, 0.833f),
                new KimayaPlatformDef("P0_RightStairStep", 5.231f, 2.222f, 3.241f, 0.741f),
                new KimayaPlatformDef("P0_MidArchLedge", 0.255f, 3.958f, 3.472f, 0.741f),
                new KimayaPlatformDef("P0_LeftPillarLedge", -4.838f, 5.810f, 3.472f, 0.741f),
                new KimayaPlatformDef("P0_TopExitBridge", -0.903f, 7.662f, 3.009f, 0.741f),
            },
            // Sala 2: El Salón de las Columnas Olvidadas (Piso 2, Y: 10 a 20)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P1_EntryCatch", -0.787f, 10.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P1_RightWallBracket", 5.579f, 12.569f, 3.009f, 0.741f),
                new KimayaPlatformDef("P1_MidPillarCapital", 0.602f, 14.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P1_LeftHighBalcony", -5.185f, 16.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P1_TopExitLedge", 3.380f, 17.894f, 3.241f, 0.741f),
            },
            // Sala 3: La Cámara de los Glifos Rúnicos (Piso 3, Y: 20 a 30)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P2_EntryPier", 3.380f, 20.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P2_CenterGlyphPlinth", -1.250f, 22.569f, 3.241f, 0.741f),
                new KimayaPlatformDef("P2_LeftShrineLedge", -5.532f, 24.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P2_CenterHighArch", -0.324f, 26.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P2_TopExitBracket", -4.028f, 27.894f, 3.241f, 0.741f),
            },
            // Sala 4: El Pozo Vertical del Silencio (Piso 4, Y: 30 a 40)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P3_ShaftLanding", -4.028f, 30.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P3_RightWallPerch", 5.231f, 32.569f, 3.241f, 0.741f),
                new KimayaPlatformDef("P3_LeftWallPerch", -5.648f, 34.421f, 3.009f, 0.741f),
                new KimayaPlatformDef("P3_MidStoneGirder", 0.023f, 36.273f, 3.009f, 0.741f),
                new KimayaPlatformDef("P3_TopExitShelf", 2.917f, 37.894f, 3.241f, 0.741f),
            },
            // Sala 5: El Gran Acueducto de Piedra (Piso 5, Y: 40 a 50)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P4_AqueductPier", 2.917f, 40.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P4_CanalArch", -1.944f, 42.569f, 3.241f, 0.741f),
                new KimayaPlatformDef("P4_SpillwayLedge", -5.532f, 44.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P4_HighAqueductSpan", -0.208f, 46.273f, 3.472f, 0.741f),
                new KimayaPlatformDef("P4_TopExitSlab", -3.565f, 47.894f, 3.241f, 0.741f),
            },
            // Sala 6: La Cripta de los Cristales Cyan (Piso 6, Y: 50 a 60)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P5_CrystalShelf", -3.565f, 50.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P5_MidGeodePlinth", 1.296f, 52.569f, 3.241f, 0.741f),
                new KimayaPlatformDef("P5_RightCrystalLedge", 5.231f, 54.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P5_CenterVaultBeam", -0.556f, 56.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P5_TopExitCorbel", 3.380f, 57.894f, 3.241f, 0.741f),
            },
            // Sala 7: El Balcón de los Vientos (Piso 7, Y: 60 a 70)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P6_BalconyStep", 3.380f, 60.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P6_ColonnadePlinth", -2.060f, 62.569f, 3.472f, 0.741f),
                new KimayaPlatformDef("P6_OuterCliffPier", -5.764f, 64.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P6_FlyingButtress", 0.370f, 66.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P6_TopExitBalustrade", -3.796f, 67.894f, 3.241f, 0.741f),
            },
            // Sala 8: El Campanario Ruinoso (Piso 8, Y: 70 a 80)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P7_BelfryFloor", -3.796f, 70.718f, 3.704f, 0.741f),
                new KimayaPlatformDef("P7_BellTimberBeam", 2.338f, 72.569f, 3.472f, 0.741f),
                new KimayaPlatformDef("P7_RightSpireBracket", 5.463f, 74.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P7_CenterChainLedge", -1.019f, 76.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P7_TopBelfryArch", 3.032f, 77.894f, 3.472f, 0.741f),
            },
            // Sala 9: La Escalera Celestial (Piso 9, Y: 80 a 90)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P8_EntryMonolith", 3.032f, 80.718f, 3.935f, 0.741f),
                new KimayaPlatformDef("P8_CelestialStep1", -2.292f, 82.569f, 3.472f, 0.741f),
                new KimayaPlatformDef("P8_CelestialStep2", -5.764f, 84.421f, 3.241f, 0.741f),
                new KimayaPlatformDef("P8_CelestialStep3", 0.602f, 86.273f, 3.241f, 0.741f),
                new KimayaPlatformDef("P8_SummitApproachLedge", -0.324f, 87.894f, 4.630f, 0.741f),
            },
            // Sala 10: El Sagrario de la Cumbre - Altar de la Luz (Piso 10, Y: 90 a 100)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P9_SanctumLanding", -0.324f, 90.718f, 5.093f, 0.741f),
                new KimayaPlatformDef("P9_LeftHonorStep", -5.532f, 92.801f, 3.241f, 0.741f),
                new KimayaPlatformDef("P9_RightHonorStep", 5.347f, 92.801f, 3.241f, 0.741f),
                new KimayaPlatformDef("P9_GRAND_ALTAR_PODIUM", -0.093f, 95.197f, 10.185f, 1.042f),
            },
        };

        [MenuItem("Tools/Kimaya/🦘 [4] Construir Modo Jump King (Ascenso Vertical)")]
        public static void BuildMenu()
        {
            BuildJumpKingScene();
        }

        public static void BuildJumpKingScene()
        {
            Debug.Log("[KimayaJumpKingBuilder] 🚀 Iniciando construcción de escena Jump King v12 (10 Salas del Templo)...");

            // 1. Asegurar importación de los 10 fondos de templo con PPU 43.2f (768x432 px -> 17.777f x 10f unidades)
            foreach (var path in TEMPLE_SCREEN_PATHS)
            {
                EnsureTextureIsSprite(path, 43.2f);
            }

            // Crear escena nueva
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cargar sprites
            Sprite falinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FALIN_SPRITE_PATH);
            Sprite miaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MIA_SPRITE_PATH);
            Sprite altarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ALTAR_SPRITE_PATH);

            // Layer Ground
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");

            // 2. Jugador (Falin / Mía en vista de perfil lateral)
            GameObject playerGo = new GameObject("Player_JumpKing");
            playerGo.transform.position = new Vector3(-5.0f, 1.8f, 0f); // Spawnea en el suelo del Atrio

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

            // Animador Jump King con poses reales de estiramiento, carga agachada, caída y splat
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

            // 4. Paredes Verticales de Contención (10 Salas = 100 unidades verticales de Y: 0 a 100)
            GameObject wallsRoot = new GameObject("BoundaryWalls");
            CreateWall(wallsRoot.transform, -8.888f, 50f, 0.8f, 102f, groundLayer); // Pared izquierda
            CreateWall(wallsRoot.transform, 8.888f, 50f, 0.8f, 102f, groundLayer);  // Pared derecha
            CreateWall(wallsRoot.transform, 0f, 0.2f, 18.5f, 1f, groundLayer);       // Suelo base debajo de Sala 1
            CreateWall(wallsRoot.transform, 0f, 100.5f, 18.5f, 1f, groundLayer);     // Techo cerrado sobre Sala 10

            // 5. Fondos Pixel Art Ilustrados por Sala (10 Fondos de Templo Congruentes)
            GameObject bgRoot = new GameObject("Backgrounds");
            for (int i = 0; i < TEMPLE_SCREEN_PATHS.Length; i++)
            {
                Sprite bgSprite = LoadAnySprite(TEMPLE_SCREEN_PATHS[i]);
                CreateScreenBackground(bgRoot.transform, i, bgSprite, $"BG_Sala{i + 1}");
            }

            // 6. Colisionadores Físicos en las Plataformas Ilustradas (Calculados al píxel)
            GameObject platformsRoot = new GameObject("Platforms");
            for (int r = 0; r < TempleRooms.Length; r++)
            {
                GameObject roomFolder = new GameObject($"Sala_{r + 1}_Colliders");
                roomFolder.transform.SetParent(platformsRoot.transform);
                foreach (var p in TempleRooms[r])
                {
                    CreateInvisiblePlatform(roomFolder.transform, p.centerX, p.centerY, p.width, p.height, groundLayer, p.name);
                }
            }

            // 7. Altar Sagrado de Victoria en la Cumbre (Sala 10, Y = 96.0f)
            GameObject altarGo = new GameObject("Altar_Rescate");
            altarGo.transform.position = new Vector3(0.0f, 96.3f, 0f);
            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite;
            altarSr.sortingOrder = 5;
            altarGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            // Compañera esperando rescate en la cima
            GameObject rescuedGo = new GameObject("Rescued_Companion");
            rescuedGo.transform.position = new Vector3(1.2f, 96.1f, 0f);
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
            manager.finalScreenIndex = 9; // 10 Pisos (0 a 9)

            // Panel Superior HUD
            GameObject topHud = CreateUIPanel(canvasGo.transform, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(1400, 65));
            var topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.04f, 0.07f, 0.14f, 0.90f);

            // Texto Piso
            manager.screenText = CreateUIText(topHud.transform, "Txt_Piso", "Piso 1 / 10: El Gran Atrio y las Puertas del Templo", 24, TextAnchor.MiddleLeft, new Vector2(-460, 0), new Vector2(450, 50));
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

            Text vicTitle = CreateUIText(victoryGo.transform, "Txt_VicTitle", "🏆 ¡RESCATE EN LA CUMBRE DEL TEMPLO!", 38, TextAnchor.MiddleCenter, new Vector2(0, 120), new Vector2(700, 70));
            vicTitle.color = new Color(1f, 0.85f, 0.2f);

            Text vicDesc = CreateUIText(victoryGo.transform, "Txt_VicDesc", "¡Has conquistado las 10 Salas del Templo Ancestral!\nLlegaste al Sagrario de la Cumbre y rescataste a tu compañera.\n\nFalin y Mía han demostrado maestría absoluta en el salto.", 24, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(650, 130));
            vicDesc.color = Color.white;

            manager.victoryPanel = victoryGo;
            victoryGo.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[KimayaJumpKingBuilder] ✅ Escena Jump King v12 (10 Salas del Templo) creada exitosamente en {SCENE_PATH}");

            RegisterSceneInBuildSettings(SCENE_PATH);
        }

        private static void EnsureTextureIsSprite(string path, float ppu = 43.2f)
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

        private static void CreateInvisiblePlatform(Transform parent, float x, float y, float w, float h, int layer, string name)
        {
            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.layer = layer;
            platGo.transform.position = new Vector3(x, y, 0f);

            var col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            var pMat = new PhysicsMaterial2D("PlatMat") { friction = 0.5f, bounciness = 0f };
            col.sharedMaterial = pMat;

            // Componente de superficie Jump King
            var surface = platGo.AddComponent<KimayaSurface>();
            surface.surfaceType = SurfaceType.Normal;
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
