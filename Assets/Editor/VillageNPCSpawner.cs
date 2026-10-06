using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Platformer.TopDown;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Editor Tool: crea NPCs aldeanos en Village_Scene usando los sprites de Pixel Crawler.
    /// Menú: Tools > Village > Spawn Village NPCs
    /// </summary>
    public static class VillageNPCSpawner
    {
        // ── Rutas de sprites ───────────────────────────────────────────────────
        private const string PC_ROOT = "Assets/Sprites/PixelCrawler";

        // Peasant_A (aldeana femenina) — sprites individuales por frame
        private static readonly string PEASANT_IDLE_DIR =
            PC_ROOT + "/Entities/Npc's/Citizen_F/Peasant_A/Idle";
        private static readonly string PEASANT_WALK_DIR =
            PC_ROOT + "/Entities/Npc's/Citizen_F/Peasant_A/Walk";

        // Knight
        private static readonly string KNIGHT_IDLE_DIR =
            PC_ROOT + "/Entities/Npc's/Knight/Idle";
        private static readonly string KNIGHT_RUN_DIR =
            PC_ROOT + "/Entities/Npc's/Knight/Run";

        // Wizzard
        private static readonly string WIZZARD_IDLE_DIR =
            PC_ROOT + "/Entities/Npc's/Wizzard/Idle";
        private static readonly string WIZZARD_RUN_DIR =
            PC_ROOT + "/Entities/Npc's/Wizzard/Run";

        // ── Datos de NPCs a spawnear ───────────────────────────────────────────
        private struct NPCData
        {
            public string name;
            public Vector3 position;
            public float patrolDistance;
            public string idleDir;
            public string walkDir;
            public string[] lines;
            public float scale;
        }

        private static readonly NPCData[] NPC_LIST = new NPCData[]
        {
            new NPCData {
                name = "Aldeana Maya",
                position = new Vector3(-8f, 2f, 0f),
                patrolDistance = 2.5f,
                idleDir = PEASANT_IDLE_DIR,
                walkDir = PEASANT_WALK_DIR,
                lines = new[] {
                    "¡Buenos días! ¿Vienes a la feria del pueblo?",
                    "Mi abuela dice que antes había dragones en el bosque...",
                    "¿Puedes ayudarme a encontrar mis gallinas? Se escaparon."
                },
                scale = 1.5f
            },
            new NPCData {
                name = "Aldeana Rosa",
                position = new Vector3(4f, 4f, 0f),
                patrolDistance = 2f,
                idleDir = PEASANT_IDLE_DIR,
                walkDir = PEASANT_WALK_DIR,
                lines = new[] {
                    "El mercader llegó con especias del sur. ¡Pasan rápido!",
                    "Este verano ha sido muy caluroso, ¿no crees?",
                    "Cuidado con los jabalíes al norte del río."
                },
                scale = 1.5f
            },
            new NPCData {
                name = "Aldeana Lena",
                position = new Vector3(-3f, -5f, 0f),
                patrolDistance = 1.8f,
                idleDir = PEASANT_IDLE_DIR,
                walkDir = PEASANT_WALK_DIR,
                lines = new[] {
                    "¿Eres nuevo aquí? Bienvenido a la Aldea de los Pinos.",
                    "Mi esposo cultiva nabos. No son glamorosos, pero alimentan.",
                    "Los niños dicen que vieron luces extrañas en las ruinas."
                },
                scale = 1.5f
            },
            new NPCData {
                name = "Guardia Aldric",
                position = new Vector3(-12f, 0f, 0f),
                patrolDistance = 1f,
                idleDir = KNIGHT_IDLE_DIR,
                walkDir = KNIGHT_RUN_DIR,
                lines = new[] {
                    "Alto. Esta es la entrada principal de la aldea.",
                    "Todo tranquilo por hoy. Que así siga.",
                    "Si buscas posada, sigue recto y gira a la derecha."
                },
                scale = 2f
            },
            new NPCData {
                name = "Mago Erwin",
                position = new Vector3(10f, -3f, 0f),
                patrolDistance = 0.5f,
                idleDir = WIZZARD_IDLE_DIR,
                walkDir = WIZZARD_RUN_DIR,
                lines = new[] {
                    "Hmm... ¿Sientes esa energía arcana? Proviene del Este.",
                    "Llevo 40 años estudiando los cristales. Aún hay misterios.",
                    "No toques el cristal rojo. Te lo digo por experiencia."
                },
                scale = 2f
            }
        };

        // ── Menú ───────────────────────────────────────────────────────────────
        [MenuItem("Tools/Village/Spawn Village NPCs")]
        public static void SpawnNPCs()
        {
            // Verificar que estamos en Village_Scene
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.name.Contains("Village") && !scene.name.Contains("village"))
            {
                bool ok = EditorUtility.DisplayDialog("VillageNPCSpawner",
                    $"La escena activa es '{scene.name}', no 'Village_Scene'.\n¿Continuar de todas formas?",
                    "Sí, continuar", "Cancelar");
                if (!ok) return;
            }

            // Limpiar NPCs viejos
            var existingRoot = GameObject.Find("[Village_NPCs]");
            if (existingRoot != null)
            {
                bool replace = EditorUtility.DisplayDialog("VillageNPCSpawner",
                    "Ya existe [Village_NPCs]. ¿Reemplazar todos los NPCs?",
                    "Sí, reemplazar", "Cancelar");
                if (!replace) return;
                Object.DestroyImmediate(existingRoot);
            }

            // Crear root
            var root = new GameObject("[Village_NPCs]");
            Undo.RegisterCreatedObjectUndo(root, "Spawn Village NPCs");

            int created = 0;
            foreach (var data in NPC_LIST)
            {
                if (CreateNPC(data, root.transform))
                    created++;
            }

            // Marcar escena como sucia
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            EditorUtility.DisplayDialog("VillageNPCSpawner",
                $"✅ {created} NPCs creados en [Village_NPCs].\n\n" +
                "Guarda la escena con Ctrl+S.", "OK");

            Selection.activeGameObject = root;
        }

        // ── Crear un NPC ──────────────────────────────────────────────────────
        private static bool CreateNPC(NPCData data, Transform parent)
        {
            // Cargar sprite idle (primer frame o spritesheet)
            Sprite idleSprite = LoadFirstSprite(data.idleDir);
            if (idleSprite == null)
            {
                Debug.LogWarning($"[VillageNPCSpawner] No se encontró sprite idle en: {data.idleDir}");
                idleSprite = null; // crear igualmente sin sprite
            }

            // Crear GameObject
            var go = new GameObject(data.name);
            go.transform.parent = parent;
            go.transform.position = data.position;
            go.transform.localScale = Vector3.one * data.scale;

            // Añadir SpriteRenderer
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = idleSprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 500 - Mathf.RoundToInt(data.position.y * 10f);

            // Rigidbody2D cinemático
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            // Collider pequeño en pies
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;
            col.offset = new Vector2(0f, -0.3f);
            col.isTrigger = false;

            // Script VillageNPC
            var npc = go.AddComponent<VillageNPC>();
            npc.patrolDistance = data.patrolDistance;
            npc.moveSpeed = 0.8f;
            npc.waitTime = 1.5f;
            npc.inlineDialogueLines = data.lines;

            // Intentar crear Animator con clips
            TryAttachAnimator(go, sr, data);

            Undo.RegisterCreatedObjectUndo(go, $"Create NPC {data.name}");
            Debug.Log($"[VillageNPCSpawner] NPC '{data.name}' creado en {data.position}");
            return true;
        }

        // ── Animator (si existen los frames) ─────────────────────────────────
        private static void TryAttachAnimator(GameObject go, SpriteRenderer sr, NPCData data)
        {
            var idleFrames = LoadSpritesFromDir(data.idleDir);
            var walkFrames = LoadSpritesFromDir(data.walkDir);

            if (idleFrames.Count == 0 && walkFrames.Count == 0) return;

            // Crear carpeta de clips
            string clipDir = $"Assets/Animations/NPCs/{go.name.Replace(" ", "_")}";
            if (!AssetDatabase.IsValidFolder(clipDir))
                Directory.CreateDirectory(clipDir);

            // Crear clips
            AnimationClip idleClip = MakeClip(idleFrames, "Idle", 8);
            AnimationClip walkClip = MakeClip(walkFrames.Count > 0 ? walkFrames : idleFrames, "Walk", 8);

            string idlePath = $"{clipDir}/Idle.anim";
            string walkPath = $"{clipDir}/Walk.anim";

            SaveOrReplaceClip(idleClip, idlePath);
            SaveOrReplaceClip(walkClip, walkPath);

            // Crear AnimatorController
            string ctrlPath = $"{clipDir}/NPC_Controller.controller";
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            ctrl.AddParameter("isWalking", AnimatorControllerParameterType.Bool);

            var rootState = ctrl.layers[0].stateMachine.AddState("Idle");
            rootState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);

            var walkState = ctrl.layers[0].stateMachine.AddState("Walk");
            walkState.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkPath);

            ctrl.layers[0].stateMachine.defaultState = rootState;

            var toWalk = rootState.AddTransition(walkState);
            toWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
            toWalk.hasExitTime = false;

            var toIdle = walkState.AddTransition(rootState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
            toIdle.hasExitTime = false;

            AssetDatabase.SaveAssets();

            // Adjuntar al GameObject
            var animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static Sprite LoadFirstSprite(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir)) return null;
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { dir });
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static List<Sprite> LoadSpritesFromDir(string dir)
        {
            var list = new List<Sprite>();
            if (!AssetDatabase.IsValidFolder(dir)) return list;
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { dir });
            foreach (var g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                // Cargar todos los sub-sprites (si es spritesheet)
                var subs = AssetDatabase.LoadAllAssetsAtPath(p);
                bool addedSub = false;
                foreach (var s in subs)
                {
                    if (s is Sprite sp) { list.Add(sp); addedSub = true; }
                }
                if (!addedSub)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    if (sp != null) list.Add(sp);
                }
            }
            return list;
        }

        private static AnimationClip MakeClip(List<Sprite> frames, string name, int fps)
        {
            var clip = new AnimationClip { name = name };
            clip.frameRate = fps;

            if (frames.Count == 0) return clip;

            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keyframes = new ObjectReferenceKeyframe[frames.Count];
            for (int i = 0; i < frames.Count; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / (float)fps,
                    value = frames[i]
                };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            return clip;
        }

        private static void SaveOrReplaceClip(AnimationClip clip, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                AssetDatabase.SaveAssets();
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
        }
    }
}
