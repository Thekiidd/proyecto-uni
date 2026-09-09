using Platformer.Core;
using Platformer.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Tools > Animate Dog Characters
/// 1. Slices all 11 dog sprite sheets for all 6 breeds with calibrated PPU and pivots.
/// 2. Creates separate AnimationClips for all interactions:
///    - Idle base (peaceful breathing)
///    - Run, Walk, Death
///    - Scratch (itching)
///    - Stretch (stretching)
///    - Lick (licking1)
///    - Lick2 (licking2)
///    - Bark (bark)
///    - Sit (sitting)
///    - LayDown (lying-down)
///    - Sleep (sleeping)
/// 3. Builds AnimatorControllers with instant interruption to Run on movement.
/// 4. Creates / updates CharacterData assets for all 6 dogs and registers them in CharacterSelectUI.
/// </summary>
[InitializeOnLoad]
public static class DogAnimationSetup
{
    private const int FRAME_SIZE = 100;
    private const string SETUP_KEY = "DogAnimationSetup_v6_All6DogsAllInteractions";

    public struct DogBreedConfig
    {
        public string folder;
        public string prefix;
        public string characterAsset;
        public string charName;
        public string desc;
        public float moveSpeed;
        public float jumpStrength;
        public float gravityModifier;
        public int startingLives;
        public bool hasDoubleJump;
        public bool hasDash;
        public float pivotY;
        public float ppu;
        public Color characterColor;

        public DogBreedConfig(string folder, string prefix, string charAsset, string name, string desc,
            float speed, float jump, float grav, int lives, bool dblJump, bool dash, float pivotY, float ppu, Color col)
        {
            this.folder = folder;
            this.prefix = prefix;
            this.characterAsset = charAsset;
            this.charName = name;
            this.desc = desc;
            this.moveSpeed = speed;
            this.jumpStrength = jump;
            this.gravityModifier = grav;
            this.startingLives = lives;
            this.hasDoubleJump = dblJump;
            this.hasDash = dash;
            this.pivotY = pivotY;
            this.ppu = ppu;
            this.characterColor = col;
        }
    }

    private static readonly DogBreedConfig[] Dogs = new[]
    {
        new DogBreedConfig("Assets/Sprites/Dogs/Golden-Retriever", "Golden-Retriever", "Assets/Characters/Runner.asset",
            "Golden Retriever", "Veloz y energico. El corredor nato por excelencia.",
            10f, 14f, 3f, 3, false, false, 0.39f, 30f, new Color(0.95f, 0.75f, 0.2f)),

        new DogBreedConfig("Assets/Sprites/Dogs/Akita", "Akita", "Assets/Characters/Jumper.asset",
            "Akita", "Agil y saltarin. Cuenta con la destreza de doble salto.",
            7f, 20f, 2.2f, 3, true, false, 0.42f, 20f, new Color(0.95f, 0.6f, 0.3f)),

        new DogBreedConfig("Assets/Sprites/Dogs/Saint-Bernard", "Saint-Bernard", "Assets/Characters/Tank.asset",
            "San Bernardo", "Grande, resistente y noble. Comienza la aventura con 6 vidas.",
            5f, 12f, 3.5f, 6, false, false, 0.35f, 30f, new Color(0.85f, 0.5f, 0.3f)),

        new DogBreedConfig("Assets/Sprites/Dogs/Great-Dane", "Great-Dane", "Assets/Characters/GreatDane.asset",
            "Gran Danes", "Elegante de zancada imponente. Equipado con potente impulso Dash.",
            8.5f, 15f, 2.8f, 4, false, true, 0.33f, 32f, new Color(0.6f, 0.55f, 0.5f)),

        new DogBreedConfig("Assets/Sprites/Dogs/Schnauzer", "Schnauzer", "Assets/Characters/Schnauzer.asset",
            "Schnauzer", "Valiente y determinado. Salto acrobatico doble con 4 vidas.",
            8f, 18f, 2.5f, 4, true, false, 0.38f, 28f, new Color(0.7f, 0.7f, 0.75f)),

        new DogBreedConfig("Assets/Sprites/Dogs/Siberian-Husky", "Siberian-Husky", "Assets/Characters/Husky.asset",
            "Husky Siberiano", "Explorador artico veloz e incansable con habilidad Dash.",
            9.5f, 16f, 2.8f, 3, false, true, 0.38f, 30f, new Color(0.4f, 0.65f, 0.9f))
    };

