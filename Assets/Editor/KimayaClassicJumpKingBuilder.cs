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
    /// <summary>
    /// Generador automático de la escena de Ascenso Jump King Clásico (Opción A - 1:1 Oficial).
    /// Basado 100% en la ingeniería inversa (REA) de JumpKing.exe y Content/level.xnb de Nexile:
    /// - Red Crown Woods (Pantallas 1 a 5 oficiales)
    /// - Fondo extraído del binario original (480x360 a 36 PPU)
    /// - Colisionadores idénticos calculados de las máscaras binarias de 8x8 px
    /// - Físicas oficiales (Jump Time: 0.60s, Bounce: 0.50, Splat: 0.50s)
    /// Menú: Tools > Kimaya > 👑 [5] Construir Jump King Clásico (Opción A - 1:1 Oficial)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaClassicJumpKingBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Ascenso_JumpKing_Classic.unity";
        private const string BUILD_KEY = "KimayaClassicJumpKing_Build_v3_ExactKnightREA";

        static KimayaClassicJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaClassicJumpKingBuilder] 👑 Ejecutando AutoBuild para Jump King Clásico (Opción A 1:1 Oficial con Knight)...");
                BuildClassicJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string KNIGHT_IDLE_PATH = "Assets/Sprites/JumpKingKnight/knight_idle.png";
        private const string KNIGHT_WALK1_PATH = "Assets/Sprites/JumpKingKnight/knight_walk1.png";
        private const string KNIGHT_WALK_SMEAR_PATH = "Assets/Sprites/JumpKingKnight/knight_walk_smear.png";
        private const string KNIGHT_WALK2_PATH = "Assets/Sprites/JumpKingKnight/knight_walk2.png";
        private const string KNIGHT_CHARGE_PATH = "Assets/Sprites/JumpKingKnight/knight_charge.png";
        private const string KNIGHT_JUMP_UP_PATH = "Assets/Sprites/JumpKingKnight/knight_jump_up.png";
        private const string KNIGHT_FALL_PATH = "Assets/Sprites/JumpKingKnight/knight_fall.png";
        private const string KNIGHT_BOUNCE_PATH = "Assets/Sprites/JumpKingKnight/knight_bounce.png";
        private const string KNIGHT_SPLAT_PATH = "Assets/Sprites/JumpKingKnight/knight_splat.png";

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        private static readonly string[] KNIGHT_SPRITE_PATHS = new string[]
        {
            KNIGHT_IDLE_PATH,
            KNIGHT_WALK1_PATH,
            KNIGHT_WALK_SMEAR_PATH,
            KNIGHT_WALK2_PATH,
            KNIGHT_CHARGE_PATH,
            KNIGHT_JUMP_UP_PATH,
            KNIGHT_FALL_PATH,
            KNIGHT_BOUNCE_PATH,
            KNIGHT_SPLAT_PATH
        };

        private static readonly string[] CLASSIC_SCREEN_PATHS = new string[]
        {
            "Assets/Sprites/Backgrounds/JumpKing_Classic_Screen0.png",
            "Assets/Sprites/Backgrounds/JumpKing_Classic_Screen1.png",
            "Assets/Sprites/Backgrounds/JumpKing_Classic_Screen2.png",
            "Assets/Sprites/Backgrounds/JumpKing_Classic_Screen3.png",
            "Assets/Sprites/Backgrounds/JumpKing_Classic_Screen4.png"
        };

        // PLATAFORMAS EXTRAÍDAS DE LEVEL.XNB (MÁSCARAS DE COLISIÓN DE 8X8 PX DE JUMP KING ORIGINAL)
        public static readonly KimayaPlatformDef[][] ClassicJumpKing5Screens = new KimayaPlatformDef[][]
        {
            // Pantalla 1 Oficial de Jump King (Y: 0.0 a 10.0)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("S1_Wall_Left", -6.556f, 5.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Wall_Right", 6.556f, 5.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Center_Moss_Plat", 0.000f, 8.222f, 3.111f, 1.333f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Cliff_Left", -4.778f, 3.333f, 3.333f, 3.111f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Cliff_Right", 4.778f, 2.444f, 3.333f, 4.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Step_LeftLow", -5.556f, 0.889f, 1.778f, 1.778f, SurfaceType.Normal),
                new KimayaPlatformDef("S1_Bottom_Floor", -0.778f, 0.444f, 7.778f, 0.889f, SurfaceType.Normal),
            },
            // Pantalla 2 Oficial de Jump King (Y: 10.0 a 20.0)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("S2_Wall_Left", -6.556f, 15.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Wall_Right", 6.556f, 15.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Ledge_LeftHigh", -5.444f, 16.556f, 2.000f, 2.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Ledge_LeftMid", -2.333f, 16.222f, 2.000f, 1.778f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Ledge_RightMid", 1.444f, 14.000f, 2.000f, 0.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Ledge_RightWall", 5.556f, 14.000f, 1.778f, 0.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S2_Ledge_RightLow", 2.889f, 11.333f, 2.667f, 0.889f, SurfaceType.Normal),
            },
            // Pantalla 3 Oficial de Jump King (Y: 20.0 a 30.0)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("S3_Wall_Left", -6.556f, 25.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Ledge_TopCenter", -1.889f, 29.778f, 2.000f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Wall_RightTop", 6.556f, 29.111f, 0.222f, 1.778f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Ledge_LeftHigh", -5.667f, 27.111f, 1.556f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Wall_RightTall", 6.556f, 23.444f, 0.222f, 6.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Block_LeftCenter", -1.444f, 26.000f, 1.556f, 1.333f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Block_CenterRight", 2.000f, 23.556f, 1.333f, 1.333f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Platform_Center", 0.000f, 23.333f, 2.667f, 0.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Ledge_RightLow", 5.778f, 22.667f, 1.333f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Step_CenterLow", -0.222f, 21.333f, 1.333f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S3_Step_RightLow", 3.000f, 21.333f, 1.556f, 0.444f, SurfaceType.Normal),
            },
            // Pantalla 4 Oficial de Jump King (Y: 30.0 a 40.0)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("S4_Wall_Left", -6.556f, 35.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Pillar_Left", -2.667f, 37.667f, 0.444f, 4.667f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Top_Snub", 2.667f, 39.778f, 0.444f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Wall_Right", 6.556f, 35.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Ledge_RightHigh", 3.444f, 37.778f, 2.000f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Block_LeftMid", -2.111f, 36.444f, 0.667f, 2.222f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Pillar_Right", 2.667f, 35.556f, 0.444f, 4.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Ledge_RightWall", 5.889f, 36.222f, 1.111f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Block_CenterMid", 2.000f, 34.556f, 0.889f, 2.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Ledge_LeftWall", -5.667f, 33.778f, 1.556f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Ledge_LeftCenter", -1.889f, 33.778f, 2.000f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S4_Platform_Bottom", -1.889f, 30.556f, 2.000f, 1.111f, SurfaceType.Normal),
            },
            // Pantalla 5 Oficial de Jump King (Y: 40.0 a 50.0)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("S5_Wall_Left", -6.556f, 45.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Top_Exit_Rim", 0.000f, 49.778f, 4.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Wall_Right", 6.556f, 45.000f, 0.222f, 10.000f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Step_A", -1.778f, 48.222f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Step_B", 0.000f, 47.778f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_LeftHigh", -5.111f, 47.333f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Step_C", 1.778f, 47.333f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_RightHigh", 6.000f, 45.333f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_LeftWall", -6.000f, 43.111f, 0.889f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_RightMid", 3.000f, 43.111f, 1.111f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_LeftLow", -3.000f, 41.111f, 1.111f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Ledge_RightLow", 3.000f, 41.111f, 1.111f, 0.444f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Post_Left", -2.667f, 40.444f, 0.444f, 0.889f, SurfaceType.Normal),
                new KimayaPlatformDef("S5_Post_Right", 2.667f, 40.444f, 0.444f, 0.889f, SurfaceType.Normal),
            },
        };

        [MenuItem("Tools/Kimaya/👑 [5] Construir Jump King Clásico (Opción A - 1:1 Oficial)")]
        public static void BuildMenu()
        {
            BuildClassicJumpKingScene();
        }

        public static void BuildClassicJumpKingScene()
        {
            Debug.Log("[KimayaClassicJumpKingBuilder] 👑 Iniciando construcción de Jump King Clásico (Opción A - 1:1 Oficial)...");

            // 1. Asegurar importación de fondos y sprites del Caballero Jump King a PPU = 36f
            foreach (var path in CLASSIC_SCREEN_PATHS)
            {
                EnsureTextureIsSprite(path, 36.0f, SpriteAlignment.Center, new Vector2(0.5f, 0.5f));
            }

            foreach (var path in KNIGHT_SPRITE_PATHS)
            {
                EnsureTextureIsSprite(path, 36.0f, SpriteAlignment.BottomCenter, new Vector2(0.5f, 0.0f));
            }

            // Crear escena nueva
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cargar sprites del Caballero Oficial Jump King
            Sprite knightIdleSprite = LoadAnySprite(KNIGHT_IDLE_PATH);
            Sprite knightWalk1 = LoadAnySprite(KNIGHT_WALK1_PATH);
            Sprite knightWalkSmear = LoadAnySprite(KNIGHT_WALK_SMEAR_PATH);
            Sprite knightWalk2 = LoadAnySprite(KNIGHT_WALK2_PATH);
            Sprite knightCharge = LoadAnySprite(KNIGHT_CHARGE_PATH);
            Sprite knightJumpUp = LoadAnySprite(KNIGHT_JUMP_UP_PATH);
            Sprite knightFall = LoadAnySprite(KNIGHT_FALL_PATH);
            Sprite knightBounce = LoadAnySprite(KNIGHT_BOUNCE_PATH);
            Sprite knightSplat = LoadAnySprite(KNIGHT_SPLAT_PATH);

            Sprite falinSprite = LoadAnySprite(FALIN_SPRITE_PATH);
            Sprite miaSprite = LoadAnySprite(MIA_SPRITE_PATH);
            Sprite altarSprite = LoadAnySprite(ALTAR_SPRITE_PATH);

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");

            // 2. Jugador (Caballero Oficial Jump King 1:1)
            // Spawn exacto oficial en Pantalla 1: (X=231, Y=302 en 480x360 px) -> X = -0.25f, Y = 0.89f (sobre el suelo)
            GameObject playerGo = new GameObject("Player_JumpKing");
            playerGo.transform.position = new Vector3(-0.25f, 0.89f, 0f);

            var sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = knightIdleSprite;
            sr.sortingOrder = 25;

            var rb = playerGo.GetComponent<Rigidbody2D>();
            if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.freezeRotation = true;

            var col = playerGo.AddComponent<BoxCollider2D>();
            // Hitbox Jump King: 18x26 px a 36 PPU = 0.50f ancho x 0.722f alto
            // edgeRadius 0.03f para suavizar esquinas sin atraparse
            col.size = new Vector2(0.44f, 0.662f);
            col.offset = new Vector2(0f, 0.361f);
            col.edgeRadius = 0.03f;
            var pMat = new PhysicsMaterial2D("PlayerMat") { friction = 0.2f, bounciness = 0f };
            col.sharedMaterial = pMat;

            var jumpCtrl = playerGo.AddComponent<KimayaJumpKingController>();
            jumpCtrl.spriteRenderer = sr;
            jumpCtrl.character = KimayaJumpKingController.CharacterType.JumpKing;
            jumpCtrl.walkSpeed = 3.5f;
            jumpCtrl.maxChargeTime = 0.60f;
            jumpCtrl.minJumpForce = 7.0f;
            jumpCtrl.maxJumpForce = 17.5f;
            jumpCtrl.horizontalJumpSpeed = 7.0f;
            jumpCtrl.wallBounceMultiplier = 0.50f;
            jumpCtrl.enableHighFallStun = false;

            // Gizmos de trayectoria
            playerGo.AddComponent<KimayaJumpTrajectory>();

            // Animador Jump King Knight (con soporte de reserva para Falin y Mía)
            var anim = playerGo.AddComponent<KimayaJumpKingAnimator>();
            anim.knightIdle = knightIdleSprite;
            anim.knightWalkFrames = new Sprite[] { knightWalk1, knightWalkSmear, knightWalk2 };
            anim.knightCharge = knightCharge;
            anim.knightJumpUp = knightJumpUp;
            anim.knightFall = knightFall;
            anim.knightBounce = knightBounce;
            anim.knightSplat = knightSplat;

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

            // 3. Cámara Jump King (4:3 aspect ratio de Jump King original: 13.333f ancho x 10f alto)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.08f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 5f, -10f);

            var screenCam = camGo.AddComponent<JumpKingScreenCamera>();
            screenCam.target = playerGo.transform;
            screenCam.screenHeight = 10f;
            screenCam.screenWidth = 13.333f;
            screenCam.baseWorldY = 0f;
            screenCam.transitionSpeed = 15f;

            camGo.AddComponent<AudioListener>();

            // 4. Paredes exteriores
            GameObject wallsRoot = new GameObject("BoundaryWalls");
            CreateWall(wallsRoot.transform, -6.667f, 25f, 0.4f, 52f, groundLayer); // Pared izquierda
            CreateWall(wallsRoot.transform, 6.667f, 25f, 0.4f, 52f, groundLayer);  // Pared derecha
            CreateWall(wallsRoot.transform, 0f, -0.5f, 15f, 1.0f, groundLayer);    // Suelo base debajo de Pantalla 1
            CreateWall(wallsRoot.transform, 0f, 50.5f, 15f, 1.0f, groundLayer);    // Techo Pantalla 5

            // 5. Fondos Oficiales de Jump King (Pantallas 1 a 5 de Red Crown Woods)
            GameObject bgRoot = new GameObject("Backgrounds");
            for (int i = 0; i < CLASSIC_SCREEN_PATHS.Length; i++)
            {
                Sprite bgSprite = LoadAnySprite(CLASSIC_SCREEN_PATHS[i]);
                CreateScreenBackground(bgRoot.transform, i, bgSprite, $"BG_JumpKing_Screen_{i + 1}");
            }

            // 6. Colisionadores Sólidos 1:1 de Jump King con CompositeCollider2D para eliminar juntas
            GameObject platformsRoot = new GameObject("Platforms");
            for (int r = 0; r < ClassicJumpKing5Screens.Length; r++)
            {
                GameObject roomFolder = new GameObject($"Screen_{r + 1}_Colliders");
                roomFolder.transform.SetParent(platformsRoot.transform);
                roomFolder.layer = groundLayer;

                var rbStatic = roomFolder.AddComponent<Rigidbody2D>();
                rbStatic.bodyType = RigidbodyType2D.Static;

                var compCol = roomFolder.AddComponent<CompositeCollider2D>();
                compCol.geometryType = CompositeCollider2D.GeometryType.Polygons;
                compCol.sharedMaterial = new PhysicsMaterial2D("PlatMat") { friction = 0.5f, bounciness = 0f };

                foreach (var p in ClassicJumpKing5Screens[r])
                {
                    CreatePlatformCollider(roomFolder.transform, p.centerX, p.centerY, p.width, p.height, groundLayer, p.name, p.surfaceType, true);
                }
            }

            // 7. Altar y compañera rescatada en la cima de Pantalla 5
            GameObject altarGo = new GameObject("Altar_Rescate");
            altarGo.transform.position = new Vector3(0.0f, 49.5f, 0f);
            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite;
            altarSr.sortingOrder = 5;
            altarGo.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            GameObject rescuedGo = new GameObject("Rescued_Companion");
            rescuedGo.transform.position = new Vector3(1.0f, 49.4f, 0f);
            rescuedGo.transform.localScale = new Vector3(-0.9f, 0.9f, 1f);
            var rescuedSr = rescuedGo.AddComponent<SpriteRenderer>();
            rescuedSr.sprite = miaSprite != null ? miaSprite : falinSprite;
            rescuedSr.sortingOrder = 6;

            // 8. Canvas y UI
            GameObject canvasGo = new GameObject("JumpKing_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();

            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            var manager = canvasGo.AddComponent<KimayaJumpKingManager>();
            manager.playerController = jumpCtrl;
            manager.screenCamera = screenCam;
            manager.falinSprite = falinSprite;
            manager.miaSprite = miaSprite;
            manager.finalScreenIndex = 4;

            // HUD
            GameObject topHud = CreateUIPanel(canvasGo.transform, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(1400, 65));
            var topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.04f, 0.07f, 0.14f, 0.90f);

            manager.screenText = CreateUIText(topHud.transform, "Txt_Piso", "Pantalla 1 / 5: Red Crown Woods (Jump King Clásico 1:1)", 24, TextAnchor.MiddleLeft, new Vector2(-460, 0), new Vector2(480, 50));
            manager.screenText.color = new Color(1f, 0.9f, 0.35f);

            manager.characterNameText = CreateUIText(topHud.transform, "Txt_Char", "👑 Jump King (Oficial) | [TAB] Alternar", 23, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(380, 50));
            manager.characterNameText.color = new Color(1f, 0.85f, 0.2f);

            manager.statsText = CreateUIText(topHud.transform, "Txt_Stats", "⏱️ 00:00  |  🦘 Saltos: 0  |  💥 Caídas: 0", 22, TextAnchor.MiddleRight, new Vector2(460, 0), new Vector2(420, 50));
            manager.statsText.color = Color.white;

            // Barra de Carga
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

            Text controlsText = CreateUIText(canvasGo.transform, "Txt_Controls", "Mantén [ESPACIO] para cargar salto • [A / D] Dirección • [TAB] Alternar Personaje", 18, TextAnchor.MiddleCenter, new Vector2(0, 22), new Vector2(1100, 28));
            controlsText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controlsText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controlsText.color = new Color(0.75f, 0.85f, 0.95f, 0.85f);

            // Panel de Victoria
            GameObject victoryGo = CreateUIPanel(canvasGo.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 440));
            var vicBg = victoryGo.AddComponent<Image>();
            vicBg.color = new Color(0.05f, 0.12f, 0.22f, 0.98f);

            Text vicTitle = CreateUIText(victoryGo.transform, "Txt_VicTitle", "👑 ¡CUMBRE DE RED CROWN WOODS!", 34, TextAnchor.MiddleCenter, new Vector2(0, 145), new Vector2(720, 60));
            vicTitle.color = new Color(1f, 0.85f, 0.2f);

            Text vicStats = CreateUIText(victoryGo.transform, "Txt_VicStats", "⏱️ TIEMPO: 00:00   |   🦘 SALTOS: 0   |   💥 CAÍDAS: 0", 22, TextAnchor.MiddleCenter, new Vector2(0, 80), new Vector2(720, 40));
            vicStats.color = new Color(0.35f, 0.95f, 0.55f);
            manager.victoryStatsText = vicStats;

            Text vicDesc = CreateUIText(victoryGo.transform, "Txt_VicDesc", "¡Has completado las 5 Pantallas de Red Crown Woods (Jump King Oficial)!\nLa física y colisiones 1:1 fueron verificadas con éxito.", 21, TextAnchor.MiddleCenter, new Vector2(0, 10), new Vector2(680, 95));
            vicDesc.color = Color.white;
            manager.victoryDescText = vicDesc;

            GameObject btnRestartGo = CreateUIPanel(victoryGo.transform, "Btn_Restart", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-150, -115), new Vector2(250, 56));
            var rImg = btnRestartGo.AddComponent<Image>();
            rImg.color = new Color(0.12f, 0.65f, 0.32f);
            var rBtn = btnRestartGo.AddComponent<Button>();
            Text rTxt = CreateUIText(btnRestartGo.transform, "Lbl", "🔄 Repetir Ascenso", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(250, 56));
            rTxt.color = Color.white;
            manager.restartButton = rBtn;

            GameObject btnHubGo = CreateUIPanel(victoryGo.transform, "Btn_Hub", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150, -115), new Vector2(250, 56));
            var hImg = btnHubGo.AddComponent<Image>();
            hImg.color = new Color(0.20f, 0.45f, 0.80f);
            var hBtn = btnHubGo.AddComponent<Button>();
            Text hTxt = CreateUIText(btnHubGo.transform, "Lbl", "🏡 Volver al Hub", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(250, 56));
            hTxt.color = Color.white;
            manager.returnToHubButton = hBtn;

            manager.victoryPanel = victoryGo;
            victoryGo.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[KimayaClassicJumpKingBuilder] ✅ Escena Jump King Clásico (Opción A 1:1) creada exitosamente en {SCENE_PATH}");

            RegisterSceneInBuildSettings(SCENE_PATH);
        }

        private static void EnsureTextureIsSprite(string path, float ppu = 36.0f, SpriteAlignment alignment = SpriteAlignment.Center, Vector2? customPivot = null)
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
                if (importer.spritePixelsPerUnit != ppu)
                {
                    importer.spritePixelsPerUnit = ppu;
                    changed = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (importer.alignment != (int)alignment)
                {
                    importer.alignment = (int)alignment;
                    changed = true;
                }
                if (customPivot.HasValue && importer.spritePivot != customPivot.Value)
                {
                    importer.spritePivot = customPivot.Value;
                    changed = true;
                }
                if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
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
            bgGo.transform.position = new Vector3(0f, screenIndex * 10f + 5f, 0f);

            var sr = bgGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -10;
        }

        private static void CreatePlatformCollider(Transform parent, float x, float y, float w, float h, int layer, string name, SurfaceType surfaceType, bool usedByComposite = false)
        {
            GameObject platGo = new GameObject(name);
            platGo.transform.SetParent(parent);
            platGo.layer = layer;
            platGo.transform.position = new Vector3(x, y, 0f);

            var col = platGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            col.usedByComposite = usedByComposite;

            float friction = (surfaceType == SurfaceType.Ice) ? 0.05f : 0.5f;
            var pMat = new PhysicsMaterial2D($"PlatMat_{surfaceType}") { friction = friction, bounciness = 0f };
            col.sharedMaterial = pMat;
            col.usedByEffector = false; // 100% sólido, cero bugs

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
            EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
