using System.Collections.Generic;
using System.IO;
using Platformer.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Platformer.EditorScripts
{
    /// <summary>
    /// Tools > Setup All Dogs RPG (4 Directions)
    /// Configura los spritesheets RPG, animaciones 4-direccionales y CharacterData
    /// para todos los perros existentes del juego.
    /// </summary>
    public static class AllDogsRPGSetup
    {
        private class BreedConfig
        {
            public string breedName;
            public string prefix;
            public string spriteSheetPath;
            public string animFolder;
            public string controllerPath;
            public string characterAssetPath;

            public BreedConfig(string name, string pfx, string sheetPath, string animDir, string ctrlPath, string charPath)
            {
                breedName = name;
                prefix = pfx;
                spriteSheetPath = sheetPath;
                animFolder = animDir;
                controllerPath = ctrlPath;
                characterAssetPath = charPath;
            }
        }

        private static readonly BreedConfig[] Breeds = new[]
        {
            new BreedConfig(
                "Golden Retriever", "Golden-Retriever",
                "Assets/Sprites/Dogs/Golden-Retriever/Golden-Retriever-RPG-Sheet.png",
                "Assets/Animations/Dogs/Golden-Retriever",
                "Assets/Animations/Dogs/Golden-Retriever/Golden-Retriever-controller.controller",
                "Assets/Characters/Runner.asset"
            ),
            new BreedConfig(
                "Akita", "Akita",
                "Assets/Sprites/Dogs/Akita/Akita-RPG-Sheet.png",
                "Assets/Animations/Dogs/Akita",
                "Assets/Animations/Dogs/Akita/Akita-controller.controller",
                "Assets/Characters/Jumper.asset"
            ),
            new BreedConfig(
                "San Bernardo", "Saint-Bernard",
                "Assets/Sprites/Dogs/Saint-Bernard/Saint-Bernard-RPG-Sheet.png",
                "Assets/Animations/Dogs/Saint-Bernard",
                "Assets/Animations/Dogs/Saint-Bernard/Saint-Bernard-controller.controller",
                "Assets/Characters/Tank.asset"
            ),
            new BreedConfig(
                "Gran Danes", "Great-Dane",
                "Assets/Sprites/Dogs/Great-Dane/Great-Dane-RPG-Sheet.png",
                "Assets/Animations/Dogs/Great-Dane",
                "Assets/Animations/Dogs/Great-Dane/Great-Dane-controller.controller",
                "Assets/Characters/GreatDane.asset"
            ),
            new BreedConfig(
                "Schnauzer", "Schnauzer",
                "Assets/Sprites/Dogs/Schnauzer/Schnauzer-RPG-Sheet.png",
                "Assets/Animations/Dogs/Schnauzer",
                "Assets/Animations/Dogs/Schnauzer/Schnauzer-controller.controller",
                "Assets/Characters/Schnauzer.asset"
            ),
            new BreedConfig(
                "Husky Siberiano", "Siberian-Husky",
                "Assets/Sprites/Dogs/Siberian-Husky/Siberian-Husky-RPG-Sheet.png",
                "Assets/Animations/Dogs/Siberian-Husky",
                "Assets/Animations/Dogs/Siberian-Husky/Siberian-Husky-controller.controller",
                "Assets/Characters/Husky.asset"
            )
        };

        [InitializeOnLoadMethod]
        private static void AutoRunIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                bool needsSetup = false;
                foreach (var b in Breeds)
                {
                    if (!File.Exists($"{b.animFolder}/{b.prefix}_RPG_Idle_Down.anim"))
                    {
                        needsSetup = true;
                        break;
                    }
                }

                if (needsSetup)
                {
                    Debug.Log("[AllDogsRPG] Auto-configurando todos los perros RPG...");
                    SetupAll();
                }
            };
        }

        [MenuItem("Tools/Setup All Dogs RPG (4 Directions)")]
        public static void SetupAll()
        {
            Debug.Log("[AllDogsRPG] Iniciando configuracion RPG 4-direcciones para los 6 perros...");

            foreach (var b in Breeds)
            {
                SetupBreed(b);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AllDogsRPG] Completada la adaptacion de todos los perros a RPG 4-direcciones.");
        }

        private static void SetupBreed(BreedConfig b)
        {
            if (!File.Exists(b.spriteSheetPath))
            {
                Debug.LogWarning($"[AllDogsRPG] No existe {b.spriteSheetPath}, saltando...");
                return;
            }

            // 1. Slice sprite sheet
            SliceSheet(b.spriteSheetPath, b.prefix);

            // 2. Load sprites dict
            var dict = LoadSpritesDict(b.spriteSheetPath);
            if (dict.Count == 0)
            {
                Debug.LogError($"[AllDogsRPG] No se pudieron cargar sprites de {b.spriteSheetPath}");
                return;
            }

            // 3. Ensure folders
            if (!AssetDatabase.IsValidFolder("Assets/Animations"))
                AssetDatabase.CreateFolder("Assets", "Animations");
            if (!AssetDatabase.IsValidFolder("Assets/Animations/Dogs"))
                AssetDatabase.CreateFolder("Assets/Animations", "Dogs");
            if (!AssetDatabase.IsValidFolder(b.animFolder))
                AssetDatabase.CreateFolder("Assets/Animations/Dogs", Path.GetFileName(b.animFolder));

            // 4. Create Animation Clips
            var clipDownIdle = CreateSingleFrameClip(dict, $"{b.prefix}_Down_Idle", $"{b.animFolder}/{b.prefix}_RPG_Idle_Down.anim");
            var clipUpIdle   = CreateSingleFrameClip(dict, $"{b.prefix}_Up_Idle",   $"{b.animFolder}/{b.prefix}_RPG_Idle_Up.anim");
            var clipRightIdle= CreateSingleFrameClip(dict, $"{b.prefix}_Right_Idle",$"{b.animFolder}/{b.prefix}_RPG_Idle_Right.anim");
            var clipLeftIdle = CreateSingleFrameClip(dict, $"{b.prefix}_Left_Idle", $"{b.animFolder}/{b.prefix}_RPG_Idle_Left.anim");

            var clipDownWalk = CreateWalkClip(dict, b.prefix, "Down",  $"{b.animFolder}/{b.prefix}_RPG_Walk_Down.anim");
            var clipUpWalk   = CreateWalkClip(dict, b.prefix, "Up",    $"{b.animFolder}/{b.prefix}_RPG_Walk_Up.anim");
            var clipRightWalk= CreateWalkClip(dict, b.prefix, "Right", $"{b.animFolder}/{b.prefix}_RPG_Walk_Right.anim");
            var clipLeftWalk = CreateWalkClip(dict, b.prefix, "Left",  $"{b.animFolder}/{b.prefix}_RPG_Walk_Left.anim");

            var clipSleep    = CreateSleepClip(dict, b.prefix, $"{b.animFolder}/{b.prefix}_RPG_Sleep.anim");

            // 5. Create / Update AnimatorController with 2D Blend Trees
            var controller = CreateController(
                b.controllerPath,
                clipDownIdle, clipUpIdle, clipRightIdle, clipLeftIdle,
                clipDownWalk, clipUpWalk, clipRightWalk, clipLeftWalk,
                clipSleep
            );

            // 6. Update CharacterData asset
            UpdateCharacterData(b, dict, controller);

            Debug.Log($"[AllDogsRPG] Listo: {b.breedName}");
        }

        private static void SliceSheet(string path, string prefix)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return;

            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Multiple;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.spritePixelsPerUnit = 16f;

            var metas = new List<SpriteMetaData>();
            string[] rowNames = { "Sleep", "Up", "Right", "Left", "Down" };

            for (int r = 0; r < 5; r++)
            {
                string dir = rowNames[r];
                int y = r * 32;

                for (int c = 0; c < 3; c++)
                {
                    int x = c * 32;
                    string spriteName;

                    if (dir == "Sleep")
                    {
                        spriteName = $"{prefix}_Sleep_{c}";
                    }
                    else
                    {
                        if (c == 0) spriteName = $"{prefix}_{dir}_0";
                        else if (c == 1) spriteName = $"{prefix}_{dir}_Idle";
                        else spriteName = $"{prefix}_{dir}_1";
                    }

                    metas.Add(new SpriteMetaData
                    {
                        name = spriteName,
                        rect = new Rect(x, y, 32, 32),
                        pivot = new Vector2(0.5f, 0.05f),
                        alignment = (int)SpriteAlignment.Custom
                    });
                }
            }

            ti.spritesheet = metas.ToArray();
            ti.SaveAndReimport();
        }

        private static Dictionary<string, Sprite> LoadSpritesDict(string path)
        {
            var dict = new Dictionary<string, Sprite>();
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
            {
                if (a is Sprite s)
                {
                    dict[s.name] = s;
                }
            }
            return dict;
        }

        private static AnimationClip CreateSingleFrameClip(Dictionary<string, Sprite> dict, string spriteName, string savePath)
        {
            if (!dict.TryGetValue(spriteName, out var sprite))
            {
                Debug.LogWarning($"[AllDogsRPG] Sprite no encontrado: {spriteName}");
                return null;
            }

            var clip = new AnimationClip { frameRate = 1f };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new[]
            {
                new ObjectReferenceKeyframe { time = 0f, value = sprite }
            };

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                return existing;
            }
            AssetDatabase.CreateAsset(clip, savePath);
            return clip;
        }

        private static AnimationClip CreateWalkClip(Dictionary<string, Sprite> dict, string prefix, string dir, string savePath)
        {
            dict.TryGetValue($"{prefix}_{dir}_0", out var s0);
            dict.TryGetValue($"{prefix}_{dir}_Idle", out var sIdle);
            dict.TryGetValue($"{prefix}_{dir}_1", out var s1);

            if (s0 == null || sIdle == null || s1 == null)
            {
                Debug.LogWarning($"[AllDogsRPG] Faltan sprites para walk {dir} de {prefix}");
                return null;
            }

            var sprites = new[] { s0, sIdle, s1, sIdle };
            float fps = 8f;

            var clip = new AnimationClip { frameRate = fps };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                return existing;
            }
            AssetDatabase.CreateAsset(clip, savePath);
            return clip;
        }

        private static AnimationClip CreateSleepClip(Dictionary<string, Sprite> dict, string prefix, string savePath)
        {
            dict.TryGetValue($"{prefix}_Sleep_0", out var s0);
            dict.TryGetValue($"{prefix}_Sleep_1", out var s1);

            if (s0 == null || s1 == null)
            {
                Debug.LogWarning($"[AllDogsRPG] Faltan sprites para sleep de {prefix}");
                return null;
            }

            var sprites = new[] { s0, s1 };
            float fps = 2f;

            var clip = new AnimationClip { frameRate = fps };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            var keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                return existing;
            }
            AssetDatabase.CreateAsset(clip, savePath);
            return clip;
        }

        private static AnimatorController CreateController(
            string path,
            AnimationClip idleDown, AnimationClip idleUp, AnimationClip idleRight, AnimationClip idleLeft,
            AnimationClip walkDown, AnimationClip walkUp, AnimationClip walkRight, AnimationClip walkLeft,
            AnimationClip sleep)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            var controller = existing != null ? existing : AnimatorController.CreateAnimatorControllerAtPath(path);

            controller.parameters = new AnimatorControllerParameter[0];
            controller.AddParameter("moveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("moveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("lastMoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("lastMoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("isMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("velocityX", AnimatorControllerParameterType.Float);
            controller.AddParameter("grounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("dead", AnimatorControllerParameterType.Bool);
            controller.AddParameter("hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("sleep", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("lay_down", AnimatorControllerParameterType.Trigger);

            var root = controller.layers[0].stateMachine;

            while (root.states.Length > 0)
            {
                root.RemoveState(root.states[0].state);
            }

            // 1. Estado Idle con 2D BlendTree (lastMoveX, lastMoveY)
            var stateIdle = root.AddState("Idle");
            var idleTree = new BlendTree
            {
                name = "Idle_Tree",
                blendType = BlendTreeType.SimpleDirectional2D,
                blendParameter = "lastMoveX",
                blendParameterY = "lastMoveY"
            };
            AssetDatabase.AddObjectToAsset(idleTree, controller);

            idleTree.AddChild(idleDown,  new Vector2(0f, -1f));
            idleTree.AddChild(idleUp,    new Vector2(0f,  1f));
            idleTree.AddChild(idleRight, new Vector2(1f,  0f));
            idleTree.AddChild(idleLeft,  new Vector2(-1f, 0f));
            stateIdle.motion = idleTree;
            root.defaultState = stateIdle;

            // 2. Estado Walk con 2D BlendTree (moveX, moveY)
            var stateWalk = root.AddState("Walk");
            var walkTree = new BlendTree
            {
                name = "Walk_Tree",
                blendType = BlendTreeType.SimpleDirectional2D,
                blendParameter = "moveX",
                blendParameterY = "moveY"
            };
            AssetDatabase.AddObjectToAsset(walkTree, controller);

            walkTree.AddChild(walkDown,  new Vector2(0f, -1f));
            walkTree.AddChild(walkUp,    new Vector2(0f,  1f));
            walkTree.AddChild(walkRight, new Vector2(1f,  0f));
            walkTree.AddChild(walkLeft,  new Vector2(-1f, 0f));
            stateWalk.motion = walkTree;

            // 3. Estado Sleep
            var stateSleep = root.AddState("Sleep");
            stateSleep.motion = sleep;

            // Transiciones Idle <-> Walk
            var tIdleToWalk1 = stateIdle.AddTransition(stateWalk);
            tIdleToWalk1.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
            tIdleToWalk1.hasExitTime = false; tIdleToWalk1.duration = 0.05f;

            var tIdleToWalk2 = stateIdle.AddTransition(stateWalk);
            tIdleToWalk2.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocityX");
            tIdleToWalk2.hasExitTime = false; tIdleToWalk2.duration = 0.05f;

            var tWalkToIdle = stateWalk.AddTransition(stateIdle);
            tWalkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
            tWalkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocityX");
            tWalkToIdle.hasExitTime = false; tWalkToIdle.duration = 0.05f;

            // Transiciones hacia Sleep
            var tSleepTrigger = root.AddAnyStateTransition(stateSleep);
            tSleepTrigger.AddCondition(AnimatorConditionMode.If, 0, "sleep");
            tSleepTrigger.hasExitTime = false; tSleepTrigger.duration = 0.1f;

            var tLayDownTrigger = root.AddAnyStateTransition(stateSleep);
            tLayDownTrigger.AddCondition(AnimatorConditionMode.If, 0, "lay_down");
            tLayDownTrigger.hasExitTime = false; tLayDownTrigger.duration = 0.1f;

            // Despertar de Sleep
            var tSleepToWalk = stateSleep.AddTransition(stateWalk);
            tSleepToWalk.AddCondition(AnimatorConditionMode.If, 0, "isMoving");
            tSleepToWalk.hasExitTime = false; tSleepToWalk.duration = 0.05f;

            var tSleepToIdle = stateSleep.AddTransition(stateIdle);
            tSleepToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isMoving");
            tSleepToIdle.hasExitTime = false; tSleepToIdle.duration = 0.05f;

            return controller;
        }

        private static void UpdateCharacterData(BreedConfig b, Dictionary<string, Sprite> dict, AnimatorController controller)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(b.characterAssetPath);
            if (data == null)
            {
                Debug.LogWarning($"[AllDogsRPG] No existe {b.characterAssetPath}");
                return;
            }

            dict.TryGetValue($"{b.prefix}_Down_Idle", out var idleSprite);
            dict.TryGetValue($"{b.prefix}_Right_0", out var runSprite);
            dict.TryGetValue($"{b.prefix}_Right_Idle", out var rightIdle);
            dict.TryGetValue($"{b.prefix}_Right_1", out var right1);
            dict.TryGetValue($"{b.prefix}_Sleep_0", out var sleep0);
            dict.TryGetValue($"{b.prefix}_Sleep_1", out var sleep1);

            data.portrait = idleSprite;
            data.idleSprite = idleSprite;
            data.runSprite = runSprite;
            data.animatorController = controller;

            if (idleSprite != null)
                data.idleFrames = new[] { idleSprite };

            if (runSprite != null && rightIdle != null && right1 != null)
            {
                data.walkFrames = new[] { runSprite, rightIdle, right1 };
                data.runFrames  = new[] { runSprite, rightIdle, right1 };
            }

            if (sleep0 != null && sleep1 != null)
            {
                data.sleepFrames = new[] { sleep0, sleep1 };
                data.layDownFrames = new[] { sleep0, sleep1 };
                data.deathFrames = new[] { sleep0 };
            }

            EditorUtility.SetDirty(data);
        }
    }
}