    static DogAnimationSetup()
    {
        EditorApplication.delayCall += AutoRunIfMissing;
    }

    [InitializeOnLoadMethod]
    private static void AutoRunIfMissing()
    {
        if (!EditorPrefs.GetBool(SETUP_KEY, false))
        {
            Debug.Log("[DogAnim] Configurando animaciones e interacciones completas para los 6 perros...");
            SetupAnimations(false);
            EditorPrefs.SetBool(SETUP_KEY, true);
        }
    }

    [MenuItem("Tools/Animate Dog Characters")]
    public static void MenuSetupAnimations()
    {
        SetupAnimations(true);
    }

    public static void SetupAnimations(bool showDialog = false)
    {
        string animFolder = "Assets/Animations/Dogs";
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Dogs");

        if (!AssetDatabase.IsValidFolder("Assets/Characters"))
            AssetDatabase.CreateFolder("Assets", "Characters");

        var createdCharacters = new List<CharacterData>();

        foreach (var dog in Dogs)
        {
            Debug.Log($"[DogAnim] Procesando {dog.prefix} ({dog.charName})...");

            string idlePath = File.Exists(Application.dataPath + "/../" + dog.folder + "/" + dog.prefix + "-Idle.png")
                ? dog.folder + "/" + dog.prefix + "-Idle.png"
                : dog.folder + "/" + dog.prefix + "-idle.png";

            string runPath      = dog.folder + "/" + dog.prefix + "-run.png";
            string walkPath     = dog.folder + "/" + dog.prefix + "-walk.png";
            string deathPath    = dog.folder + "/" + dog.prefix + "-lying-down.png";
            string barkPath     = dog.folder + "/" + dog.prefix + "-bark.png";
            string itchPath     = dog.folder + "/" + dog.prefix + "-itching.png";
            string lick1Path    = dog.folder + "/" + dog.prefix + "-licking1.png";
            string lick2Path    = dog.folder + "/" + dog.prefix + "-licking2.png";
            string stretchPath  = dog.folder + "/" + dog.prefix + "-stretching.png";
            string sitPath      = dog.folder + "/" + dog.prefix + "-sitting.png";
            string sleepPath    = dog.folder + "/" + dog.prefix + "-sleeping.png";

            // 1 ── Slicing de todas las hojas de sprites con pivot exacto
            SliceSpriteSheet(idlePath,    1000, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(runPath,      800, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(walkPath,     800, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(deathPath,    700, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(barkPath,     300, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(itchPath,     200, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(lick1Path,    400, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(lick2Path,    400, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(stretchPath, 1000, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(sitPath,      100, FRAME_SIZE, dog.pivotY, dog.ppu);
            SliceSpriteSheet(sleepPath,    100, FRAME_SIZE, dog.pivotY, dog.ppu);

            AssetDatabase.Refresh();

            // 2 ── Cargar sprites cortados
            var rawIdleSprites = LoadSlicedSprites(idlePath);
            var runSprites     = LoadSlicedSprites(runPath);
            var walkSprites    = LoadSlicedSprites(walkPath);
            var deathSprites   = LoadSlicedSprites(deathPath);
            var barkSprites    = LoadSlicedSprites(barkPath);
            var itchSprites    = LoadSlicedSprites(itchPath);
            var lick1Sprites   = LoadSlicedSprites(lick1Path);
            var lick2Sprites   = LoadSlicedSprites(lick2Path);
            var stretchSprites = LoadSlicedSprites(stretchPath);
            var sitSprites     = LoadSlicedSprites(sitPath);
            var sleepSprites   = LoadSlicedSprites(sleepPath);

            if (rawIdleSprites.Count == 0 || runSprites.Count == 0)
            {
                Debug.LogWarning($"[DogAnim] No se pudieron cargar sprites basicos de {dog.prefix}.");
                continue;
            }

            // 3 ── Construcción de secuencias orgánicas
            // Idle base tranquilo (respiración suave: primeros 7 cuadros)
            var baseIdleSprites = rawIdleSprites.Take(Mathf.Min(7, rawIdleSprites.Count)).ToList();

            // Rascado: alternar 3 veces para ritmo natural
            var scratchSeq = new List<Sprite>();
            if (itchSprites.Count >= 2)
            {
                for (int i = 0; i < 3; i++) { scratchSeq.Add(itchSprites[0]); scratchSeq.Add(itchSprites[1]); }
            }

            // Lamido 1 (patita): ida y vuelta suave
            var lick1Seq = new List<Sprite>();
            if (lick1Sprites.Count >= 4)
            {
                lick1Seq.AddRange(lick1Sprites);
                lick1Seq.Add(lick1Sprites[2]);
                lick1Seq.Add(lick1Sprites[1]);
            }

            // Lamido 2 (pecho/hocico): ida y vuelta suave
            var lick2Seq = new List<Sprite>();
            if (lick2Sprites.Count >= 4)
            {
                lick2Seq.AddRange(lick2Sprites);
                lick2Seq.Add(lick2Sprites[2]);
                lick2Seq.Add(lick2Sprites[1]);
            }

            // Ladrido: abrir y cerrar hocico
            var barkSeq = new List<Sprite>();
            if (barkSprites.Count >= 3)
            {
                barkSeq.Add(barkSprites[0]);
                barkSeq.Add(barkSprites[1]);
                barkSeq.Add(barkSprites[2]);
                barkSeq.Add(barkSprites[1]);
                barkSeq.Add(barkSprites[0]);
            }

            // Sentado: sentarse atento por ~2 segundos (12 cuadros a 6 FPS)
            var sitSeq = new List<Sprite>();
            if (sitSprites.Count > 0)
            {
                for (int i = 0; i < 12; i++) sitSeq.Add(sitSprites[0]);
            }

            // Acostarse en el piso: acostarse (0..6), descansar un momento, y levantarse (6..0)
            var layDownSeq = new List<Sprite>();
            if (deathSprites.Count >= 7)
            {
                layDownSeq.AddRange(deathSprites); // 0 a 6
                layDownSeq.Add(deathSprites[6]);
                layDownSeq.Add(deathSprites[6]);
                layDownSeq.Add(deathSprites[6]);
                layDownSeq.Add(deathSprites[5]);
                layDownSeq.Add(deathSprites[4]);
                layDownSeq.Add(deathSprites[3]);
                layDownSeq.Add(deathSprites[2]);
                layDownSeq.Add(deathSprites[1]);
                layDownSeq.Add(deathSprites[0]);
            }

            // Dormir: hacerse bolita y dormir una siesta zZz por ~3 segundos (18 cuadros a 6 FPS)
            var sleepSeq = new List<Sprite>();
            if (sleepSprites.Count > 0)
            {
                for (int i = 0; i < 18; i++) sleepSeq.Add(sleepSprites[0]);
            }

            // 4 ── Crear AnimationClips independientes
            string dogAnimFolder = animFolder + "/" + dog.prefix;
            if (!AssetDatabase.IsValidFolder(dogAnimFolder))
                AssetDatabase.CreateFolder(animFolder, dog.prefix);

            var clipIdle     = CreateClip(baseIdleSprites, dogAnimFolder + "/" + dog.prefix + "-idle.anim", 6f, loop: true);
            var clipRun      = CreateClip(runSprites,      dogAnimFolder + "/" + dog.prefix + "-run.anim",  12f, loop: true);
            var clipWalk     = CreateClip(walkSprites,     dogAnimFolder + "/" + dog.prefix + "-walk.anim",  8f, loop: true);
            var clipDeath    = deathSprites.Count > 0 ? CreateClip(deathSprites, dogAnimFolder + "/" + dog.prefix + "-death.anim", 10f, loop: false) : null;

            var clipScratch  = scratchSeq.Count > 0 ? CreateClip(scratchSeq, dogAnimFolder + "/" + dog.prefix + "-scratch.anim", 8f, loop: false) : null;
            var clipStretch  = stretchSprites.Count > 0 ? CreateClip(stretchSprites, dogAnimFolder + "/" + dog.prefix + "-stretch.anim", 9f, loop: false) : null;
            var clipLick     = lick1Seq.Count > 0 ? CreateClip(lick1Seq, dogAnimFolder + "/" + dog.prefix + "-lick.anim", 7f, loop: false) : null;
            var clipLick2    = lick2Seq.Count > 0 ? CreateClip(lick2Seq, dogAnimFolder + "/" + dog.prefix + "-lick2.anim", 7f, loop: false) : null;
            var clipBark     = barkSeq.Count > 0 ? CreateClip(barkSeq, dogAnimFolder + "/" + dog.prefix + "-bark.anim", 8f, loop: false) : null;
            var clipSit      = sitSeq.Count > 0 ? CreateClip(sitSeq, dogAnimFolder + "/" + dog.prefix + "-sit.anim", 6f, loop: false) : null;
            var clipLayDown  = layDownSeq.Count > 0 ? CreateClip(layDownSeq, dogAnimFolder + "/" + dog.prefix + "-laydown.anim", 7f, loop: false) : null;
            var clipSleep    = sleepSeq.Count > 0 ? CreateClip(sleepSeq, dogAnimFolder + "/" + dog.prefix + "-sleep.anim", 6f, loop: false) : null;

            // 5 ── Crear AnimatorController con todas las interacciones
            var controller = CreateAnimatorController(
                dog.prefix, dogAnimFolder,
                clipIdle, clipRun, clipWalk, clipDeath,
                clipScratch, clipStretch, clipLick, clipLick2, clipBark,
                clipSit, clipLayDown, clipSleep);

            // 6 ── Crear o Actualizar CharacterData ScriptableObject
            var charData = AssetDatabase.LoadAssetAtPath<CharacterData>(dog.characterAsset);
            if (charData == null)
            {
                charData = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(charData, dog.characterAsset);
            }

            charData.characterName   = dog.charName;
            charData.description     = dog.desc;
            charData.moveSpeed       = dog.moveSpeed;
            charData.jumpStrength    = dog.jumpStrength;
            charData.gravityModifier = dog.gravityModifier;
            charData.startingLives   = dog.startingLives;
            charData.hasDoubleJump   = dog.hasDoubleJump;
            charData.hasDash         = dog.hasDash;
            charData.dashSpeed       = dog.hasDash ? 14f : 0f;
            charData.characterColor  = dog.characterColor;

            charData.animatorController = controller;

            if (baseIdleSprites.Count > 0)
            {
                charData.idleSprite = baseIdleSprites[0];
                charData.portrait   = baseIdleSprites[0];
                charData.idleFrames = baseIdleSprites.ToArray();
            }

            if (runSprites.Count > 0)
            {
                charData.runSprite = runSprites[0];
                charData.runFrames = runSprites.ToArray();
            }

            if (walkSprites.Count > 0)  charData.walkFrames  = walkSprites.ToArray();
            if (deathSprites.Count > 0) charData.deathFrames = deathSprites.ToArray();

            if (scratchSeq.Count > 0)     charData.scratchFrames = scratchSeq.ToArray();
            if (stretchSprites.Count > 0) charData.stretchFrames = stretchSprites.ToArray();
            if (lick1Seq.Count > 0)       charData.lickFrames    = lick1Seq.ToArray();
            if (lick2Seq.Count > 0)       charData.lick2Frames   = lick2Seq.ToArray();
            if (barkSeq.Count > 0)        charData.barkFrames    = barkSeq.ToArray();
            if (sitSeq.Count > 0)         charData.sitFrames     = sitSeq.ToArray();
            if (layDownSeq.Count > 0)     charData.layDownFrames = layDownSeq.ToArray();
            if (sleepSeq.Count > 0)       charData.sleepFrames   = sleepSeq.ToArray();

            EditorUtility.SetDirty(charData);
            createdCharacters.Add(charData);
            Debug.Log($"[DogAnim] ✅ {dog.charName} ({dog.prefix}) configurado con todas sus interacciones.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 7 ── Asignar los 6 personajes a CharacterSelectUI si está presente
        UpdateActiveCharacterSelectUI(createdCharacters);

        if (showDialog)
        {
            EditorUtility.DisplayDialog("6 Perros e Interacciones Completas",
@"¡Se han configurado con exito los 6 perros y todas sus interacciones!

Perros listos:
  1. Golden Retriever (Runner)
  2. Akita (Jumper)
  3. San Bernardo (Tank)
  4. Gran Danes (Great Dane con Dash)
  5. Schnauzer (Doble Salto)
  6. Husky Siberiano (Explorador con Dash)

Interacciones incluidas:
  - Respiracion tranquila (Idle base)
  - Rascado de orejita (scratch)
  - Estiramiento con bostezo (stretch)
  - Lamido de patita (lick)
  - Lamido de pecho (lick2)
  - Ladrido amistoso (bark)
  - Sentarse atento (sit)
  - Acostarse en el suelo (lay_down)
  - Dormir siesta en bolita zZz (sleep)
  - Correr, caminar y derrota

Todas con cancelacion inmediata al moverse.",
                "¡Excelente!");
        }
    }

    private static void UpdateActiveCharacterSelectUI(List<CharacterData> characters)
    {
        var selectUI = Object.FindFirstObjectByType<CharacterSelectUI>();
        if (selectUI != null && characters.Count > 0)
        {
            selectUI.characters = characters.ToArray();
            EditorUtility.SetDirty(selectUI);
            Debug.Log($"[DogAnim] Asignados {characters.Count} perros al CharacterSelectUI de la escena activa.");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void SliceSpriteSheet(string path, int totalWidth, int frameSize, float pivotY, float ppu)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        int frameCount = totalWidth / frameSize;

        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Multiple;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.spritePixelsPerUnit = ppu;

        var metas = new List<SpriteMetaData>();
        for (int i = 0; i < frameCount; i++)
        {
            metas.Add(new SpriteMetaData
            {
                name      = $"{Path.GetFileNameWithoutExtension(path)}_{i}",
                rect      = new Rect(i * frameSize, 0, frameSize, frameSize),
                pivot     = new Vector2(0.5f, pivotY),
                alignment = (int)SpriteAlignment.Custom
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
        sprites.Sort((a, b) =>
        {
            int idxA = ExtractFrameIndex(a.name);
            int idxB = ExtractFrameIndex(b.name);
            return idxA.CompareTo(idxB);
        });
        return sprites;
    }

    static int ExtractFrameIndex(string name)
    {
        int lastUnder = name.LastIndexOf('_');
        if (lastUnder >= 0 && int.TryParse(name.Substring(lastUnder + 1), out int val))
            return val;
        return 0;
    }

    static AnimationClip CreateClip(List<Sprite> sprites, string savePath, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps };

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

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
        AnimationClip idle, AnimationClip run, AnimationClip walk, AnimationClip death,
        AnimationClip scratch, AnimationClip stretch, AnimationClip lick, AnimationClip lick2,
        AnimationClip bark, AnimationClip sit, AnimationClip layDown, AnimationClip sleep)
    {
        string path = folder + "/" + prefix + "-controller.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null && existing.layers != null && existing.layers.Length > 0 &&
            existing.layers[0].stateMachine != null && existing.layers[0].stateMachine.states != null &&
            existing.layers[0].stateMachine.states.Length >= 3)
        {
            return existing;
        }
        var controller = existing != null ? existing : AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.parameters = new AnimatorControllerParameter[0];
        controller.AddParameter("velocityX", AnimatorControllerParameterType.Float);
        controller.AddParameter("grounded",  AnimatorControllerParameterType.Bool);
        controller.AddParameter("dead",      AnimatorControllerParameterType.Bool);
        controller.AddParameter("hurt",      AnimatorControllerParameterType.Trigger);
        controller.AddParameter("scratch",   AnimatorControllerParameterType.Trigger);
        controller.AddParameter("stretch",   AnimatorControllerParameterType.Trigger);
        controller.AddParameter("lick",      AnimatorControllerParameterType.Trigger);
        controller.AddParameter("lick2",     AnimatorControllerParameterType.Trigger);
        controller.AddParameter("bark",      AnimatorControllerParameterType.Trigger);
        controller.AddParameter("sit",       AnimatorControllerParameterType.Trigger);
        controller.AddParameter("lay_down",  AnimatorControllerParameterType.Trigger);
        controller.AddParameter("sleep",     AnimatorControllerParameterType.Trigger);

        var root = controller.layers[0].stateMachine;

        while (root.states.Length > 0)
        {
            root.RemoveState(root.states[0].state);
        }

        var stateIdle = root.AddState("Idle");
        var stateRun  = root.AddState("Run");
        var stateWalk = root.AddState("Walk");

        stateIdle.motion = idle;
        stateRun.motion  = run;
        stateWalk.motion = walk;

        root.defaultState = stateIdle;

        // Transiciones basicas de movimiento
        var t1 = stateIdle.AddTransition(stateRun);
        t1.AddCondition(AnimatorConditionMode.Greater, 2.5f, "velocityX");
        t1.hasExitTime = false; t1.duration = 0f;

        var t2 = stateIdle.AddTransition(stateWalk);
        t2.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocityX");
        t2.hasExitTime = false; t2.duration = 0f;

        var t3 = stateRun.AddTransition(stateIdle);
        t3.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocityX");
        t3.hasExitTime = false; t3.duration = 0f;

        var t4 = stateWalk.AddTransition(stateIdle);
        t4.AddCondition(AnimatorConditionMode.Less, 0.1f, "velocityX");
        t4.hasExitTime = false; t4.duration = 0f;

        var t5 = stateWalk.AddTransition(stateRun);
        t5.AddCondition(AnimatorConditionMode.Greater, 2.5f, "velocityX");
        t5.hasExitTime = false; t5.duration = 0f;

        var t6 = stateRun.AddTransition(stateWalk);
        t6.AddCondition(AnimatorConditionMode.Less, 2.5f, "velocityX");
        t6.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocityX");
        t6.hasExitTime = false; t6.duration = 0f;

        // Estados de acciones espontáneas
        AddSpecialIdleState(root, stateIdle, stateRun, "Scratch",  scratch,  "scratch");
        AddSpecialIdleState(root, stateIdle, stateRun, "Stretch",  stretch,  "stretch");
        AddSpecialIdleState(root, stateIdle, stateRun, "Lick",     lick,     "lick");
        AddSpecialIdleState(root, stateIdle, stateRun, "Lick2",    lick2,    "lick2");
        AddSpecialIdleState(root, stateIdle, stateRun, "Bark",     bark,     "bark");
        AddSpecialIdleState(root, stateIdle, stateRun, "Sit",      sit,      "sit");
        AddSpecialIdleState(root, stateIdle, stateRun, "LayDown",  layDown,  "lay_down");
        AddSpecialIdleState(root, stateIdle, stateRun, "Sleep",    sleep,    "sleep");

        if (death != null)
        {
            var stateDeath = root.AddState("Death");
            stateDeath.motion = death;

            var tDeath = root.AddAnyStateTransition(stateDeath);
            tDeath.AddCondition(AnimatorConditionMode.If, 0, "dead");
            tDeath.hasExitTime = false; tDeath.duration = 0f;

            var tRevive = stateDeath.AddTransition(stateIdle);
            tRevive.AddCondition(AnimatorConditionMode.IfNot, 0, "dead");
            tRevive.hasExitTime = false; tRevive.duration = 0f;
        }

        AssetDatabase.SaveAssets();
        return controller;
    }

    static void AddSpecialIdleState(
        AnimatorStateMachine root, AnimatorState stateIdle, AnimatorState stateRun,
        string stateName, AnimationClip clip, string triggerName)
    {
        if (clip == null) return;

        var state = root.AddState(stateName);
        state.motion = clip;

        // De Idle a la acción al disparar el trigger
        var toAction = stateIdle.AddTransition(state);
        toAction.AddCondition(AnimatorConditionMode.If, 0, triggerName);
        toAction.hasExitTime = false;
        toAction.duration = 0f;

        // Al terminar la acción, regresa automáticamente a Idle
        var backToIdle = state.AddTransition(stateIdle);
        backToIdle.hasExitTime = true;
        backToIdle.exitTime = 1f;
        backToIdle.duration = 0.05f;

        // Si el jugador se mueve en cualquier momento, interrumpe de inmediato hacia Run
        var toRun = state.AddTransition(stateRun);
        toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "velocityX");
        toRun.hasExitTime = false;
        toRun.duration = 0f;
    }
}
