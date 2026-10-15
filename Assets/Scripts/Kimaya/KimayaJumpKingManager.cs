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
            float btnW = 120f;
            float btnH = 32f;
            float btnX = Screen.width - btnW - 16f;
            float btnY = 16f;

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "🏡 Volver al Hub"))
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

            if (playerController.character == KimayaJumpKingController.CharacterType.Falin)
            {
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Mia);
            }
            else
            {
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Falin);
            }

            UpdateCharacterUI();
        }

        private void UpdateCharacterUI()
        {
            if (characterNameText != null && playerController != null)
            {
                bool isFalin = playerController.character == KimayaJumpKingController.CharacterType.Falin;
                characterNameText.text = isFalin ? "🐾 Falin (Fuerza)" : "🌸 Mía (Agilidad)";
                characterNameText.color = isFalin ? new Color(1f, 0.75f, 0.2f) : new Color(0.9f, 0.5f, 1f);
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
                    "El Bosque Profundo y la Entrada a las Ruinas",
                    "La Fortaleza y Muros de Piedra Ancestrales",
                    "Los Acueductos y el Campanario Olvidado",
                    "La Aguja de Hielo y Cristales Celestiales",
                    "La Cumbre del Templo al Atardecer (Altar de Kiara)"
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

        public void TriggerVictory()
        {
            _hasWon = true;
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }
    }
}
