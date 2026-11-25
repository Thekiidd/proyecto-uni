using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Controla la pantalla de inicio del juego con botones Pixel-Art (Crema / Oro / Negro) y tipografía Pixel.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelMain;
        [SerializeField] private GameObject panelCharacterSelect;
        [SerializeField] private GameObject panelSettings;
        [SerializeField] private GameObject panelHelp;

        [Header("Botones Menú Principal")]
        [SerializeField] private Button btnStory;
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnHelp;
        [SerializeField] private Button btnQuit;

        [Header("Botones Modales")]
        [SerializeField] private Button btnSettingsBack;
        [SerializeField] private Button btnHelpBack;

        [Header("Botones Selección de Personaje")]
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnBack;

        [Header("Referencias")]
        [SerializeField] private CharacterSelectUI characterSelectUI;

        [Header("Animación de Fondo")]
        [SerializeField] private RectTransform backgroundTransform;

        private void Start()
        {
            EnsureSingletons();
            Kimaya.KimayaUITheme.ApplyThemeToActiveScene();

            ShowMainPanel();

            if (btnPlay != null) btnPlay.onClick.AddListener(StartPlayGame);
            if (btnStory != null) btnStory.onClick.AddListener(StartStoryMode);
            if (btnSettings != null) btnSettings.onClick.AddListener(ShowSettings);
            if (btnHelp != null) btnHelp.onClick.AddListener(ShowHelp);
            if (btnQuit != null) btnQuit.onClick.AddListener(QuitGame);

            if (btnStart != null) btnStart.onClick.AddListener(StartGame);
            if (btnBack != null) btnBack.onClick.AddListener(ShowMainPanel);
            if (btnSettingsBack != null) btnSettingsBack.onClick.AddListener(ShowMainPanel);
            if (btnHelpBack != null) btnHelpBack.onClick.AddListener(ShowMainPanel);
        }

        // Navegación con el control del celular:
        // palanca arriba/izquierda = anterior, abajo/derecha = siguiente,
        // A = presiona el botón marcado, START = JUGAR en el menú principal, B = regresar.
        private const int PanelMain = 0, PanelCharacter = 1, PanelHelp = 2, PanelSettings = 3;
        private int currentPanel = -1;
        private int focusIndex;
        private readonly System.Collections.Generic.List<Button> focusList = new System.Collections.Generic.List<Button>();
        private readonly bool[] prevDirs = new bool[4];

        private void Update()
        {
            bool up = Kimaya.Networking.PhonePadServer.Held("UP") || Kimaya.Networking.PhonePadServer.Held("LEFT");
            bool down = Kimaya.Networking.PhonePadServer.Held("DOWN") || Kimaya.Networking.PhonePadServer.Held("RIGHT");
            bool prevUp = prevDirs[0], prevDown = prevDirs[1];
            // Solo cuenta el momento en que la palanca cruza el umbral, para no saltar varios botones por frame.
            bool moveUp = up && !prevUp;
            bool moveDown = down && !prevDown;
            prevDirs[0] = up;
            prevDirs[1] = down;

            int panel = ActivePanel();
            if (panel != currentPanel)
            {
                currentPanel = panel;
                BuildFocusList(panel);
                focusIndex = panel == PanelMain ? Mathf.Max(0, focusList.IndexOf(btnPlay)) : 0;
                Highlight();
            }

            if (focusList.Count > 0)
            {
                if (moveUp) MoveFocus(-1);
                if (moveDown) MoveFocus(1);

                if (Kimaya.Networking.PhonePadServer.Pressed("A"))
                {
                    focusList[focusIndex].onClick.Invoke();
                    return;
                }
            }

            if (Kimaya.Networking.PhonePadServer.Pressed("START") && panel == PanelMain && btnPlay != null)
            {
                btnPlay.onClick.Invoke();
                return;
            }

            if (Kimaya.Networking.PhonePadServer.Pressed("B") && panel != PanelMain)
            {
                ShowMainPanel();
            }
        }

        private int ActivePanel()
        {
            if (panelCharacterSelect != null && panelCharacterSelect.activeInHierarchy) return PanelCharacter;
            if (panelHelp != null && panelHelp.activeInHierarchy) return PanelHelp;
            if (panelSettings != null && panelSettings.activeInHierarchy) return PanelSettings;
            return PanelMain;
        }

        private void BuildFocusList(int panel)
        {
            focusList.Clear();
            if (panel == PanelMain)
                AddVisible(btnStory, btnPlay, btnSettings, btnHelp, btnQuit);
            else if (panel == PanelCharacter)
                AddVisible(btnStart, btnBack);
            else if (panel == PanelHelp)
                AddVisible(btnHelpBack);
            // Ajustes tiene sus propios controles; ahí el control solo regresa con B.
        }

        private void AddVisible(params Button[] buttons)
        {
            foreach (var b in buttons)
            {
                if (b != null && b.gameObject.activeInHierarchy && !focusList.Contains(b))
                    focusList.Add(b);
            }
        }

        private void MoveFocus(int step)
        {
            int n = focusList.Count;
            focusIndex = ((focusIndex + step) % n + n) % n;
            Highlight();
        }

        private void Highlight()
        {
            if (focusList.Count == 0 || UnityEngine.EventSystems.EventSystem.current == null) return;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(focusList[focusIndex].gameObject);
        }

        private void ShowMainPanel()
        {
            if (panelMain != null) panelMain.SetActive(true);
            if (panelCharacterSelect != null) panelCharacterSelect.SetActive(false);
            if (panelSettings != null) panelSettings.SetActive(false);
            if (panelHelp != null) panelHelp.SetActive(false);
            Time.timeScale = 1f;
        }

        private void ShowCharacterSelect()
        {
            if (panelMain != null) panelMain.SetActive(false);
            if (panelSettings != null) panelSettings.SetActive(false);
            if (panelHelp != null) panelHelp.SetActive(false);
            if (panelCharacterSelect != null)
            {
                panelCharacterSelect.SetActive(true);
                if (characterSelectUI != null) characterSelectUI.Initialize();
            }
        }

        private void ShowSettings()
        {
            if (panelMain != null) panelMain.SetActive(false);
            if (panelCharacterSelect != null) panelCharacterSelect.SetActive(false);
            if (panelHelp != null) panelHelp.SetActive(false);
            if (panelSettings != null)
            {
                panelSettings.SetActive(true);

                var content = panelSettings.transform.Find("Content");
                if (content != null) content.gameObject.SetActive(false);
                var header = panelSettings.transform.Find("Header");
                if (header != null) header.gameObject.SetActive(false);
                var backBtn = panelSettings.transform.Find("BtnSettingsBack");
                if (backBtn != null) backBtn.gameObject.SetActive(false);

                // Ocultar el marco marrón para que luzca limpio el panel interactivo de ajustes
                var pImg = panelSettings.GetComponent<Image>();
                if (pImg != null) pImg.enabled = false;

                if (panelSettings.GetComponent<Kimaya.KimayaSettingsController>() == null)
                {
                    panelSettings.AddComponent<Kimaya.KimayaSettingsController>();
                }
            }
        }

        private void ShowHelp()
        {
            if (panelMain != null) panelMain.SetActive(false);
            if (panelCharacterSelect != null) panelCharacterSelect.SetActive(false);
            if (panelSettings != null) panelSettings.SetActive(false);
            if (panelHelp != null)
            {
                panelHelp.SetActive(true);
                Kimaya.KimayaUITheme.ApplyThemeToActiveScene();
            }
        }

        private void StartPlayGame()
        {
            Debug.Log("[MainMenu] Iniciando Nueva Partida limpia: Cinemática de Prólogo -> Hub de Kimaya");
            // Al darle JUGAR desde el Menú Principal, iniciar siempre una Nueva Partida limpia
            PlayerPrefs.SetInt("KiaraRescued", 0);
            PlayerPrefs.SetInt("FirstRuneActive", 0);
            PlayerPrefs.SetInt("HasSeenMVPEnding", 0);
            PlayerPrefs.SetInt("Kimaya_TalkedToMaya", 0);
            PlayerPrefs.SetInt("Kimaya_SeenPrologue", 0);
            PlayerPrefs.Save();

            if (Application.CanStreamedLevelBeLoaded("Prologue_Story"))
            {
                Kimaya.KimayaSceneTransition.LoadScene("Prologue_Story");
            }
            else if (Application.CanStreamedLevelBeLoaded("Hub"))
                Kimaya.KimayaSceneTransition.LoadScene("Hub");
            else if (Application.CanStreamedLevelBeLoaded("Village_Scene"))
                Kimaya.KimayaSceneTransition.LoadScene("Village_Scene");
            else if (Application.CanStreamedLevelBeLoaded("Level_01_Bosque"))
                Kimaya.KimayaSceneTransition.LoadScene("Level_01_Bosque");
            else if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                Kimaya.KimayaSceneTransition.LoadScene(GameData.VillageScene);
        }

        private void StartStoryMode()
        {
            Debug.Log("[MainMenu] Iniciando Modo Historia: Hub de Kimaya");
            if (Application.CanStreamedLevelBeLoaded("Hub"))
                Kimaya.KimayaSceneTransition.LoadScene("Hub");
            else if (Application.CanStreamedLevelBeLoaded("Prologue_Story"))
                Kimaya.KimayaSceneTransition.LoadScene("Prologue_Story");
            else if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                Kimaya.KimayaSceneTransition.LoadScene(GameData.VillageScene);
        }

        private void StartGame()
        {
            CharacterData selected = characterSelectUI.GetSelectedCharacter();
            if (selected == null)
            {
                Debug.LogWarning("[MainMenu] No hay personaje seleccionado.");
                return;
            }

            if (GameData.Instance != null)
                GameData.Instance.StartNewGame(selected);

            if (Application.CanStreamedLevelBeLoaded("Hub"))
                Kimaya.KimayaSceneTransition.LoadScene("Hub");
            else if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                Kimaya.KimayaSceneTransition.LoadScene(GameData.VillageScene);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureSingletons()
        {
            if (GameData.Instance == null)
            {
                var gd = new GameObject("GameData");
                gd.AddComponent<GameData>();
            }
            if (SceneLoader.Instance == null)
            {
                var sl = new GameObject("SceneLoader");
                sl.AddComponent<SceneLoader>();
            }
        }
    }
}
