using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Controlador de la escena Ending.
    /// Conecta los botones de Menú / Rejugar.
    /// Anima las luciérnagas con pulso.
    /// </summary>
    public class KimayaEndingController : MonoBehaviour
    {
        private Button _menuBtn;
        private Button _replayBtn;
        private float _pulseTime = 0f;

        private void Start()
        {
            var menuGo = GameObject.Find("Btn_Menu");
            if (menuGo != null)
            {
                _menuBtn = menuGo.GetComponent<Button>();
                _menuBtn?.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
            }

            var replayGo = GameObject.Find("Btn_Rejugar");
            if (replayGo != null)
            {
                _replayBtn = replayGo.GetComponent<Button>();
                _replayBtn?.onClick.AddListener(() => SceneManager.LoadScene("Level_01_Bosque"));
            }

            // Asegurarse de que el tiempo fluya
            Time.timeScale = 1f;
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

            // Espacio / Enter para volver al menú
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                SceneManager.LoadScene("MainMenu");
        }
    }
}
