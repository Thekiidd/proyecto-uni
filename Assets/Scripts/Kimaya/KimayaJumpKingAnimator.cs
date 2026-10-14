using System.Collections.Generic;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador cuadro por cuadro para Falin y Mía en el modo Jump King usando los sprites de PixelLab.
    /// Soporta:
    /// - Idle (respiración / quieto)
    /// - Caminar (caminar a izquierda / derecha con flipX)
    /// - Cargar salto (agacharse / concentrar fuerza de salto)
    /// - En el aire (saltando hacia arriba o cayendo)
    /// - Aturdido (tras caída fuerte)
    /// - Cambio instantáneo entre Falin y Mía.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(KimayaJumpKingController))]
    public class KimayaJumpKingAnimator : MonoBehaviour
    {
        [Header("Frames de Falin (PixelLab)")]
        public Sprite[] falinIdleFrames;
        public Sprite[] falinWalkFrames;
        public Sprite[] falinChargeFrames;
        public Sprite[] falinJumpFrames;

        [Header("Frames de Mía (PixelLab)")]
        public Sprite[] miaIdleFrames;
        public Sprite[] miaWalkFrames;
        public Sprite[] miaChargeFrames;
        public Sprite[] miaJumpFrames;

        [Header("Velocidad de Animación (FPS)")]
        public float idleFPS = 6f;
        public float walkFPS = 10f;
        public float chargeFPS = 8f;

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

            // Detectar si cambió de personaje
            if (_ctrl.character != _lastChar)
            {
                _lastChar = _ctrl.character;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            // Determinar estado actual
            string newState = DetermineState();
            if (newState != _currentState)
            {
                _currentState = newState;
                _currentFrameIndex = 0;
                _animTimer = 0f;
            }

            // Obtener lista de frames según personaje y estado
            Sprite[] activeFrames = GetActiveFrames();
            if (activeFrames == null || activeFrames.Length == 0) return;

            // Calcular FPS según estado
            float fps = (_currentState == "Walk") ? walkFPS :
                        (_currentState == "Charging") ? chargeFPS : idleFPS;

            _animTimer += Time.deltaTime;
            if (_animTimer >= (1f / fps))
            {
                _animTimer -= (1f / fps);
                _currentFrameIndex = (_currentFrameIndex + 1) % activeFrames.Length;
            }

            // Aplicar sprite
            if (_currentFrameIndex < activeFrames.Length && activeFrames[_currentFrameIndex] != null)
            {
                _sr.sprite = activeFrames[_currentFrameIndex];
            }

            // Flip según dirección horizontal
            if (Mathf.Abs(_rb.linearVelocity.x) > 0.15f)
            {
                _sr.flipX = (_rb.linearVelocity.x < 0f);
            }
        }

        private string DetermineState()
        {
            if (_ctrl.IsStunned) return "Stunned";
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
                case "Stunned":
                    return isFalin ? falinJumpFrames : miaJumpFrames;
                case "Idle":
                default:
                    return isFalin ? falinIdleFrames : miaIdleFrames;
            }
        }
    }
}
