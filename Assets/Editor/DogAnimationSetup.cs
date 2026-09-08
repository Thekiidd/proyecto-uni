using Platformer.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Tools > Animate Dog Characters
/// 1. Slice todos los sprite sheets de perros (100x100 px por frame)
/// 2. Crea AnimationClips (idle, run, walk, bark)
/// 3. Crea un AnimatorController por perro
/// 4. Asigna los controllers a los CharacterData
/// </summary>
public static class DogAnimationSetup
{
    // Frames por sprite sheet (ancho / frameSize)
    private const int FRAME_SIZE   = 100;   // cada frame es 100x100 px
    private const float FRAME_RATE = 10f;   // FPS de las animaciones

    // Datos de cada perro: carpeta, prefijo de archivo, asset CharacterData
    private static readonly (string folder, string prefix, string characterAsset)[] Dogs =
    {
        ("Assets/Sprites/Dogs/Golden-Retriever", "Golden-Retriever", "Assets/Characters/Runner.asset"),
        ("Assets/Sprites/Dogs/Akita",            "Akita",            "Assets/Characters/Jumper.asset"),
        ("Assets/Sprites/Dogs/Saint-Bernard",    "Saint-Bernard",    "Assets/Characters/Tank.asset"),
    };

    [MenuItem("Tools/Animate Dog Characters")]
    public static void SetupAnimations()
    {
        string animFolder = "Assets/Animations/Dogs";
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Dogs");

        foreach (var (folder, prefix, charAsset) in Dogs)
        {
            Debug.Log($"[DogAnim] Procesando {prefix}...");

            string idlePath = File.Exists(Application.dataPath + "/../" + $"{folder}/{prefix}-Idle.png") 
                ? $"{folder}/{prefix}-Idle.png" 
                : $"{folder}/{prefix}-idle.png";

            // 1 ── Slice sprite sheets
            SliceSpriteSheet(idlePath, 1000, FRAME_SIZE); // 10 frames
            SliceSpriteSheet($"{folder}/{prefix}-run.png",  800,  FRAME_SIZE); // 8 frames
            SliceSpriteSheet($"{folder}/{prefix}-walk.png", 800,  FRAME_SIZE); // 8 frames

            // Intentar bark si existe
            string barkPath = $"{folder}/{prefix}-bark.png";
            if (File.Exists(Application.dataPath + "/../" + barkPath))
                SliceSpriteSheet(barkPath, 400, FRAME_SIZE);

            AssetDatabase.Refresh();

            // 2 ── Cargar sprites sliceados
            var idleSprites = LoadSlicedSprites(idlePath);
            var runSprites  = LoadSlicedSprites($"{folder}/{prefix}-run.png");
            var walkSprites = LoadSlicedSprites($"{folder}/{prefix}-walk.png");

            if (idleSprites.Count == 0 || runSprites.Count == 0)
            {
                Debug.LogWarning($"[DogAnim] No se pudieron cargar sprites de {prefix}. Revisando importación...");
                continue;
            }

            // 3 ── Crear AnimationClips
            string dogAnimFolder = $"{animFolder}/{prefix}";
            if (!AssetDatabase.IsValidFolder(dogAnimFolder))
                AssetDatabase.CreateFolder(animFolder, prefix);

            var clipIdle  = CreateClip(idleSprites, $"{dogAnimFolder}/{prefix}-idle.anim",  FRAME_RATE, loop: true);
            var clipRun   = CreateClip(runSprites,  $"{dogAnimFolder}/{prefix}-run.anim",   FRAME_RATE, loop: true);
            var clipWalk  = CreateClip(walkSprites, $"{dogAnimFolder}/{prefix}-walk.anim",  FRAME_RATE, loop: true);

            // 4 ── Crear AnimatorController
            var controller = CreateAnimatorController(prefix, dogAnimFolder, clipIdle, clipRun, clipWalk);

            // 5 ── Asignar al CharacterData
            var charData = AssetDatabase.LoadAssetAtPath<CharacterData>(charAsset);
            if (charData != null)
            {
                var so = new SerializedObject(charData);
                so.FindProperty("animatorController").objectReferenceValue = controller;
                // Actualizar idleSprite al primer frame
                if (idleSprites.Count > 0)
                    so.FindProperty("idleSprite").objectReferenceValue = idleSprites[0];
                so.ApplyModifiedProperties();
                Debug.Log($"[DogAnim] ✅ {prefix}: controller asignado a {charAsset}");
            }
            else
            {
                Debug.LogWarning($"[DogAnim] No encontrado CharacterData en {charAsset}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Animaciones de Perros Listas",
            "Se crearon AnimatorControllers para:\n" +
            "  Golden Retriever  (idle, run, walk)\n" +
            "  Akita             (idle, run, walk)\n" +
            "  San Bernardo      (idle, run, walk)\n\n" +
            "Los perros se animaran automaticamente en el juego.",
            "Entendido");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void SliceSpriteSheet(string path, int totalWidth, int frameSize)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        int frameCount = totalWidth / frameSize;

        ti.textureType        = TextureImporterType.Sprite;
        ti.spriteImportMode   = SpriteImportMode.Multiple;
        ti.filterMode         = FilterMode.Point;           // Pixel art: sin blur
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.spritePixelsPerUnit = 16f;                       // Escala pixel art

        var metas = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            metas.Add(new SpriteMetaData
            {
                name   = $"{Path.GetFileNameWithoutExtension(path)}_{i}",
                rect   = new Rect(i * frameSize, 0, frameSize, frameSize),
                pivot  = new Vector2(0.5f, 0f),    // Pivot en los pies
                alignment = (int)SpriteAlignment.BottomCenter
            });
        }
        ti.spritesheet = metas.ToArray();
        ti.SaveAndReimport();
    }

    static List<Sprite> LoadSlicedSprites(string path)
    {
        var sprites = new List<Sprite>();
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in all)
        {
            if (a is Sprite s) sprites.Add(s);
        }
        // Ordenar por nombre (frame_0, frame_1, ...)
        sprites.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
        return sprites;
    }

