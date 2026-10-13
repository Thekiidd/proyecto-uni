using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    public enum DogDirection
    {
        South, // Down (0)
        North, // Up (1)
        East,  // Right (2)
        West   // Left (3)
    }

    public enum DogAnimState
    {
        Idle,
        Walking,
        Running,
        Power
    }

    [System.Serializable]
    public class DogSpriteSet
    {
        public string dogName = "Perro";
        public Sprite defaultPortrait;

        [Header("Idle 4-Direcciones")]
        public Sprite[] idleSouth;
        public Sprite[] idleNorth;
        public Sprite[] idleEast;
        public Sprite[] idleWest;

        [Header("Walking 4-Direcciones")]
        public Sprite[] walkSouth;
        public Sprite[] walkNorth;
        public Sprite[] walkEast;
        public Sprite[] walkWest;

        [Header("Running 4-Direcciones")]
        public Sprite[] runSouth;
        public Sprite[] runNorth;
        public Sprite[] runEast;
        public Sprite[] runWest;

        [Header("Poder / Ladrido 4-Direcciones")]
        public Sprite[] powerSouth;
        public Sprite[] powerNorth;
        public Sprite[] powerEast;
        public Sprite[] powerWest;

        public Sprite[] GetFrames(DogAnimState state, DogDirection dir)
        {
            switch (state)
            {
                case DogAnimState.Idle:
                    return dir switch
                    {
                        DogDirection.South => (idleSouth != null && idleSouth.Length > 0) ? idleSouth : null,
                        DogDirection.North => (idleNorth != null && idleNorth.Length > 0) ? idleNorth : null,
                        DogDirection.East  => (idleEast  != null && idleEast.Length > 0)  ? idleEast  : null,
                        DogDirection.West  => (idleWest  != null && idleWest.Length > 0)  ? idleWest  : null,
                        _ => idleSouth
                    };
                case DogAnimState.Walking:
                    return dir switch
                    {
                        DogDirection.South => (walkSouth != null && walkSouth.Length > 0) ? walkSouth : idleSouth,
                        DogDirection.North => (walkNorth != null && walkNorth.Length > 0) ? walkNorth : idleNorth,
                        DogDirection.East  => (walkEast  != null && walkEast.Length > 0)  ? walkEast  : idleEast,
                        DogDirection.West  => (walkWest  != null && walkWest.Length > 0)  ? walkWest  : idleWest,
                        _ => walkSouth
                    };
                case DogAnimState.Running:
                    return dir switch
                    {
                        DogDirection.South => (runSouth != null && runSouth.Length > 0) ? runSouth : walkSouth,
                        DogDirection.North => (runNorth != null && runNorth.Length > 0) ? runNorth : walkNorth,
                        DogDirection.East  => (runEast  != null && runEast.Length > 0)  ? runEast  : walkEast,
                        DogDirection.West  => (runWest  != null && runWest.Length > 0)  ? runWest  : walkWest,
                        _ => runSouth
                    };
                case DogAnimState.Power:
                    return dir switch
                    {
                        DogDirection.South => (powerSouth != null && powerSouth.Length > 0) ? powerSouth : idleSouth,
                        DogDirection.North => (powerNorth != null && powerNorth.Length > 0) ? powerNorth : idleNorth,
                        DogDirection.East  => (powerEast  != null && powerEast.Length > 0)  ? powerEast  : idleEast,
                        DogDirection.West  => (powerWest  != null && powerWest.Length > 0)  ? powerWest  : idleWest,
                        _ => powerSouth
                    };
                default:
                    return idleSouth;
            }
        }
    }

    /// <summary>
    /// Controlador principal para Falin y Mia en la Aldea Hub (2D Top-Down / Isométrico).
    /// Permite alternar control con TAB, mientras el otro perro lo sigue fielmente como compañero.
    /// Soporta 4 direcciones de animación nítidas generadas en PixelLab, ladridos/poderes y Y-Sorting.
    /// </summary>
    public class KimayaHubDuoController : MonoBehaviour
    {
        public static KimayaHubDuoController Instance { get; private set; }

        [Header("Referencias de Personajes")]
        public GameObject falinObject;
        public GameObject miaObject;
        public DogSpriteSet falinSprites;
        public DogSpriteSet miaSprites;

        [Header("Velocidad")]
        public float walkSpeed = 4.8f;
        public float runSpeed = 7.5f;
        public float companionFollowDistance = 1.4f;

        [Header("Estado")]
        public bool isFalinActive = true;
        public bool controlsEnabled = true;

        private Rigidbody2D falinRb;
        private Rigidbody2D miaRb;
        private SpriteRenderer falinSr;
        private SpriteRenderer miaSr;

        // Estados individuales
        private DogDirection falinDir = DogDirection.South;
        private DogDirection miaDir = DogDirection.South;
        private DogAnimState falinState = DogAnimState.Idle;
        private DogAnimState miaState = DogAnimState.Idle;

        private float falinTimer;
        private int falinFrame;
        private float miaTimer;
        private int miaFrame;

        private bool falinPerformingPower = false;
        private bool miaPerformingPower = false;

        public event Action<bool> OnDogSwitched; // true = Falin, false = Mia

        private void Awake()
        {
            Instance = this;
            if (falinObject != null)
            {
                falinRb = falinObject.GetComponent<Rigidbody2D>();
                falinSr = falinObject.GetComponent<SpriteRenderer>();
            }
            if (miaObject != null)
            {
                miaRb = miaObject.GetComponent<Rigidbody2D>();
                miaSr = miaObject.GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            UpdateActiveDogMarker();
            OnDogSwitched?.Invoke(isFalinActive);
        }

        private void Update()
        {
            HandleDogSwitch();
            HandlePowerInput();
            UpdateSortingOrders();
            AnimateDog(true);
            AnimateDog(false);
        }

        private void FixedUpdate()
        {
            if (!controlsEnabled)
            {
                if (falinRb != null) falinRb.linearVelocity = Vector2.zero;
                if (miaRb != null) miaRb.linearVelocity = Vector2.zero;
                return;
            }

            MoveActiveDog();
            MoveCompanionDog();
        }

        private void HandleDogSwitch()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isFalinActive = !isFalinActive;
                UpdateActiveDogMarker();
                OnDogSwitched?.Invoke(isFalinActive);
            }
        }

        private void HandlePowerInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.F))
            {
                if (isFalinActive && !falinPerformingPower)
                {
                    StartCoroutine(PerformPowerRoutine(true));
                }
                else if (!isFalinActive && !miaPerformingPower)
                {
                    StartCoroutine(PerformPowerRoutine(false));
                }
            }
        }

        private IEnumerator PerformPowerRoutine(bool forFalin)
        {
            if (forFalin)
            {
                falinPerformingPower = true;
                falinState = DogAnimState.Power;
                falinFrame = 0;
                falinTimer = 0f;
                // Pequeño sonido / efecto de ladrido
                SpawnBarkEffect(falinObject.transform.position);
                yield return new WaitForSeconds(0.6f);
                falinPerformingPower = false;
            }
            else
            {
                miaPerformingPower = true;
                miaState = DogAnimState.Power;
                miaFrame = 0;
                miaTimer = 0f;
                SpawnBarkEffect(miaObject.transform.position);
                yield return new WaitForSeconds(0.6f);
                miaPerformingPower = false;
            }
        }

        private void SpawnBarkEffect(Vector3 pos)
        {
            // Crear onda sónica de ladrido decorativa
            var waveGO = new GameObject("BarkWave");
            waveGO.transform.position = pos + Vector3.up * 0.4f;
            var sr = waveGO.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.95f, 0.4f, 0.8f);
            sr.sortingOrder = 900;
            StartCoroutine(BarkWaveExpand(waveGO));
        }

        private IEnumerator BarkWaveExpand(GameObject go)
        {
            float elapsed = 0f;
            float duration = 0.35f;
            Vector3 initScale = Vector3.one * 0.3f;
            var sr = go.GetComponent<SpriteRenderer>();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (go != null)
                {
                    go.transform.localScale = Vector3.Lerp(initScale, Vector3.one * 1.5f, t);
                    if (sr != null)
                    {
                        var c = sr.color;
                        c.a = Mathf.Lerp(0.8f, 0f, t);
                        sr.color = c;
                    }
                }
                yield return null;
            }
            if (go != null) Destroy(go);
        }

        private void MoveActiveDog()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector2 input = new Vector2(h, v).normalized;

            bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            Rigidbody2D activeRb = isFalinActive ? falinRb : miaRb;
            ref DogAnimState activeState = ref (isFalinActive ? ref falinState : ref miaState);
            ref DogDirection activeDir = ref (isFalinActive ? ref falinDir : ref miaDir);
            bool isPerforming = isFalinActive ? falinPerformingPower : miaPerformingPower;

            if (activeRb == null) return;

            if (isPerforming)
            {
                activeRb.linearVelocity = Vector2.zero;
                return;
            }

            if (input.sqrMagnitude > 0.01f)
            {
                activeRb.linearVelocity = input * currentSpeed;
                activeState = isRunning ? DogAnimState.Running : DogAnimState.Walking;

                // Actualizar dirección cardinal predominante
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                {
                    activeDir = input.x > 0 ? DogDirection.East : DogDirection.West;
                }
                else
                {
                    activeDir = input.y > 0 ? DogDirection.North : DogDirection.South;
                }
            }
            else
            {
                activeRb.linearVelocity = Vector2.zero;
                activeState = DogAnimState.Idle;
            }
        }

        private void MoveCompanionDog()
        {
            GameObject leader = isFalinActive ? falinObject : miaObject;
            GameObject follower = isFalinActive ? miaObject : falinObject;
            Rigidbody2D followerRb = isFalinActive ? miaRb : falinRb;
            ref DogAnimState followerState = ref (isFalinActive ? ref miaState : ref falinState);
            ref DogDirection followerDir = ref (isFalinActive ? ref miaDir : ref falinDir);
            bool isPerforming = isFalinActive ? miaPerformingPower : falinPerformingPower;

            if (leader == null || follower == null || followerRb == null) return;

            if (isPerforming)
            {
                followerRb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toLeader = leader.transform.position - follower.transform.position;
            float dist = toLeader.magnitude;

            if (dist > companionFollowDistance)
            {
                Vector2 dir = toLeader.normalized;
                bool leaderRunning = (isFalinActive ? falinState : miaState) == DogAnimState.Running;
                float followSpeed = (dist > companionFollowDistance * 2.2f || leaderRunning) ? runSpeed : walkSpeed;

                followerRb.linearVelocity = dir * followSpeed;
                followerState = (followSpeed >= runSpeed) ? DogAnimState.Running : DogAnimState.Walking;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                {
                    followerDir = dir.x > 0 ? DogDirection.East : DogDirection.West;
                }
                else
                {
                    followerDir = dir.y > 0 ? DogDirection.North : DogDirection.South;
                }
            }
            else
            {
                // Se detiene suavemente junto a su compañero
                followerRb.linearVelocity = Vector2.zero;
                followerState = DogAnimState.Idle;
            }
        }

        private void AnimateDog(bool forFalin)
        {
            DogSpriteSet set = forFalin ? falinSprites : miaSprites;
            SpriteRenderer sr = forFalin ? falinSr : miaSr;
            DogAnimState state = forFalin ? falinState : miaState;
            DogDirection dir = forFalin ? falinDir : miaDir;

            if (set == null || sr == null) return;

            Sprite[] frames = set.GetFrames(state, dir);
            if (frames == null || frames.Length == 0) return;

            ref float timer = ref (forFalin ? ref falinTimer : ref miaTimer);
            ref int frameIdx = ref (forFalin ? ref falinFrame : ref miaFrame);

            float fps = state switch
            {
                DogAnimState.Running => 14f,
                DogAnimState.Walking => 10f,
                DogAnimState.Power   => 12f,
                _ => 8f // Idle
            };

            timer += Time.deltaTime;
            if (timer >= 1f / fps)
            {
                timer = 0f;
                frameIdx = (frameIdx + 1) % frames.Length;
            }

            if (frameIdx >= frames.Length) frameIdx = 0;
            sr.sprite = frames[frameIdx];
        }

        private void UpdateSortingOrders()
        {
            // Y-Sorting estándar top-down: entre menor Y (más abajo), mayor sorting order
            if (falinObject != null && falinSr != null)
            {
                falinSr.sortingOrder = 500 - Mathf.RoundToInt((falinObject.transform.position.y - 0.2f) * 10f);
            }
            if (miaObject != null && miaSr != null)
            {
                miaSr.sortingOrder = 500 - Mathf.RoundToInt((miaObject.transform.position.y - 0.2f) * 10f);
            }
        }

        private void UpdateActiveDogMarker()
        {
            // Puede añadir una sutil flecha o resplandor bajo las patitas del perro activo
            if (falinObject != null)
            {
                var marker = falinObject.transform.Find("ActiveMarker");
                if (marker != null) marker.gameObject.SetActive(isFalinActive);
            }
            if (miaObject != null)
            {
                var marker = miaObject.transform.Find("ActiveMarker");
                if (marker != null) marker.gameObject.SetActive(!isFalinActive);
            }
        }

        public Transform GetActiveTransform()
        {
            return isFalinActive ? falinObject?.transform : miaObject?.transform;
        }
    }
}
