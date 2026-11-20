using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace Kimaya
{
    /// <summary>
    /// Controlador de la escena Ending.
    /// Conecta los botones de Menú / Rejugar (compatibles con New Input System y Legacy),
    /// aplica la transición Pixel-Art y anima las luciérnagas con pulso.
    /// </summary>
    public class KimayaEndingController : MonoBehaviour
    {
        private Button _menuBtn;
        private Button _replayBtn;
        private float _pulseTime = 0f;

        private void Start()
        {
            Time.timeScale = 1f;
            EnsureModernEventSystem();
            KimayaUITheme.ApplyThemeToActiveScene();

            var menuGo = GameObject.Find("Btn_Menu");
            if (menuGo != null)
            {
                _menuBtn = menuGo.GetComponent<Button>();
                if (_menuBtn != null)
                {
                    _menuBtn.onClick.RemoveAllListeners();
                    _menuBtn.onClick.AddListener(GoToMainMenu);
                }
            }

            var replayGo = GameObject.Find("Btn_Rejugar");
            if (replayGo != null)
            {
                _replayBtn = replayGo.GetComponent<Button>();
                if (_replayBtn != null)
                {
                    _replayBtn.onClick.RemoveAllListeners();
                    _replayBtn.onClick.AddListener(PlayAgain);
                }
            }
        }

        private static void EnsureModernEventSystem()
        {
            var es = Object.FindAnyObjectByType<EventSystem>();
            if (es == null)
            {
                var esGo = new GameObject("EventSystem");
                es = esGo.AddComponent<EventSystem>();
            }
#if ENABLE_INPUT_SYSTEM
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy != null) Destroy(legacy);
            if (es.GetComponent<InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#endif
        }

        public void GoToMainMenu()
        {
            KimayaSceneTransition.LoadScene("MainMenu");
        }

        public void PlayAgain()
        {
            string target = Application.CanStreamedLevelBeLoaded("Ascenso_Kimaya") ? "Ascenso_Kimaya" : "Hub";
            KimayaSceneTransition.LoadScene(target);
        }

        private void Update()
        {
            _pulseTime += Time.deltaTime;

            // Animar las luciérnagas del círculo (pulso de alpha)
            for (int i = 0; i < 12; i++)
            {
                var fairy = GameObject.Find($"Hada_{i}");
                if (fairy == null) continue;
                var img = fairy.GetComponent<Image>();
                if (img == null) continue;
                float offset = i * (Mathf.PI * 2f / 12f);
                float alpha = 0.4f + 0.5f * Mathf.Sin(_pulseTime * 1.5f + offset);
                img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
            }

            bool menuKey = Kimaya.Networking.PhonePadServer.Pressed("A") || Kimaya.Networking.PhonePadServer.Pressed("START");
            if (Kimaya.Networking.PhonePadServer.Pressed("B"))
            {
                PlayAgain();
                return;
            }
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.escapeKey.wasPressedThisFrame))
            {
                menuKey = true;
            }
#endif
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
                    menuKey = true;
            }
            catch { }

            if (menuKey)
                GoToMainMenu();
        }
    }
}
