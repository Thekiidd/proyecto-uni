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
        public int finalScreenIndex = 7;
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

            UpdateHUD();
            CheckVictory();
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
                stunnedBadge.SetActive(false);
            }

            // Piso / Pantalla actual
            if (screenText != null && screenCamera != null)
            {
                screenText.text = $"Piso {screenCamera.CurrentScreenIndex + 1} / {finalScreenIndex + 1}";
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
            if (screenCamera != null && screenCamera.CurrentScreenIndex >= finalScreenIndex && !_hasWon)
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
