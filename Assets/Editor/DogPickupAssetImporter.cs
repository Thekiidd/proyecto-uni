using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    [InitializeOnLoad]
    public static class DogPickupAssetImporter
    {
        public const float PPU = 32f;

        static DogPickupAssetImporter()
        {
            EditorApplication.delayCall += ProcessAllPickups;
        }

        [MenuItem("Tools/Dogs/Configure Pickups & Checkpoint Sprites")]
        public static void MenuProcess()
        {
            ProcessAllPickups();
            EditorUtility.DisplayDialog("Listo", "Sprites de Huesos, Galletas, Pelotas y Caseta configurados.", "OK");
        }

        public static void ProcessAllPickups()
        {
            SetupBoneSheet("Assets/Sprites/Pickups/Bone_Spin.png", 32, 32, 8);
            SetupSingleSprite("Assets/Sprites/Pickups/DogBiscuit.png");
            SetupSingleSprite("Assets/Sprites/Pickups/TennisBall.png");
            SetupDogHouseSheet("Assets/Sprites/Props/DogHouse_Checkpoint.png", 48, 48, 2);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetupBoneSheet(string path, int frameW, int frameH, int count)
        {
            if (!File.Exists(path)) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var sheet = new List<SpriteMetaData>();
            for (int i = 0; i < count; i++)
            {
                var smd = new SpriteMetaData
                {
                    name = $"Bone_Spin_{i}",
                    rect = new Rect(i * frameW, 0, frameW, frameH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
                sheet.Add(smd);
            }

            importer.spritesheet = sheet.ToArray();
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void SetupDogHouseSheet(string path, int frameW, int frameH, int count)
        {
            if (!File.Exists(path)) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var sheet = new List<SpriteMetaData>();
            string[] names = new[] { "DogHouse_Inactive", "DogHouse_Active" };
            for (int i = 0; i < count; i++)
            {
                var smd = new SpriteMetaData
                {
                    name = names[i],
                    rect = new Rect(i * frameW, 0, frameW, frameH),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0.05f)
                };
                sheet.Add(smd);
            }

            importer.spritesheet = sheet.ToArray();
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }

        private static void SetupSingleSprite(string path)
        {
            if (!File.Exists(path)) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
        }
    }
}
