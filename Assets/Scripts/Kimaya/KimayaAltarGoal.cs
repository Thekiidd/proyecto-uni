using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente de rescate final en la cumbre (KIM-14).
    /// - En Ascenso_Kimaya (useThornyCage = true): muestra a Kiara atrapada dentro de la Jaula Espinosa,
    ///   que se rompe (Cage_Broken) al rescatarla.
    /// - En Ascenso_JumpKing_Classic (useThornyCage = false): muestra el Altar de Piedra clásico con Kiara encima.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaAltarGoal : MonoBehaviour
    {
        [Header("Configuración de Meta y Rescate")]
        [Tooltip("Mensaje de éxito que se muestra en el panel de victoria")]
        public string victoryMessage = "¡Has alcanzado la cumbre y rescatado a Kiara!";

        [Tooltip("Si es true, encierra a Kiara en la Jaula Espinosa (nivel Kimaya). Si es false, usa el altar clásico (nivel Jump King).")]
        public bool useThornyCage = true;

        [Tooltip("Congelar físicas del jugador al llegar a la cima para evitar caídas")]
        public bool freezePlayerOnVictory = true;

        [Header("Efectos Opcionales")]
        public ParticleSystem victoryAura;

        private bool _isGoalReached = false;
        private BoxCollider2D _triggerCollider;

        private GameObject _cageBackGo;
        private GameObject _cageFrontGo;
        private GameObject _cageBrokenGo;

        private SpriteRenderer _kiaraSr;
        private Sprite[] _kiaraIdleFrames;
        private Sprite[] _kiaraHappyFrames;
        private float _kiaraAnimTimer = 0f;
        private int _kiaraFrameIdx = 0;

        private static Sprite LoadCagePartSprite(string resPath, float pivotY)
        {
            Texture2D tex = Resources.Load<Texture2D>(resPath);
            if (tex == null)
            {
                Sprite sp = Resources.Load<Sprite>(resPath);
                if (sp != null) tex = sp.texture;
            }
            if (tex != null)
            {
                tex.filterMode = FilterMode.Point;
                return Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, pivotY),
                    36.0f
                );
            }
            return Resources.Load<Sprite>(resPath);
        }

        private void Awake()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (sceneName.Contains("JumpKing_Classic"))
            {
                useThornyCage = false;
            }

            _triggerCollider = GetComponent<BoxCollider2D>();
            if (_triggerCollider != null)
            {
                _triggerCollider.isTrigger = true;
                _triggerCollider.size = useThornyCage ? new Vector2(1.90f, 1.95f) : new Vector2(1.80f, 1.40f);
                _triggerCollider.offset = useThornyCage ? new Vector2(0f, 0.90f) : new Vector2(0f, 0.65f);
            }
        }

        private void Start()
        {
            EnsureCageAndKiaraVisuals();
        }

        public Vector3 GetHeroStandPosition()
        {
            return transform.position + new Vector3(-0.58f, 0.02f, 0f);
        }

        public void EnsureCageAndKiaraVisuals()
        {
            var legacyCompanion = GameObject.Find("Rescued_Companion");
            if (legacyCompanion != null && legacyCompanion != gameObject)
                legacyCompanion.SetActive(false);

            var legacyZarzal = transform.Find("Zarzal_Trap");
            if (legacyZarzal != null)
                legacyZarzal.gameObject.SetActive(false);

            var rootSr = GetComponent<SpriteRenderer>();
            if (useThornyCage)
            {
                if (rootSr != null) rootSr.enabled = false;
                transform.localScale = Vector3.one;
            }
            else
            {
                if (rootSr != null) rootSr.enabled = true;
                transform.localScale = Vector3.one;
            }

            // Kiara en escala pequeña consistente con el resto del juego (0.42f)
            Transform kiaraTr = transform.Find("Kiara_Summit");
            if (kiaraTr == null)
            {
                var kiaraGo = new GameObject("Kiara_Summit");
                kiaraGo.transform.SetParent(transform, false);
                kiaraTr = kiaraGo.transform;
                _kiaraSr = kiaraGo.AddComponent<SpriteRenderer>();
            }
            else
            {
                _kiaraSr = kiaraTr.GetComponent<SpriteRenderer>();
                if (_kiaraSr == null) _kiaraSr = kiaraTr.gameObject.AddComponent<SpriteRenderer>();
            }

            kiaraTr.localPosition = useThornyCage ? new Vector3(0.08f, 0.05f, 0f) : new Vector3(0.22f, 0.04f, 0f);
            kiaraTr.localScale = new Vector3(0.42f, 0.42f, 1f);
            _kiaraSr.sortingOrder = 505;

            Sprite kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_south");
            if (kiaraSprite == null) kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_east");
            if (kiaraSprite != null) _kiaraSr.sprite = kiaraSprite;

            _kiaraIdleFrames  = Resources.LoadAll<Sprite>("NPCs/Kiara/Idle_South");
            _kiaraHappyFrames = Resources.LoadAll<Sprite>("NPCs/Kiara/Bark_South");

            if (useThornyCage)
            {
                Sprite sprBack   = LoadCagePartSprite("TowerMap/Props/Cage_Back", 0.16f);
                Sprite sprFront  = LoadCagePartSprite("TowerMap/Props/Cage_FrontBars", 0.16f);
                Sprite sprBroken = LoadCagePartSprite("TowerMap/Props/Cage_Broken", 0.10f);

                Vector3 cageScale = new Vector3(0.86f, 0.86f, 1f);
                _cageBackGo   = GetOrCreatePart("Cage_Back", Vector3.zero, cageScale, 500, sprBack);
                _cageFrontGo  = GetOrCreatePart("Cage_FrontBars", Vector3.zero, cageScale, 510, sprFront);
                _cageBrokenGo = GetOrCreatePart("Cage_Broken", Vector3.zero, cageScale, 500, sprBroken);
                UpdateCageStateVisuals(_isGoalReached);
            }
            else
            {
                HidePartIfPresent("Cage_Back");
                HidePartIfPresent("Cage_FrontBars");
                HidePartIfPresent("Cage_Broken");
            }
        }

        private void HidePartIfPresent(string partName)
        {
            Transform tr = transform.Find(partName);
            if (tr != null) tr.gameObject.SetActive(false);
        }

        private GameObject GetOrCreatePart(string partName, Vector3 localPos, Vector3 localScale, int sortOrder, Sprite sprite)
        {
            Transform tr = transform.Find(partName);
            GameObject go;
            if (tr == null)
            {
                go = new GameObject(partName);
                go.transform.SetParent(transform, false);
            }
            else
            {
                go = tr.gameObject;
            }
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr == null) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortOrder;
            sr.color = Color.white;
            return go;
        }

        private void UpdateCageStateVisuals(bool rescued)
        {
            if (!useThornyCage) return;
            if (_cageBackGo != null)   _cageBackGo.SetActive(!rescued);
            if (_cageFrontGo != null)  _cageFrontGo.SetActive(!rescued);
            if (_cageBrokenGo != null) _cageBrokenGo.SetActive(rescued);
        }

        private void Update()
        {
            if (_kiaraSr != null)
            {
                var activeFrames = (_isGoalReached && _kiaraHappyFrames != null && _kiaraHappyFrames.Length > 0)
                    ? _kiaraHappyFrames
                    : _kiaraIdleFrames;
                if (activeFrames != null && activeFrames.Length > 0)
                {
                    _kiaraAnimTimer += Time.deltaTime;
                    if (_kiaraAnimTimer >= 0.125f)
                    {
                        _kiaraAnimTimer -= 0.125f;
                        _kiaraFrameIdx = (_kiaraFrameIdx + 1) % activeFrames.Length;
                        _kiaraSr.sprite = activeFrames[_kiaraFrameIdx];
                    }
                }
            }

            if (_isGoalReached || !useThornyCage) return;

            float breath = 0.86f + 0.012f * Mathf.Sin(Time.time * 2.8f);
            Vector3 cageScale = new Vector3(0.86f, breath, 1f);
            if (_cageBackGo != null)  _cageBackGo.transform.localScale = cageScale;
            if (_cageFrontGo != null) _cageFrontGo.transform.localScale = cageScale;
        }

        public void ForceRescue()
        {
            if (_isGoalReached) return;
            _isGoalReached = true;
            if (_triggerCollider != null) _triggerCollider.enabled = false;
            UpdateCageStateVisuals(true);
            if (victoryAura != null) victoryAura.Play();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isGoalReached) return;

            var player = other.GetComponent<KimayaJumpKingController>();
            if (player == null && !other.CompareTag("Player") && !other.name.Contains("Player"))
            {
                return;
            }

            ForceRescue();

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Victory);
            }

            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.TriggerVictory(victoryMessage, GetHeroStandPosition());
            }
        }

        public void ResetGoal()
        {
            _isGoalReached = false;
            if (_triggerCollider != null) _triggerCollider.enabled = true;
            UpdateCageStateVisuals(false);
        }
    }
}
