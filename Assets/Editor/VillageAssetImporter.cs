using System.IO;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class VillageAssetImporter
    {
        public const float PPU = 16f; // 16 píxeles por unidad para The Fan-tasy Tileset
        public const string VILLAGE_FOLDER = "Assets/Sprites/Village";

        static VillageAssetImporter()
        {
            EditorApplication.delayCall += AutoProcessIfMissing;
        }

        private static void AutoProcessIfMissing()
        {
            string testAsset = $"{VILLAGE_FOLDER}/Tiled/Tilemaps/Beginning Fields.png";
            if (File.Exists(testAsset))
            {
                var ti = AssetImporter.GetAtPath(testAsset) as TextureImporter;
                if (ti != null && (ti.spritePixelsPerUnit != PPU || ti.filterMode != FilterMode.Point))
                {
                    ProcessVillageAssets(false);
                }
            }
        }

        [MenuItem("Tools/Village/🎨 Configurar Sprites del Pueblo (PPU 16)")]
        public static void MenuProcessAll()
        {
            ProcessVillageAssets(true);
        }

        public static void ProcessVillageAssets(bool showDialog)
        {
            if (!Directory.Exists(VILLAGE_FOLDER)) return;

            string[] pngFiles = Directory.GetFiles(VILLAGE_FOLDER, "*.png", SearchOption.AllDirectories);
            int count = 0;

            foreach (var file in pngFiles)
            {
                string assetPath = file.Replace('\\', '/');
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti == null) continue;

                ti.textureType = TextureImporterType.Sprite;
                ti.filterMode = FilterMode.Point;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.spritePixelsPerUnit = PPU;
                ti.alphaIsTransparency = true;

                // Si es el mapa base o un edificio/prop, modo Single con pivot inferior o central
                if (assetPath.Contains("Characters"))
                {
                    // Personaje: cortar en frames si es Character_Idle o Character_Walk
                    ti.spriteImportMode = SpriteImportMode.Multiple;
                    SliceCharacter(ti, assetPath);
                }
                else
                {
                    ti.spriteImportMode = SpriteImportMode.Single;
                    var settings = new TextureImporterSettings();
                    ti.ReadTextureSettings(settings);
                    if (assetPath.Contains("Buildings") || assetPath.Contains("Props") || assetPath.Contains("Trees"))
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
                        settings.spritePivot = new Vector2(0.5f, 0f);
                    }
                    else
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.Center;
                        settings.spritePivot = new Vector2(0.5f, 0.5f);
                    }
                    ti.SetTextureSettings(settings);
                }

                ti.SaveAndReimport();
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[VillageImporter] ✅ {count} sprites de Village configurados a PPU={PPU}, Point Filter, Sin compresión.");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Sprites del Pueblo Configurados",
                    $"Se procesaron {count} texturas de Village con éxito.\n" +
                    $"• Modo Pixel Art nítido (Point, No Compression)\n" +
                    $"• PPU exacto: {PPU}\n" +
                    $"• Pivots alineados para orden de profundidad Top-Down.",
                    "Aceptar");
            }
        }

        private static void SliceCharacter(TextureImporter ti, string path)
        {
            // Character_Idle.png y Character_Walk.png son 160x192 (5 columnas x 4 filas = frames de 32x48)
            int frameW = 32;
            int frameH = 48;
            int cols = 5;
            int rows = 4;

            var metas = new System.Collections.Generic.List<SpriteMetaData>();
            string fileName = Path.GetFileNameWithoutExtension(path);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int px = c * frameW;
                    int py = (rows - 1 - r) * frameH;

                    metas.Add(new SpriteMetaData
                    {
                        name = $"{fileName}_{r}_{c}",
                        rect = new Rect(px, py, frameW, frameH),
                        pivot = new Vector2(0.5f, 0f), // Pies del personaje
                        alignment = (int)SpriteAlignment.BottomCenter
                    });
                }
            }

            ti.spritesheet = metas.ToArray();
        }
    }
}
