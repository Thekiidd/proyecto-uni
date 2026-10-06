using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Kimaya;

namespace Platformer.EditorTools
{
    /// <summary>
    /// Construye la escena Hub — vista isométrica 2D.
    /// Un claro del bosque con:
    ///   - Falin y Mia caminando libremente (top-down isométrico)
    ///   - Portal 1 → Level_01_Bosque (misión principal)
    ///   - Portal 2 → próxima misión (bloqueado por ahora)
    ///   - Maya como NPC guía
    ///   - Árboles, pasto, decoración
    /// Tools > Kimaya > [3] Build Hub Isométrico
    /// </summary>
    public static class KimayaHubBuilder
    {
        private const string SCENE_PATH = "Assets/Scenes/Hub.unity";
        private const string FALIN_SPR  = "Assets/Sprites/Dogs/Great-Dane/Great-Dane-idle.png";
        private const string MIA_SPR    = "Assets/Sprites/Dogs/Schnauzer/Schnauzer-Idle.png";

        [MenuItem("Tools/Kimaya/[3] Build Hub Isométrico")]
        public static void Build()
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Cámara isométrica ──────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(0f, 8f, -12f);
            camGo.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic     = true;
            cam.orthographicSize = 7f;
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.06f, 0.14f, 0.06f);
            camGo.AddComponent<AudioListener>();

            // ── Terreno del Hub ────────────────────────────────────────────
            BuildTerrain();

            // ── Portales ───────────────────────────────────────────────────
            BuildPortal("Portal_Mision1", new Vector3(-4f, 0.5f, 2f),
                new Color(0.3f, 0.8f, 1f), "Level_01_Bosque",
                "Misión 1\nEl Bosque del Eco\n[RESCATAR A KIARA]", false);

            BuildPortal("Portal_Mision2", new Vector3(4f, 0.5f, 2f),
                new Color(0.5f, 0.5f, 0.5f), "",
                "Misión 2\nLas Cuevas del Zarzal\n[PRÓXIMAMENTE...]", true);

            // ── Maya (NPC guía del hub) ────────────────────────────────────
            BuildMayaHub();

            // ── Jugadores ──────────────────────────────────────────────────
            BuildHubPlayers();

            // ── Decoración ─────────────────────────────────────────────────
            BuildDecoration();

            // ── UI del Hub ─────────────────────────────────────────────────
            BuildHubUI();

            // ── Hub Manager ────────────────────────────────────────────────
            var mgr = new GameObject("HubManager");
            mgr.AddComponent<HubManager>();

            SaveScene(scene, SCENE_PATH, 2);

