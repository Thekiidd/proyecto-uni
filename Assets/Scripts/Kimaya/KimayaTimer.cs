using UnityEngine;
using UnityEngine.UI;

namespace Kimaya
{
    /// <summary>
    /// Cronómetro de Speedrun estilo Fireboy & Watergirl.
    /// Cuenta el tiempo transcurrido con precisión de centésimas.
    /// </summary>
    public class KimayaTimer : MonoBehaviour
    {
        public static KimayaTimer Instance { get; private set; }

        public Text timerText;
        public bool isRunning = true;

        private float _elapsedTime = 0f;

        public float ElapsedSeconds => _elapsedTime;
        public string FormattedTime => FormatTime(_elapsedTime);

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (timerText == null)
            {
                var txtGo = GameObject.Find("TimerText");
                if (txtGo != null) timerText = txtGo.GetComponent<Text>();
            }
        }

        private void Update()
        {
            if (!isRunning) return;

            _elapsedTime += Time.deltaTime;

            if (timerText != null)
                timerText.text = FormattedTime;
        }

        public void StopTimer()
        {
            isRunning = false;
        }

        public void ResetTimer()
        {
            _elapsedTime = 0f;
            isRunning = true;
        }

        public static string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            int hundredths = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);

            return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, hundredths);
        }
    }
}
