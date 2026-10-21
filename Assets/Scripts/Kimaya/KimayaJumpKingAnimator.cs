using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador cinemático cuadro por cuadro estilo Jump King para Jump King, Falin y Mía.
    /// Soporta completamente:
    /// - Modo Con Skin: Falin y Mía equipan casco de acero y capa real roja en TODOS sus estados (Idle, Walk, Charge, Jump Leap, Fall, Splat).
    /// - Modo Sin Skin: Falin y Mía lucen su aspecto canino natural clásico en todos sus estados.
    /// - Alternar con tecla C / H o botón en pantalla para jugar con o sin skin al instante.
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

        [Header("Modo de Skin")]
        [Tooltip("Si es true, Falin y Mía usan la armadura y capa Jump King en todos los estados. Si es false, usan su aspecto canino clásico.")]
        public bool useKnightSkin = true;

        [Header("Falin - Con Skin Caballero (Casco & Capa)")]
        public Sprite falinKnightIdle;
        public Sprite[] falinKnightWalkFrames;
        public Sprite[] falinKnightChargeProgress;
        public Sprite falinKnightJumpStretch;
        public Sprite falinKnightFall;
        public Sprite falinKnightSplat;

        [Header("Falin - Sin Skin (Perro Clásico)")]
        public Sprite[] falinClassicIdleFrames;
        public Sprite[] falinClassicWalkFrames;
        public Sprite[] falinClassicChargeProgress;
        public Sprite falinClassicJumpStretch;
        public Sprite falinClassicFall;
        public Sprite falinClassicSplat;

        [Header("Mía - Con Skin Caballero (Casco & Capa)")]
        public Sprite miaKnightIdle;
        public Sprite[] miaKnightWalkFrames;
        public Sprite[] miaKnightChargeProgress;
        public Sprite miaKnightJumpStretch;
        public Sprite miaKnightFall;
        public Sprite miaKnightSplat;

        [Header("Mía - Sin Skin (Perra Clásica)")]
        public Sprite[] miaClassicIdleFrames;
        public Sprite[] miaClassicWalkFrames;
        public Sprite[] miaClassicChargeProgress;
        public Sprite miaClassicJumpStretch;
        public Sprite miaClassicFall;
        public Sprite miaClassicSplat;

        [Header("Velocidad de Animación (FPS)")]
        public float idleFPS = 8f;
        public float walkFPS = 11f;

        // Campos de compatibilidad histórica
        [HideInInspector] public Sprite[] falinIdleFrames;
        [HideInInspector] public Sprite[] falinWalkFrames;
        [HideInInspector] public Sprite[] falinChargeProgress;
        [HideInInspector] public Sprite falinJumpStretch;
        [HideInInspector] public Sprite falinFall;
        [HideInInspector] public Sprite falinSplat;

        [HideInInspector] public Sprite[] miaIdleFrames;
        [HideInInspector] public Sprite[] miaWalkFrames;
        [HideInInspector] public Sprite[] miaChargeProgress;
        [HideInInspector] public Sprite miaJumpStretch;
        [HideInInspector] public Sprite miaFall;
        [HideInInspector] public Sprite miaSplat;

        private SpriteRenderer _sr;
        private KimayaJumpKingController _ctrl;
        private Rigidbody2D _rb;

        private float _animTimer = 0f;
        private int _currentFrameIndex = 0;
        private string _currentState = "Idle";
        private KimayaJumpKingController.CharacterType _lastChar;
        private bool _lastSkinState = true;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _ctrl = GetComponent<KimayaJumpKingController>();
            _rb = GetComponent<Rigidbody2D>();
            _lastChar = _ctrl != null ? _ctrl.character : KimayaJumpKingController.CharacterType.JumpKing;
            _lastSkinState = useKnightSkin;
            AutoLoadPixelLabSprites();
        }

        private void AutoLoadPixelLabSprites()
        {
            Sprite[] all = Resources.FindObjectsOfTypeAll<Sprite>();
            if (all == null || all.Length == 0) return;

            // Falin
            var falinIdle = new List<Sprite>();
            var falinCharge = new List<Sprite>();
            var falinJump = new List<Sprite>();
            var falinFall = new List<Sprite>();

            // Mia
            var miaIdle = new List<Sprite>();
            var miaCharge = new List<Sprite>();
            var miaJump = new List<Sprite>();
            var miaFall = new List<Sprite>();

            foreach (var s in all)
            {
                string n = s.name;
                if (n.StartsWith("Falin_Idle_east_")) falinIdle.Add(s);
                else if (n.StartsWith("Falin_Charge_east_")) falinCharge.Add(s);
                else if (n.StartsWith("Falin_Jump_east_")) falinJump.Add(s);
                else if (n.StartsWith("Falin_Fall_east_")) falinFall.Add(s);
                else if (n.StartsWith("Mia_Idle_east_")) miaIdle.Add(s);
                else if (n.StartsWith("Mia_Charge_east_")) miaCharge.Add(s);
                else if (n.StartsWith("Mia_Jump_east_")) miaJump.Add(s);
                else if (n.StartsWith("Mia_Fall_east_")) miaFall.Add(s);
            }

            falinIdle.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            falinCharge.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            falinJump.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            falinFall.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            miaIdle.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            miaCharge.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            miaJump.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));
            miaFall.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

            if (falinIdle.Count > 0 && (falinClassicIdleFrames == null || falinClassicIdleFrames.Length == 0))
                falinClassicIdleFrames = falinIdle.ToArray();
            if (falinCharge.Count > 0 && (falinClassicChargeProgress == null || falinClassicChargeProgress.Length == 0))
                falinClassicChargeProgress = falinCharge.ToArray();
            if (falinJump.Count > 0 && falinClassicJumpStretch == null)
                falinClassicJumpStretch = falinJump[Mathf.Min(2, falinJump.Count - 1)];
            if (falinFall.Count > 0 && falinClassicFall == null)
                falinClassicFall = falinFall[0];

            if (miaIdle.Count > 0 && (miaClassicIdleFrames == null || miaClassicIdleFrames.Length == 0))
                miaClassicIdleFrames = miaIdle.ToArray();
            if (miaCharge.Count > 0 && (miaClassicChargeProgress == null || miaClassicChargeProgress.Length == 0))
                miaClassicChargeProgress = miaCharge.ToArray();
            if (miaJump.Count > 0 && miaClassicJumpStretch == null)
                miaClassicJumpStretch = miaJump[Mathf.Min(2, miaJump.Count - 1)];
            if (miaFall.Count > 0 && miaClassicFall == null)
                miaClassicFall = miaFall[0];
        }

        private void Start()
        {
            ApplyInitialSprite();
        }

        public void ToggleKnightSkin()
        {
            useKnightSkin = !useKnightSkin;
            _lastSkinState = useKnightSkin;
            _currentFrameIndex = 0;
            _animTimer = 0f;
            ApplyInitialSprite();
        }

        public void SetKnightSkin(bool enabled)
        {
            useKnightSkin = enabled;
            _lastSkinState = useKnightSkin;
            _currentFrameIndex = 0;
            _animTimer = 0f;
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
                if (useKnightSkin && falinKnightIdle != null)
                {
                    _sr.sprite = falinKnightIdle;
                }
                else
                {
                    var idleArr = GetActiveFalinIdleFrames();
                    if (idleArr != null && idleArr.Length > 0 && idleArr[0] != null)
                        _sr.sprite = idleArr[0];
                    else if (falinKnightIdle != null)
                        _sr.sprite = falinKnightIdle;
                }
            }
            else
            {
                if (useKnightSkin && miaKnightIdle != null)
                {
                    _sr.sprite = miaKnightIdle;
                }
                else
                {
                    var idleArr = GetActiveMiaIdleFrames();
                    if (idleArr != null && idleArr.Length > 0 && idleArr[0] != null)
                        _sr.sprite = idleArr[0];
                    else if (miaKnightIdle != null)
                        _sr.sprite = miaKnightIdle;
                }
            }
        }

        private Sprite[] GetActiveFalinIdleFrames()
        {
            if (falinClassicIdleFrames != null && falinClassicIdleFrames.Length > 0) return falinClassicIdleFrames;
            return falinIdleFrames;
        }

        private Sprite[] GetActiveMiaIdleFrames()
        {
            if (miaClassicIdleFrames != null && miaClassicIdleFrames.Length > 0) return miaClassicIdleFrames;
            return miaIdleFrames;
        }

        private Sprite[] GetActiveFalinWalkFrames()
        {
            if (useKnightSkin)
            {
                if (falinKnightWalkFrames != null && falinKnightWalkFrames.Length > 0) return falinKnightWalkFrames;
            }
            if (falinClassicWalkFrames != null && falinClassicWalkFrames.Length > 0) return falinClassicWalkFrames;
            return falinWalkFrames;
        }

        private Sprite[] GetActiveMiaWalkFrames()
        {
            if (useKnightSkin)
            {
                if (miaKnightWalkFrames != null && miaKnightWalkFrames.Length > 0) return miaKnightWalkFrames;
            }
            if (miaClassicWalkFrames != null && miaClassicWalkFrames.Length > 0) return miaClassicWalkFrames;
            return miaWalkFrames;
        }

        private Sprite[] GetActiveFalinCharge()
        {
            if (useKnightSkin)
            {
                if (falinKnightChargeProgress != null && falinKnightChargeProgress.Length > 0) return falinKnightChargeProgress;
            }
            if (falinClassicChargeProgress != null && falinClassicChargeProgress.Length > 0) return falinClassicChargeProgress;
            return falinChargeProgress;
        }

        private Sprite[] GetActiveMiaCharge()
        {
            if (useKnightSkin)
            {
                if (miaKnightChargeProgress != null && miaKnightChargeProgress.Length > 0) return miaKnightChargeProgress;
            }
            if (miaClassicChargeProgress != null && miaClassicChargeProgress.Length > 0) return miaClassicChargeProgress;
            return miaChargeProgress;
        }

        private Sprite GetActiveFalinJumpStretch()
        {
            if (useKnightSkin && falinKnightJumpStretch != null) return falinKnightJumpStretch;
            if (falinClassicJumpStretch != null) return falinClassicJumpStretch;
            return falinJumpStretch;
        }

        private Sprite GetActiveMiaJumpStretch()
        {
            if (useKnightSkin && miaKnightJumpStretch != null) return miaKnightJumpStretch;
            if (miaClassicJumpStretch != null) return miaClassicJumpStretch;
            return miaJumpStretch;
        }

        private Sprite GetActiveFalinFall()
        {
            if (useKnightSkin && falinKnightFall != null) return falinKnightFall;
            if (falinClassicFall != null) return falinClassicFall;
            return falinFall;
        }

        private Sprite GetActiveMiaFall()
        {
            if (useKnightSkin && miaKnightFall != null) return miaKnightFall;
            if (miaClassicFall != null) return miaClassicFall;
            return miaFall;
        }

        private Sprite GetActiveFalinSplat()
        {
            if (useKnightSkin && falinKnightSplat != null) return falinKnightSplat;
            if (falinClassicSplat != null) return falinClassicSplat;
            return falinSplat;
        }

        private Sprite GetActiveMiaSplat()
        {
            if (useKnightSkin && miaKnightSplat != null) return miaKnightSplat;
            if (miaClassicSplat != null) return miaClassicSplat;
            return miaSplat;
        }

        private void Update()
        {
            if (_ctrl == null || _sr == null) return;

            // Detectar cambio de personaje o de skin
            if (_ctrl.character != _lastChar || useKnightSkin != _lastSkinState)
            {
                _lastChar = _ctrl.character;
                _lastSkinState = useKnightSkin;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            bool isKing = (_ctrl.character == KimayaJumpKingController.CharacterType.JumpKing);
            bool isFalin = (_ctrl.character == KimayaJumpKingController.CharacterType.Falin);

            // 1. ESTADO DE ATURDIMIENTO / SPLAT TRAS CAÍDA
            if (_ctrl.IsStunned)
            {
                _currentState = "Splat";
                Sprite splatSprite = isKing ? knightSplat : (isFalin ? GetActiveFalinSplat() : GetActiveMiaSplat());
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
                    var chargeArr = isFalin ? GetActiveFalinCharge() : GetActiveMiaCharge();
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
                        var stretchSprite = isFalin ? GetActiveFalinJumpStretch() : GetActiveMiaJumpStretch();
                        if (stretchSprite != null) _sr.sprite = stretchSprite;
                    }
                    else
                    {
                        _currentState = "Falling";
                        var fallSprite = isFalin ? GetActiveFalinFall() : GetActiveMiaFall();
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
                    if (useKnightSkin)
                    {
                        Sprite knightSprite = isFalin ? falinKnightIdle : miaKnightIdle;
                        if (knightSprite != null)
                        {
                            _sr.sprite = knightSprite;
                            _sr.flipX = (_ctrl.FacingDirection < 0);
                            return;
                        }
                    }
                }

                Sprite[] activeFrames = (_currentState == "Walk")
                    ? (isFalin ? GetActiveFalinWalkFrames() : GetActiveMiaWalkFrames())
                    : (isFalin ? GetActiveFalinIdleFrames() : GetActiveMiaIdleFrames());

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
