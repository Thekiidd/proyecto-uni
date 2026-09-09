using System.Collections;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Caseta de perro interactiva que actúa como punto de control (Checkpoint).
    /// Al cruzarla, el cuenco de comida se llena, la bandera verde sube
    /// y el jugador reaparece en ella si pierde una vida.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class DogHouseCheckpoint : MonoBehaviour
    {
        [Header("Sprites de Estado")]
        public Sprite inactiveSprite;
        public Sprite activeSprite;

        [Header("Audio")]
        public AudioClip activateAudio;

        [Header("Estado")]
        public bool isActivated = false;

        private SpriteRenderer _spriteRenderer;
        private AudioSource _audioSource;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
            }

            if (inactiveSprite != null && !isActivated)
            {
                _spriteRenderer.sprite = inactiveSprite;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ActivateCheckpoint(player);
            }
        }

        public void ActivateCheckpoint(PlayerController player)
        {
            isActivated = true;

            if (activeSprite != null)
            {
                _spriteRenderer.sprite = activeSprite;
            }

            // Actualizar punto de respawn en el modelo
            var model = Simulation.GetModel<PlatformerModel>();
            if (model != null)
            {
                model.spawnPoint = this.transform;
            }

            // Efecto de sonido
            if (activateAudio != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(activateAudio);
            }

            // Pequeña animación de salto/rebote de celebración
            StartCoroutine(BounceRoutine());

            Debug.Log($"[DogHouseCheckpoint] ¡Checkpoint canino activado en {transform.position}!");
        }

        private IEnumerator BounceRoutine()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bounce = Mathf.Sin(t * Mathf.PI) * 0.25f;
                transform.localScale = new Vector3(originalScale.x * (1f - bounce * 0.5f), originalScale.y * (1f + bounce), originalScale.z);
                yield return null;
            }

            transform.localScale = originalScale;
        }
    }
}
