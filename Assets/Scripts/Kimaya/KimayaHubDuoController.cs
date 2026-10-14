using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    public enum DogDirection
    {
        South, // Abajo
        North, // Arriba
        East,  // Derecha
        West   // Izquierda
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
    /// Controlador de dúo de perros en el Hub con:
    /// - Soporte universal para New Input System y Legacy.
    /// - Histéresis de dirección para evitar parpadeos y stutter en diagonales.
    /// - Cambio limpio de estado de animación para evitar que se traben los sprites.
    /// - Sistema de compañero recíproco fiel sin empujes ni colisiones mutuas.
    /// </summary>
    public class KimayaHubDuoController : MonoBehaviour
    {
        public static KimayaHubDuoController Instance { get; private set; }

        [Header("Referencias de Personajes")]
        public GameObject falinObject;
        public GameObject miaObject;
        public DogSpriteSet falinSprites;
        public DogSpriteSet miaSprites;

        [Header("Velocidad de Movimiento")]
        public float walkSpeed = 4.8f;
        public float runSpeed = 7.6f;
        public float companionFollowDistance = 1.35f;

        [Header("Estado")]
        public bool isFalinActive = true;
        public bool controlsEnabled = true;

        private Rigidbody2D _falinRb;
        private Rigidbody2D _miaRb;
        private SpriteRenderer _falinSr;
        private SpriteRenderer _miaSr;

        // Estados individuales
        private DogDirection _falinDir = DogDirection.South;
        private DogDirection _miaDir = DogDirection.South;
        private DogAnimState _falinState = DogAnimState.Idle;
        private DogAnimState _miaState = DogAnimState.Idle;

        private float _falinTimer;
        private int _falinFrame;
        private float _miaTimer;
        private int _miaFrame;

        private bool _falinPerformingPower = false;
        private bool _miaPerformingPower = false;

        public event Action<bool> OnDogSwitched;

        private void Awake()
        {
            Instance = this;
            EnsureReferences();
        }

        private void Start()
        {
            EnsureReferences();
            IgnoreDogToDogCollisions();
            UpdateActiveDogMarker();
            OnDogSwitched?.Invoke(isFalinActive);
        }

        private void EnsureReferences()
        {
            if (falinObject == null) falinObject = GameObject.Find("Falin_SanBernardo");
            if (miaObject == null) miaObject = GameObject.Find("Mia_Schnauzer");

            if (falinObject != null)
            {
                if (_falinRb == null) _falinRb = falinObject.GetComponent<Rigidbody2D>();
                if (_falinSr == null) _falinSr = falinObject.GetComponent<SpriteRenderer>();
            }

            if (miaObject != null)
            {
                if (_miaRb == null) _miaRb = miaObject.GetComponent<Rigidbody2D>();
                if (_miaSr == null) _miaSr = miaObject.GetComponent<SpriteRenderer>();
            }
        }

        private void IgnoreDogToDogCollisions()
        {
            if (falinObject == null || miaObject == null) return;
            var c1 = falinObject.GetComponent<Collider2D>();
            var c2 = miaObject.GetComponent<Collider2D>();
            if (c1 != null && c2 != null)
            {
                Physics2D.IgnoreCollision(c1, c2, true);
            }
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
                if (_falinRb != null) _falinRb.linearVelocity = Vector2.zero;
                if (_miaRb != null) _miaRb.linearVelocity = Vector2.zero;
                return;
            }

            MoveActiveDog();
            MoveCompanionDog();
        }

        private void HandleDogSwitch()
        {
            if (ReadSwitchInput())
            {
                SwitchActiveDog();
            }
        }

        public void SwitchActiveDog()
        {
            isFalinActive = !isFalinActive;
            UpdateActiveDogMarker();
            OnDogSwitched?.Invoke(isFalinActive);
            Debug.Log($"[KimayaHubDuoController] Líder cambiado a: {(isFalinActive ? "Falin" : "Mía")}");
        }

        private bool ReadSwitchInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame)
                    return true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q))
                    return true;
            }
            catch { }
            return false;
        }

        private Vector2 ReadMovementInput()
        {
            Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            }
