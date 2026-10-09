using Platformer.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Controla la pantalla de inicio del juego.
    /// Asignar en el Inspector: los paneles, botones y el CharacterSelectUI.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelMain;           // Panel con botones principales
        [SerializeField] private GameObject panelCharacterSelect; // Panel de selección de personaje
        [SerializeField] private GameObject panelSettings;       // Panel de ajustes
        [SerializeField] private GameObject panelHelp;           // Panel de ayuda/controles

        [Header("Botones Menú Principal")]
        [SerializeField] private Button btnStory;    // Jugar Modo Historia (Prólogo)
        [SerializeField] private Button btnPlay;     // Selección de Perro Libre
        [SerializeField] private Button btnSettings; // Abrir Ajustes
        [SerializeField] private Button btnHelp;     // Abrir Ayuda
        [SerializeField] private Button btnQuit;     // Salir

        [Header("Botones Modales")]
        [SerializeField] private Button btnSettingsBack;
        [SerializeField] private Button btnHelpBack;

        [Header("Botones Selección de Personaje")]
        [SerializeField] private Button btnStart;   // Confirmar personaje y jugar
        [SerializeField] private Button btnBack;    // Volver al menú principal

        [Header("Referencias")]
        [SerializeField] private CharacterSelectUI characterSelectUI;

        [Header("Animación de Fondo")]
        [SerializeField] private RectTransform backgroundTransform;
        // [SerializeField] private float bgScrollSpeed = 0f;

        private void Start()
        {
            // Asegurar que GameData y SceneLoader existan
            EnsureSingletons();

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
            if (panelSettings != null) panelSettings.SetActive(true);
        }

        private void ShowHelp()
        {
            if (panelMain != null) panelMain.SetActive(false);
            if (panelCharacterSelect != null) panelCharacterSelect.SetActive(false);
            if (panelSettings != null) panelSettings.SetActive(false);
            if (panelHelp != null) panelHelp.SetActive(true);
        }

        private void StartPlayGame()
        {
            Debug.Log("[MainMenu] Iniciando Partida: Level_01_Bosque");
            if (Application.CanStreamedLevelBeLoaded("Level_01_Bosque"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Level_01_Bosque");
            else if (Application.CanStreamedLevelBeLoaded("Prologue_Story"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Prologue_Story");
            else if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.VillageScene);
        }

        private void StartStoryMode()
        {
            Debug.Log("[MainMenu] Iniciando Modo Historia: Prólogo");
            // Cargar escena de prólogo si existe, de lo contrario ir a VillageMap
            if (Application.CanStreamedLevelBeLoaded("Prologue_Story"))
                UnityEngine.SceneManagement.SceneManager.LoadScene("Prologue_Story");
            else if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.VillageScene);
        }

        private void StartGame()
        {
            CharacterData selected = characterSelectUI.GetSelectedCharacter();
            if (selected == null)
            {
                Debug.LogWarning("[MainMenu] No hay personaje seleccionado.");
                return;
            }

            // Inicializar GameData con el personaje y cargar el primer nivel
            if (GameData.Instance != null)
                GameData.Instance.StartNewGame(selected);

            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadVillage();
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(GameData.VillageScene);
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
