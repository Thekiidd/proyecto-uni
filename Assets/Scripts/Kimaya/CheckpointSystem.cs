using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Punto de control — al tocarlo, se actualiza el spawn de ambas perritas.
    /// Reutiliza el sprite de PixelAdventure Checkpoint.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Visual")]
        public Sprite flagIdleSprite;
        public Sprite flagOutSprite;
        public Color  activatedColor = new Color(0.4f, 1f, 0.5f);

        private bool _activated = false;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_activated) return;
            if (!other.CompareTag("Player")) return;

            _activated = true;
            RespawnManager.Instance?.SetCheckpoint(transform.position);

            if (_sr != null)
            {
                _sr.color = activatedColor;
                if (flagOutSprite != null) _sr.sprite = flagOutSprite;
            }

            // Partícula de confirmación
            StartCoroutine(FlashEffect());
        }

        private IEnumerator FlashEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                if (_sr != null) _sr.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                if (_sr != null) _sr.color = activatedColor;
                yield return new WaitForSeconds(0.08f);
            }
        }
    }

    /// <summary>
    /// Maneja la reaparición de ambas perritas tras caer o perder energía.
    /// Singleton accesible desde cualquier script.
    /// </summary>
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance { get; private set; }

        [Header("Referencias")]
        public PlayerController2D falin;
        public PlayerController2D mia;
        public CharacterSwitchController switcher;

        [Header("Configuración")]
        public float fallDeathY      = -10f;
        public float respawnDelay    = 1.2f;
        public int   maxLives        = 3;

        [Header("Offset entre perritas al reaparecer")]
        public Vector2 falinSpawnOffset = new Vector2(-0.8f, 0f);
        public Vector2 miaSpawnOffset   = new Vector2( 0.8f, 0f);

        private Vector3 _checkpointPos;
        private int     _lives;
        private bool    _respawning = false;

        public int Lives => _lives;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _lives         = maxLives;
            _checkpointPos = falin != null ? falin.transform.position : Vector3.zero;

            KimayaGameManager.Instance?.UpdateLives(_lives);
        }

        public void SetCheckpoint(Vector3 pos) => _checkpointPos = pos;

        private void Update()
        {
            if (_respawning) return;

            // Detectar caída
            if (falin != null && falin.transform.position.y < fallDeathY)
                StartCoroutine(Respawn());
            if (mia != null && mia.transform.position.y < fallDeathY)
                StartCoroutine(Respawn());
        }

        public void TakeDamage()
        {
            if (_respawning) return;
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            _respawning = true;
            _lives--;

            KimayaGameManager.Instance?.UpdateLives(_lives);
            KimayaGameManager.Instance?.ShowRespawnFlash();

            if (_lives <= 0)
            {
                yield return new WaitForSeconds(respawnDelay);
                KimayaGameManager.Instance?.GameOver();
                yield break;
            }

            // Congelar personajes
            if (falin != null) falin.SetActive(false);
            if (mia   != null) mia.SetActive(false);

            yield return new WaitForSeconds(respawnDelay);

            // Reposicionar
            if (falin != null)
            {
                falin.transform.position = _checkpointPos + (Vector3)falinSpawnOffset;
                falin.Rb.linearVelocity  = Vector2.zero;
                falin.SetActive(true);
            }
            if (mia != null)
            {
                mia.transform.position = _checkpointPos + (Vector3)miaSpawnOffset;
                mia.Rb.linearVelocity  = Vector2.zero;
            }

            switcher?.SwitchCharacter(); // reset al activo
            _respawning = false;
        }
    }
}
