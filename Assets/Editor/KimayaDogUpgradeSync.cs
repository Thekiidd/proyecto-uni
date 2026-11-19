using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Platformer.Core;
using Platformer.Dialogue;

namespace Kimaya.EditorTools
{
    /// <summary>
    /// Sincroniza integralmente el nuevo elenco de personajes PixelLab en todo el proyecto Unity:
    ///   - Falin: Border Collie (blanco y negro) — 4 direcciones + Jump King Clásico y Caballero
    ///   - Mía: Schnauzer (gris plata y blanco) — 4 direcciones + Jump King Clásico y Caballero
    ///   - Kiara: Schnauzer (color canela/crema cálido con collar magenta) — 4 direcciones + animaciones Idle y Bark
    /// También genera capturas reales de verificación desde las cámaras de las escenas y compila Kimaya.exe.
    /// </summary>
    [InitializeOnLoad]
    public static class KimayaDogUpgradeSync
    {
        private const float FALIN_PIVOT_Y = 26.0f / 92.0f; // 0.2826087f
        private const float MIA_PIVOT_Y   = 19.0f / 68.0f; // 0.2794118f
        private const float KIARA_PIVOT_Y = 18.0f / 68.0f; // 0.2647059f
        private const float MAYA_PIVOT_Y  = 0.0600f;
        private const float UNIFIED_PPU   = 36.0f;
        private const string PIXEL_HUD_SYNC_KEY = "Kimaya_Fairy_MathTower_Spikes_v6";

        static KimayaDogUpgradeSync()
        {
            EditorApplication.delayCall += AutoImportPixelHUDAssets;
        }

        private static void AutoImportPixelHUDAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorPrefs.GetBool(PIXEL_HUD_SYNC_KEY, false)) return;

            ConfigureDirectorySprites("Assets/Resources/UI/PixelHUD", UNIFIED_PPU, 0.5f);
            ConfigureDirectorySprites("Assets/Resources/UI/LoadingDogs", UNIFIED_PPU, 0.5f);
            ConfigureDirectorySprites("Assets/Resources/NPCs/Maya", UNIFIED_PPU, MAYA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/NPCs/Maya", UNIFIED_PPU, MAYA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Resources/TowerMap", UNIFIED_PPU, 0.5f, recursive: false);
            AssetDatabase.ImportAsset("Assets/Resources/UI/PixelButtons", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Sprites/UI/PixelButtons", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/TowerMap/Props", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Sprites/Backgrounds", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/Fonts", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/Music", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.SaveAssets();
            EditorPrefs.SetBool(PIXEL_HUD_SYNC_KEY, true);
            Debug.Log("[KimayaDogUpgradeSync] ✅ Nuevo Mapa Personalizado, Jaula Espinosa, Botones Pixel (Crema/Oro/Negro) y Fuentes Pixel importados automáticamente.");
        }

        private const string CAPTURE_DIR = @"C:\Users\lbati\.gemini\antigravity\brain\65c5e88e-c6fe-404b-acce-8b8e379c8f06\scratch\unity_captures";

