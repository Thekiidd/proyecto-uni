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
    /// Generador automático de la escena de Ascenso Vertical estilo Jump King.
    /// Crea el mapa de pantallas verticales, plataformas de salto táctico,
    /// cámara pantalla por pantalla y HUD completo.
    /// Menú: Tools > Kimaya > 🦘 [4] Construir Modo Jump King (Ascenso Vertical)
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaJumpKingBuilder
    {
        public const string SCENE_PATH = "Assets/Scenes/Ascenso_Vertical.unity";
        private const string BUILD_KEY = "KimayaJumpKing_Build_v1";

        static KimayaJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaJumpKingBuilder] 🚀 Ejecutando AutoBuildCheck para Jump King...");
                BuildJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/south.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/south.png";
        private const string PLATFORM_SPRITE_PATH = "Assets/Sprites/Tiles/Stone_Platform.png";
        private const string PLANK_SPRITE_PATH = "Assets/Sprites/Tiles/Wood_Plank.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        [MenuItem("Tools/Kimaya/🦘 [4] Construir Modo Jump King (Ascenso Vertical)")]
        public static void BuildMenu()
        {
            BuildJumpKingScene();
        }

        public static void BuildJumpKingScene()
        {
            Debug.Log("[KimayaJumpKingBuilder] 🚀 Iniciando construcción de escena Jump King...");

            // 1. Crear escena nueva
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cargar sprites
            Sprite falinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FALIN_SPRITE_PATH);
            Sprite miaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MIA_SPRITE_PATH);
            Sprite platSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PLATFORM_SPRITE_PATH);
            Sprite plankSprite = AssetDatabase.LoadAssetAtPath<Sprite>(PLANK_SPRITE_PATH);
            Sprite altarSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ALTAR_SPRITE_PATH);

            // Layer Ground
            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");
            LayerMask groundMask = 1 << groundLayer;

            // 2. Jugador (Falin / Mía)
            GameObject playerGo = new GameObject("Player_JumpKing");
            playerGo.transform.position = new Vector3(0f, 0.8f, 0f);

            var sr = playerGo.AddComponent<SpriteRenderer>();
            sr.sprite = falinSprite != null ? falinSprite : miaSprite;
            sr.sortingOrder = 10;

            var rb = playerGo.GetComponent<Rigidbody2D>(); // Ya requerido o añadido por el script
            if (rb == null) rb = playerGo.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            var col = playerGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.8f, 0.9f);
            col.offset = new Vector2(0f, 0.45f);

            var groundCheckGo = new GameObject("GroundCheck");
            groundCheckGo.transform.SetParent(playerGo.transform);
            groundCheckGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);

            var jumpCtrl = playerGo.AddComponent<KimayaJumpKingController>();
            jumpCtrl.spriteRenderer = sr;
            jumpCtrl.groundCheck = groundCheckGo.transform;
            jumpCtrl.groundLayer = groundMask;
            jumpCtrl.character = KimayaJumpKingController.CharacterType.Falin;

            // 3. Cámara Jump King
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f; // Altura visible = 10 unidades
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f); // Fondo azul noche atmosférico
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 5f, -10f);

            var screenCam = camGo.AddComponent<JumpKingScreenCamera>();
            screenCam.target = playerGo.transform;
            screenCam.screenHeight = 10f;
            screenCam.screenWidth = 16f;
            screenCam.baseWorldY = 0f;
            screenCam.transitionSpeed = 12f;

            camGo.AddComponent<AudioListener>();

            // 4. Estructura de Paredes Verticales (Límites)
            GameObject wallsRoot = new GameObject("BoundaryWalls");
            // Pared izquierda
            CreateWall(wallsRoot.transform, -8.2f, 40f, 0.8f, 85f, groundLayer);
            // Pared derecha
            CreateWall(wallsRoot.transform, 8.2f, 40f, 0.8f, 85f, groundLayer);
            // Suelo del fondo
            CreateWall(wallsRoot.transform, 0f, -0.5f, 18f, 1f, groundLayer);
            // Techo de la cima
            CreateWall(wallsRoot.transform, 0f, 82f, 18f, 1f, groundLayer);

            // 5. Generar Plataformas por Pantallas (8 Pantallas)
            GameObject platformsRoot = new GameObject("Platforms");

            // --- Pantalla 0: Base del Bosque (Y: 0 a 10) ---
            CreatePlatform(platformsRoot.transform, 0f, 0.1f, 16f, 0.6f, platSprite, groundLayer, "Piso_0_Suelo");
            CreatePlatform(platformsRoot.transform, -3.5f, 2.5f, 3.5f, 0.5f, platSprite, groundLayer, "P0_Plat_1");
            CreatePlatform(platformsRoot.transform, 3.5f, 5.0f, 3.5f, 0.5f, platSprite, groundLayer, "P0_Plat_2");
            CreatePlatform(platformsRoot.transform, -2.0f, 7.5f, 3.0f, 0.5f, platSprite, groundLayer, "P0_Plat_3");
            CreatePlatform(platformsRoot.transform, 2.5f, 9.8f, 2.6f, 0.5f, platSprite, groundLayer, "P0_Plat_4_Salida");

            // --- Pantalla 1: Peñascos del Zarzal (Y: 10 a 20) ---
            CreatePlatform(platformsRoot.transform, -4.5f, 12.2f, 2.8f, 0.5f, platSprite, groundLayer, "P1_Plat_1");
            CreatePlatform(platformsRoot.transform, 1.2f, 14.5f, 2.6f, 0.5f, platSprite, groundLayer, "P1_Plat_2");
            CreatePlatform(platformsRoot.transform, 5.2f, 16.8f, 2.4f, 0.5f, platSprite, groundLayer, "P1_Plat_3");
            CreatePlatform(platformsRoot.transform, -1.0f, 19.5f, 2.2f, 0.5f, platSprite, groundLayer, "P1_Plat_4_Salida");

            // --- Pantalla 2: El Gran Árbol Ancestral (Y: 20 a 30) ---
            CreatePlatform(platformsRoot.transform, -5.2f, 22.0f, 2.4f, 0.5f, plankSprite, groundLayer, "P2_Plat_1");
            CreatePlatform(platformsRoot.transform, 2.5f, 24.3f, 2.2f, 0.5f, plankSprite, groundLayer, "P2_Plat_2");
            CreatePlatform(platformsRoot.transform, -2.5f, 26.6f, 2.0f, 0.5f, plankSprite, groundLayer, "P2_Plat_3");
            CreatePlatform(platformsRoot.transform, 4.2f, 29.3f, 2.0f, 0.5f, plankSprite, groundLayer, "P2_Plat_4_Salida");

            // --- Pantalla 3: Acantilados de Viento (Y: 30 a 40) ---
            CreatePlatform(platformsRoot.transform, -3.2f, 31.8f, 1.8f, 0.5f, platSprite, groundLayer, "P3_Plat_1");
            CreatePlatform(platformsRoot.transform, 3.2f, 34.0f, 1.8f, 0.5f, platSprite, groundLayer, "P3_Plat_2");
            CreatePlatform(platformsRoot.transform, -4.2f, 36.5f, 1.8f, 0.5f, platSprite, groundLayer, "P3_Plat_3");
            CreatePlatform(platformsRoot.transform, 0.5f, 39.3f, 1.8f, 0.5f, platSprite, groundLayer, "P3_Plat_4_Salida");

            // --- Pantalla 4: Cavernas de Cristal (Y: 40 a 50) ---
            CreatePlatform(platformsRoot.transform, 4.5f, 41.5f, 1.8f, 0.5f, platSprite, groundLayer, "P4_Plat_1");
            CreatePlatform(platformsRoot.transform, -2.0f, 43.8f, 1.6f, 0.5f, platSprite, groundLayer, "P4_Plat_2");
            CreatePlatform(platformsRoot.transform, -5.2f, 46.2f, 1.6f, 0.5f, platSprite, groundLayer, "P4_Plat_3");
            CreatePlatform(platformsRoot.transform, 2.5f, 49.2f, 1.6f, 0.5f, platSprite, groundLayer, "P4_Plat_4_Salida");

            // --- Pantalla 5: Salientes Resbaladizos (Y: 50 a 60) ---
            CreatePlatform(platformsRoot.transform, -3.5f, 51.5f, 1.5f, 0.5f, platSprite, groundLayer, "P5_Plat_1");
            CreatePlatform(platformsRoot.transform, 3.5f, 53.8f, 1.5f, 0.5f, platSprite, groundLayer, "P5_Plat_2");
            CreatePlatform(platformsRoot.transform, -1.0f, 56.5f, 1.5f, 0.5f, platSprite, groundLayer, "P5_Plat_3");
            CreatePlatform(platformsRoot.transform, 4.2f, 59.3f, 1.5f, 0.5f, platSprite, groundLayer, "P5_Plat_4_Salida");

            // --- Pantalla 6: Ruinas Celestiales (Y: 60 a 70) ---
            CreatePlatform(platformsRoot.transform, -4.2f, 61.5f, 1.5f, 0.5f, platSprite, groundLayer, "P6_Plat_1");
            CreatePlatform(platformsRoot.transform, 1.5f, 64.0f, 1.5f, 0.5f, platSprite, groundLayer, "P6_Plat_2");
            CreatePlatform(platformsRoot.transform, -2.5f, 66.8f, 1.5f, 0.5f, platSprite, groundLayer, "P6_Plat_3");
            CreatePlatform(platformsRoot.transform, 3.0f, 69.5f, 2.0f, 0.5f, platSprite, groundLayer, "P6_Plat_4_Salida");

            // --- Pantalla 7: La Cima Sagrada / Rescate (Y: 70 a 80) ---
            GameObject summit = CreatePlatform(platformsRoot.transform, 0f, 73.0f, 11f, 0.8f, platSprite, groundLayer, "Cima_Altar_Piso");

            // Altar / Círculo de Rescate en la cima
            GameObject altarGo = new GameObject("Altar_Rescate");
            altarGo.transform.position = new Vector3(0f, 74.2f, 0f);
            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = altarSprite != null ? altarSprite : platSprite;
            altarSr.sortingOrder = 5;
            altarGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            // 6. UI / Canvas de Jump King
            GameObject canvasGo = new GameObject("JumpKing_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Manager
            var manager = canvasGo.AddComponent<KimayaJumpKingManager>();
            manager.playerController = jumpCtrl;
            manager.screenCamera = screenCam;
            manager.falinSprite = falinSprite;
            manager.miaSprite = miaSprite;
            manager.finalScreenIndex = 7;

            // Panel Superior HUD
            GameObject topHud = CreateUIPanel(canvasGo.transform, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(900, 70));
            var topBg = topHud.AddComponent<Image>();
            topBg.color = new Color(0.05f, 0.08f, 0.15f, 0.85f);

            // Texto Piso
            manager.screenText = CreateUIText(topHud.transform, "Txt_Piso", "Piso 1 / 8: Base del Bosque", 26, TextAnchor.MiddleLeft, new Vector2(30, 0), new Vector2(350, 50));
            manager.screenText.color = new Color(1f, 0.9f, 0.4f);

            // Texto Personaje
            manager.characterNameText = CreateUIText(topHud.transform, "Txt_Char", "🐾 Falin (Fuerza)", 24, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(300, 50));
            manager.characterNameText.color = new Color(1f, 0.75f, 0.2f);

            // Texto Stats
            manager.statsText = CreateUIText(topHud.transform, "Txt_Stats", "⏱️ 00:00 | 🦘 Saltos: 0 | 💥 Caídas: 0", 22, TextAnchor.MiddleRight, new Vector2(-30, 0), new Vector2(400, 50));
            manager.statsText.color = Color.white;

            // Barra de Carga de Salto (Inferior)
            GameObject chargeBarGo = CreateUIPanel(canvasGo.transform, "ChargeBar_Panel", new Vector2(0.5f, 0.12f), new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(400, 30));
            var slider = chargeBarGo.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            manager.chargeSlider = slider;

            // Fondo de la barra
            var barBg = chargeBarGo.AddComponent<Image>();
            barBg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Relleno de la barra
            GameObject fillGo = CreateUIPanel(chargeBarGo.transform, "Fill", new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.3f);
            slider.fillRect = fillGo.GetComponent<RectTransform>();
            manager.chargeFillImage = fillImg;

            // Texto de Ayuda / Controles (Inferior)
            Text controlsText = CreateUIText(canvasGo.transform, "Txt_Controls", "Mantén [ESPACIO] para cargar salto • [A/D] o Flechas dirección • [TAB] Cambiar entre Falin y Mía", 20, TextAnchor.MiddleCenter, new Vector2(0, 45), new Vector2(1000, 35));
            controlsText.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            controlsText.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            controlsText.color = new Color(0.8f, 0.85f, 0.9f, 0.9f);

            // Badge de Aturdimiento
            GameObject stunnedGo = CreateUIPanel(canvasGo.transform, "StunnedBadge", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(300, 60));
            var stunnedBg = stunnedGo.AddComponent<Image>();
            stunnedBg.color = new Color(0.8f, 0.1f, 0.1f, 0.9f);
            Text stunnedTxt = CreateUIText(stunnedGo.transform, "Txt_Stunned", "💫 ¡ATURDIDO POR LA CAÍDA!", 22, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 60));
            stunnedTxt.color = Color.white;
            manager.stunnedBadge = stunnedGo;
            stunnedGo.SetActive(false);

            // Panel de Victoria (Cima)
            GameObject victoryGo = CreateUIPanel(canvasGo.transform, "VictoryPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700, 400));
            var vicBg = victoryGo.AddComponent<Image>();
            vicBg.color = new Color(0.06f, 0.15f, 0.25f, 0.95f);

            Text vicTitle = CreateUIText(victoryGo.transform, "Txt_VicTitle", "🏆 ¡RESCATE EN LA CIMA!", 42, TextAnchor.MiddleCenter, new Vector2(0, 110), new Vector2(650, 70));
            vicTitle.color = new Color(1f, 0.85f, 0.2f);

            Text vicDesc = CreateUIText(victoryGo.transform, "Txt_VicDesc", "¡Has escalado la montaña ancestral sin checkpoints!\nLograste llegar a la cima y rescatar a tu compañera.\n\nFalin y Mía han demostrado su valentía.", 24, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(600, 120));
            vicDesc.color = Color.white;

            manager.victoryPanel = victoryGo;
            victoryGo.SetActive(false);

            // Guardar Escena
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[KimayaJumpKingBuilder] ✅ Escena Jump King creada exitosamente en {SCENE_PATH}");

            // Registrar en EditorBuildSettings
            RegisterSceneInBuildSettings(SCENE_PATH);
        }

        private static GameObject CreatePlatform(Transform parent, float x, float y, float w, float h, Sprite sprite, int layer, string name)
        {
            GameObject plat = new GameObject(name);
            plat.layer = layer;
            plat.transform.SetParent(parent);
            plat.transform.position = new Vector3(x, y, 0f);

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(w, h);
            sr.color = new Color(0.9f, 0.95f, 1f);

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);

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
        }

        private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return go;
        }

        private static Text CreateUIText(Transform parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;

            var txt = go.AddComponent<Text>();
            txt.text = content;
            txt.fontSize = fontSize;
            txt.alignment = alignment;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return txt;
        }

        private static void RegisterSceneInBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return; // ya existe
            }
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log($"[KimayaJumpKingBuilder] 📝 Escena {scenePath} agregada a EditorBuildSettings.");
        }
    }
}
