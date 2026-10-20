using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Administrador de la partida en el modo Jump King de Kimaya (MVP - Evidencia 1).
    /// Arquitectura orientada a componentes:
    /// - Gestiona el estado de juego: Puntaje, Coleccionables (Reliquias), Cronómetro, Pantalla actual.
    /// - Sistema de Vidas / Intentos con condiciones de Éxito (Cumbre/Altar) y Falla (Caídas/Game Over).
    /// - Control de ciclo: Reinicio inmediato con [R] o botón UI.
    /// - Alternancia entre Falin, Mía y Caballero Jump King ([TAB] / [Q]).
    /// - Cambio de Skin táctico con/sin casco ([C] / [H]).
    /// </summary>
    public class KimayaJumpKingManager : MonoBehaviour
    {
        public static KimayaJumpKingManager Instance { get; private set; }

        [Header("Referencias de Personaje")]
        public KimayaJumpKingController playerController;
        public Sprite falinSprite;
        public Sprite miaSprite;
        public RuntimeAnimatorController falinAnimator;
        public RuntimeAnimatorController miaAnimator;

        [Header("Cámara")]
        public JumpKingScreenCamera screenCamera;

        [Header("UI del HUD")]
        public Slider chargeSlider;
        public Image chargeFillImage;
        public Text screenText;
        public Text characterNameText;
        public Text statsText;
        public Text scoreText;
        public Text livesText;
        public GameObject stunnedBadge;

        [Header("Panel de Victoria (Condición de Éxito)")]
        public GameObject victoryPanel;
        public Text victoryStatsText;
        public Text victoryDescText;
        public Button restartButton;
        public Button returnToHubButton;

        [Header("Panel de Game Over (Condición de Falla / Intentos)")]
        public GameObject gameOverPanel;
        public Text gameOverStatsText;
        public Text gameOverDescText;
        public Button gameOverRestartButton;
        public Button gameOverHubButton;

        [Header("Reglas de Juego: Sistema Jump King Equivalente")]
        [Tooltip("Si es false, opera en modo Jump King Puro (la caída y pérdida de altura es la penalización). Si es true, activa límite de vidas")]
        public bool enableLivesSystem = false;
        [Tooltip("Cantidad máxima de vidas o intentos si se activa el modo desafío")]
        public int maxLives = 3;
        [Tooltip("Vidas o intentos restantes")]
        public int currentLives = 3;

        [Header("Reglas de Juego: Puntaje y Reliquias")]
        public int currentScore = 0;
        public int relicsCollected = 0;
        public int totalRelicsInLevel = 3;

        [Header("Meta / Rescate")]
        public int finalScreenIndex = 4; // 5 Pisos (0 a 4)
        public string rescuedCharacterName = "Kiara";

        private float _sessionTimer = 0f;
        private bool _hasWon = false;
        private bool _isGameOver = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Ocultar cualquier HUD heredado de niveles anteriores
            HideLegacyHUDs();

            if (playerController == null)
            {
                playerController = FindFirstObjectByType<KimayaJumpKingController>();
            }

            if (screenCamera == null)
            {
                screenCamera = FindFirstObjectByType<JumpKingScreenCamera>();
            }

            currentLives = maxLives;
            currentScore = 0;
            relicsCollected = 0;

            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (stunnedBadge != null) stunnedBadge.SetActive(false);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(RestartSession);
            }
            if (returnToHubButton != null)
            {
                returnToHubButton.onClick.RemoveAllListeners();
                returnToHubButton.onClick.AddListener(ReturnToHub);
            }

            if (gameOverRestartButton != null)
            {
                gameOverRestartButton.onClick.RemoveAllListeners();
                gameOverRestartButton.onClick.AddListener(RestartSession);
            }
            if (gameOverHubButton != null)
            {
                gameOverHubButton.onClick.RemoveAllListeners();
                gameOverHubButton.onClick.AddListener(ReturnToHub);
            }

            UpdateCharacterUI();
            UpdateHUD();
        }

        private void HideLegacyHUDs()
        {
            string[] legacyNames = new string[] { "HUD", "GameHUD", "Canvas_HUD", "MetaGameController", "TokenHUD" };
            foreach (var n in legacyNames)
            {
                var go = GameObject.Find(n);
                if (go != null && go != gameObject && !go.transform.IsChildOf(transform))
                {
                    go.SetActive(false);
                }
            }
        }

        private void Update()
        {
            // Permitir reinicio rápido con R en cualquier momento o tras Victoria/Falla
            if (ReadRestartKey())
            {
                RestartSession();
                return;
            }

            if (_hasWon || _isGameOver) return;

            _sessionTimer += Time.deltaTime;

            // Cambiar de personaje con TAB o Q
            if (ReadTabOrQ())
            {
                SwitchCharacter();
            }

            // Alternar skin con C o H
            if (ReadSkinToggleKey())
            {
                ToggleSkin();
            }

            if (ReadEscapeKey())
            {
                ReturnToHub();
            }

            UpdateHUD();
            CheckVictoryFallback();
        }

        private bool ReadRestartKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                return true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.R))
                    return true;
            }
            catch { }
            return false;
        }

        private bool ReadEscapeKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    return true;
            }
            catch { }
            return false;
        }

        public void ReturnToHub()
        {
            if (Application.CanStreamedLevelBeLoaded("Hub"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Hub");
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        // ================= CAMBIOS DE ESTADO: PUNTAJE Y VIDAS =================

        /// <summary>
        /// Añade puntaje al recolectar una Reliquia Sagrada (modificación de estado).
        /// </summary>
        public void AddScore(int points, string sourceName = "Reliquia")
        {
            if (_hasWon || _isGameOver) return;

            currentScore += points;
            relicsCollected++;
            Debug.Log($"[KimayaManager] ¡{sourceName} recogida! +{points} pts. Total: {currentScore} (Reliquias: {relicsCollected}/{totalRelicsInLevel})");
            UpdateHUD();
        }

        /// <summary>
        /// Registra una caída del jugador, reduciendo vidas/intentos y evaluando condición de falla.
        /// </summary>
        public void RegisterFall()
        {
            if (_hasWon || _isGameOver) return;

            if (enableLivesSystem)
            {
                currentLives = Mathf.Max(0, currentLives - 1);
                Debug.Log($"[KimayaManager] 💥 Caída registrada. Vidas restantes: {currentLives}/{maxLives}");

                if (currentLives <= 0)
                {
                    TriggerGameOver();
                    return;
                }
            }

            UpdateHUD();
        }

        /// <summary>
        /// Dispara la condición de falla (Game Over) cuando se agotan los intentos.
        /// </summary>
        public void TriggerGameOver()
        {
            if (_isGameOver || _hasWon) return;
            _isGameOver = true;

            Debug.Log("[KimayaManager] 💀 ¡GAME OVER! Se han agotado todos los intentos de ascenso.");

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Damage);
            }

            // Congelar físicas del jugador
            if (playerController != null)
            {
                var rb = playerController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
            int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
            string timeStr = $"{minutes:00}:{seconds:00}";
            int jumps = playerController != null ? playerController.TotalJumps : 0;
            int falls = playerController != null ? playerController.TotalFalls : 0;

            if (gameOverStatsText != null)
            {
                gameOverStatsText.text = $"⏱️ TIEMPO: {timeStr}    |    🏆 PUNTAJE: {currentScore} pts\n🦘 SALTOS: {jumps}    |    💥 CAÍDAS: {falls}    |    ✨ RELIQUIAS: {relicsCollected}/{totalRelicsInLevel}";
            }

            if (gameOverDescText != null)
            {
                gameOverDescText.text = "Has agotado todos tus intentos en el ascenso del Árbol Sagrado.\nPresiona [R] para reiniciar el ciclo y volver a intentarlo.";
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // Subir sesión de Game Over a la base de datos Supabase
            string heroName = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "falin" : "mia";
            Kimaya.Networking.DatabaseManager.Instance?.SaveJumpKingSession(
                "game_over",
                currentScore,
                Mathf.RoundToInt(_sessionTimer),
                jumps,
                falls,
                heroName
            );

            UpdateHUD();
        }

        /// <summary>
        /// Dispara la condición de éxito (Victoria) al interactuar con el Altar de la Cumbre.
        /// </summary>
        public void TriggerVictory(string customMessage = null)
        {
            if (_hasWon || _isGameOver) return;
            _hasWon = true;

            int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
            int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
            string timeStr = $"{minutes:00}:{seconds:00}";
            int jumps = playerController != null ? playerController.TotalJumps : 0;
            int falls = playerController != null ? playerController.TotalFalls : 0;
            string hero = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "Falin" :
                          (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Mia) ? "Mía" : "Caballero";

            // Bonificación por reliquias
            int totalFinalScore = currentScore + (relicsCollected * 200);

            if (victoryStatsText != null)
            {
                victoryStatsText.text = $"⏱️ TIEMPO: {timeStr}    |    🏆 PUNTAJE: {totalFinalScore} pts\n🦘 SALTOS: {jumps}    |    💥 CAÍDAS: {falls}    |    ✨ RELIQUIAS: {relicsCollected}/{totalRelicsInLevel}";
            }

            if (victoryDescText != null)
            {
                if (!string.IsNullOrEmpty(customMessage))
                {
                    victoryDescText.text = $"{customMessage}\nLlegaste al Altar de la Cumbre con {hero} conservando {currentLives} intentos.";
                }
                else
                {
                    victoryDescText.text = $"¡Has conquistado los 5 Pisos del Gran Árbol Sagrado!\nLlegaste al Altar de la Cumbre y rescataste a {rescuedCharacterName} con {hero}.\n\n¡Felicidades por completar el ciclo del MVP!";
                }
            }

            // Congelar al jugador en la cumbre
            if (playerController != null)
            {
                var rb = playerController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            // Subir sesión de Victoria a la base de datos Supabase
            string victoryHero = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "falin" : "mia";
            Kimaya.Networking.DatabaseManager.Instance?.SaveJumpKingSession(
                "nivel_completado",
                totalFinalScore,
                Mathf.RoundToInt(_sessionTimer),
                jumps,
                falls,
                victoryHero
            );
        }

        public void RestartSession()
        {
            // Reinicia la escena activa limpiando el estado y restableciendo el ciclo
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        // ================= GESTIÓN DE SKINS Y PERSONAJES =================

        private void OnGUI()
        {
            // Fallback visual en caso de que los paneles de UI no tengan Canvas configurado
            if (_isGameOver)
            {
                float btnW = 240f;
                float btnH = 50f;
                float spacing = 20f;
                float totalW = btnW * 2f + spacing;
                float startX = (Screen.width - totalW) * 0.5f;
                float startY = (Screen.height * 0.5f) + 140f;

                GUI.skin.button.fontSize = 18;
                GUI.backgroundColor = new Color(0.9f, 0.25f, 0.25f, 1f);
                if (GUI.Button(new Rect(startX, startY, btnW, btnH), "🔄 Reiniciar Intento [R]"))
                {
                    RestartSession();
                }

                GUI.backgroundColor = new Color(0.20f, 0.50f, 0.85f, 1f);
                if (GUI.Button(new Rect(startX + btnW + spacing, startY, btnW, btnH), "🏡 Volver al Hub"))
                {
                    ReturnToHub();
                }
                return;
            }

            if (_hasWon)
            {
                float btnW = 220f;
                float btnH = 50f;
                float spacing = 20f;
                float totalW = btnW * 2f + spacing;
                float startX = (Screen.width - totalW) * 0.5f;
                float startY = (Screen.height * 0.5f) + 130f;

                GUI.skin.button.fontSize = 18;
                GUI.backgroundColor = new Color(0.15f, 0.75f, 0.35f, 1f);
                if (GUI.Button(new Rect(startX, startY, btnW, btnH), "🔄 Repetir Ascenso [R]"))
                {
                    RestartSession();
                }

                GUI.backgroundColor = new Color(0.20f, 0.50f, 0.85f, 1f);
                if (GUI.Button(new Rect(startX + btnW + spacing, startY, btnW, btnH), "🏡 Volver al Hub"))
                {
                    ReturnToHub();
                }
                return;
            }

            float topBtnW = 120f;
            float topBtnH = 32f;
            float topBtnX = Screen.width - topBtnW - 16f;
            float topBtnY = 16f;

            if (GUI.Button(new Rect(topBtnX, topBtnY, topBtnW, topBtnH), "🏡 Volver al Hub"))
            {
                ReturnToHub();
            }

            // Botón interactivo para alternar skin con o sin casco
            if (playerController != null && playerController.character != KimayaJumpKingController.CharacterType.JumpKing)
            {
                var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
                bool hasSkin = anim != null && anim.useKnightSkin;
                string btnText = hasSkin ? "🛡️ Skin: Con Casco [C]" : "🐾 Skin: Sin Casco [C]";
                float skinBtnW = 165f;
                float skinBtnH = 32f;
                float skinBtnX = topBtnX - skinBtnW - 12f;

                GUI.backgroundColor = hasSkin ? new Color(0.95f, 0.75f, 0.2f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
                if (GUI.Button(new Rect(skinBtnX, topBtnY, skinBtnW, skinBtnH), btnText))
                {
                    ToggleSkin();
                }
            }
        }

        private bool ReadSkinToggleKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.hKey.wasPressedThisFrame)
                    return true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.H))
                    return true;
            }
            catch { }
            return false;
        }

        public void ToggleSkin()
        {
            if (playerController == null) return;
            var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
            if (anim != null)
            {
                anim.ToggleKnightSkin();
                UpdateCharacterUI();
            }
        }

        private bool ReadTabOrQ()
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

        public void SwitchCharacter()
        {
            if (playerController == null || !playerController.IsGrounded || playerController.IsCharging)
                return;

            if (playerController.character == KimayaJumpKingController.CharacterType.JumpKing)
            {
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Falin);
            }
            else if (playerController.character == KimayaJumpKingController.CharacterType.Falin)
            {
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Mia);
            }
            else
            {
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.JumpKing);
            }

            var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
            if (anim != null) anim.ApplyInitialSprite();

            UpdateCharacterUI();
        }

        private void UpdateCharacterUI()
        {
            if (characterNameText != null && playerController != null)
            {
                var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
                bool hasSkin = (anim != null && anim.useKnightSkin);

                if (playerController.character == KimayaJumpKingController.CharacterType.JumpKing)
                {
                    characterNameText.text = "👑 Jump King (Oficial) | [TAB] Personaje";
                    characterNameText.color = new Color(1f, 0.85f, 0.2f);
                }
                else
                {
                    bool isFalin = playerController.character == KimayaJumpKingController.CharacterType.Falin;
                    string skinLabel = hasSkin ? " [🛡️ Con Casco]" : " [🐾 Sin Casco]";
                    string charName = isFalin ? "🐾 Falin" : "🌸 Mía";
                    characterNameText.text = $"{charName}{skinLabel} | [TAB] Personaje | [C] Casco";
                    characterNameText.color = isFalin ? new Color(1f, 0.75f, 0.2f) : new Color(0.9f, 0.5f, 1f);
                }
            }
        }

        private void UpdateHUD()
        {
            if (playerController == null) return;

            // Barra de carga
            if (chargeSlider != null)
            {
                chargeSlider.gameObject.SetActive(playerController.IsCharging);
                chargeSlider.value = playerController.ChargePercent;

                if (chargeFillImage != null)
                {
                    chargeFillImage.color = Color.Lerp(new Color(0.2f, 0.9f, 0.3f), new Color(1f, 0.2f, 0.2f), playerController.ChargePercent);
                }
            }

            if (stunnedBadge != null)
            {
                stunnedBadge.SetActive(playerController.IsStunned);
            }

            // Piso / Pantalla actual con nombre de zona
            if (screenText != null && screenCamera != null)
            {
                int curIdx = Mathf.Clamp(screenCamera.CurrentScreenIndex, 0, finalScreenIndex);
                string[] floorNames = new string[] {
                    "El Suelo del Bosque y Campamento Base",
                    "El Gran Tronco y Catch Ledges",
                    "Canopia Media y Ramas Deliberadas",
                    "Raíces Colosales y Hojas Rojas",
                    "La Copa del Árbol y Altar de Kiara"
                };
                string fName = (curIdx < floorNames.Length) ? floorNames[curIdx] : "Ascenso Sagrado";
                screenText.text = $"Piso {curIdx + 1} / {finalScreenIndex + 1}: {fName}";
            }

            // Estadísticas Jump King Oficial: Tiempo, Saltos y Caídas (Progreso de Ascenso)
            if (statsText != null)
            {
                int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
                int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
                string livesIndicator = enableLivesSystem ? $"  |  ❤️ Intentos: {currentLives}/{maxLives}" : "";
                string relicIndicator = (relicsCollected > 0) ? $"  |  ✨ Reliquias: {relicsCollected}/{totalRelicsInLevel}" : "";
                string scoreIndicator = (currentScore > 0) ? $"  |  🏆 {currentScore} PTS" : "";

                statsText.text = $"⏱️ {minutes:00}:{seconds:00}  |  🦘 Saltos: {playerController.TotalJumps}  |  💥 Caídas: {playerController.TotalFalls}{relicIndicator}{scoreIndicator}{livesIndicator}";
            }

            if (scoreText != null)
            {
                scoreText.text = (currentScore > 0) ? $"🏆 {currentScore} PTS" : "";
            }

            if (livesText != null)
            {
                livesText.text = enableLivesSystem ? $"❤️ {currentLives}/{maxLives}" : "";
            }
        }

        private void CheckVictoryFallback()
        {
            // Victoria de respaldo por coordenada si no tocó el trigger directo (Piso 5, Y >= 49.3f)
            if (playerController != null && playerController.transform.position.y >= 49.3f && !_hasWon)
            {
                TriggerVictory("¡Has alcanzado la cumbre sagrada!");
            }
        }
    }
}
