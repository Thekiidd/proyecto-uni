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
            Debug.Log("[MainMenu] Iniciando Partida: Hub de Kimaya");
            if (Application.CanStreamedLevelBeLoaded("Prologue_Story") && PlayerPrefs.GetInt("Kimaya_SeenPrologue", 0) == 0)
            {
                PlayerPrefs.SetInt("Kimaya_SeenPrologue", 1);
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
