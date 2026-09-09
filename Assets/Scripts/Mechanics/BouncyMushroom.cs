using System.Collections;
using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Hongo elástico del bosque que actúa como trampolín para los perros.
    /// Al caer sobre él, impulsa al jugador hacia arriba con gran fuerza.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BouncyMushroom : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Fuerza del impulso vertical al rebotar.")]
        public float bounceForce = 20f;

        [Header("Audio")]
        public AudioClip bounceAudio;

        private AudioSource _audioSource;
        private Vector3 _originalScale;
        private bool _isBouncing = false;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryBounce(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryBounce(collision.gameObject);
        }

        private void TryBounce(GameObject go)
        {
            var player = go.GetComponent<PlayerController>();
            if (player == null) return;

            // Comprobar que el jugador esté encima del hongo (cayendo o saltando sobre él)
            if (player.transform.position.y > transform.position.y - 0.2f)
            {
                // Aplicar impulso vertical directo
                player.velocity.y = bounceForce;
                player.jumpState = PlayerController.JumpState.InFlight;

                if (bounceAudio != null && _audioSource != null)
                {
                    _audioSource.PlayOneShot(bounceAudio);
                }

                if (!_isBouncing)
                {
                    StartCoroutine(SquashAndStretchRoutine());
                }
            }
        }

        private IEnumerator SquashAndStretchRoutine()
        {
            _isBouncing = true;

            // 1. Aplastarse (squash)
            float elapsed = 0f;
            float duration = 0.08f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = new Vector3(_originalScale.x * 1.35f, _originalScale.y * 0.6f, _originalScale.z);
                yield return null;
            }

            // 2. Estirarse hacia arriba (stretch)
            elapsed = 0f;
            duration = 0.12f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = new Vector3(_originalScale.x * 0.85f, _originalScale.y * 1.35f, _originalScale.z);
                yield return null;
            }

            // 3. Volver al tamaño normal
            transform.localScale = _originalScale;
            _isBouncing = false;
        }
    }
}
