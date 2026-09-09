using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class WoodsAssetImporter
    {
        public const float PPU = 32f;
        public const int TILE_SIZE = 32;

        public const string BACKGROUNDS_PATH = "Assets/Sprites/Woods/Backgrounds";
        public const string DECORATIONS_PATH = "Assets/Sprites/Woods/Decorations";
        public const string TILESHEET_PATH   = "Assets/Sprites/Woods/Tiles/Tilesheet - WOODS.png";
        public const string TILES_OUT_PATH   = "Assets/Tiles/Woods";

        static WoodsAssetImporter()
        {
            EditorApplication.delayCall += AutoImportIfMissing;
        }

        private static void AutoImportIfMissing()
        {
            if (!Directory.Exists(TILES_OUT_PATH) || Directory.GetFiles(TILES_OUT_PATH, "*.asset").Length == 0)
            {
                Debug.Log("[WoodsImporter] Importando y procesando assets de Woods...");
                ProcessAll(false);
            }
        }

        [MenuItem("Tools/Woods/Import and Slice Assets")]
        public static void MenuProcessAll()
        {
            ProcessAll(true);
        }

        public static void ProcessAll(bool showDialog)
        {
            // 1. Asegurar directorios
            if (!Directory.Exists(TILES_OUT_PATH))
                Directory.CreateDirectory(TILES_OUT_PATH);

            // 2. Configurar Backgrounds
            ProcessBackgrounds();

            // 3. Configurar Decoraciones
            ProcessDecorations();

            // 4. Cortar Tilesheet
            ProcessTilesheet();

            // 5. Generar Tiles
            int tilesCreated = GenerateTiles();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[WoodsImporter] ✅ Proceso completado. {tilesCreated} Tiles generados en {TILES_OUT_PATH}.");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Woods Assets Procesados",
                    $"Se configuraron con éxito:\n" +
                    $"• 7 Fondos en {BACKGROUNDS_PATH}\n" +
                    $"• 16 Decoraciones en {DECORATIONS_PATH}\n" +
                    $"• {tilesCreated} Tiles en {TILES_OUT_PATH}\n\n" +
                    $"¡Listos para construir niveles!", "Aceptar");
            }
        }

        private static void ProcessBackgrounds()
        {
            if (!Directory.Exists(BACKGROUNDS_PATH)) return;

            string[] bgFiles = Directory.GetFiles(BACKGROUNDS_PATH, "*.png");
            foreach (var file in bgFiles)
            {
                string assetPath = file.Replace('\\', '/');
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti == null) continue;

                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.spritePixelsPerUnit = PPU;
                ti.alphaIsTransparency = true;
                ti.wrapMode = TextureWrapMode.Repeat;
                ti.SaveAndReimport();
            }
        }

        private static void ProcessDecorations()
        {
            if (!Directory.Exists(DECORATIONS_PATH)) return;

            string[] decoFiles = Directory.GetFiles(DECORATIONS_PATH, "*.png");
            foreach (var file in decoFiles)
            {
                string assetPath = file.Replace('\\', '/');
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti == null) continue;

                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.spritePixelsPerUnit = PPU;
                ti.alphaIsTransparency = true;

                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                settings.spritePivot = new Vector2(0.5f, 0f);
                ti.SetTextureSettings(settings);

                ti.SaveAndReimport();
            }
        }

        private static void ProcessTilesheet()
        {
            if (!File.Exists(TILESHEET_PATH))
            {
                Debug.LogWarning($"[WoodsImporter] No se encontró el tilesheet en {TILESHEET_PATH}");
                return;
            }

            var ti = AssetImporter.GetAtPath(TILESHEET_PATH) as TextureImporter;
            if (ti == null) return;

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = PPU;
            ti.alphaIsTransparency = true;

            int sheetWidth = 512;
            int sheetHeight = 288;
            int cols = sheetWidth / TILE_SIZE;  // 16
            int rows = sheetHeight / TILE_SIZE; // 9

            var metas = new List<SpriteMetaData>();

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int pixelX = c * TILE_SIZE;
                    int pixelY = (rows - 1 - r) * TILE_SIZE;

                    metas.Add(new SpriteMetaData
                    {
                        name = $"Woods_Tile_{r}_{c}",
                        rect = new Rect(pixelX, pixelY, TILE_SIZE, TILE_SIZE),
                        pivot = new Vector2(0.5f, 0.5f),
                        alignment = (int)SpriteAlignment.Center
                    });
                }
            }

            ti.spritesheet = metas.ToArray();
            ti.SaveAndReimport();
        }

        private static int GenerateTiles()
        {
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(TILESHEET_PATH);
            int count = 0;

            foreach (var asset in allAssets)
            {
                if (asset is Sprite sprite)
                {
                    string tileAssetPath = $"{TILES_OUT_PATH}/{sprite.name}.asset";
                    var tile = AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = sprite;
                        tile.color = Color.white;
                        tile.colliderType = Tile.ColliderType.Grid;
                        AssetDatabase.CreateAsset(tile, tileAssetPath);
                    }
                    else
                    {
                        tile.sprite = sprite;
                        tile.colliderType = Tile.ColliderType.Grid;
                        EditorUtility.SetDirty(tile);
                    }
                    count++;
                }
            }

            return count;
        }
    }
}
