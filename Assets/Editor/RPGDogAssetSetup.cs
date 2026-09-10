using System.Collections.Generic;
using System.IO;
using Platformer.Core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Platformer.EditorScripts
{
    /// <summary>
    /// Tools > Setup RPG Dog Character
    /// Configura los spritesheets, animaciones en 4 direcciones y CharacterData para el perro RPG autentico.
    /// </summary>
    public static class RPGDogAssetSetup
    {
        private const string SPRITE_PATH = "Assets/Sprites/Dogs/RPG-Dog/RPG_Dog_Sheet.png";
        private const string ANIM_FOLDER = "Assets/Animations/Dogs/RPG-Dog";
        private const string CONTROLLER_PATH = "Assets/Animations/Dogs/RPG-Dog/RPG-Dog-controller.controller";
        private const string CHARACTER_PATH = "Assets/Characters/Terrier.asset";

        [InitializeOnLoadMethod]
        private static void AutoRunIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(CHARACTER_PATH) || !File.Exists(CONTROLLER_PATH))
                {
                    Debug.Log("[RPGDogSetup] Configurando perro RPG automaticamente...");
                    Setup();
                }
            };
        }

        [MenuItem("Tools/Setup RPG Dog Character")]
        public static void Setup()
        {
            Debug.Log("[RPGDogSetup] Iniciando configuracion del perro RPG...");

            // 1. Slicing de la hoja de sprites
            SliceSpriteSheet();

            // 2. Cargar sprites cortados
            var sprites = LoadSpritesDict();
            if (sprites.Count == 0)
            {
                Debug.LogError("[RPGDogSetup] Error: No se encontraron los sprites cortados en " + SPRITE_PATH);
                return;
            }

            // 3. Crear carpetas si no existen
            if (!AssetDatabase.IsValidFolder("Assets/Animations"))
                AssetDatabase.CreateFolder("Assets", "Animations");
            if (!AssetDatabase.IsValidFolder("Assets/Animations/Dogs"))
                AssetDatabase.CreateFolder("Assets/Animations", "Dogs");
            if (!AssetDatabase.IsValidFolder(ANIM_FOLDER))
                AssetDatabase.CreateFolder("Assets/Animations/Dogs", "RPG-Dog");
            if (!AssetDatabase.IsValidFolder("Assets/Characters"))
                AssetDatabase.CreateFolder("Assets", "Characters");

            // 4. Crear AnimationClips
            var clipDownIdle = CreateSingleFrameClip(sprites, "RPG_Dog_Down_Idle", $"{ANIM_FOLDER}/RPG_Dog_Idle_Down.anim");
            var clipUpIdle   = CreateSingleFrameClip(sprites, "RPG_Dog_Up_Idle",   $"{ANIM_FOLDER}/RPG_Dog_Idle_Up.anim");
            var clipRightIdle= CreateSingleFrameClip(sprites, "RPG_Dog_Right_Idle",$"{ANIM_FOLDER}/RPG_Dog_Idle_Right.anim");
            var clipLeftIdle = CreateSingleFrameClip(sprites, "RPG_Dog_Left_Idle", $"{ANIM_FOLDER}/RPG_Dog_Idle_Left.anim");

            var clipDownWalk = CreateWalkClip(sprites, "Down", $"{ANIM_FOLDER}/RPG_Dog_Walk_Down.anim");
            var clipUpWalk   = CreateWalkClip(sprites, "Up",   $"{ANIM_FOLDER}/RPG_Dog_Walk_Up.anim");
            var clipRightWalk= CreateWalkClip(sprites, "Right",$"{ANIM_FOLDER}/RPG_Dog_Walk_Right.anim");
            var clipLeftWalk = CreateWalkClip(sprites, "Left", $"{ANIM_FOLDER}/RPG_Dog_Walk_Left.anim");

            var clipSleep = CreateSleepClip(sprites, $"{ANIM_FOLDER}/RPG_Dog_Sleep.anim");

            // 5. Crear AnimatorController con Blend Trees 2D
            var controller = CreateController(
                clipDownIdle, clipUpIdle, clipRightIdle, clipLeftIdle,
                clipDownWalk, clipUpWalk, clipRightWalk, clipLeftWalk,
                clipSleep);

            // 6. Crear / Actualizar CharacterData (Terrier.asset)
            SetupCharacterData(sprites, controller);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[RPGDogSetup] Configuracion del perro RPG completada con exito!");
        }

        private static void SliceSpriteSheet()
        {
            var ti = AssetImporter.GetAtPath(SPRITE_PATH) as TextureImporter;
            if (ti == null)
            {
                Debug.LogError("[RPGDogSetup] No se encontro la textura en " + SPRITE_PATH);
                return;
            }

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
                        spriteName = $"RPG_Dog_Sleep_{c}";
                    }
                    else
                    {
                        if (c == 0) spriteName = $"RPG_Dog_{dir}_0";
                        else if (c == 1) spriteName = $"RPG_Dog_{dir}_Idle";
                        else spriteName = $"RPG_Dog_{dir}_1";
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
            Debug.Log($"[RPGDogSetup] Sliced {metas.Count} sprites en {SPRITE_PATH}");
        }

        private static Dictionary<string, Sprite> LoadSpritesDict()
        {
            var dict = new Dictionary<string, Sprite>();
            var all = AssetDatabase.LoadAllAssetsAtPath(SPRITE_PATH);
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
                Debug.LogWarning($"[RPGDogSetup] Sprite no encontrado: {spriteName}");
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

        private static AnimationClip CreateWalkClip(Dictionary<string, Sprite> dict, string dir, string savePath)
        {
            dict.TryGetValue($"RPG_Dog_{dir}_0", out var s0);
            dict.TryGetValue($"RPG_Dog_{dir}_Idle", out var sIdle);
            dict.TryGetValue($"RPG_Dog_{dir}_1", out var s1);

            if (s0 == null || sIdle == null || s1 == null)
            {
                Debug.LogWarning($"[RPGDogSetup] Faltan sprites para walk {dir}");
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

        private static AnimationClip CreateSleepClip(Dictionary<string, Sprite> dict, string savePath)
        {
            dict.TryGetValue("RPG_Dog_Sleep_0", out var s0);
            dict.TryGetValue("RPG_Dog_Sleep_1", out var s1);

            if (s0 == null || s1 == null)
            {
                Debug.LogWarning("[RPGDogSetup] Faltan sprites para sleep");
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
            AnimationClip idleDown, AnimationClip idleUp, AnimationClip idleRight, AnimationClip idleLeft,
            AnimationClip walkDown, AnimationClip walkUp, AnimationClip walkRight, AnimationClip walkLeft,
            AnimationClip sleep)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
            var controller = existing != null ? existing : AnimatorController.CreateAnimatorControllerAtPath(CONTROLLER_PATH);

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

        private static void SetupCharacterData(Dictionary<string, Sprite> dict, AnimatorController controller)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(CHARACTER_PATH);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(data, CHARACTER_PATH);
            }

            dict.TryGetValue("RPG_Dog_Down_Idle", out var idleSprite);
            dict.TryGetValue("RPG_Dog_Right_0", out var runSprite);
            dict.TryGetValue("RPG_Dog_Sleep_0", out var sleep0);
            dict.TryGetValue("RPG_Dog_Sleep_1", out var sleep1);

            dict.TryGetValue("RPG_Dog_Right_Idle", out var rightIdle);
            dict.TryGetValue("RPG_Dog_Right_1", out var right1);

            data.characterName = "Terrier RPG";
            data.description = "Perrito audaz con animaciones completas en 4 direcciones (frente, espalda y laterales). Disenado especialmente para la aldea sin deformarse!";
            data.moveSpeed = 8f;
            data.jumpStrength = 16f;
            data.gravityModifier = 2.8f;
            data.startingLives = 4;
            data.hasDoubleJump = true;
            data.hasDash = false;
            data.characterColor = new Color(0.75f, 0.55f, 0.35f);

            data.portrait = idleSprite;
            data.idleSprite = idleSprite;
            data.runSprite = runSprite;
            data.animatorController = controller;

            if (idleSprite != null)
                data.idleFrames = new[] { idleSprite };

            if (runSprite != null && rightIdle != null && right1 != null)
            {
                data.walkFrames = new[] { runSprite, rightIdle, right1 };
                data.runFrames = new[] { runSprite, rightIdle, right1 };
            }

            if (sleep0 != null && sleep1 != null)
            {
                data.sleepFrames = new[] { sleep0, sleep1 };
                data.layDownFrames = new[] { sleep0, sleep1 };
                data.deathFrames = new[] { sleep0 };
            }

            EditorUtility.SetDirty(data);
            Debug.Log($"[RPGDogSetup] CharacterData guardado en {CHARACTER_PATH}");
        }
    }
}
