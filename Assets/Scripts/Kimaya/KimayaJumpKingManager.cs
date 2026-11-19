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
    /// - En Ascenso_Kimaya (useCustomKimayaTower = true): aplica el mapa original de Kimaya (Bosque Mágico -> Zarzal Sombrío + Jaula Espinosa).
    /// - En Ascenso_JumpKing_Classic (useCustomKimayaTower = false): respeta las 5 pantallas clásicas de Jump King.
    /// - Al rescatar a Kiara en la cumbre: bloquea al personaje limpiamente junto a Kiara (sin trabarse en el aire ni salir de pantalla)
    ///   y muestra el cartel interactivo Pixel-Art de Victoria.
    /// </summary>
    public class KimayaJumpKingManager : MonoBehaviour
    {
        public static KimayaJumpKingManager Instance { get; private set; }

        [Header("Modo de Nivel")]
        [Tooltip("True para el nivel original de Kimaya (Bosque Mágico / Zarzal). False para conservar el nivel clásico Jump King.")]
        public bool useCustomKimayaTower = false;

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

        [Header("Panel de Victoria (Legacy - reemplazado por modal Pixel-Art interactivo)")]
        public GameObject victoryPanel;
        public Text victoryStatsText;
        public Text victoryDescText;
        public Button restartButton;
        public Button returnToHubButton;

        [Header("Panel de Game Over")]
        public GameObject gameOverPanel;
        public Text gameOverStatsText;
        public Text gameOverDescText;
        public Button gameOverRestartButton;
        public Button gameOverHubButton;

        [Header("Reglas de Juego")]
        public bool isPaused = false;
        private bool _showHubConfirmModal = false;
        private bool _showSettingsModal = false;

        public bool enableLivesSystem = false;
        public int maxLives = 3;
        public int currentLives = 3;

        [Header("Puntaje y Scooby Galletas")]
        public int currentScore = 0;
        public int relicsCollected = 0;
        public int totalRelicsInLevel = 3;

        [Header("Meta / Rescate")]
        public int finalScreenIndex = 4; // 5 Pisos (0 a 4)
        public string rescuedCharacterName = "Kiara";

        private float _sessionTimer = 0f;
        private float _noticeTimer = 0f;
        private string _noticeMessage = "";
        private bool _hasWon = false;
        private bool _isGameOver = false;
        private string _victorySummaryStats = "";
        private string _victoryDialogueBody = "";
        private KimayaPixelHUD _pixelHud;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            enableLivesSystem = false;
        }

        private void Start()
        {
            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isCustom = useCustomKimayaTower || KimayaCustomTowerMap.IsCustomKimayaScene(activeScene);
            if (activeScene.Contains("JumpKing_Classic") || activeScene.Contains("Test_Mecanica"))
            {
                isCustom = false;
                useCustomKimayaTower = false;
            }
            else if (isCustom)
            {
                useCustomKimayaTower = true;
                KimayaCustomTowerMap.ApplyCustomTowerToCurrentScene(forceApply: true);
            }

            HideLegacyHUDs();

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<KimayaJumpKingController>();
            }

            if (screenCamera == null)
            {
                screenCamera = FindAnyObjectByType<JumpKingScreenCamera>();
            }
            if (screenCamera != null)
            {
                screenCamera.maxScreenIndex = finalScreenIndex;
            }

            currentLives = maxLives;
            currentScore = 0;
            relicsCollected = 0;
            isPaused = false;
            Time.timeScale = 1f;
            enableLivesSystem = false;
            _showHubConfirmModal = false;

            string selectedChar = PlayerPrefs.GetString("SelectedCharacter", "Falin");
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController.SetCharacter(selectedChar == "Mia"
                    ? KimayaJumpKingController.CharacterType.Mia
                    : KimayaJumpKingController.CharacterType.Falin);
                var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
                if (anim != null)
                {
                    anim.useKnightSkin = false;
                    anim.ApplyInitialSprite();
                }
            }

            // Ocultar paneles legacy que dependían de StandaloneInputModule para evitar bloqueos de clic
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (stunnedBadge != null) stunnedBadge.SetActive(false);

            var hudCanvas = GetComponent<Canvas>();
            if (hudCanvas == null) hudCanvas = FindAnyObjectByType<Canvas>();
            if (hudCanvas != null)
            {
                _pixelHud = gameObject.GetComponent<KimayaPixelHUD>();
                if (_pixelHud == null) _pixelHud = gameObject.AddComponent<KimayaPixelHUD>();
                _pixelHud.Initialize(hudCanvas);
            }

            EnsureMidLevelCheckpoint(isCustom);
            KimayaUITheme.ApplyThemeToActiveScene();
            UpdateHUD();
            NotifyPhone();
        }

        void NotifyPhone()
        {
            string hero = playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Mia
                ? "mia"
                : "falin";
            string state = (isPaused || _hasWon || _isGameOver) ? "paused" : "playing";
            Kimaya.Networking.PhonePadServer.Send("{\"event\":\"character_switch\",\"character\":\"" + hero + "\"}");
            Kimaya.Networking.PhonePadServer.Send("{\"event\":\"game_state\",\"state\":\"" + state + "\"}");
        }

        private void HideLegacyHUDs()
        {
            string[] legacyNames = new string[] { "HUD", "GameHUD", "Canvas_HUD", "MetaGameController", "TokenHUD", "TopHUD", "Txt_Controls" };
            foreach (var n in legacyNames)
            {
                var go = GameObject.Find(n);
                if (go != null && go != gameObject && !go.transform.IsChildOf(transform))
                {
                    go.SetActive(false);
                }
            }
            var childTopHud = transform.Find("TopHUD");
            if (childTopHud != null) childTopHud.gameObject.SetActive(false);
            var childCtrls = transform.Find("Txt_Controls");
            if (childCtrls != null) childCtrls.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (ReadRestartKey())
            {
                RestartSession();
                return;
            }

            if (_hasWon || _isGameOver) return;

            _sessionTimer += Time.deltaTime;
            if (_noticeTimer > 0f) _noticeTimer -= Time.deltaTime;

            if (ReadTabOrQ())
            {
                _noticeTimer = 3.5f;
                _noticeMessage = "! Personaje bloqueado durante el nivel. Regresa al Hub para cambiarlo.";
            }

            if (ReadSkinToggleKey())
            {
                ToggleSkin();
            }

            if (ReadEscapeKey())
            {
                if (_showSettingsModal) _showSettingsModal = false;
                else if (_showHubConfirmModal) _showHubConfirmModal = false;
                else TogglePause();
            }

            if (isPaused) return;

            UpdateHUD();
        }

        private bool ReadRestartKey()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.R)) return true; } catch { }
            return false;
        }

        private bool ReadEscapeKey()
        {
            if (Kimaya.Networking.PhonePadServer.Pressed("START"))
                return true;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;
#endif
            try { if (Input.GetKeyDown(KeyCode.Escape)) return true; } catch { }
            return false;
        }

        public void TogglePause()
        {
            if (_hasWon || _isGameOver) return;
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
            _showHubConfirmModal = false;
            NotifyPhone();
        }

        public void ReturnToHub()
        {
            Time.timeScale = 1f;
            isPaused = false;
            _showHubConfirmModal = false;
            if (Application.CanStreamedLevelBeLoaded("Hub"))
                KimayaSceneTransition.LoadScene("Hub");
            else
                KimayaSceneTransition.LoadScene("MainMenu");
        }

        public void GoToEndingScene()
        {
            Time.timeScale = 1f;
            isPaused = false;
            if (Application.CanStreamedLevelBeLoaded("Ending"))
                KimayaSceneTransition.LoadScene("Ending");
            else
                ReturnToHub();
        }

        public void AddScore(int points, string sourceName = "Scooby Galleta")
        {
            if (_hasWon || _isGameOver) return;
            currentScore += points;
            relicsCollected++;
            if (playerController != null)
            {
                KimayaPixelVFX.SpawnCookieSparkles(playerController.transform.position + new Vector3(0f, 0.35f, 0f));
            }
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Checkpoint);
            }
            UpdateHUD();
        }

        public void RegisterFall()
        {
            if (_hasWon || _isGameOver) return;
            UpdateHUD();
        }

        public void TriggerGameOver()
        {
            if (_isGameOver || _hasWon) return;
            _isGameOver = true;

            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Damage);
            }

            if (playerController != null)
            {
                playerController.LockAtSummit(playerController.transform.position, playerController.FacingDirection);
            }

            string heroName = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "falin" : "mia";
            Kimaya.Networking.DatabaseManager.Instance?.SaveJumpKingSession(
                "game_over",
                currentScore,
                Mathf.RoundToInt(_sessionTimer),
                playerController != null ? playerController.TotalJumps : 0,
                playerController != null ? playerController.TotalFalls : 0,
                heroName
            );
            NotifyPhone();
            UpdateHUD();
        }

        public void TriggerVictory(string customMessage = null, Vector3? heroStandPos = null)
        {
            if (_hasWon || _isGameOver) return;
            _hasWon = true;

            // Asegurar que la jaula espinosa (si está presente) se rompa visualmente
            var altar = FindAnyObjectByType<KimayaAltarGoal>();
            if (altar != null)
            {
                altar.ForceRescue();
            }

            // Posicionar y bloquear limpiamente al perrito en el pedestal junto a Kiara (sin trabarse ni salir de pantalla)
            if (playerController != null)
            {
                Vector3 standPos = heroStandPos ?? (altar != null ? altar.GetHeroStandPosition() : playerController.transform.position);
                playerController.LockAtSummit(standPos, 1f);
                var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
                if (anim != null) anim.ApplyInitialSprite();
            }

            if (screenCamera != null)
            {
                screenCamera.maxScreenIndex = finalScreenIndex;
                screenCamera.RecalculateScreenIndex(instant: true);
            }

            // Mantener oculto cualquier panel legacy que bloquee botones por StandaloneInputModule
            if (victoryPanel != null) victoryPanel.SetActive(false);

            int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
            int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
            string timeStr = $"{minutes:00}:{seconds:00}";
            int jumps = playerController != null ? playerController.TotalJumps : 0;
            int falls = playerController != null ? playerController.TotalFalls : 0;
            string hero = (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Falin) ? "Falin" :
                          (playerController != null && playerController.character == KimayaJumpKingController.CharacterType.Mia) ? "Mía" : "Caballero";

            int totalFinalScore = currentScore + (relicsCollected * 200);

            float prevBest = PlayerPrefs.GetFloat("Kimaya_BestTimeSeconds", 999999f);
            bool isNewRecord = (_sessionTimer > 0.5f && _sessionTimer < prevBest);
            if (isNewRecord)
            {
                PlayerPrefs.SetFloat("Kimaya_BestTimeSeconds", _sessionTimer);
            }

            PlayerPrefs.SetInt("KiaraRescued", 1);
            PlayerPrefs.SetInt("FirstRuneActive", 1);
            PlayerPrefs.Save();

            float bestNow = Mathf.Min(prevBest, _sessionTimer > 0.5f ? _sessionTimer : prevBest);
            int bMin = Mathf.FloorToInt(bestNow / 60f);
            int bSec = Mathf.FloorToInt(bestNow % 60f);
            string bestTag = (bestNow < 900000f) ? $"  |  Mejor: <b>{bMin:00}:{bSec:00}</b>{(isNewRecord ? " <color=#5AFF8C>(¡RÉCORD!)</color>" : "")}" : "";

            string medalTag = _sessionTimer < 105f
                ? "<color=#FFD84A>[★ MEDALLA DE ORO ★]</color>"
                : (_sessionTimer < 180f ? "<color=#E0E8F6>[★ MEDALLA DE PLATA ★]</color>" : "<color=#ECA062>[★ MEDALLA DE BRONCE ★]</color>");

            if (playerController != null)
            {
                KimayaPixelVFX.SpawnCookieSparkles(playerController.transform.position + new Vector3(0f, 0.45f, 0f));
            }
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Victory);
            }

            _victorySummaryStats = $"{medalTag}  Tiempo: <b>{timeStr}</b>{bestTag}  |  Galletas: <b>{relicsCollected}/{totalRelicsInLevel}</b>  |  Saltos: <b>{jumps}</b>";
            _victoryDialogueBody = (hero == "Falin")
                ? "— <b>Falin:</b> ¡Kiara! ¡Rompimos la trampa, ya estás libre!\n— <b>Kiara:</b> ¡Gracias hermana! ¡Volvamos juntas al Bosque del Eco!"
                : "— <b>Mía:</b> ¡Kiara! ¡La jaula cayó, ya estás a salvo!\n— <b>Kiara:</b> ¡Mía, valiente hermana! ¡El Bosque del Eco nos espera!";

            string victoryHero = (hero == "Falin") ? "falin" : "mia";
            Kimaya.Networking.DatabaseManager.Instance?.SaveJumpKingSession(
                "nivel_completado",
                totalFinalScore,
                Mathf.RoundToInt(_sessionTimer),
                jumps,
                falls,
                victoryHero
            );
            NotifyPhone();
        }

        public void RestartSession()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnGUI()
        {
            if (isPaused)
            {
                Color prevColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, 0.78f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = prevColor;

                int minutes = Mathf.FloorToInt(_sessionTimer / 60f);
                int seconds = Mathf.FloorToInt(_sessionTimer % 60f);
                int curFloor = screenCamera != null ? screenCamera.CurrentScreenIndex + 1 : 1;

                if (_showSettingsModal)
                {
                    float setW = Mathf.Min(650f, Screen.width - 40f);
                    float setH = 440f;
                    float setX = (Screen.width - setW) * 0.5f;
                    float setY = (Screen.height - setH) * 0.5f;

                    if (KimayaSettingsController.Instance != null)
                    {
                        KimayaSettingsController.Instance.DrawSettingsUI(new Rect(setX, setY, setW, setH), () => {
                            _showSettingsModal = false;
                        });
                    }
                    else
                    {
                        _showSettingsModal = false;
                    }
                    return;
                }

                if (!_showHubConfirmModal)
                {
                    float modalW = Mathf.Min(580f, Screen.width - 40f);
                    float modalH = 380f;
                    float modalX = (Screen.width - modalW) * 0.5f;
                    float modalY = (Screen.height - modalH) * 0.5f;

                    GUI.backgroundColor = new Color(0.10f, 0.07f, 0.16f, 0.96f);
                    GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none);

                    GUIStyle titleStyle = KimayaUITheme.GetOnGUILabelStyle(24, TextAnchor.MiddleCenter, false);
                    GUI.Label(new Rect(modalX, modalY + 18f, modalW, 36f), "<color=#FFD95A>PAUSA</color>", titleStyle);

                    GUIStyle statStyle = KimayaUITheme.GetOnGUILabelStyle(19, TextAnchor.MiddleCenter, true);
                    GUI.Label(new Rect(modalX, modalY + 58f, modalW, 30f),
                        $"<color=#E2E8F0>Tiempo: <b>{minutes:00}:{seconds:00}</b>  |  Piso: <b>{curFloor}/5</b>  |  Scooby Galletas: <b>{relicsCollected}/{totalRelicsInLevel}</b></color>", statStyle);

                    float btnW = modalW - 80f;
                    float btnX = modalX + 40f;

                    GUIStyle btnStyle = KimayaUITheme.GetOnGUIButtonStyle(15);
                    GUI.backgroundColor = Color.white;

                    if (GUI.Button(new Rect(btnX, modalY + 104f, btnW, 50f), "> CONTINUAR ASCENSO [ESC]", btnStyle))
                    {
                        TogglePause();
                    }

                    if (GUI.Button(new Rect(btnX, modalY + 164f, btnW, 50f), "AJUSTES (AUDIO Y PANTALLA)", btnStyle))
                    {
                        _showSettingsModal = true;
                    }

                    if (GUI.Button(new Rect(btnX, modalY + 224f, btnW, 50f), "REINICIAR DESDE LA BASE [R]", btnStyle))
                    {
                        Time.timeScale = 1f;
                        isPaused = false;
                        RestartSession();
                    }

                    if (GUI.Button(new Rect(btnX, modalY + 284f, btnW, 50f), "REGRESAR AL CAMPAMENTO", btnStyle))
                    {
                        _showHubConfirmModal = true;
                    }
                }
                else
                {
                    float modalW = Mathf.Min(620f, Screen.width - 40f);
                    float modalH = 350f;
                    float modalX = (Screen.width - modalW) * 0.5f;
                    float modalY = (Screen.height - modalH) * 0.5f;

                    GUI.backgroundColor = new Color(0.14f, 0.07f, 0.12f, 0.98f);
                    GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none);

                    GUIStyle warnTitleStyle = KimayaUITheme.GetOnGUILabelStyle(20, TextAnchor.MiddleCenter, false);
                    GUI.Label(new Rect(modalX, modalY + 20f, modalW, 35f), "<color=#FF6B6B>¿ABANDONAR EL ASCENSO?</color>", warnTitleStyle);

                    GUIStyle warnBodyStyle = KimayaUITheme.GetOnGUILabelStyle(20, TextAnchor.MiddleCenter, true);
                    string warningMsg =
                        "<color=#FFFFFF>Si regresas al Campamento ahora, perderás tu progreso actual:</color>\n\n" +
                        "<color=#FFBBBB>• Se perderá la altura y el piso alcanzado.\n" +
                        "• Se reiniciará el tiempo del cronómetro.\n" +
                        "• Tendrás que volver a escalar desde la base.</color>\n\n" +
                        "<color=#FFDD88>¿Deseas regresar al Campamento?</color>";

                    GUI.Label(new Rect(modalX + 30f, modalY + 62f, modalW - 60f, 175f), warningMsg, warnBodyStyle);

                    float btnW = (modalW - 90f) * 0.5f;
                    float btnH = 52f;
                    float btnY = modalY + 268f;

                    GUIStyle btnStyle = KimayaUITheme.GetOnGUIButtonStyle(13);
                    GUI.backgroundColor = Color.white;

                    if (GUI.Button(new Rect(modalX + 30f, btnY, btnW, btnH), "SÍ, SALIR AL HUB", btnStyle))
                    {
                        ReturnToHub();
                    }

                    if (GUI.Button(new Rect(modalX + 60f + btnW, btnY, btnW, btnH), "< SEGUIR JUGANDO", btnStyle))
                    {
                        _showHubConfirmModal = false;
                    }
                }
                return;
            }

            if (_noticeTimer > 0f)
            {
                float noticeW = 640f;
                float noticeH = 44f;
                float noticeX = (Screen.width - noticeW) * 0.5f;
                float noticeY = 108f;
                GUI.backgroundColor = new Color(0.14f, 0.08f, 0.18f, 0.95f);
                GUI.Box(new Rect(noticeX, noticeY, noticeW, noticeH), GUIContent.none);
                GUIStyle nStyle = KimayaUITheme.GetOnGUILabelStyle(18, TextAnchor.MiddleCenter, true);
                GUI.Label(new Rect(noticeX, noticeY, noticeW, noticeH), $"<color=#FFD95A>{_noticeMessage}</color>", nStyle);
            }

            if (_hasWon)
            {
                float modalW = Mathf.Min(680f, Screen.width - 40f);
                float modalH = 310f;
                float modalX = (Screen.width - modalW) * 0.5f;
                // Colocado en la mitad inferior para dejar visible arriba a Kiara liberada junto a Falin/Mía
                float modalY = Mathf.Clamp(Screen.height * 0.54f, 220f, Screen.height - modalH - 24f);

                GUI.backgroundColor = new Color(0.08f, 0.06f, 0.15f, 0.95f);
                GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none);

                GUIStyle titleStyle = KimayaUITheme.GetOnGUILabelStyle(22, TextAnchor.MiddleCenter, false);
                GUI.Label(new Rect(modalX, modalY + 16f, modalW, 34f), "<color=#FFD95A>¡KIARA RESCATADA!</color>", titleStyle);

                GUIStyle statStyle = KimayaUITheme.GetOnGUILabelStyle(18, TextAnchor.MiddleCenter, true);
                GUI.Label(new Rect(modalX + 20f, modalY + 54f, modalW - 40f, 30f), $"<color=#A7F3D0>{_victorySummaryStats}</color>", statStyle);

                GUIStyle bodyStyle = KimayaUITheme.GetOnGUILabelStyle(19, TextAnchor.MiddleCenter, true);
                GUI.Label(new Rect(modalX + 28f, modalY + 92f, modalW - 56f, 110f), $"<color=#F8FAFC>{_victoryDialogueBody}</color>", bodyStyle);

                float btnSpacing = 14f;
                float btnW = (modalW - 56f - btnSpacing * 2f) / 3f;
                float btnH = 52f;
                float btnY = modalY + modalH - 72f;
                float startX = modalX + 28f;

                GUIStyle btnStyle = KimayaUITheme.GetOnGUIButtonStyle(13);
                GUI.backgroundColor = Color.white;

                if (GUI.Button(new Rect(startX, btnY, btnW, btnH), "VER FINAL", btnStyle))
                {
                    GoToEndingScene();
                }

                if (GUI.Button(new Rect(startX + btnW + btnSpacing, btnY, btnW, btnH), "IR AL HUB", btnStyle))
                {
                    ReturnToHub();
                }

                if (GUI.Button(new Rect(startX + (btnW + btnSpacing) * 2f, btnY, btnW, btnH), "REPETIR [R]", btnStyle))
                {
                    RestartSession();
                }
                return;
            }

            if (_isGameOver)
            {
                float btnW = 240f;
                float btnH = 52f;
                float spacing = 20f;
                float totalW = btnW * 2f + spacing;
                float startX = (Screen.width - totalW) * 0.5f;
                float startY = (Screen.height * 0.5f) + 120f;

                GUIStyle btnStyle = KimayaUITheme.GetOnGUIButtonStyle(14);
                GUI.backgroundColor = Color.white;
                if (GUI.Button(new Rect(startX, startY, btnW, btnH), "REINICIAR [R]", btnStyle))
                {
                    RestartSession();
                }

                if (GUI.Button(new Rect(startX + btnW + spacing, startY, btnW, btnH), "VOLVER AL HUB", btnStyle))
                {
                    ReturnToHub();
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
            try { if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.H)) return true; } catch { }
            return false;
        }

        public void ToggleSkin()
        {
            if (playerController == null) return;
            var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
            if (anim != null) anim.ToggleKnightSkin();
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
            try { if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q)) return true; } catch { }
            return false;
        }

        public void SwitchCharacter()
        {
            if (playerController == null || !playerController.IsGrounded || playerController.IsCharging)
                return;

            if (playerController.character == KimayaJumpKingController.CharacterType.JumpKing)
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Falin);
            else if (playerController.character == KimayaJumpKingController.CharacterType.Falin)
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.Mia);
            else
                playerController.SetCharacter(KimayaJumpKingController.CharacterType.JumpKing);

            var anim = playerController.GetComponent<KimayaJumpKingAnimator>();
            if (anim != null) anim.ApplyInitialSprite();
        }

        private void UpdateHUD()
        {
            if (playerController == null) return;

            if (stunnedBadge != null)
            {
                stunnedBadge.SetActive(playerController.IsStunned);
            }

            int curFloor = (screenCamera != null ? screenCamera.CurrentScreenIndex + 1 : 1);

            if (_pixelHud != null)
            {
                _pixelHud.Refresh(
                    relicsCollected,
                    totalRelicsInLevel,
                    _sessionTimer,
                    curFloor,
                    finalScreenIndex + 1,
                    playerController.IsCharging,
                    playerController.ChargePercent
                );
            }

            if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);
            if (screenText != null) { screenText.text = ""; screenText.gameObject.SetActive(false); }
            if (characterNameText != null) { characterNameText.text = ""; characterNameText.gameObject.SetActive(false); }
            if (statsText != null) { statsText.text = ""; statsText.gameObject.SetActive(false); }
            if (scoreText != null) scoreText.text = "";
            if (livesText != null) livesText.text = "";
        }

        private void EnsureMidLevelCheckpoint(bool isCustomTower)
        {
            Vector3 cpPos = isCustomTower
                ? KimayaCustomTowerMap.MidCheckpointPosition
                : new Vector3(0.0f, 24.0f, 0f);

            var existingCp = FindAnyObjectByType<KimayaCheckpointTrigger>();
            if (existingCp == null)
            {
                var cpGo = new GameObject("Checkpoint_Canopia_Media");
                cpGo.transform.position = cpPos;
                var col = cpGo.AddComponent<BoxCollider2D>();
                col.size = new Vector2(3.4f, 2.0f);
                col.isTrigger = true;
                cpGo.AddComponent<KimayaCheckpointTrigger>();
            }
            else
            {
                existingCp.transform.position = cpPos;
            }
        }
    }
}
