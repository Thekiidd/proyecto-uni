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
    /// Generador automático de la escena de Ascenso Vertical estilo Jump King (3 Pantallas Maestras Integradas).
    /// El nivel utiliza fondos ilustrados completos en pixel art donde las ramas y salientes YA SON las plataformas:
    /// - Pantalla 0 (Piso 1): El Campamento del Bosque Antiguo (Fogata base y grandes ramas laterales).
    /// - Pantalla 1 (Piso 2): El Gran Árbol Milenario (Ascenso en zigzag entre ramas colosales y follaje).
    /// - Pantalla 2 (Piso 3): La Cumbre de la Torre Ancestral (Ruinas de piedra, almenas y el Altar Sagrado).
    /// Menú: Tools > Kimaya > 🦘 [4] Construir Modo Jump King (Ascenso Vertical)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaJumpKingBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaJumpKing_Build_v10";

        static KimayaJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaJumpKingBuilder] 🚀 Ejecutando AutoBuildCheck v10 para Jump King (Pantallas Integradas)...");
                BuildJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png";

        private const string SCREEN0_BG_PATH = "Assets/Sprites/Backgrounds/JumpKing_Screen0_ForestBase.png";
        private const string SCREEN1_BG_PATH = "Assets/Sprites/Backgrounds/JumpKing_Screen1_TreeClimb.png";
        private const string SCREEN2_BG_PATH = "Assets/Sprites/Backgrounds/JumpKing_Screen2_TowerSummit.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        [MenuItem("Tools/Kimaya/🦘 [4] Construir Modo Jump King (Ascenso Vertical)")]
        public static void BuildMenu()
        {
            BuildJumpKingScene();
        }

        public static void BuildJumpKingScene()
        {
            Debug.Log("[KimayaJumpKingBuilder] 🚀 Iniciando construcción de escena Jump King v10...");

            // 1. Asegurar importación de fondos
            EnsureTextureIsSprite(SCREEN0_BG_PATH, 21.6f);
            EnsureTextureIsSprite(SCREEN1_BG_PATH, 21.6f);
            EnsureTextureIsSprite(SCREEN2_BG_PATH, 21.6f);

            // Crear escena nueva
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cargar sprites
            Sprite falinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FALIN_SPRITE_PATH);
            Sprite miaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MIA_SPRITE_PATH);
            Sprite altarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ALTAR_SPRITE_PATH);
            Sprite screen0Bg = AssetDatabase.LoadAssetAtPath<Sprite>(SCREEN0_BG_PATH);
            Sprite screen1Bg = AssetDatabase.LoadAssetAtPath<Sprite>(SCREEN1_BG_PATH);
            Sprite screen2Bg = AssetDatabase.LoadAssetAtPath<Sprite>(SCREEN2_BG_PATH);

            // Layer Ground
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");

            // 2. Jugador (Falin / Mía en vista de perfil lateral)
            GameObject playerGo = new GameObject("Player_JumpKing");
            playerGo.transform.position = new Vector3(0f, 2.0f, 0f);

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

            // Animador cinemático Jump King con poses reales de estiramiento, carga agachada, caída y splat
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

            // 3. Cámara Jump King (Pantalla por pantalla con altura de 10 unidades)
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f; // Altura visible = 10 unidades
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.09f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 5f, -10f);

            var screenCam = camGo.AddComponent<JumpKingScreenCamera>();
            screenCam.target = playerGo.transform;
            screenCam.screenHeight = 10f;
            screenCam.screenWidth = 17.77f;
            screenCam.baseWorldY = 0f;
            screenCam.transitionSpeed = 15f;

            camGo.AddComponent<AudioListener>();

            // 4. Paredes Verticales de Contención (3 Pantallas = 30 unidades verticales de Y: 0 a 30)
            GameObject wallsRoot = new GameObject("BoundaryWalls");
            CreateWall(wallsRoot.transform, -8.88f, 15f, 0.8f, 32f, groundLayer);
            CreateWall(wallsRoot.transform, 8.88f, 15f, 0.8f, 32f, groundLayer);
            CreateWall(wallsRoot.transform, 0f, 0.2f, 18.5f, 1f, groundLayer);
            CreateWall(wallsRoot.transform, 0f, 30.5f, 18.5f, 1f, groundLayer);

            // 5. Fondos Pixel Art Ilustrados por Pantalla (Los fondos YA CONTIENEN las ramas y plataformas)
            GameObject bgRoot = new GameObject("Backgrounds");
            CreateScreenBackground(bgRoot.transform, 0, screen0Bg, "BG_Piso0_CampamentoBosque");
            CreateScreenBackground(bgRoot.transform, 1, screen1Bg, "BG_Piso1_GranArbol");
            CreateScreenBackground(bgRoot.transform, 2, screen2Bg, "BG_Piso2_CumbreTorre");

            // 6. Colisionadores Físicos en las Ramas Ilustradas (Sin bloques verdes artificiales)
            GameObject platformsRoot = new GameObject("Platforms");

            // =========================================================================
            // --- PISO 1: El Campamento del Bosque Antiguo (Y: 0 a 10) ---
            // =========================================================================
            CreateInvisiblePlatform(platformsRoot.transform, 0.0f, 1.20f, 16.0f, 0.6f, groundLayer, "P0_Ground_Campfire");
            CreateInvisiblePlatform(platformsRoot.transform, -4.0f, 6.85f, 4.4f, 0.5f, groundLayer, "P0_Left_MossyBranch");
            CreateInvisiblePlatform(platformsRoot.transform, 4.1f, 6.30f, 4.4f, 0.5f, groundLayer, "P0_Right_MossyBranch");

            // =========================================================================
            // --- PISO 2: El Gran Árbol Milenario (Y: 10 a 20) ---
            // Ascenso en zigzag a través de las ramas del tronco colosal
            // =========================================================================
            CreateInvisiblePlatform(platformsRoot.transform, 1.06f, 12.22f, 3.2f, 0.4f, groundLayer, "P1_Plat1_LowRight");
            CreateInvisiblePlatform(platformsRoot.transform, -1.02f, 13.80f, 3.2f, 0.4f, groundLayer, "P1_Plat2_MidLeft");
            CreateInvisiblePlatform(platformsRoot.transform, 0.65f, 15.56f, 3.4f, 0.4f, groundLayer, "P1_Plat3_CenterRight");
            CreateInvisiblePlatform(platformsRoot.transform, -0.79f, 17.13f, 3.2f, 0.4f, groundLayer, "P1_Plat4_TopCenter");

            // =========================================================================
            // --- PISO 3: La Cumbre de la Torre Ancestral (Y: 20 a 30) ---
            // Salientes de roca, puente y la cima del torreón con el Altar de Rescate
            // =========================================================================
            CreateInvisiblePlatform(platformsRoot.transform, 0.0f, 22.22f, 16.0f, 0.6f, groundLayer, "P2_Ground_Bridge");
            CreateInvisiblePlatform(platformsRoot.transform, 1.99f, 22.96f, 2.4f, 0.4f, groundLayer, "P2_Step_Lower");
            CreateInvisiblePlatform(platformsRoot.transform, 5.0f, 28.33f, 4.8f, 0.4f, groundLayer, "P2_Tower_TopBattlement");
            CreateInvisiblePlatform(platformsRoot.transform, 7.77f, 27.22f, 2.2f, 0.4f, groundLayer, "P2_Tower_Overhang");

            // Altar Ancestral de Victoria en lo alto de la torre
            GameObject altarGo = new GameObject("Altar_Rescate");
            altarGo.transform.position = new Vector3(5.0f, 29.2f, 0f);
            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite;
            altarSr.sortingOrder = 5;
            altarGo.transform.localScale = new Vector3(0.7f, 0.7f, 1f);

            // Compañera esperando rescate en la cima
            GameObject rescuedGo = new GameObject("Rescued_Companion");
            rescuedGo.transform.position = new Vector3(6.2f, 29.0f, 0f);
            rescuedGo.transform.localScale = new Vector3(-0.9f, 0.9f, 1f); // Mirando hacia el héroe
            var rescuedSr = rescuedGo.AddComponent<SpriteRenderer>();
            rescuedSr.sprite = miaSprite != null ? miaSprite : falinSprite;
            rescuedSr.sortingOrder = 6;

            // 7. UI / Canvas de Jump King
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
            manager.finalScreenIndex = 2; // 3 Pantallas (Índices 0, 1, 2)

            // Panel Superior HUD
            GameObject topHud = CreateUIPanel(canvasGo.transform, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -45), new Vector2(1400, 65));
            var topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.04f, 0.07f, 0.14f, 0.90f);

            // Texto Piso
            manager.screenText = CreateUIText(topHud.transform, "Txt_Piso", "Piso 1 / 3: El Campamento del Bosque Antiguo", 24, TextAnchor.MiddleLeft, new Vector2(-460, 0), new Vector2(420, 50));
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
            Text controlsText = CreateUIText(canvasGo.transform, "Txt_Controls", "Mantén [ESPACIO] para cargar salto (te agachas) • [A / D] Dirección • [TAB] Alternar Falin / Mía", 18, TextAnchor.MiddleCenter, new Vector2(0, 22), new Vector2(1100, 28));
            controlsText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controlsText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controlsText.color = new Color(0.75f, 0.85f, 0.95f, 0.85f);

            // Badge de Aturdimiento
            GameObject stunnedGo = CreateUIPanel(canvasGo.transform, "StunnedBadge", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(340, 60));
            var stunnedBg = stunnedGo.AddComponent<Image>();
            stunnedBg.color = new Color(0.85f, 0.15f, 0.15f, 0.92f);
            Text stunnedTxt = CreateUIText(stunnedGo.transform, "Txt_Stunned", "💫 ¡CAÍDA FUERTE! RECUPERANDO...", 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(340, 60));
            stunnedTxt.color = Color.white;
            manager.stunnedBadge = stunnedGo;
            stunnedGo.SetActive(false);

            // Panel de Victoria
            GameObject victoryGo = CreateUIPanel(canvasGo.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 400));
            var vicBg = victoryGo.AddComponent<Image>();
            vicBg.color = new Color(0.06f, 0.15f, 0.25f, 0.95f);

            Text vicTitle = CreateUIText(victoryGo.transform, "Txt_VicTitle", "🏆 ¡RESCATE EN LA CIMA!", 42, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(650, 70));
            vicTitle.color = new Color(1f, 0.85f, 0.2f);

            Text vicDesc = CreateUIText(victoryGo.transform, "Txt_VicDesc", "¡Has conquistado el Ascenso Vertical al estilo Jump King!\nLlegaste a la cumbre de la torre ancestral y rescataste a tu compañera.\n\nFalin y Mía han triunfado juntos.", 24, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(600, 120));
            vicDesc.color = Color.white;

            manager.victoryPanel = victoryGo;
            victoryGo.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[KimayaJumpKingBuilder] ✅ Escena Jump King v10 creada exitosamente en {SCENE_PATH}");

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
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    changed = true;
                }
                if (importer.wrapMode != TextureWrapMode.Clamp)
                {
                    importer.wrapMode = TextureWrapMode.Clamp;
                    changed = true;
                }
                if (Mathf.Abs(importer.spritePixelsPerUnit - ppu) > 0.1f)
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

        private static void CreateScreenBackground(Transform parent, int screenIdx, Sprite sprite, string name)
        {
            if (sprite == null) return;
            GameObject bgGo = new GameObject(name);
            bgGo.transform.SetParent(parent);
            float centerY = screenIdx * 10f + 5f;
            bgGo.transform.position = new Vector3(0f, centerY, 5f);

            var sr = bgGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -50;

            float targetW = 17.77f;
            float targetH = 10.0f;
            float spriteW = sprite.bounds.size.x;
            float spriteH = sprite.bounds.size.y;
            if (spriteW > 0.01f && spriteH > 0.01f)
            {
                bgGo.transform.localScale = new Vector3(targetW / spriteW, targetH / spriteH, 1f);
            }
        }

        private static GameObject CreateInvisiblePlatform(Transform parent, float x, float y, float w, float h, int layer, string name, SurfaceType surface = SurfaceType.Normal)
        {
            GameObject plat = new GameObject(name);
            plat.layer = layer;
            plat.transform.SetParent(parent);
            plat.transform.position = new Vector3(x, y, 0f);

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            var pMat = new PhysicsMaterial2D("PlatMat") { friction = 0.4f, bounciness = 0f };
            col.sharedMaterial = pMat;

            if (surface != SurfaceType.Normal)
            {
                var sComp = plat.AddComponent<KimayaSurface>();
                sComp.surfaceType = surface;
                if (surface == SurfaceType.Ice) pMat.friction = 0.05f;
                else if (surface == SurfaceType.Sand) pMat.friction = 0.8f;
            }

            return plat;
        }

        private static void CreateWall(Transform parent, float x, float y, float w, float h, int layer)
        {
            GameObject wall = new GameObject("Wall");
            wall.layer = layer;
            wall.transform.SetParent(parent);
            wall.transform.position = new Vector3(x, y, 0f);

            var col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            var pMat = new PhysicsMaterial2D("WallMat") { friction = 0f, bounciness = 0f };
            col.sharedMaterial = pMat;
        }

        private static Sprite LoadSprite(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite[] LoadFrames(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return new Sprite[0];
            string[] files = Directory.GetFiles(folderPath, "*.png");
            Array.Sort(files);
            var list = new System.Collections.Generic.List<Sprite>();
            foreach (var f in files)
            {
                string assetPath = f.Replace("\\", "/");
                var s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (s != null) list.Add(s);
            }
            return list.ToArray();
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static Text CreateUIText(Transform parent, string name, string text, int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var t = go.AddComponent<Text>();
            t.text = text;
            t.fontSize = fontSize;
            t.alignment = alignment;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (t.font == null) t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return t;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }
            var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
            Array.Copy(scenes, newScenes, scenes.Length);
            newScenes[newScenes.Length - 1] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = newScenes;
        }
    }
}