#endif
            if (input.sqrMagnitude < 0.01f)
            {
                try
                {
                    input.x = Input.GetAxisRaw("Horizontal");
                    input.y = Input.GetAxisRaw("Vertical");
                }
                catch { }
            }

            if (input.sqrMagnitude > 1f) input.Normalize();
            return input;
        }

        private bool ReadRunInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed)
                    return true;
            }
#endif
            try
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    return true;
            }
            catch { }
            return false;
        }

        private void SetDogState(bool forFalin, DogAnimState newState)
        {
            ref DogAnimState state = ref (forFalin ? ref _falinState : ref _miaState);
            ref int frame = ref (forFalin ? ref _falinFrame : ref _miaFrame);
            ref float timer = ref (forFalin ? ref _falinTimer : ref _miaTimer);

            if (state != newState)
            {
                state = newState;
                frame = 0;
                timer = 0f;
            }
        }

        private DogDirection CalculateDirectionWithHysteresis(Vector2 input, DogDirection currentDir)
        {
            float absX = Mathf.Abs(input.x);
            float absY = Mathf.Abs(input.y);

            // Umbral de histéresis: solo cambiar eje si supera claramente al otro
            if (absX > absY + 0.18f)
            {
                return input.x > 0 ? DogDirection.East : DogDirection.West;
            }
            if (absY > absX + 0.18f)
            {
                return input.y > 0 ? DogDirection.North : DogDirection.South;
            }

            // Diagonal equilibrada: mantener la dirección previa para evitar vibraciones
            return currentDir;
        }

        private void MoveActiveDog()
        {
            Vector2 input = ReadMovementInput();
            bool isRunning = ReadRunInput();
            float currentSpeed = isRunning ? runSpeed : walkSpeed;

            Rigidbody2D activeRb = isFalinActive ? _falinRb : _miaRb;
            ref DogDirection activeDir = ref (isFalinActive ? ref _falinDir : ref _miaDir);
            bool isPerforming = isFalinActive ? _falinPerformingPower : _miaPerformingPower;

            if (activeRb == null) return;

            if (isPerforming)
            {
                activeRb.linearVelocity = Vector2.zero;
                return;
            }

            if (input.sqrMagnitude > 0.01f)
            {
                activeRb.linearVelocity = input * currentSpeed;
                SetDogState(isFalinActive, isRunning ? DogAnimState.Running : DogAnimState.Walking);
                activeDir = CalculateDirectionWithHysteresis(input, activeDir);
            }
            else
            {
                activeRb.linearVelocity = Vector2.zero;
                SetDogState(isFalinActive, DogAnimState.Idle);
            }
        }

        private void MoveCompanionDog()
        {
            GameObject leader = isFalinActive ? falinObject : miaObject;
            GameObject follower = isFalinActive ? miaObject : falinObject;
            Rigidbody2D followerRb = isFalinActive ? _miaRb : _falinRb;
            ref DogDirection followerDir = ref (isFalinActive ? ref _miaDir : ref _falinDir);
            bool isPerforming = isFalinActive ? _miaPerformingPower : _falinPerformingPower;

            if (leader == null || follower == null || followerRb == null) return;

            if (isPerforming)
            {
                followerRb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 toLeader = (Vector2)leader.transform.position - (Vector2)follower.transform.position;
            float dist = toLeader.magnitude;

            if (dist > companionFollowDistance)
            {
                Vector2 dir = toLeader.normalized;
                bool leaderRunning = (isFalinActive ? _falinState : _miaState) == DogAnimState.Running;
                
                float followSpeed = (dist > companionFollowDistance * 2.2f || leaderRunning) 
                    ? runSpeed * 1.05f 
                    : walkSpeed;

                followerRb.linearVelocity = dir * followSpeed;
                SetDogState(!isFalinActive, followSpeed >= runSpeed ? DogAnimState.Running : DogAnimState.Walking);
                followerDir = CalculateDirectionWithHysteresis(dir, followerDir);
            }
            else
            {
                followerRb.linearVelocity = Vector2.zero;
                SetDogState(!isFalinActive, DogAnimState.Idle);
            }
        }

        private void HandlePowerInput()
        {
            bool powerPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
                powerPressed = true;
#endif
            if (!powerPressed)
            {
                try
                {
                    if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
                        powerPressed = true;
                }
                catch { }
            }

            if (powerPressed)
            {
                if (isFalinActive && !_falinPerformingPower)
                {
                    StartCoroutine(PerformPowerRoutine(true));
                }
                else if (!isFalinActive && !_miaPerformingPower)
                {
                    StartCoroutine(PerformPowerRoutine(false));
                }
            }
        }

        private IEnumerator PerformPowerRoutine(bool forFalin)
        {
            if (forFalin)
            {
                _falinPerformingPower = true;
                SetDogState(true, DogAnimState.Power);
                yield return new WaitForSeconds(0.55f);
                _falinPerformingPower = false;
            }
            else
            {
                _miaPerformingPower = true;
                SetDogState(false, DogAnimState.Power);
                yield return new WaitForSeconds(0.55f);
                _miaPerformingPower = false;
            }
        }

        private void AnimateDog(bool forFalin)
        {
            DogSpriteSet set = forFalin ? falinSprites : miaSprites;
            SpriteRenderer sr = forFalin ? _falinSr : _miaSr;
            DogAnimState state = forFalin ? _falinState : _miaState;
            DogDirection dir = forFalin ? _falinDir : _miaDir;

            if (set == null || sr == null) return;

            // Asegurar siempre flipX = false (PixelLab tiene sprites nativos en cada dirección)
            sr.flipX = false;

            Sprite[] frames = set.GetFrames(state, dir);
            if (frames == null || frames.Length == 0) return;

            ref float timer = ref (forFalin ? ref _falinTimer : ref _miaTimer);
            ref int frameIdx = ref (forFalin ? ref _falinFrame : ref _miaFrame);

            float fps = state switch
            {
                DogAnimState.Running => 13f,
                DogAnimState.Walking => 9.5f,
                DogAnimState.Power   => 12f,
                _ => 7.5f // Idle
            };

            timer += Time.deltaTime;
            if (timer >= 1f / fps)
            {
                timer = 0f;
                frameIdx = (frameIdx + 1) % frames.Length;
            }

            if (frameIdx >= frames.Length || frameIdx < 0) frameIdx = 0;

            if (frames[frameIdx] != null)
            {
                sr.sprite = frames[frameIdx];
            }
        }

        private void UpdateSortingOrders()
        {
            // Y-Sorting preciso para que el perro se oculte tras los arcos/troncos y se muestre frente a ellos
            if (falinObject != null && _falinSr != null)
            {
                _falinSr.sortingOrder = 500 - Mathf.RoundToInt((falinObject.transform.position.y - 0.25f) * 10f);
            }
            if (miaObject != null && _miaSr != null)
            {
                _miaSr.sortingOrder = 500 - Mathf.RoundToInt((miaObject.transform.position.y - 0.20f) * 10f);
            }
        }

        private void UpdateActiveDogMarker()
        {
            if (falinObject != null)
            {
                var marker = falinObject.transform.Find("DiamondMarker") 
                          ?? falinObject.transform.Find("ActiveMarker") 
                          ?? falinObject.transform.Find("Selection_Marker");
                if (marker != null) marker.gameObject.SetActive(isFalinActive);
            }
            if (miaObject != null)
            {
                var marker = miaObject.transform.Find("DiamondMarker") 
                          ?? miaObject.transform.Find("ActiveMarker") 
                          ?? miaObject.transform.Find("Selection_Marker");
                if (marker != null) marker.gameObject.SetActive(!isFalinActive);
            }
        }

        public Transform GetActiveTransform()
        {
            return isFalinActive ? falinObject?.transform : miaObject?.transform;
        }
    }
}
