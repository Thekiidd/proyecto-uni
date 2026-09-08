using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Aplica el sprite, animator y estadísticas del personaje elegido al Player.
    /// Agrega este componente al GameObject Player en cada escena de nivel.
    /// </summary>
    public class CharacterApplier : MonoBehaviour
    {
        private SpriteRenderer _sr;
        private Animator       _animator;
        private PlayerController _player;
        private CharacterData  _data;

        private void Start()
        {
            _sr       = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _player   = GetComponent<PlayerController>();

            ApplyCharacter();
        }

        private void Update()
        {
            // Pasar velocidad al animator del perro en cada frame
            // Usamos valor absoluto real (no normalizado) para que coincida con
            // las transiciones del AnimatorController del perro (velocityX > 3, > 0.1)
            if (_animator != null && _player != null && _data != null && _data.animatorController != null)
            {
                float velX = Mathf.Abs(_player.velocity.x);
                _animator.SetFloat("velocityX", velX);
                _animator.SetBool("grounded", _player.IsGrounded);
            }
        }


        private void ApplyCharacter()
        {
            if (GameData.Instance == null || GameData.Instance.selectedCharacter == null)
            {
                Debug.Log("[CharacterApplier] Sin personaje seleccionado, usando valores por defecto.");
                return;
            }

            _data = GameData.Instance.selectedCharacter;

            // ── 1. Estadísticas de movimiento ──
            if (_player != null)
                _player.maxSpeed = _data.moveSpeed;

            var model = Simulation.GetModel<Platformer.Model.PlatformerModel>();
            if (model != null)
                model.jumpModifier = _data.jumpStrength / 10f;

            // ── 2. Aplicar AnimatorController del perro ──
            if (_data.animatorController != null && _animator != null)
            {
                _animator.runtimeAnimatorController = _data.animatorController;
                Debug.Log($"[CharacterApplier] Animator aplicado: {_data.animatorController.name}");
            }
            else if (_sr != null)
            {
                // Fallback sin animator: mostrar sprite idle con color
                if (_data.idleSprite != null)
                    _sr.sprite = _data.idleSprite;
                else
                    _sr.color = _data.characterColor;
            }

            // ── 3. Escala del sprite de perro ──
            if (_data.animatorController != null || _data.idleSprite != null)
            {
                // Los sprites de perro son 100x100 px a 16 PPU → tamaño ~6 unidades
                // Ajustar para que sea del tamaño correcto en el nivel
                transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }

            Debug.Log($"[CharacterApplier] ✅ {_data.characterName} | " +
                      $"Vel:{_data.moveSpeed} Salto:{_data.jumpStrength} Vidas:{GameData.Instance.lives}");
        }
    }
}
