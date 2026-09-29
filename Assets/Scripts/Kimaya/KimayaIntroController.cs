using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Controla los 6 paneles de la intro de Kimaya.
    /// Avanza con Espacio / E / Click en Continuar.
    /// Skip salta directo al nivel.
    /// </summary>
    public class KimayaIntroController : MonoBehaviour
    {
        // ── Datos de los 6 paneles ─────────────────────────────────────────
        private static readonly string[] TITLES = new[]
        {
            "El Paseo Matutino",
            "La Pelota Perdida",
            "El Círculo de Hongos",
            "El Bosque del Eco",
            "¡Kiara!",
            "La Hada Maya"
        };

        private static readonly string[] TEXTS = new[]
        {
            "Era una mañana perfecta.\n\nFalin, Mia y Kiara salieron a pasear con su familia por los campos cerca del bosque.\n\nFalin corría adelante, como siempre.\nMia olfateaba cada flor del camino.\nKiara las vigilaba con ojo atento.",

            "De repente, uno de sus dueños lanzó la pelota demasiado lejos.\n\nLa pelota desapareció entre los árboles, rodando hacia lo profundo del bosque.\n\n¡Por aquí! ¡Puedo olerla! —dijo Mia.\n¡Yo llegaré primero! —gritó Falin y corrió.\n¡Esperen! ¡No se alejen tanto! —les advirtió Kiara.",

            "Las tres llegaron a un claro extraño.\n\nEn el centro había un tronco hueco rodeado por un círculo perfecto de hongos brillantes.\n\nLa pelota estaba justo al otro lado.\n\nFalin entró primero.\nMia la siguió.\nKiara fue tras ellas.",

            "Cuando las tres cruzaron el tronco, el círculo de hongos se iluminó con una luz dorada.\n\nEl suelo desapareció bajo sus patas.\n\nCayeron... y cayeron...\n\nY despertaron en un lugar que no existía en ningún mapa.",

            "El Bosque del Eco.\n\nUn mundo mágico que reacciona a las emociones.\n\nMia culpó a Falin por haber corrido sin pensar.\nFalin respondió que fue Mia quien señaló el camino.\n\nDe la tierra emergieron raíces oscuras.\n\nKiara empujó a sus hermanas para protegerlas...\n...y quedó atrapada.",

            "Una pequeña hada de luz apareció entre los árboles.\n\n\"Me llamo Maya. Soy la guardiana de este bosque.\"\n\n\"El Zarzal Sombrío se alimenta de las discusiones. Mientras peleen, crecerá más fuerte.\"\n\n\"Kiara está cerca, pero ninguna podrá alcanzarla sola.\"\n\n\"Deben trabajar juntas.\"\n\n¿Lista para rescatar a Kiara?"
        };

        private static readonly Color[] PANEL_COLORS = new[]
        {
            new Color(0.08f, 0.15f, 0.07f),  // verde bosque
            new Color(0.12f, 0.18f, 0.08f),  // verde claro
            new Color(0.06f, 0.12f, 0.12f),  // verde azulado
            new Color(0.04f, 0.08f, 0.15f),  // azul oscuro (caída)
            new Color(0.15f, 0.05f, 0.05f),  // rojo oscuro (peligro)
            new Color(0.08f, 0.06f, 0.18f),  // morado (Maya)
        };

        // ── Referencias UI (buscadas en Awake) ────────────────────────────
        private Text _titleText;
        private Text _bodyText;
        private Text _panelNumText;
        private Image _scenePanel;
        private Button _continueBtn;
        private Button _skipBtn;

        private int _currentPanel = 0;
        private bool _typing = false;
        private string _fullText = "";
        private Coroutine _typeRoutine;

        private void Awake()
        {
            _titleText  = FindUI<Text>("PanelTitle");
            _bodyText   = FindUI<Text>("PanelText");
            _panelNumText = FindUI<Text>("PanelNum");
            _scenePanel = FindUI<Image>("ScenePanel");

            var continueBtnGo = GameObject.Find("Btn_Continuar");
            if (continueBtnGo != null)
            {
                _continueBtn = continueBtnGo.GetComponent<Button>();
                _continueBtn?.onClick.AddListener(OnContinue);
            }

            var skipBtnGo = GameObject.Find("Btn_Saltar");
            if (skipBtnGo != null)
            {
                _skipBtn = skipBtnGo.GetComponent<Button>();
                _skipBtn?.onClick.AddListener(SkipIntro);
            }
        }

        private void Start() => ShowPanel(0);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)
                || Input.GetKeyDown(KeyCode.Return))
                OnContinue();
        }

        private void OnContinue()
        {
            if (_typing)
            {
                // Mostrar texto completo inmediatamente
                StopCoroutine(_typeRoutine);
                _typing = false;
                if (_bodyText != null) _bodyText.text = _fullText;
                return;
            }

            _currentPanel++;
            if (_currentPanel >= TITLES.Length)
                LoadLevel();
            else
                ShowPanel(_currentPanel);
        }

        private void ShowPanel(int index)
        {
            if (_titleText  != null) _titleText.text = TITLES[index];
            if (_panelNumText != null) _panelNumText.text = $"{index + 1} / {TITLES.Length}";
            if (_scenePanel != null) _scenePanel.color = PANEL_COLORS[index];

            _fullText = TEXTS[index];
            if (_bodyText != null) _bodyText.text = "";

            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText(_fullText));
        }

        private IEnumerator TypeText(string text)
        {
            _typing = true;
            string current = "";
            foreach (char c in text)
            {
                current += c;
                if (_bodyText != null) _bodyText.text = current;
                yield return new WaitForSeconds(0.025f);
            }
            _typing = false;
        }

        public void SkipIntro() => LoadLevel();

        private void LoadLevel() => SceneManager.LoadScene("Level_01_Bosque");

        private T FindUI<T>(string name) where T : Component
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<T>() : null;
        }
    }
}