            EditorUtility.DisplayDialog("✅ Hub Isométrico creado",
                "Hub.unity listo.\n\n" +
                "🗺️ Vista isométrica con:\n" +
                "  • Portal 1 → Level_01_Bosque (Misión principal)\n" +
                "  • Portal 2 → Bloqueado (próxima misión)\n" +
                "  • Falin y Mia caminando con WASD / Flechas\n" +
                "  • Maya como NPC guía\n\n" +
                "Controles en el Hub:\n" +
                "  WASD: mover\n" +
                "  TAB: cambiar personaje\n" +
                "  E: interactuar / entrar a portal", "OK");
        }

        // ── Terreno ───────────────────────────────────────────────────────
        private static void BuildTerrain()
        {
            // Suelo principal (pasto)
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Suelo_Hub";
            ground.transform.position   = new Vector3(0f, 0f, 0f);
            ground.transform.localScale = new Vector3(2f, 1f, 2f);
            var groundMR = ground.GetComponent<MeshRenderer>();
            groundMR.sharedMaterial = CreateColorMaterial(new Color(0.15f, 0.38f, 0.10f));

            // Zona central más clara (plaza)
            var plaza = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plaza.name = "Plaza_Centro";
            plaza.transform.position   = new Vector3(0f, 0.02f, 0f);
            plaza.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            plaza.GetComponent<MeshRenderer>().sharedMaterial =
                CreateColorMaterial(new Color(0.25f, 0.55f, 0.15f));
            Destroy(plaza.GetComponent<Collider>());
        }

        // ── Portal ────────────────────────────────────────────────────────
        private static void BuildPortal(string name, Vector3 pos, Color color,
            string targetScene, string labelText, bool locked)
        {
            var root = new GameObject(name);
            root.transform.position = pos;

            // Anillo del portal (cilindro hueco simulado con aros)
            for (int i = 0; i < 3; i++)
            {
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"Ring_{i}";
                ring.transform.SetParent(root.transform);
                ring.transform.localPosition = new Vector3(0f, i * 0.06f, 0f);
                ring.transform.localScale    = new Vector3(1.5f - i * 0.15f, 0.05f, 1.5f - i * 0.15f);
                var c = locked ? new Color(0.4f, 0.4f, 0.4f) : color;
                ring.GetComponent<MeshRenderer>().sharedMaterial = CreateColorMaterial(c);
                Destroy(ring.GetComponent<Collider>());
            }

            // Centro del portal (plano brillante)
            var center = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            center.name = "Center";
            center.transform.SetParent(root.transform);
            center.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            center.transform.localScale    = new Vector3(1.2f, 0.02f, 1.2f);
            var centerColor = locked
                ? new Color(0.3f, 0.3f, 0.3f, 0.6f)
                : new Color(color.r, color.g, color.b, 0.7f);
            center.GetComponent<MeshRenderer>().sharedMaterial = CreateColorMaterial(centerColor);
            Destroy(center.GetComponent<Collider>());

            // Columnas decorativas
            for (int side = -1; side <= 1; side += 2)
            {
                var col = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                col.name = $"Col_{side}";
                col.transform.SetParent(root.transform);
                col.transform.localPosition = new Vector3(side * 0.85f, 1f, 0f);
                col.transform.localScale    = new Vector3(0.15f, 1.2f, 0.15f);
                col.GetComponent<MeshRenderer>().sharedMaterial =
                    CreateColorMaterial(new Color(0.4f, 0.35f, 0.25f));
                Destroy(col.GetComponent<Collider>());
            }

            // Colisión invisible para detectar entrada
            var triggerGo = new GameObject("PortalTrigger");
            triggerGo.transform.SetParent(root.transform);
            triggerGo.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            var col2 = triggerGo.AddComponent<SphereCollider>();
            col2.radius    = 1.2f;
            col2.isTrigger = true;

            // Script de portal
            var portal = root.AddComponent<HubPortal>();
            portal.targetScene = targetScene;
            portal.isLocked    = locked;
            portal.portalColor = color;
            portal.labelText   = labelText;
        }

        // ── Maya NPC en el hub ────────────────────────────────────────────
        private static void BuildMayaHub()
        {
            var go = new GameObject("Maya_Hub");
            go.transform.position = new Vector3(0f, 0.5f, -2f);

            // Esfera como placeholder visual de Maya
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(go.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale    = new Vector3(0.5f, 0.7f, 0.5f);
            sphere.GetComponent<MeshRenderer>().sharedMaterial =
                CreateColorMaterial(new Color(0.75f, 0.45f, 1f));
            Destroy(sphere.GetComponent<Collider>());

            // Trigger de diálogo
            var col = go.AddComponent<SphereCollider>();
            col.radius = 2.5f; col.isTrigger = true;

            go.AddComponent<HubMayaDialogue>();
        }

        // ── Jugadores en el Hub ────────────────────────────────────────────
        private static void BuildHubPlayers()
        {
            Sprite falinSpr = LoadSprite(FALIN_SPR);
            Sprite miaSpr   = LoadSprite(MIA_SPR);

            var falin = BuildHubPlayer("Falin_Hub", falinSpr,
                new Color(0.9f, 0.75f, 0.3f), new Vector3(-1f, 0.5f, 0f));
            var mia   = BuildHubPlayer("Mia_Hub",   miaSpr,
                new Color(0.4f, 0.8f, 1f, 0.7f), new Vector3(1f, 0.4f, 0f));

            // Hub switch controller
            var sw = new GameObject("HubSwitcher");
            var hsc = sw.AddComponent<HubCharacterController>();
            hsc.falinGo = falin;
            hsc.miaGo   = mia;
        }

        private static GameObject BuildHubPlayer(string name, Sprite spr, Color color, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.tag = "Player";

            // Billboard sprite (siempre mira a la cámara)
            var sprGo = new GameObject("Sprite");
            sprGo.transform.SetParent(go.transform);
            sprGo.transform.localPosition = Vector3.zero;
            var sr = sprGo.AddComponent<SpriteRenderer>();
            sr.sprite = spr; sr.color = color;
            sr.sortingOrder = 10;
            sprGo.transform.localScale = Vector3.one * 1.2f;

            // Collider
            go.AddComponent<CapsuleCollider>().height = 1.2f;

            // Rigidbody 3D (para el hub 3D)
            var rb = go.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            return go;
        }

        // ── Decoración del Hub ────────────────────────────────────────────
        private static void BuildDecoration()
        {
            // Árboles alrededor del claro
            float[] angles  = { 0, 40, 80, 120, 160, 200, 240, 280, 320 };
            float   radius  = 9f;
            foreach (float a in angles)
            {
                float rad = a * Mathf.Deg2Rad;
                var tree = new GameObject($"Arbol_{(int)a}");
                tree.transform.position = new Vector3(
                    Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius);

                var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.SetParent(tree.transform);
                trunk.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                trunk.transform.localScale    = new Vector3(0.3f, 0.9f, 0.3f);
                trunk.GetComponent<MeshRenderer>().sharedMaterial =
                    CreateColorMaterial(new Color(0.38f, 0.24f, 0.12f));
                Destroy(trunk.GetComponent<Collider>());

                var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.transform.SetParent(tree.transform);
                leaves.transform.localPosition = new Vector3(0f, 2.2f, 0f);
                leaves.transform.localScale    = new Vector3(1.4f, 1.8f, 1.4f);
                leaves.GetComponent<MeshRenderer>().sharedMaterial =
                    CreateColorMaterial(new Color(0.12f, 0.38f, 0.08f));
                Destroy(leaves.GetComponent<Collider>());

                // Collider del árbol
                var tc = tree.AddComponent<CapsuleCollider>();
                tc.height = 3f; tc.radius = 0.5f;
                tc.center = new Vector3(0f, 1.5f, 0f);
            }

            // Pocas piedras decorativas
            Vector3[] stonePositions = {
                new Vector3(-2f, 0.1f, 4f), new Vector3(3f, 0.1f, -3f), new Vector3(-5f, 0.1f, 1f)
            };
            foreach (var sp in stonePositions)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "Piedra";
                stone.transform.position   = sp;
                stone.transform.localScale = new Vector3(0.4f, 0.25f, 0.35f);
                stone.GetComponent<MeshRenderer>().sharedMaterial =
                    CreateColorMaterial(new Color(0.45f, 0.43f, 0.4f));
                Destroy(stone.GetComponent<Collider>());
            }

            // Flores pequeñas
            for (int i = 0; i < 15; i++)
            {
                var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flower.name = $"Flor_{i}";
                float fx = Random.Range(-7f, 7f);
                float fz = Random.Range(-7f, 7f);
                flower.transform.position   = new Vector3(fx, 0.08f, fz);
                flower.transform.localScale = Vector3.one * 0.12f;
                Color[] flColors = {
                    new Color(1f, 0.4f, 0.6f), new Color(1f, 0.9f, 0.3f),
                    new Color(0.6f, 0.4f, 1f), new Color(1f, 0.6f, 0.2f)
                };
                flower.GetComponent<MeshRenderer>().sharedMaterial =
                    CreateColorMaterial(flColors[i % flColors.Length]);
                Destroy(flower.GetComponent<Collider>());
            }
        }

        // ── UI del Hub ─────────────────────────────────────────────────────
        private static void BuildHubUI()
        {
            var cvGo = new GameObject("Canvas_Hub");
            var cv   = cvGo.AddComponent<Canvas>();
            cv.renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = cvGo.AddComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1920, 1080);
            cvGo.AddComponent<GraphicRaycaster>();

            // Título del Hub
            CreateUIText(cvGo.transform, "HubTitle", "🌿 El Bosque del Eco",
                new Vector2(0, 480), new Vector2(600, 60),
                new Color(0.6f, 1f, 0.45f, 0.8f), 30, FontStyle.Bold);

            // Personaje activo
            CreateUIText(cvGo.transform, "ActiveChar", "▶ Falin",
                new Vector2(-880, 470), new Vector2(250, 45),
                new Color(0.9f, 0.75f, 0.3f), 24, FontStyle.Bold);

            // Hint de controles
            CreateUIText(cvGo.transform, "HubHint",
                "WASD / ←→↑↓: Mover   |   TAB: Cambiar personaje   |   E: Entrar al portal",
                new Vector2(0, -470), new Vector2(1000, 35),
                new Color(0.65f, 0.8f, 0.55f, 0.5f), 16, FontStyle.Normal);

            // Cuadro de diálogo de portal (aparece al acercarse)
            var portalMsgGo = new GameObject("PortalMsg");
            portalMsgGo.transform.SetParent(cvGo.transform, false);
            portalMsgGo.SetActive(false);
            var pmImg = portalMsgGo.AddComponent<Image>();
            pmImg.color = new Color(0.04f, 0.1f, 0.04f, 0.9f);
            var pmRT = portalMsgGo.GetComponent<RectTransform>();
            pmRT.anchorMin = pmRT.anchorMax = pmRT.pivot = new Vector2(0.5f, 0.5f);
            pmRT.anchoredPosition = new Vector2(0, -350); pmRT.sizeDelta = new Vector2(500, 80);

            var pmTxtGo = new GameObject("PortalMsgText"); pmTxtGo.transform.SetParent(portalMsgGo.transform, false);
            var pmTxt = pmTxtGo.AddComponent<Text>();
            pmTxt.text = "[E] Entrar a la misión"; pmTxt.color = new Color(0.7f, 1f, 0.55f);
            pmTxt.fontSize = 26; pmTxt.alignment = TextAnchor.MiddleCenter;
            pmTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var pmTxtRT = pmTxtGo.GetComponent<RectTransform>();
            pmTxtRT.anchorMin = Vector2.zero; pmTxtRT.anchorMax = Vector2.one;
            pmTxtRT.offsetMin = pmTxtRT.offsetMax = Vector2.zero;
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private static Material CreateColorMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }

        private static void CreateUIText(Transform parent, string name, string text,
            Vector2 pos, Vector2 size, Color color, int fontSize, FontStyle style)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.text = text; t.color = color; t.fontSize = fontSize;
            t.fontStyle = style; t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        private static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s != null) return s;
            foreach (var a in AssetDatabase.LoadAllAssetsAtPath(path))
                if (a is Sprite sp) return sp;
            return null;
        }

        private static void SaveScene(UnityEngine.SceneManagement.Scene scene, string path, int idx)
        {
            string dir = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(dir)) Directory.CreateDirectory(dir);
            EditorSceneManager.SaveScene(scene, path);
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool found = false;
            foreach (var s in list) if (s.path == path) { found = true; break; }
            if (!found) { list.Insert(Mathf.Min(idx, list.Count), new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = list.ToArray(); }
            AssetDatabase.Refresh();
        }

        private static void Destroy(Object obj) => Object.DestroyImmediate(obj);
    }
}
