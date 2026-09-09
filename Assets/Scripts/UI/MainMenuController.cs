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

        [Header("Botones Menú Principal")]
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnQuit;

        [Header("Botones Selección de Personaje")]
        [SerializeField] private Button btnStart;   // Confirmar personaje y jugar
        [SerializeField] private Button btnBack;    // Volver al menú principal

        [Header("Referencias")]
        [SerializeField] private CharacterSelectUI characterSelectUI;

        [Header("Animación de Fondo")]
        [SerializeField] private RectTransform backgroundTransform;
        [SerializeField] private float bgScrollSpeed = 20f;

        private void Start()
        {
            // Asegurar que GameData y SceneLoader existan
            EnsureSingletons();

            ShowMainPanel();

            btnPlay.onClick.AddListener(ShowCharacterSelect);
            btnQuit.onClick.AddListener(QuitGame);
            btnStart.onClick.AddListener(StartGame);
            btnBack.onClick.AddListener(ShowMainPanel);
        }

        private void Update()
        {
            // Desplazamiento suave del fondo
            if (backgroundTransform != null)
            {
                backgroundTransform.anchoredPosition += Vector2.left * bgScrollSpeed * Time.deltaTime;
                if (backgroundTransform.anchoredPosition.x < -1920f)
                    backgroundTransform.anchoredPosition = Vector2.zero;
            }
        }

        private void ShowMainPanel()
        {
            panelMain.SetActive(true);
            panelCharacterSelect.SetActive(false);
            Time.timeScale = 1f;
        }

        private void ShowCharacterSelect()
        {
            panelMain.SetActive(false);
            panelCharacterSelect.SetActive(true);
            characterSelectUI.Initialize();
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
