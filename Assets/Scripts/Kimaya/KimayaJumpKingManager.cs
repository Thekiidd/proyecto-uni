using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Kimaya
{
    /// <summary>
    /// Administrador de la partida en el modo Jump King de Kimaya.
    /// - Alterna entre Falin y Mía al presionar TAB o Q (con soporte universal para Input System).
    /// - Gestiona la UI de carga de salto, pantalla actual, saltos y caídas.
    /// - Desactiva HUDs residuales de otras escenas.
    /// - Dispara la victoria al llegar a la cima.
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
        public GameObject stunnedBadge;
        public GameObject victoryPanel;
        public Text victoryStatsText;
        public Text victoryDescText;
        public Button restartButton;
        public Button returnToHubButton;

        [Header("Meta / Rescate")]
        public int finalScreenIndex = 4; // 5 Pisos (0 a 4)
        public string rescuedCharacterName = "Kiara";

        private float _sessionTimer = 0f;
        private bool _hasWon = false;

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

            if (victoryPanel != null) victoryPanel.SetActive(false);
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

            UpdateCharacterUI();
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
            if (_hasWon) return;

            _sessionTimer += Time.deltaTime;

            // Cambiar de personaje con TAB o Q (Input System universal)
            if (ReadTabOrQ())
            {
                SwitchCharacter();
            }

            if (ReadEscapeKey())
            {
                ReturnToHub();
            }

            UpdateHUD();
            CheckVictory();
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

        private void OnGUI()
        {
            if (_hasWon)
            {
                // Botones grandes en el centro de la pantalla durante la victoria
                float btnW = 220f;
                float btnH = 50f;
                float spacing = 20f;
                float totalW = btnW * 2f + spacing;
                float startX = (Screen.width - totalW) * 0.5f;
                float startY = (Screen.height * 0.5f) + 130f;

                GUI.skin.button.fontSize = 18;
                GUI.backgroundColor = new Color(0.15f, 0.75f, 0.35f, 1f);
                if (GUI.Button(new Rect(startX, startY, btnW, btnH), "🔄 Repetir Ascenso"))
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

            UpdateCharacterUI();
        }

        private void UpdateCharacterUI()
        {
            if (characterNameText != null && playerController != null)
            {
                if (playerController.character == KimayaJumpKingController.CharacterType.JumpKing)
                {
                    characterNameText.text = "👑 Jump King (Oficial)";
                    characterNameText.color = new Color(1f, 0.85f, 0.2f);
                }
                else
                {
                    bool isFalin = playerController.character == KimayaJumpKingController.CharacterType.Falin;
                    characterNameText.text = isFalin ? "🐾 Falin (Fuerza)" : "🌸 Mía (Agilidad)";
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

            // Estadísticas (Saltos y Caídas)
            if (statsText != null)
            {
                int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
                int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
                statsText.text = $"⏱️ {minutes:00}:{seconds:00}  |  🦘 Saltos: {playerController.TotalJumps}  |  💥 Caídas: {playerController.TotalFalls}";
            }
        }

        private void CheckVictory()
        {
            // Victoria cuando el jugador llega al gran altar sagrado en la cumbre del templo (Piso 5, Y >= 45.0f)
            if (playerController != null && playerController.transform.position.y >= 45.0f && !_hasWon)
            {
                TriggerVictory();
            }
        }

        public void RestartSession()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        public void TriggerVictory()
        {
            _hasWon = true;

            int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
            int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
            string timeStr = $"{minutes:00}:{seconds:00}";
            int jumps = playerController != null ? playerController.TotalJumps : 0;
            int falls = playerController != null ? playerController.TotalFalls : 0;
            string hero = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "Falin" : "Mía";

            if (victoryStatsText != null)
            {
                victoryStatsText.text = $"⏱️ TIEMPO: {timeStr}    |    🦘 SALTOS: {jumps}    |    💥 CAÍDAS: {falls}";
            }

            if (victoryDescText != null)
            {
                victoryDescText.text = $"¡Has conquistado los 5 Pisos del Gran Árbol Sagrado!\nLlegaste al Altar de la Cumbre y rescataste a Kiara con {hero}.\n\n¡Felicidades por completar el ascenso!";
            }

            // Congelar al jugador en la cumbre para evitar caídas post-victoria
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
        }
    }
}
