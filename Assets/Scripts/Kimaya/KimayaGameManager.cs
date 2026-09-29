using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace Kimaya
{
    /// <summary>
    /// GameManager global de Kimaya.
    /// Maneja victoria, derrota, pausa, cambio de escenas y UI principal.
    /// </summary>
    public class KimayaGameManager : MonoBehaviour
    {
        public static KimayaGameManager Instance { get; private set; }

        [Header("UI Panels")]
        public GameObject pausePanel;
        public GameObject gameOverPanel;
        public GameObject victoryPanel;
        public GameObject hud;

        [Header("HUD - Vidas")]
        public TextMeshProUGUI livesText;
        public TextMeshProUGUI activeCharText;
        public Image smellCooldownBar;

        [Header("Flash de daño")]
        public Image damageFlashImage;
        public float flashDuration = 0.3f;

        [Header("Escenas")]
        public string mainMenuScene  = "MainMenu";
        public string nextLevelScene = "Ending";

        private bool _isPaused = false;
        private CharacterSwitchController _switcher;
        private MiaAbilities _mia;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            _switcher = FindAnyObjectByType<CharacterSwitchController>();
            _mia      = FindAnyObjectByType<MiaAbilities>();

            if (damageFlashImage != null) damageFlashImage.color = Color.clear;
            if (pausePanel   != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                TogglePause();

            // Actualizar HUD
            if (activeCharText != null && _switcher != null)
                activeCharText.text = $"▶ {_switcher.ActiveName}";

            if (smellCooldownBar != null && _mia != null)
                smellCooldownBar.fillAmount = _mia.SmellCooldownNormalized;
        }

        public void TogglePause()
        {
            _isPaused = !_isPaused;
            Time.timeScale = _isPaused ? 0f : 1f;
            if (pausePanel != null) pausePanel.SetActive(_isPaused);
            if (hud != null) hud.SetActive(!_isPaused);
        }

        public void UpdateLives(int lives)
        {
            if (livesText != null)
                livesText.text = $"❤ {lives}";
        }

        public void ShowRespawnFlash()
        {
            if (damageFlashImage != null)
                StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            if (damageFlashImage == null) yield break;
            damageFlashImage.color = new Color(1f, 0.2f, 0.2f, 0.6f);
            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                damageFlashImage.color = Color.Lerp(
                    new Color(1f, 0.2f, 0.2f, 0.6f), Color.clear, t / flashDuration);
                yield return null;
            }
            damageFlashImage.color = Color.clear;
        }

        public void GameOver()
        {
            Time.timeScale = 0f;
            if (hud != null) hud.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(true);
        }

        public void Victory()
        {
            Time.timeScale = 0f;
            if (hud != null) hud.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(true);
        }

        // ── Botones de UI ───────────────────────────────────────────────────
        public void OnResumeButton()     => TogglePause();
        public void OnRestartButton()    { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        public void OnMainMenuButton()   { Time.timeScale = 1f; SceneManager.LoadScene(mainMenuScene); }
        public void OnNextLevelButton()  { Time.timeScale = 1f; SceneManager.LoadScene(nextLevelScene); }
    }

    /// <summary>
    /// Enemigo patrullero sencillo — el Zarzal Sombrío.
    /// Patrulla entre dos puntos; si toca a una perra, activa TakeDamage.
    /// </summary>
    public class EnemyPatrol : MonoBehaviour
    {
        [Header("Patrulla")]
        public float patrolDistance = 4f;
        public float moveSpeed      = 1.8f;
        public float waitAtEnd      = 0.8f;

        [Header("Daño")]
        public float damageKnockback = 5f;

        private Vector3 _startPos;
        private Vector3 _endPos;
        private bool    _movingRight = true;
        private bool    _waiting = false;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr       = GetComponent<SpriteRenderer>();
            if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
            _startPos = transform.position;
            _endPos   = transform.position + Vector3.right * patrolDistance;
        }

        private void Update()
        {
            if (_waiting) return;

            Vector3 target = _movingRight ? _endPos : _startPos;
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);

            if (_sr != null) _sr.flipX = !_movingRight;

            if (Vector3.Distance(transform.position, target) < 0.05f)
                StartCoroutine(WaitAndTurn());
        }

        private IEnumerator WaitAndTurn()
        {
            _waiting = true;
            yield return new WaitForSeconds(waitAtEnd);
            _movingRight = !_movingRight;
            _waiting = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            // Knockback
            var rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                rb.linearVelocity = dir * damageKnockback + Vector2.up * 4f;
            }

            RespawnManager.Instance?.TakeDamage();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 s = Application.isPlaying ? _startPos : transform.position;
            Vector3 e = Application.isPlaying ? _endPos   : transform.position + Vector3.right * patrolDistance;
            Gizmos.DrawLine(s, e);
            Gizmos.DrawWireSphere(s, 0.2f);
            Gizmos.DrawWireSphere(e, 0.2f);
        }
    }
}
