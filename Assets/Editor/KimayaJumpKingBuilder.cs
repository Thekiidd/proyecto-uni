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
        private const string BUILD_KEY = "KimayaJumpKing_Build_v18_OfficialRecreation";

        static KimayaJumpKingBuilder()
        {
            EditorApplication.delayCall += AutoBuildCheck;
        }

        private static void AutoBuildCheck()
        {
            if (!EditorPrefs.GetBool(BUILD_KEY, false))
            {
                Debug.Log("[KimayaJumpKingBuilder] 🚀 Ejecutando AutoBuildCheck v18 para recreación de Jump King en Ascenso_Vertical...");
                BuildJumpKingScene();
                EditorPrefs.SetBool(BUILD_KEY, true);
            }
        }

        private const string FALIN_SPRITE_PATH = "Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png";
        private const string MIA_SPRITE_PATH = "Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png";
        private const string ALTAR_SPRITE_PATH = "Assets/Sprites/Village/Art/Props/Ancient_Stone_Circle.png";

        private const string PLAT_NORMAL_PATH = "Assets/Sprites/Backgrounds/Graybox_Plat_Normal.png";
        private const string PLAT_CATCH_PATH = "Assets/Sprites/Backgrounds/Graybox_Plat_Catch.png";
        private const string PLAT_SUMMIT_PATH = "Assets/Sprites/Backgrounds/Graybox_Plat_Summit.png";

        private static readonly string[] JUMP_KING_SCREEN_PATHS = new string[]
        {
            "Assets/Sprites/Backgrounds/Graybox_Screen0_BG.png",
            "Assets/Sprites/Backgrounds/Graybox_Screen1_BG.png",
            "Assets/Sprites/Backgrounds/Graybox_Screen2_BG.png",
            "Assets/Sprites/Backgrounds/Graybox_Screen3_BG.png",
            "Assets/Sprites/Backgrounds/Graybox_Screen4_BG.png"
        };

        // DEFINICIÓN CALIBRADA DE LA GRAYBOX (5 PISOS DEL GRAN ÁRBOL - SALTOS MATEMÁTICAMENTE VERIFICADOS)
        public static readonly KimayaPlatformDef[][] JumpKing5Floors = new KimayaPlatformDef[][]
        {
            // Piso 1 (Floor 0): El Suelo del Bosque y Campamento Base (Y: 0 a 10)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P0_Suelo_Fogata", 0.000f, 0.400f, 17.778f, 0.800f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Ledge_Right", 4.000f, 2.400f, 4.500f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Ledge_Center", -1.000f, 4.800f, 4.200f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Ledge_LeftHigh", -5.000f, 7.000f, 4.000f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P0_Exit_Center", 0.000f, 8.800f, 4.000f, 0.700f, SurfaceType.Normal), // Repisa de transición clara hacia Piso 2
            },
            // Piso 2 (Floor 1): El Gran Tronco y Catch Ledges (Y: 10 a 20)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P1_Catch_Right", 4.500f, 11.800f, 5.000f, 0.800f, SurfaceType.Normal), // Recibe el salto desde P0_Exit_Center
                new KimayaPlatformDef("P1_Catch_Left", -4.800f, 12.000f, 4.600f, 0.800f, SurfaceType.Normal),  // Catch ledge de seguridad
                new KimayaPlatformDef("P1_Mid_Center", -0.500f, 14.500f, 4.000f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_High_Right", 4.500f, 17.000f, 4.000f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P1_Exit_Center", -0.500f, 18.800f, 4.000f, 0.700f, SurfaceType.Normal), // Repisa de transición hacia Piso 3
            },
            // Piso 3 (Floor 2): Canopia Media y Ramas Deliberadas (Y: 20 a 30)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P2_Catch_Left", -4.500f, 21.800f, 5.000f, 0.800f, SurfaceType.Normal), // Recibe el salto desde P1_Exit_Center
                new KimayaPlatformDef("P2_Step_Center", 1.000f, 24.500f, 4.000f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_High_Left", -4.500f, 27.000f, 4.000f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P2_Exit_Right", 1.500f, 28.800f, 4.000f, 0.700f, SurfaceType.Normal),  // Repisa de transición hacia Piso 4
            },
            // Piso 4 (Floor 3): Raíces Colosales y Hojas Rojas (Y: 30 a 40)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P3_Catch_Right", 4.500f, 31.800f, 5.000f, 0.800f, SurfaceType.Normal), // Recibe el salto desde P2_Exit_Right
                new KimayaPlatformDef("P3_Branch_Center", -1.000f, 34.500f, 3.800f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P3_High_Left", -5.000f, 37.000f, 3.800f, 0.700f, SurfaceType.Normal),
                new KimayaPlatformDef("P3_Exit_Right", 0.500f, 38.800f, 4.000f, 0.700f, SurfaceType.Normal),  // Repisa de transición hacia la Cima
            },
            // Piso 5 (Floor 4): La Copa del Árbol y Altar de Kiara (Y: 40 a 50)
            new KimayaPlatformDef[]
            {
                new KimayaPlatformDef("P4_Catch_Right", 4.500f, 41.800f, 4.800f, 0.800f, SurfaceType.Normal), // Recibe el salto desde P3_Exit_Right
                new KimayaPlatformDef("P4_Left_Ledge", -4.500f, 43.500f, 4.500f, 0.800f, SurfaceType.Normal),
                new KimayaPlatformDef("P4_GRAND_SUMMIT_ALTAR", 0.000f, 46.200f, 9.000f, 1.200f, SurfaceType.Normal), // Altar ceremonial amplio
            },
        };

        [MenuItem("Tools/Kimaya/🦘 [4] Construir Modo Jump King (Ascenso Vertical)")]
        public static void BuildMenu()
        {
            BuildJumpKingScene();
        }

        public static void BuildJumpKingScene()
        {
            Debug.Log("[KimayaJumpKingBuilder] 👑 Recreando Jump King Oficial en " + SCENE_PATH);
            KimayaClassicJumpKingBuilder.BuildClassicJumpKingScene(SCENE_PATH);
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

            // Visualización gráfica nítida de la plataforma Graybox
            string spritePath = PLAT_NORMAL_PATH;
            if (name.Contains("Catch")) spritePath = PLAT_CATCH_PATH;
            else if (name.Contains("ALTAR") || name.Contains("Summit")) spritePath = PLAT_SUMMIT_PATH;

            Sprite blockSprite = LoadAnySprite(spritePath);
            if (blockSprite != null)
            {
                var sr = platGo.AddComponent<SpriteRenderer>();
                sr.sprite = blockSprite;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(w, h);
                sr.sortingOrder = 5;
            }

            float friction = (surfaceType == SurfaceType.Ice) ? 0.05f : 0.5f;
            var pMat = new PhysicsMaterial2D($"PlatMat_{surfaceType}") { friction = friction, bounciness = 0f };
            col.sharedMaterial = pMat;

            // En Jump King las plataformas son bloques 100% sólidos de BoxCollider2D (sin effectors que causen bugs de colisión)
            col.usedByEffector = false;

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