        [MenuItem("Tools/Kimaya/🐾 Sincronizar Sprites PixelLab (Solo Escenas y Assets)")]
        public static void SyncOnly()
        {
            Debug.Log("[KimayaDogUpgradeSync] === INICIANDO SINCRONIZACIÓN INTEGRAL DE PERSONAJES ===");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 1. Configurar TextureImporter en todos los sprites nuevos y existentes (incluyendo MainMenu_Background)
            ConfigureAllDogTextureImporters();
            var bgTi = AssetImporter.GetAtPath("Assets/Sprites/UI/MainMenu_Background.png") as TextureImporter;
            if (bgTi != null)
            {
                bgTi.filterMode = FilterMode.Point;
                bgTi.textureCompression = TextureImporterCompression.Uncompressed;
                bgTi.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // 2. Actualizar ScriptableObjects (Prologue_Dialogue.asset y Characters/*.asset)
            PatchPrologueAndCharacterAssets();
            AssetDatabase.SaveAssets();

            // 3. Reconstruir MainMenu y Prologue_Story con los fondos y diálogos actualizados
            MainMenuSceneBuilder.BuildScene();
            PrologueSceneBuilder.BuildScene();

            // 4. Actualizar escenas Hub y Village
            PatchHubLikeScene("Assets/Scenes/Hub.unity", capturePreviews: true);
            PatchHubLikeScene("Assets/Scenes/Village_Scene.unity", capturePreviews: false);

            // 5. Construir AMBOS niveles de ascenso separados en el proyecto Unity:
            //    a) Ascenso_JumpKing_Classic.unity -> Nivel anterior tipo Jump King (Red Crown Woods) para jugarlo en el Editor
            Platformer.EditorTools.KimayaClassicJumpKingBuilder.BuildClassicJumpKingScene("Assets/Scenes/Ascenso_JumpKing_Classic.unity");
            PatchJumpKingScene("Assets/Scenes/Ascenso_JumpKing_Classic.unity", useCustomTower: false, capturePreviews: false);

            //    b) Ascenso_Kimaya.unity -> Nuevo nivel original de Kimaya (Bosque Mágico -> Zarzal Sombrío + Jaula Espinosa), incluido también en Kimaya.exe
            Platformer.EditorTools.KimayaClassicJumpKingBuilder.BuildClassicJumpKingScene("Assets/Scenes/Ascenso_Kimaya.unity");
            PatchJumpKingScene("Assets/Scenes/Ascenso_Kimaya.unity", useCustomTower: true, capturePreviews: true);

            PatchJumpKingScene("Assets/Scenes/Test_Mecanica_Interaccion.unity", useCustomTower: false, capturePreviews: false);

            // 6. Actualizar escena Ending
            PatchEndingScene("Assets/Scenes/Ending.unity");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[KimayaDogUpgradeSync] ✅ Todas las escenas y assets sincronizados exitosamente.");
        }

        [MenuItem("Tools/Kimaya/🐾 Sincronizar Sprites PixelLab (Border Collie + 2 Schnauzers) y Compilar")]
        public static void SyncAndBuildAll()
        {
            SyncOnly();
            KimayaBuildAutomation.BuildStandaloneWindows();
        }

        private static void ConfigureAllDogTextureImporters()
        {
            ConfigureDirectorySprites("Assets/Sprites/PixelLab/Falin", UNIFIED_PPU, FALIN_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/Falin_JK", UNIFIED_PPU, FALIN_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/PixelLab/Mia", UNIFIED_PPU, MIA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/Mia_JK", UNIFIED_PPU, MIA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Resources/NPCs/Kiara", UNIFIED_PPU, KIARA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/NPCs/Kiara", UNIFIED_PPU, KIARA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/PixelLab/Kiara", UNIFIED_PPU, KIARA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Resources/NPCs/Maya", UNIFIED_PPU, MAYA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Sprites/NPCs/Maya", UNIFIED_PPU, MAYA_PIVOT_Y);
            ConfigureDirectorySprites("Assets/Resources/UI/PixelHUD", UNIFIED_PPU, 0.5f);
            ConfigureDirectorySprites("Assets/Resources/TowerMap", UNIFIED_PPU, 0.5f, recursive: false);
            AssetDatabase.ImportAsset("Assets/Resources/UI/PixelButtons", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Sprites/UI/PixelButtons", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/TowerMap/Props", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Sprites/Backgrounds", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/Fonts", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset("Assets/Resources/Music", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceSynchronousImport);
        }

        private static void ConfigureDirectorySprites(string assetFolder, float ppu, float pivotY, bool recursive = true)
        {
            string fullDir = Path.GetFullPath(Path.Combine(Application.dataPath, "../" + assetFolder));
            if (!Directory.Exists(fullDir)) return;

            string[] pngs = Directory.GetFiles(fullDir, "*.png", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            int updated = 0;
            foreach (var f in pngs)
            {
                string rel = "Assets" + f.Substring(Application.dataPath.Length).Replace("\\", "/");
                var ti = AssetImporter.GetAtPath(rel) as TextureImporter;
                if (ti == null)
                {
                    AssetDatabase.ImportAsset(rel, ImportAssetOptions.ForceSynchronousImport);
                    ti = AssetImporter.GetAtPath(rel) as TextureImporter;
                }
                if (ti == null) continue;

                bool dirty = false;
                if (ti.textureType != TextureImporterType.Sprite) { ti.textureType = TextureImporterType.Sprite; dirty = true; }
                if (ti.spriteImportMode != SpriteImportMode.Single) { ti.spriteImportMode = SpriteImportMode.Single; dirty = true; }
                if (ti.filterMode != FilterMode.Point) { ti.filterMode = FilterMode.Point; dirty = true; }
                if (ti.textureCompression != TextureImporterCompression.Uncompressed) { ti.textureCompression = TextureImporterCompression.Uncompressed; dirty = true; }
                if (!ti.alphaIsTransparency) { ti.alphaIsTransparency = true; dirty = true; }
                if (Mathf.Abs(ti.spritePixelsToUnits - ppu) > 0.01f) { ti.spritePixelsToUnits = ppu; dirty = true; }

                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                if (settings.spriteAlignment != (int)SpriteAlignment.Custom ||
                    Mathf.Abs(settings.spritePivot.x - 0.5f) > 0.001f ||
                    Mathf.Abs(settings.spritePivot.y - pivotY) > 0.001f)
                {
                    settings.spriteAlignment = (int)SpriteAlignment.Custom;
                    settings.spritePivot = new Vector2(0.5f, pivotY);
                    ti.SetTextureSettings(settings);
                    dirty = true;
                }

                if (dirty)
                {
                    ti.SaveAndReimport();
                    updated++;
                }
            }
            Debug.Log($"[KimayaDogUpgradeSync] Carpeta {assetFolder}: {pngs.Length} PNGs verificados ({updated} reimportados).");
        }

        private static Sprite LoadSpr(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj is Sprite sp) return sp;
            }
            return null;
        }

        private static Sprite[] LoadAnimFrames(string folder, string prefix, int count, int digits = 3)
        {
            var list = new List<Sprite>();
            for (int i = 0; i < count; i++)
            {
                string num = digits == 3 ? i.ToString("D3") : i.ToString();
                string p = $"{folder}/{prefix}{num}.png";
                var sp = LoadSpr(p);
                if (sp != null) list.Add(sp);
            }
            return list.ToArray();
        }

        private static DogSpriteSet BuildHubSpriteSet(string dogName, string basePath)
        {
            var set = new DogSpriteSet();
            set.dogName = dogName;
            set.defaultPortrait = LoadSpr($"{basePath}/Idle/rotations/south.png");

            set.idleSouth = LoadAnimFrames($"{basePath}/Idle/animations/idle/south", "frame_", 8, 3);
            set.idleNorth = LoadAnimFrames($"{basePath}/Idle/animations/idle/north", "frame_", 8, 3);
            set.idleEast  = LoadAnimFrames($"{basePath}/Idle/animations/idle/east",  "frame_", 8, 3);
            set.idleWest  = LoadAnimFrames($"{basePath}/Idle/animations/idle/west",  "frame_", 8, 3);

            set.walkSouth = LoadAnimFrames($"{basePath}/Idle/animations/walking/south", "frame_", 8, 3);
            set.walkNorth = LoadAnimFrames($"{basePath}/Idle/animations/walking/north", "frame_", 8, 3);
            set.walkEast  = LoadAnimFrames($"{basePath}/Idle/animations/walking/east",  "frame_", 8, 3);
            set.walkWest  = LoadAnimFrames($"{basePath}/Idle/animations/walking/west",  "frame_", 8, 3);

            set.runSouth  = LoadAnimFrames($"{basePath}/Idle/animations/running/south", "frame_", 8, 3);
            set.runNorth  = LoadAnimFrames($"{basePath}/Idle/animations/running/north", "frame_", 8, 3);
            set.runEast   = LoadAnimFrames($"{basePath}/Idle/animations/running/east",  "frame_", 8, 3);
            set.runWest   = LoadAnimFrames($"{basePath}/Idle/animations/running/west",  "frame_", 8, 3);

            set.powerSouth = LoadAnimFrames($"{basePath}/Idle/animations/barking/south", "frame_", 6, 3);
            set.powerNorth = LoadAnimFrames($"{basePath}/Idle/animations/barking/north", "frame_", 6, 3);
            set.powerEast  = LoadAnimFrames($"{basePath}/Idle/animations/barking/east",  "frame_", 6, 3);
            set.powerWest  = LoadAnimFrames($"{basePath}/Idle/animations/barking/west",  "frame_", 6, 3);

            return set;
        }

        private static void PatchPrologueAndCharacterAssets()
        {
            Sprite falinSouth = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/south.png");
            Sprite falinEast  = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png");
            Sprite miaSouth   = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/south.png");
            Sprite miaEast    = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png");
            Sprite kiaraSouth = LoadSpr("Assets/Resources/NPCs/Kiara/Kiara_south.png");
            Sprite mayaSpr    = LoadSpr("Assets/Resources/NPCs/Maya/Maya_Fairy.png");

            // Actualizar Prologue_Dialogue.asset
            var prologue = AssetDatabase.LoadAssetAtPath<StoryDialogueData>("Assets/Story/Prologue_Dialogue.asset");
            if (prologue != null)
            {
                prologue.dialogueTitle = "Prólogo: El Bosque del Eco";
                prologue.nextSceneOnComplete = "Hub";
                prologue.lines = new StoryLine[]
                {
                    new StoryLine
                    {
                        speakerName = "Falin (Border Collie)",
                        speakerPortrait = falinSouth,
                        text = "¡Qué mañana tan increíble para correr junto al bosque! ¡Miren qué lejos rodó la pelota, yo llegaré primero!",
                        typingSpeed = 0.025f
                    },
                    new StoryLine
                    {
                        speakerName = "Mía (Schnauzer)",
                        speakerPortrait = miaSouth,
                        text = "¡Por aquí, Falin! Mi olfato detecta el rastro entre ese círculo de hongos brillantes junto al tronco antiguo.",
                        typingSpeed = 0.025f
                    },
                    new StoryLine
                    {
                        speakerName = "Kiara (Schnauzer)",
                        speakerPortrait = kiaraSouth,
                        text = "¡Esperen, hermanas! ¡No se alejen tanto, ese claro desprende una luz mágica muy extraña!",
                        typingSpeed = 0.025f
                    },
                    new StoryLine
                    {
                        speakerName = "Narrador",
                        speakerPortrait = null,
                        text = "Al cruzar el círculo místico, despertaron en el Bosque del Eco. Las raíces del Zarzal Sombrío emergieron, y Kiara empujó a sus hermanas para salvarlas, quedando atrapada en la cumbre.",
                        typingSpeed = 0.025f
                    },
                    new StoryLine
                    {
                        speakerName = "Hada Maya",
                        speakerPortrait = mayaSpr != null ? mayaSpr : kiaraSouth,
                        text = "Bienvenidas al Bosque del Eco. Para liberar a su hermana Kiara y restaurar el Círculo Rúnico, deberán ascender por el Sendero de las Raíces.",
                        typingSpeed = 0.025f
                    },
                    new StoryLine
                    {
                        speakerName = "Falin y Mía",
                        speakerPortrait = falinEast,
                        text = "¡Resiste, Kiara! ¡Ninguna montaña detendrá a tres hermanas unidas!",
                        typingSpeed = 0.025f
                    }
                };
                EditorUtility.SetDirty(prologue);
                Debug.Log("[KimayaDogUpgradeSync] Prologue_Dialogue.asset actualizado con Falin (Border Collie), Mía y Kiara (Schnauzers).");
            }

            // Actualizar Characters/Tank.asset (San Bernardo -> Border Collie Falin)
            var tankAsset = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Tank.asset");
            if (tankAsset != null)
            {
                tankAsset.characterName = "Falin (Border Collie)";
                tankAsset.description = "Ágil, firme y leal Border Collie. Impulso potente y gran estabilidad.";
                if (falinSouth != null) tankAsset.portrait = falinSouth;
                if (falinEast != null) tankAsset.idleSprite = falinEast;
                EditorUtility.SetDirty(tankAsset);
            }

            var schnauzerAsset = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Schnauzer.asset");
            if (schnauzerAsset != null)
            {
                schnauzerAsset.characterName = "Mía (Schnauzer)";
                schnauzerAsset.description = "Valiente y ágil Schnauzer. Salto ligero y olfato agudo.";
                if (miaSouth != null) schnauzerAsset.portrait = miaSouth;
                if (miaEast != null) schnauzerAsset.idleSprite = miaEast;
                EditorUtility.SetDirty(schnauzerAsset);
            }
        }

        private static void PatchHubLikeScene(string scenePath, bool capturePreviews)
        {
            if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "../" + scenePath)))) return;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var duo = UnityEngine.Object.FindAnyObjectByType<KimayaHubDuoController>();

            GameObject falinGo = GameObject.Find("Falin_BorderCollie") ?? GameObject.Find("Falin_SanBernardo");
            GameObject miaGo   = GameObject.Find("Mia_Schnauzer");

            Sprite falinSouth = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/south.png");
            Sprite falinEast  = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png");
            Sprite miaSouth   = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/south.png");
            Sprite miaEast    = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png");
            Sprite kiaraSouth = LoadSpr("Assets/Resources/NPCs/Kiara/Kiara_south.png");

            if (falinGo != null)
            {
                falinGo.name = "Falin_BorderCollie";
                falinGo.transform.localScale = new Vector3(0.38f, 0.38f, 1.0f);
                var sr = falinGo.GetComponent<SpriteRenderer>();
                if (sr != null && falinSouth != null)
                {
                    sr.sprite = falinSouth;
                    sr.color = Color.white;
                }
                var bob = falinGo.GetComponentInChildren<KimayaDiamondBobber>(true);
                if (bob != null)
                {
                    bob.baseY = 2.05f;
                    bob.targetScale = 0.27f;
                    bob.transform.localPosition = new Vector3(0f, 2.05f, 0f);
                    bob.transform.localScale = new Vector3(0.27f, 0.27f, 1f);
                    var dsr = bob.GetComponent<SpriteRenderer>();
                    if (dsr != null) dsr.sortingOrder = 950;
                    EditorUtility.SetDirty(bob);
                }
                EditorUtility.SetDirty(falinGo);
            }

            if (miaGo != null)
            {
                miaGo.transform.localScale = new Vector3(0.44f, 0.44f, 1.0f);
                var sr = miaGo.GetComponent<SpriteRenderer>();
                if (sr != null && miaSouth != null)
                {
                    sr.sprite = miaSouth;
                    sr.color = Color.white;
                }
                var bob = miaGo.GetComponentInChildren<KimayaDiamondBobber>(true);
                if (bob != null)
                {
                    bob.baseY = 1.52f;
                    bob.targetScale = 0.24f;
                    bob.transform.localPosition = new Vector3(0f, 1.52f, 0f);
                    bob.transform.localScale = new Vector3(0.24f, 0.24f, 1f);
                    var dsr = bob.GetComponent<SpriteRenderer>();
                    if (dsr != null) dsr.sortingOrder = 950;
                    EditorUtility.SetDirty(bob);
                }
                EditorUtility.SetDirty(miaGo);
            }

            if (duo != null)
            {
                duo.falinObject = falinGo;
                duo.miaObject = miaGo;
                duo.falinSprites = BuildHubSpriteSet("Falin", "Assets/Sprites/PixelLab/Falin");
                duo.miaSprites = BuildHubSpriteSet("Mia", "Assets/Sprites/PixelLab/Mia");
                EditorUtility.SetDirty(duo);
            }

            Sprite mayaSpr = LoadSpr("Assets/Resources/NPCs/Maya/Hada_Idle_0.png") ?? LoadSpr("Assets/Resources/NPCs/Maya/Maya_Fairy.png");
            GameObject fairyGo = GameObject.Find("Hada_Historia_Maya");
            if (fairyGo == null)
            {
                fairyGo = new GameObject("Hada_Historia_Maya");
            }
            fairyGo.transform.position = new Vector3(-1.48f, 0.08f, 0f);
            fairyGo.transform.localScale = new Vector3(0.24f, 0.24f, 1.0f);
            var fairySr = fairyGo.GetComponent<SpriteRenderer>();
            if (fairySr == null) fairySr = fairyGo.AddComponent<SpriteRenderer>();
            if (mayaSpr != null) fairySr.sprite = mayaSpr;
            fairySr.sortingOrder = 508;
            fairySr.color = Color.white;
            EditorUtility.SetDirty(fairyGo);

            var runic = UnityEngine.Object.FindAnyObjectByType<KimayaHubRunicCircle>();
            if (runic != null)
            {
                if (kiaraSouth != null) runic.kiaraSprite = kiaraSouth;
                runic.fairyObject = fairyGo;
                runic.fairySprite = mayaSpr;
                runic.fairyHubPosition = new Vector3(-1.48f, 0.08f, 0f);
                runic.fairyHubScale = new Vector3(0.24f, 0.24f, 1.0f);
                EditorUtility.SetDirty(runic);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[KimayaDogUpgradeSync] Escena actualizada: {scenePath}");

            if (capturePreviews && falinGo != null && miaGo != null)
            {
                Directory.CreateDirectory(CAPTURE_DIR);
                var cam = Camera.main;
                if (cam != null)
                {
                    float origOrtho = cam.orthographicSize;
                    Vector3 origCamPos = cam.transform.position;

                    // Crear Kiara temporal para verificar las 3 hermanas juntas en el círculo rúnico
                    var tempKiara = new GameObject("TempKiaraPreview");
                    tempKiara.transform.position = new Vector3(0f, -0.85f, 0f);
                    tempKiara.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
                    var ksr = tempKiara.AddComponent<SpriteRenderer>();
                    ksr.sprite = kiaraSouth;
                    ksr.sortingOrder = 510;

                    var fBob = falinGo.GetComponentInChildren<KimayaDiamondBobber>(true);
                    var mBob = miaGo.GetComponentInChildren<KimayaDiamondBobber>(true);
                    if (fBob != null) fBob.gameObject.SetActive(true);
                    if (mBob != null) mBob.gameObject.SetActive(true);

                    // Zoom cercano al círculo rúnico con las 3 perritas mirando al SUR
                    cam.transform.position = new Vector3(0f, -0.75f, -10f);
                    cam.orthographicSize = 2.1f;
                    RenderCameraToPng(cam, Path.Combine(CAPTURE_DIR, "Hub_CloseUp_South_3Sisters.png"), 1280, 720);

                    // Ahora mirando al ESTE
                    falinGo.GetComponent<SpriteRenderer>().sprite = falinEast;
                    miaGo.GetComponent<SpriteRenderer>().sprite = miaEast;
                    ksr.sprite = LoadSpr("Assets/Resources/NPCs/Kiara/Kiara_east.png");
                    RenderCameraToPng(cam, Path.Combine(CAPTURE_DIR, "Hub_CloseUp_East_3Sisters.png"), 1280, 720);

                    // Restaurar estado limpio de la escena y guardar
                    UnityEngine.Object.DestroyImmediate(tempKiara);
                    falinGo.GetComponent<SpriteRenderer>().sprite = falinSouth;
                    miaGo.GetComponent<SpriteRenderer>().sprite = miaSouth;
                    if (mBob != null) mBob.gameObject.SetActive(false); // Solo Falin activa al inicio
                    cam.transform.position = origCamPos;
                    cam.orthographicSize = origOrtho;

                    RenderCameraToPng(cam, Path.Combine(CAPTURE_DIR, "Hub_FullView.png"), 1280, 720);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private static void PatchJumpKingScene(string scenePath, bool useCustomTower, bool capturePreviews)
        {
            if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "../" + scenePath)))) return;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Sprite falinClassicEast = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/east.png");
            Sprite miaClassicEast   = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/east.png");
            Sprite falinKnightEast  = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/east.png");
            Sprite miaKnightEast    = LoadSpr("Assets/Sprites/PixelLab/Mia/JumpKingKnight/east.png");
            Sprite kiaraSouth       = LoadSpr("Assets/Resources/NPCs/Kiara/Kiara_south.png");

            var manager = UnityEngine.Object.FindAnyObjectByType<KimayaJumpKingManager>();
            if (manager != null)
            {
                manager.useCustomKimayaTower = useCustomTower;
                manager.falinSprite = falinClassicEast != null ? falinClassicEast : falinKnightEast;
                manager.miaSprite   = miaClassicEast != null ? miaClassicEast : miaKnightEast;
                EditorUtility.SetDirty(manager);
            }

            var anim = UnityEngine.Object.FindAnyObjectByType<KimayaJumpKingAnimator>();
            if (anim != null)
            {
                anim.useKnightSkin = false;

                // Falin Classic (Border Collie natural)
                anim.falinClassicIdleFrames = LoadAnimFrames("Assets/Sprites/PixelLab/Falin/Idle/animations/idle/east", "frame_", 8, 3);
                anim.falinClassicWalkFrames = LoadAnimFrames("Assets/Sprites/PixelLab/Falin/Idle/animations/walking/east", "frame_", 8, 3);
                anim.falinClassicChargeProgress = new Sprite[]
                {
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/charge_0.png"),
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/charge_1.png"),
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/charge_2.png")
                };
                anim.falinClassicJumpStretch = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/jump_stretch.png");
                anim.falinClassicFall        = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/fall.png");
                anim.falinClassicSplat       = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKing/splat.png");

                // Falin Knight (Border Collie con casco y capa roja)
                anim.falinKnightIdle = falinKnightEast;
                anim.falinKnightWalkFrames = LoadAnimFrames("Assets/Sprites/PixelLab/Falin/JumpKingKnight/walk", "frame_", 8, 1);
                anim.falinKnightChargeProgress = new Sprite[]
                {
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/charge_0.png"),
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/charge_1.png"),
                    LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/charge_2.png")
                };
                anim.falinKnightJumpStretch = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/jump_stretch.png");
                anim.falinKnightFall        = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/fall.png");
                anim.falinKnightSplat       = LoadSpr("Assets/Sprites/PixelLab/Falin/JumpKingKnight/tactical/splat.png");

                // Compatibilidad histórica
                anim.falinIdleFrames = anim.falinClassicIdleFrames;
                anim.falinWalkFrames = anim.falinClassicWalkFrames;
                anim.falinChargeProgress = anim.falinClassicChargeProgress;
                anim.falinJumpStretch = anim.falinClassicJumpStretch;
                anim.falinFall = anim.falinClassicFall;
                anim.falinSplat = anim.falinClassicSplat;

                var sr = anim.GetComponent<SpriteRenderer>();
                if (sr != null && falinClassicEast != null)
                {
                    sr.sprite = falinClassicEast;
                    sr.color = Color.white;
                }
                anim.transform.localScale = new Vector3(0.35f, 0.35f, 1.0f);
                if (useCustomTower)
                {
                    anim.transform.position = KimayaCustomTowerMap.PlayerSpawnPosition;
                }
                EditorUtility.SetDirty(anim);
                EditorUtility.SetDirty(anim.gameObject);
            }

            // Actualizar Rescued_Companion en la cumbre para mostrar a Kiara Schnauzer
            var rescued = GameObject.Find("Rescued_Companion");
            if (rescued != null && kiaraSouth != null)
            {
                var rsr = rescued.GetComponent<SpriteRenderer>();
                if (rsr != null)
                {
                    rsr.sprite = kiaraSouth;
                    rsr.color = Color.white;
                    EditorUtility.SetDirty(rsr);
                }
            }

            // Aplicar Mapa Personalizado de Kimaya SOLO en Ascenso_Kimaya.unity; conservar intacto Ascenso_JumpKing_Classic.unity
            if (useCustomTower)
            {
                KimayaCustomTowerMap.ApplyCustomTowerToCurrentScene(forceApply: true);
            }
            else
            {
                var altar = UnityEngine.Object.FindAnyObjectByType<KimayaAltarGoal>();
                if (altar != null)
                {
                    altar.useThornyCage = false;
                    altar.EnsureCageAndKiaraVisuals();
                    EditorUtility.SetDirty(altar);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[KimayaDogUpgradeSync] Escena Jump King actualizada: {scenePath}");

            if (capturePreviews)
            {
                Directory.CreateDirectory(CAPTURE_DIR);
                var cam = Camera.main;
                if (cam != null)
                {
                    RenderCameraToPng(cam, Path.Combine(CAPTURE_DIR, "JumpKing_Floor1_Falin.png"), 1280, 720);
                }
            }
        }

        private static void PatchEndingScene(string scenePath)
        {
            if (!File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "../" + scenePath)))) return;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            Sprite falinSouth = LoadSpr("Assets/Sprites/PixelLab/Falin/Idle/rotations/south.png");
            Sprite miaSouth   = LoadSpr("Assets/Sprites/PixelLab/Mia/Idle/rotations/south.png");
            Sprite kiaraSouth = LoadSpr("Assets/Resources/NPCs/Kiara/Kiara_south.png");

            UpdateUIImage("Falin_Left", falinSouth, new Vector2(185f, 185f));
            UpdateUIImage("Mia_Right", miaSouth, new Vector2(150f, 150f));
            UpdateUIImage("Kiara_Center", kiaraSouth, new Vector2(155f, 155f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[KimayaDogUpgradeSync] Escena Ending actualizada: {scenePath}");
        }

        private static void UpdateUIImage(string goName, Sprite spr, Vector2 targetSize)
        {
            var go = GameObject.Find(goName);
            if (go == null || spr == null) return;
            var img = go.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = spr;
                img.color = Color.white;
                img.preserveAspect = true;
                EditorUtility.SetDirty(img);
            }
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.sizeDelta = targetSize;
                EditorUtility.SetDirty(rt);
            }
        }

        private static void RenderCameraToPng(Camera cam, string outPath, int width, int height)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var prevTarget = cam.targetTexture;
            var prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] png = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);
            File.WriteAllBytes(outPath, png);
            Debug.Log($"[KimayaDogUpgradeSync] 📸 Captura guardada: {outPath}");
        }
    }
}
