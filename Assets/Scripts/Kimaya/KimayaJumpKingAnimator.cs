using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador cinemático cuadro por cuadro estilo Jump King para Falin y Mía.
    /// Soporta:
    /// - Reposo (Idle)
    /// - Caminata (Walk)
    /// - Carga estática agachada con compresión gradual (Charge Squash - sin mover patas)
    /// - Salto con estiramiento vertical aerodinámico (Jump Stretch)
    /// - Caída libre con descenso cómico (Falling)
    /// - Golpe / Aturdimiento contra el suelo (Splat Knockdown)
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(KimayaJumpKingController))]
    public class KimayaJumpKingAnimator : MonoBehaviour
    {
        [Header("Frames Oficiales Jump King (Knight Original)")]
        public Sprite knightIdle;
        public Sprite[] knightWalkFrames;
        public Sprite knightCharge;
        public Sprite knightJumpUp;
        public Sprite knightFall;
        public Sprite knightBounce;
        public Sprite knightSplat;

        [Header("Frames de Caminata y Reposo (Falin)")]
        public Sprite falinKnightIdle;
        public Sprite[] falinIdleFrames;
        public Sprite[] falinWalkFrames;

        [Header("Frames Tácticos Jump King (Falin)")]
        [Tooltip("Carga progresiva: [0]=ligera, [1]=media, [2]=máxima compresión")]
        public Sprite[] falinChargeProgress;
        public Sprite falinJumpStretch;
        public Sprite falinFall;
        public Sprite falinSplat;

        [Header("Frames de Caminata y Reposo (Mía)")]
        public Sprite miaKnightIdle;
        public Sprite[] miaIdleFrames;
        public Sprite[] miaWalkFrames;

        [Header("Frames Tácticos Jump King (Mía)")]
        [Tooltip("Carga progresiva: [0]=ligera, [1]=media, [2]=máxima compresión")]
        public Sprite[] miaChargeProgress;
        public Sprite miaJumpStretch;
        public Sprite miaFall;
        public Sprite miaSplat;

        [Header("Velocidad de Animación (FPS)")]
        public float idleFPS = 8f;
        public float walkFPS = 11f;

        private SpriteRenderer _sr;
        private KimayaJumpKingController _ctrl;
        private Rigidbody2D _rb;

        private float _animTimer = 0f;
        private int _currentFrameIndex = 0;
        private string _currentState = "Idle";
        private KimayaJumpKingController.CharacterType _lastChar;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _ctrl = GetComponent<KimayaJumpKingController>();
            _rb = GetComponent<Rigidbody2D>();
            _lastChar = _ctrl.character;
        }

        private void Start()
        {
            ApplyInitialSprite();
        }

        public void ApplyInitialSprite()
        {
            if (_ctrl == null || _sr == null) return;
            bool isKing = (_ctrl.character == KimayaJumpKingController.CharacterType.JumpKing);
            bool isFalin = (_ctrl.character == KimayaJumpKingController.CharacterType.Falin);

            if (isKing)
            {
                if (knightIdle != null) _sr.sprite = knightIdle;
            }
            else if (isFalin)
            {
                if (falinKnightIdle != null) _sr.sprite = falinKnightIdle;
                else if (falinIdleFrames != null && falinIdleFrames.Length > 0) _sr.sprite = falinIdleFrames[0];
            }
            else
            {
                if (miaKnightIdle != null) _sr.sprite = miaKnightIdle;
                else if (miaIdleFrames != null && miaIdleFrames.Length > 0) _sr.sprite = miaIdleFrames[0];
            }
        }

        private void Update()
        {
            if (_ctrl == null || _sr == null) return;

            // Detectar cambio de personaje (Falin / Mía)
            if (_ctrl.character != _lastChar)
            {
                _lastChar = _ctrl.character;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            bool isKing = (_ctrl.character == KimayaJumpKingController.CharacterType.JumpKing);
            bool isFalin = (_ctrl.character == KimayaJumpKingController.CharacterType.Falin);

            // 1. ESTADO DE ATURDIMIENTO / SPLAT TRAS CAÍDA
            if (_ctrl.IsStunned)
            {
                _currentState = "Splat";
                Sprite splatSprite = isKing ? knightSplat : (isFalin ? falinSplat : miaSplat);
                if (splatSprite != null) _sr.sprite = splatSprite;
                _sr.flipX = (_ctrl.FacingDirection < 0);
                return;
            }

            // 2. ESTADO DE CARGA DE SALTO (Agachado acumulando tensión)
            if (_ctrl.IsCharging)
            {
                _currentState = "Charging";
                if (isKing)
                {
                    if (knightCharge != null) _sr.sprite = knightCharge;
                }
                else
                {
                    var chargeArr = isFalin ? falinChargeProgress : miaChargeProgress;
                    if (chargeArr != null && chargeArr.Length > 0)
                    {
                        int chargeIdx = Mathf.Clamp(Mathf.FloorToInt(_ctrl.ChargePercent * chargeArr.Length), 0, chargeArr.Length - 1);
                        if (chargeArr[chargeIdx] != null)
                        {
                            _sr.sprite = chargeArr[chargeIdx];
                        }
                    }
                }
                _sr.flipX = (_ctrl.FacingDirection < 0);
                return;
            }

            // 3. EN EL AIRE: SALTO CON ESTIRAMIENTO, CAÍDA O REBOTE EN MURO
            if (!_ctrl.IsGrounded)
            {
                if (isKing)
                {
                    if (_ctrl.IsBouncing && knightBounce != null)
                    {
                        _currentState = "Bounce";
                        _sr.sprite = knightBounce;
                    }
                    else if (_rb.linearVelocity.y >= -0.5f)
                    {
                        _currentState = "JumpStretch";
                        if (knightJumpUp != null) _sr.sprite = knightJumpUp;
                    }
                    else
                    {
                        _currentState = "Falling";
                        if (knightFall != null) _sr.sprite = knightFall;
                    }
                }
                else
                {
                    // Falin o Mía
                    if (_rb.linearVelocity.y >= -0.5f)
                    {
                        _currentState = "JumpStretch";
                        var stretchSprite = isFalin ? falinJumpStretch : miaJumpStretch;
                        if (stretchSprite != null) _sr.sprite = stretchSprite;
                    }
                    else
                    {
                        _currentState = "Falling";
                        var fallSprite = isFalin ? falinFall : miaFall;
                        if (fallSprite != null) _sr.sprite = fallSprite;
                    }
                }

                _sr.flipX = (_ctrl.FacingDirection < 0);
                return;
            }

            // 4. EN SUELO: CAMINAR O IDLE
            string newState = (Mathf.Abs(_rb.linearVelocity.x) > 0.15f) ? "Walk" : "Idle";
            if (newState != _currentState)
            {
                _currentState = newState;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            if (isKing)
            {
                if (_currentState == "Walk" && knightWalkFrames != null && knightWalkFrames.Length > 0)
                {
                    _animTimer += Time.deltaTime;
                    float frameDur = 1f / walkFPS;
                    if (_animTimer >= frameDur)
                    {
                        _animTimer -= frameDur;
                        _currentFrameIndex = (_currentFrameIndex + 1) % knightWalkFrames.Length;
                    }
                    if (_currentFrameIndex < knightWalkFrames.Length && knightWalkFrames[_currentFrameIndex] != null)
                    {
                        _sr.sprite = knightWalkFrames[_currentFrameIndex];
                    }
                }
                else
                {
                    if (knightIdle != null) _sr.sprite = knightIdle;
                }
            }
            else
            {
                if (_currentState == "Idle")
                {
                    Sprite knightSprite = isFalin ? falinKnightIdle : miaKnightIdle;
                    if (knightSprite != null)
                    {
                        _sr.sprite = knightSprite;
                        _sr.flipX = (_ctrl.FacingDirection < 0);
                        return;
                    }
                }

                Sprite[] activeFrames = (_currentState == "Walk")
                    ? (isFalin ? falinWalkFrames : miaWalkFrames)
                    : (isFalin ? falinIdleFrames : miaIdleFrames);

                if (activeFrames != null && activeFrames.Length > 0)
                {
                    float fps = (_currentState == "Walk") ? walkFPS : idleFPS;
                    _animTimer += Time.deltaTime;
                    if (_animTimer >= (1f / fps))
                    {
                        _animTimer -= (1f / fps);
                        _currentFrameIndex = (_currentFrameIndex + 1) % activeFrames.Length;
                    }

                    if (_currentFrameIndex < activeFrames.Length && activeFrames[_currentFrameIndex] != null)
                    {
                        _sr.sprite = activeFrames[_currentFrameIndex];
                    }
                }
            }

            _sr.flipX = (_ctrl.FacingDirection < 0);
        }
    }
}
