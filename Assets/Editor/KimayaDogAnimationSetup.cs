using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Genera AnimatorControllers para Falin y Mia con los sprites que ya tenemos.
    /// Menú: Tools > Kimaya > Setup Dog Animations
    ///
    /// Estados:
    ///   Idle → Run (isRunning=true)
    ///   Run  → Idle (isRunning=false)
    ///   Any  → Jump (isGrounded=false && velocityY > 1)
    ///   Any  → Fall (isGrounded=false && velocityY < -1)
    ///   Fall → Idle (isGrounded=true)
    /// </summary>
    public static class KimayaDogAnimationSetup
    {
        private const string ANIM_OUTPUT = "Assets/Animations/Kimaya";

        // ── Rutas de sprites ─────────────────────────────────────────────
        // Falin = Great Dane (placeholder hasta conseguir Border Collie)
        private const string FALIN_IDLE = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-idle.png";
        private const string FALIN_RUN  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-run.png";
        private const string FALIN_SIT  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-sitting.png";

        // Mia = Schnauzer
        private const string MIA_IDLE = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-Idle.png";
        private const string MIA_RUN  = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-run.png";
        private const string MIA_SIT  = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-sitting.png";

        [MenuItem("Tools/Kimaya/Setup Dog Animations")]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder(ANIM_OUTPUT))
                AssetDatabase.CreateFolder("Assets/Animations", "Kimaya");

            CreateDogController("Falin", FALIN_IDLE, FALIN_RUN, FALIN_SIT,
                new Color(0.9f, 0.7f, 0.3f));
            CreateDogController("Mia", MIA_IDLE, MIA_RUN, MIA_SIT,
                new Color(0.5f, 0.85f, 1f));

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("✅ Animaciones Kimaya",
                "AnimatorControllers creados:\n" +
                $"  {ANIM_OUTPUT}/Falin_Controller.controller\n" +
                $"  {ANIM_OUTPUT}/Mia_Controller.controller\n\n" +
                "Asigna estos controllers al componente Animator\n" +
                "de Falin y Mia en la escena Level_01_Bosque.\n\n" +
                "Parámetros:\n" +
                "  isRunning (bool) → camina\n" +
                "  isGrounded (bool) → en suelo\n" +
                "  velocityY (float) → velocidad vertical", "OK");
        }

        private static void CreateDogController(string dogName,
            string idlePath, string runPath, string sitPath, Color tint)
        {
            string ctrlPath = $"{ANIM_OUTPUT}/{dogName}_Controller.controller";

            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var root = ctrl.layers[0].stateMachine;

            // Parámetros
            ctrl.AddParameter("isRunning",  AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("isGrounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("velocityY",  AnimatorControllerParameterType.Float);

            // ── Clips ──────────────────────────────────────────────────────
            var idleClip = CreateSingleFrameClip($"{dogName}_Idle", idlePath,
                $"{ANIM_OUTPUT}/{dogName}_Idle.anim", dogName, 0.5f);
            var runClip  = CreateMultiFrameClip($"{dogName}_Run", runPath,
                $"{ANIM_OUTPUT}/{dogName}_Run.anim",  dogName, 8, 0.08f);
            var jumpClip = CreateSingleFrameClip($"{dogName}_Jump", runPath,
                $"{ANIM_OUTPUT}/{dogName}_Jump.anim", dogName, 0.4f);
            var fallClip = CreateSingleFrameClip($"{dogName}_Fall", idlePath,
                $"{ANIM_OUTPUT}/{dogName}_Fall.anim", dogName, 0.4f);

            // ── Estados ────────────────────────────────────────────────────
            var idleState = root.AddState("Idle");
            idleState.motion = idleClip;

            var runState = root.AddState("Run");
            runState.motion = runClip;
            runState.speed  = 1.4f;

            var jumpState = root.AddState("Jump");
            jumpState.motion = jumpClip;

            var fallState = root.AddState("Fall");
            fallState.motion = fallClip;

            root.defaultState = idleState;

            // ── Transiciones ───────────────────────────────────────────────
            // Idle → Run
            var t1 = idleState.AddTransition(runState);
            t1.AddCondition(AnimatorConditionMode.If, 0, "isRunning");
            t1.duration = 0.05f;

            // Run → Idle
            var t2 = runState.AddTransition(idleState);
            t2.AddCondition(AnimatorConditionMode.IfNot, 0, "isRunning");
            t2.duration = 0.05f;

            // Any → Jump (subiendo)
            var t3 = root.AddAnyStateTransition(jumpState);
            t3.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
            t3.AddCondition(AnimatorConditionMode.Greater, 1f, "velocityY");
            t3.duration = 0f; t3.canTransitionToSelf = false;

            // Any → Fall (bajando)
            var t4 = root.AddAnyStateTransition(fallState);
            t4.AddCondition(AnimatorConditionMode.IfNot, 0, "isGrounded");
            t4.AddCondition(AnimatorConditionMode.Less, -1f, "velocityY");
            t4.duration = 0f; t4.canTransitionToSelf = false;

            // Fall → Idle
            var t5 = fallState.AddTransition(idleState);
            t5.AddCondition(AnimatorConditionMode.If, 0, "isGrounded");
            t5.duration = 0.05f;

            // Jump → Fall
            var t6 = jumpState.AddTransition(fallState);
            t6.AddCondition(AnimatorConditionMode.Less, 0f, "velocityY");
            t6.duration = 0.05f;

            EditorUtility.SetDirty(ctrl);
        }

        // ── Clip de frame único ──────────────────────────────────────────
        private static AnimationClip CreateSingleFrameClip(string clipName, string spritePath,
            string outputPath, string dogName, float length)
        {
            var clip = new AnimationClip { name = clipName, frameRate = 12 };
            clip.wrapMode = WrapMode.Loop;

            Sprite spr = LoadFirstSprite(spritePath);
            if (spr != null)
            {
                var binding = new EditorCurveBinding
                {
                    type         = typeof(SpriteRenderer),
                    path         = "",
                    propertyName = "m_Sprite"
                };
                var keyframes = new ObjectReferenceKeyframe[]
                {
                    new ObjectReferenceKeyframe { time = 0f, value = spr },
                    new ObjectReferenceKeyframe { time = length, value = spr }
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            AssetDatabase.CreateAsset(clip, outputPath);
            return clip;
        }

        // ── Clip multi-frame (spritesheet) ────────────────────────────────
        private static AnimationClip CreateMultiFrameClip(string clipName, string spritePath,
            string outputPath, string dogName, int maxFrames, float frameDuration)
        {
            var clip = new AnimationClip { name = clipName, frameRate = 12 };
            clip.wrapMode = WrapMode.Loop;

            var sprites = LoadAllSprites(spritePath);
            if (sprites != null && sprites.Length > 0)
            {
                int frameCount = Mathf.Min(sprites.Length, maxFrames);
                var keyframes  = new ObjectReferenceKeyframe[frameCount];
                for (int i = 0; i < frameCount; i++)
                    keyframes[i] = new ObjectReferenceKeyframe
                        { time = i * frameDuration, value = sprites[i] };

                var binding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer), path = "", propertyName = "m_Sprite"
                };
                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
            }

            AssetDatabase.CreateAsset(clip, outputPath);
            return clip;
        }

        private static Sprite LoadFirstSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite sp) return sp;
            return null;
        }

        private static Sprite[] LoadAllSprites(string path)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            var result = new System.Collections.Generic.List<Sprite>();
            foreach (var a in all)
                if (a is Sprite sp) result.Add(sp);
            if (result.Count == 0)
            {
                var single = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (single != null) result.Add(single);
            }
            return result.ToArray();
        }
    }
}
