using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador cuadro por cuadro fluido para Falin y Mía en Jump King con sprites de PixelLab en perfil (East/West).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(KimayaJumpKingController))]
    public class KimayaJumpKingAnimator : MonoBehaviour
    {
        [Header("Frames de Falin en Perfil (PixelLab)")]
        public Sprite[] falinIdleFrames;
        public Sprite[] falinWalkFrames;
        public Sprite[] falinChargeFrames;
        public Sprite[] falinJumpFrames;

        [Header("Frames de Mía en Perfil (PixelLab)")]
        public Sprite[] miaIdleFrames;
        public Sprite[] miaWalkFrames;
        public Sprite[] miaChargeFrames;
        public Sprite[] miaJumpFrames;

        [Header("Velocidad de Animación (FPS)")]
        public float idleFPS = 8f;
        public float walkFPS = 11f;
        public float chargeFPS = 12f;

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

            // Determinar estado de animación
            string newState = DetermineState();
            if (newState != _currentState)
            {
                _currentState = newState;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            // Obtener frames del estado actual
            Sprite[] activeFrames = GetActiveFrames();
            if (activeFrames == null || activeFrames.Length == 0) return;

            // Velocidad según estado
            float fps = (_currentState == "Walk") ? walkFPS :
                        (_currentState == "Charging") ? chargeFPS : idleFPS;

            _animTimer += Time.deltaTime;
            if (_animTimer >= (1f / fps))
            {
                _animTimer -= (1f / fps);
                _currentFrameIndex = (_currentFrameIndex + 1) % activeFrames.Length;
            }

            // Aplicar sprite actual
            if (_currentFrameIndex < activeFrames.Length && activeFrames[_currentFrameIndex] != null)
            {
                _sr.sprite = activeFrames[_currentFrameIndex];
            }

            // Orientación según FacingDirection del controlador
            _sr.flipX = (_ctrl.FacingDirection < 0);
        }

        private string DetermineState()
        {
            if (_ctrl.IsCharging) return "Charging";
            if (!_ctrl.IsGrounded) return "Jumping";
            if (Mathf.Abs(_rb.linearVelocity.x) > 0.15f) return "Walk";
            return "Idle";
        }

        private Sprite[] GetActiveFrames()
        {
            bool isFalin = (_ctrl.character == KimayaJumpKingController.CharacterType.Falin);

            switch (_currentState)
            {
                case "Walk":
                    return isFalin ? falinWalkFrames : miaWalkFrames;
                case "Charging":
                    return isFalin ? falinChargeFrames : miaChargeFrames;
                case "Jumping":
                    return isFalin ? falinJumpFrames : miaJumpFrames;
                case "Idle":
                default:
                    return isFalin ? falinIdleFrames : miaIdleFrames;
            }
        }
    }
}
