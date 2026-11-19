using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Constructor y sincronizador del mapa vertical matemático estilo Jump King de Kimaya (5 Pisos: Ascenso_Kimaya):
    /// - 42 plataformas tácticas pequeñas con saltos matemáticos calibrados para Falin y Mía.
    /// - 12 trampas de Picos Espinosos del Zarzal (KimayaSpikeHazard) que empujan al jugador al vacío si los pisa.
    /// - Cumbre sellada con la Jaula Espinosa y Kiara adentro.
    /// </summary>
    public static class KimayaCustomTowerMap
    {
        public struct TowerPlat
        {
            public string name;
            public float cx;
            public float cy;
            public float w;
            public float h;

            public TowerPlat(string name, float cx, float cy, float w, float h)
            {
                this.name = name;
                this.cx = cx;
                this.cy = cy;
                this.w = w;
                this.h = h;
            }
        }

        public struct TowerSpike
        {
            public string name;
            public float cx;
            public float cy;
            public float w;
            public float h;
            public int pushDir;
            public bool isCluster;

            public TowerSpike(string name, float cx, float cy, float w, float h, int pushDir, bool isCluster)
            {
                this.name = name;
                this.cx = cx;
                this.cy = cy;
                this.w = w;
                this.h = h;
                this.pushDir = pushDir;
                this.isCluster = isCluster;
            }
        }

        public static readonly Vector3 PlayerSpawnPosition   = new Vector3(0.0000f, 0.8500f, 0f);
        public static readonly Vector3 MidCheckpointPosition = new Vector3(-0.2222f, 21.78f, 0f);
        public static readonly Vector3 SummitCagePosition    = new Vector3(0.0000f, 46.3889f, 0f);

        public static readonly Vector3[] ScoobyCookiePositions = new Vector3[]
        {
            new Vector3(-5.1944f,  8.65f, 0f),  // Piso 1: Rama alta izquierda (F1_Left_ScoobyBough)
            new Vector3( 4.4722f, 27.32f, 0f),  // Piso 3: Repisa derecha del Crepúsculo (F3_Right_ScoobyLedge)
            new Vector3(-5.1389f, 38.43f, 0f),  // Piso 4: Bastión superior izquierdo (F4_Left_ScoobyKeep)
        };

        public static readonly TowerPlat[][] Custom5Floors = new TowerPlat[][]
        {
            // Piso 1
            new TowerPlat[]
            {
                new TowerPlat("F1_Wall_Left", -6.4167f, 5.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F1_Wall_Right", 6.4167f, 5.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F1_Base_SoilFloor", -0.8333f, 0.4444f, 5.6667f, 0.8889f),
                new TowerPlat("F1_Right_LowRoot", 4.1944f, 2.1944f, 3.9444f, 0.7222f),
                new TowerPlat("F1_Center_TTrunk", -0.1389f, 4.0000f, 2.8333f, 0.7778f),
                new TowerPlat("F1_Left_MidBranch", -3.6389f, 5.6944f, 2.6111f, 0.7222f),
                new TowerPlat("F1_Center_HighPeg", 0.0556f, 7.4167f, 2.5556f, 0.7222f),
                new TowerPlat("F1_Left_ScoobyBough", -5.0000f, 7.7222f, 2.3333f, 0.6667f),
                new TowerPlat("F1_TopRight_Branch", 3.6944f, 9.1389f, 2.7222f, 0.7222f),
                new TowerPlat("F1_TrapLedge_L", -5.5556f, 5.6111f, 1.2222f, 0.5556f),
            },
            // Piso 2
            new TowerPlat[]
            {
                new TowerPlat("F2_Wall_Left", -6.4167f, 15.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F2_Wall_Right", 6.4167f, 15.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F2_LowCenter_Bough", -0.3611f, 10.9167f, 2.7222f, 0.7222f),
                new TowerPlat("F2_Left_TrunkLedge", -4.2778f, 12.6944f, 2.6667f, 0.7222f),
                new TowerPlat("F2_MidCenter_Peg", -0.1944f, 14.4722f, 2.7222f, 0.7222f),
                new TowerPlat("F2_Right_Branch", 3.6944f, 16.2500f, 2.6111f, 0.7222f),
                new TowerPlat("F2_MidLeft_Peg", -0.4444f, 17.9722f, 2.6667f, 0.7222f),
                new TowerPlat("F2_TopRight_Exit", 3.6111f, 19.6111f, 2.7778f, 0.6667f),
                new TowerPlat("F2_TrapLedge_R", 5.5833f, 16.1667f, 1.1667f, 0.5556f),
            },
            // Piso 3
            new TowerPlat[]
            {
                new TowerPlat("F3_Wall_Left", -6.4167f, 25.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F3_Wall_Right", 6.4167f, 25.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F3_Checkpoint_Bridge", -0.3056f, 21.3333f, 3.1667f, 0.7778f),
                new TowerPlat("F3_Left_MossStone", -4.2222f, 23.0278f, 2.6667f, 0.7222f),
                new TowerPlat("F3_Center_Step", -0.3333f, 24.7500f, 2.6667f, 0.7222f),
                new TowerPlat("F3_Right_ScoobyLedge", 4.2500f, 26.3611f, 2.5000f, 0.7222f),
                new TowerPlat("F3_MidLeft_Brick", -4.0000f, 26.5833f, 2.6667f, 0.7222f),
                new TowerPlat("F3_TopCenter_Float", -0.1111f, 28.4167f, 2.6667f, 0.7222f),
                new TowerPlat("F3_TrapLedge_L", -5.8611f, 22.9444f, 0.6111f, 0.5556f),
                new TowerPlat("F3_TrapLedge_R", 5.8333f, 26.2778f, 0.6667f, 0.5556f),
            },
            // Piso 4
            new TowerPlat[]
            {
                new TowerPlat("F4_Wall_Left", -6.4167f, 35.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F4_Wall_Right", 6.4167f, 35.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F4_LowRight_Ledge", 3.5833f, 30.3611f, 2.6111f, 0.7222f),
                new TowerPlat("F4_LowCenter_Block", -0.3889f, 32.1389f, 2.6667f, 0.7222f),
                new TowerPlat("F4_Left_Bastion", -4.2222f, 33.8611f, 2.6667f, 0.7222f),
                new TowerPlat("F4_MidCenter_Float", -0.2222f, 35.6389f, 2.6667f, 0.7222f),
                new TowerPlat("F4_Left_ScoobyKeep", -5.0000f, 37.4167f, 2.3333f, 0.7222f),
                new TowerPlat("F4_Right_HighLedge", 3.5833f, 37.4722f, 2.6111f, 0.7222f),
                new TowerPlat("F4_TopCenter_Gate", -0.2222f, 39.2500f, 2.6667f, 0.7222f),
                new TowerPlat("F4_TrapLedge_R1", 5.5278f, 30.2778f, 1.2778f, 0.5556f),
                new TowerPlat("F4_TrapLedge_R2", 5.5278f, 37.3889f, 1.2778f, 0.5556f),
            },
            // Piso 5
            new TowerPlat[]
            {
                new TowerPlat("F5_Wall_Left", -6.4167f, 45.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F5_Wall_Right", 6.4167f, 45.0000f, 0.5000f, 10.0000f),
                new TowerPlat("F5_LowLeft_Step", -4.1111f, 41.0278f, 2.6667f, 0.7222f),
                new TowerPlat("F5_MidCenter_Block", -0.3333f, 42.6389f, 2.6667f, 0.7222f),
                new TowerPlat("F5_MidRight_Step", 3.4722f, 44.2500f, 2.7222f, 0.7222f),
                new TowerPlat("F5_Summit_Cliff_L", -5.2500f, 45.8889f, 1.8333f, 0.8889f),
                new TowerPlat("F5_Summit_Cliff_R", 5.5556f, 45.8889f, 1.2222f, 0.8889f),
                new TowerPlat("F5_Cage_Pedestal", -0.0833f, 45.9722f, 2.8333f, 0.8333f),
                new TowerPlat("F5_Top_Ceiling", 0.0000f, 50.4500f, 14.0000f, 0.9000f),
            },
        };

        public static readonly TowerSpike[] CustomTowerSpikes = new TowerSpike[]
        {
            new TowerSpike("Spike_F1_LeftOuter", -5.5833f, 6.0694f, 0.8889f, 0.3611f, 1, false),
            new TowerSpike("Spike_F2_RightOuter", 5.5556f, 16.6250f, 0.8333f, 0.3611f, -1, false),
            new TowerSpike("Spike_F3_LeftOuter", -5.8889f, 23.4028f, 0.3889f, 0.3611f, 1, true),
            new TowerSpike("Spike_F3_ScoobyOuter", 5.8056f, 26.9028f, 0.4444f, 0.3611f, -1, true),
            new TowerSpike("Spike_F4_LowRightOuter", 5.5000f, 30.7361f, 0.9444f, 0.3611f, -1, false),
            new TowerSpike("Spike_F4_HighRightOuter", 5.5000f, 37.8472f, 0.9444f, 0.3611f, -1, false),
            new TowerSpike("Spike_F5_CliffLeft", -5.4444f, 46.5139f, 1.0556f, 0.3611f, 1, false),
        };

        public static bool IsCustomKimayaScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            if (sceneName.Contains("JumpKing_Classic") || sceneName.Contains("Test_Mecanica"))
                return false;
            return sceneName.Contains("Kimaya") || sceneName.Contains("BosqueZarzal");
        }

        private static Sprite LoadTowerScreenSprite(int index)
        {
            string resPath = $"TowerMap/Kimaya_Tower_Screen{index}";
            Texture2D tex = Resources.Load<Texture2D>(resPath);
            if (tex == null)
            {
                var sp = Resources.Load<Sprite>(resPath);
                if (sp != null) tex = sp.texture;
            }
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 36.0f);
            }
            return Resources.Load<Sprite>(resPath);
        }

        public static void ApplyCustomTowerToCurrentScene(bool forceApply = false)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (!forceApply && !IsCustomKimayaScene(sceneName))
                return;

            GameObject bgRoot = GameObject.Find("Backgrounds");
            if (bgRoot == null) bgRoot = new GameObject("Backgrounds");
            for (int i = bgRoot.transform.childCount - 1; i >= 0; i--)
                SafeDestroy(bgRoot.transform.GetChild(i).gameObject);

            for (int s = 0; s < 5; s++)
            {
                Sprite scrSpr = LoadTowerScreenSprite(s);
                if (scrSpr == null) continue;
                var bgGo = new GameObject($"BG_Kimaya_Tower_Screen_{s + 1}");
                bgGo.transform.SetParent(bgRoot.transform, false);
                bgGo.transform.position = new Vector3(0f, s * 10f + 5f, 0f);
                var sr = bgGo.AddComponent<SpriteRenderer>();
                sr.sprite = scrSpr;
                sr.sortingOrder = -10;
            }

            int groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");
            var sharedNoFriction = new PhysicsMaterial2D("KimayaTowerMat") { friction = 0f, bounciness = 0f };

            GameObject wallsRoot = GameObject.Find("BoundaryWalls");
            if (wallsRoot == null) wallsRoot = new GameObject("BoundaryWalls");
            for (int i = wallsRoot.transform.childCount - 1; i >= 0; i--)
                SafeDestroy(wallsRoot.transform.GetChild(i).gameObject);

            CreateBoundaryBox(wallsRoot.transform, "Wall_Left",   -6.667f, 25.5f,  0.4f, 53f, groundLayer, sharedNoFriction);
            CreateBoundaryBox(wallsRoot.transform, "Wall_Right",   6.667f, 25.5f,  0.4f, 53f, groundLayer, sharedNoFriction);
            CreateBoundaryBox(wallsRoot.transform, "Wall_Bottom",  0.000f, -0.5f, 15.0f, 1.0f, groundLayer, sharedNoFriction);
            CreateBoundaryBox(wallsRoot.transform, "Wall_Ceiling", 0.000f, 50.5f, 15.0f, 1.0f, groundLayer, sharedNoFriction);

            GameObject platformsRoot = GameObject.Find("Platforms");
            if (platformsRoot == null) platformsRoot = new GameObject("Platforms");
            for (int i = platformsRoot.transform.childCount - 1; i >= 0; i--)
                SafeDestroy(platformsRoot.transform.GetChild(i).gameObject);

            for (int r = 0; r < Custom5Floors.Length; r++)
            {
                var roomFolder = new GameObject($"Floor_{r + 1}_Colliders");
                roomFolder.transform.SetParent(platformsRoot.transform, false);
                roomFolder.layer = groundLayer;

                foreach (var p in Custom5Floors[r])
                {
                    var platGo = new GameObject(p.name);
                    platGo.transform.SetParent(roomFolder.transform, false);
                    platGo.layer = groundLayer;
                    platGo.transform.position = new Vector3(p.cx, p.cy, 0f);

                    var col = platGo.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(p.w, p.h);
                    col.sharedMaterial = sharedNoFriction;
                    col.usedByEffector = false;

                    var surf = platGo.AddComponent<KimayaSurface>();
                    surf.surfaceType = SurfaceType.Normal;
                }
            }

            // Crear las 12 trampas de Picos Espinosos del Zarzal (KimayaSpikeHazard)
            var spikesFolder = new GameObject("Spikes_Hazards");
            spikesFolder.transform.SetParent(platformsRoot.transform, false);
            Sprite rowSpikeSpr = Resources.Load<Sprite>("TowerMap/Props/Spikes_Row");
            Sprite clsSpikeSpr = Resources.Load<Sprite>("TowerMap/Props/Spikes_Cluster");

            foreach (var sp in CustomTowerSpikes)
            {
                var spGo = new GameObject(sp.name);
                spGo.transform.SetParent(spikesFolder.transform, false);
                spGo.transform.position = new Vector3(sp.cx, sp.cy, 0f);

                var bc = spGo.AddComponent<BoxCollider2D>();
                bc.isTrigger = true;
                bc.size = new Vector2(sp.w + 0.08f, sp.h + 0.14f);
                bc.offset = new Vector2(0f, 0.04f);

                var hazard = spGo.AddComponent<KimayaSpikeHazard>();
                hazard.pushDirection = sp.pushDir;

                Sprite sprToUse = sp.isCluster ? (clsSpikeSpr ?? rowSpikeSpr) : (rowSpikeSpr ?? clsSpikeSpr);
                if (sprToUse != null)
                {
                    var visGo = new GameObject("SpikeVisual");
                    visGo.transform.SetParent(spGo.transform, false);
                    visGo.transform.localPosition = new Vector3(0f, -sp.h * 0.45f, 0f);
                    var sr = visGo.AddComponent<SpriteRenderer>();
                    sr.sprite = sprToUse;
                    sr.sortingOrder = 12;
                }
            }

            var relics = Object.FindObjectsByType<KimayaRelic>(FindObjectsInactive.Include);
            string[] cookieNames = new string[]
            {
                "Scooby Galleta del Bosque Mágico",
                "Scooby Galleta del Crepúsculo",
                "Scooby Galleta del Zarzal Sombrío"
            };
            int[] cookiePts = new int[] { 100, 150, 200 };
            System.Array.Sort(relics, (a, b) => a.transform.position.y.CompareTo(b.transform.position.y));
            for (int i = 0; i < relics.Length && i < ScoobyCookiePositions.Length; i++)
            {
                relics[i].transform.position = ScoobyCookiePositions[i];
                relics[i].relicName = cookieNames[i];
                relics[i].scoreValue = cookiePts[i];
            }

            var cp = Object.FindAnyObjectByType<KimayaCheckpointTrigger>();
            if (cp != null)
                cp.transform.position = MidCheckpointPosition;

            var legacyCompanion = GameObject.Find("Rescued_Companion");
            if (legacyCompanion != null) legacyCompanion.SetActive(false);

            var altarGoal = Object.FindAnyObjectByType<KimayaAltarGoal>();
            if (altarGoal == null)
            {
                var altarGo = GameObject.Find("Altar_Rescate");
                if (altarGo == null) altarGo = new GameObject("Altar_Rescate");
                if (altarGo.GetComponent<BoxCollider2D>() == null)
                {
                    var bc = altarGo.AddComponent<BoxCollider2D>();
                    bc.isTrigger = true;
                }
                altarGoal = altarGo.AddComponent<KimayaAltarGoal>();
            }
            altarGoal.useThornyCage = true;
            altarGoal.transform.position = SummitCagePosition;
            altarGoal.EnsureCageAndKiaraVisuals();

            var screenCam = Object.FindAnyObjectByType<JumpKingScreenCamera>();
            if (screenCam != null) screenCam.maxScreenIndex = 4;

            var player = Object.FindAnyObjectByType<KimayaJumpKingController>();
            if (player != null && player.transform.position.y < 2.5f)
            {
                player.transform.position = PlayerSpawnPosition;
                player.respawnPoint = PlayerSpawnPosition;
            }
        }

        private static void CreateBoundaryBox(Transform parent, string name, float cx, float cy, float w, float h, int layer, PhysicsMaterial2D mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.layer = layer;
            go.transform.position = new Vector3(cx, cy, 0f);
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(w, h);
            bc.sharedMaterial = mat;
        }

        private static void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying)
                Object.Destroy(go);
            else
                Object.DestroyImmediate(go);
        }
    }
}