    static AnimationClip CreateClip(List<Sprite> sprites, string savePath, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };

        // Configurar loop
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Crear curva de sprites
        var binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Count];
        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i / fps,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Guardar como asset
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(savePath);
        if (existing != null)
        {
            EditorUtility.CopySerialized(clip, existing);
            AssetDatabase.SaveAssets();
            return existing;
        }
        AssetDatabase.CreateAsset(clip, savePath);
        return clip;
    }

    static AnimatorController CreateAnimatorController(
        string prefix, string folder,
        AnimationClip idle, AnimationClip run, AnimationClip walk)
    {
        string path       = $"{folder}/{prefix}-controller.controller";
        var    controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Parámetros (mismos que usa AnimationController.cs del Microgame)
        controller.AddParameter("velocityX", AnimatorControllerParameterType.Float);
        controller.AddParameter("grounded",  AnimatorControllerParameterType.Bool);

        var root = controller.layers[0].stateMachine;

        // Estados
        var stateIdle = root.AddState("Idle");
        var stateRun  = root.AddState("Run");
        var stateWalk = root.AddState("Walk");

        stateIdle.motion = idle;
        stateRun.motion  = run;
        stateWalk.motion = walk;

        root.defaultState = stateIdle;

        // Transiciones: Idle → Run (velocityX > 3)
        var t1 = stateIdle.AddTransition(stateRun);
        t1.AddCondition(AnimatorConditionMode.Greater, 3f, "velocityX");
        t1.hasExitTime = false; t1.duration = 0f;

        // Idle → Walk (velocityX > 0.1)
        var t2 = stateIdle.AddTransition(stateWalk);
        t2.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocityX");
        t2.hasExitTime = false; t2.duration = 0f;

        // Run → Idle (velocityX < 0.1)
        var t3 = stateRun.AddTransition(stateIdle);
        t3.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocityX");
        t3.hasExitTime = false; t3.duration = 0f;

        // Walk → Idle
        var t4 = stateWalk.AddTransition(stateIdle);
        t4.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocityX");
        t4.hasExitTime = false; t4.duration = 0f;

        // Walk → Run
        var t5 = stateWalk.AddTransition(stateRun);
        t5.AddCondition(AnimatorConditionMode.Greater, 3f, "velocityX");
        t5.hasExitTime = false; t5.duration = 0f;

        AssetDatabase.SaveAssets();
        return controller;
    }
}
