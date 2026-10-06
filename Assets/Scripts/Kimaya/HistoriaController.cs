using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Pantalla PLACEHOLDER para la historia dibujada a mano.
    /// Mientras los dibujos no estén listos, muestra:
    ///   - Fondo de color por cada escena
    ///   - Texto narrativo con typewriter
    ///   - "Los dibujos de esta escena estarán pronto..."
    ///   - ESPACIO / E para avanzar
    ///   - SKIP para ir directo al Hub
    /// Cuando lleguen los dibujos, solo reemplaza los sprites en StoryPanel.
    /// </summary>
    public class HistoriaController : MonoBehaviour
    {
        private struct Panel
        {
            public string title;
            public string text;
            public Color  bgColor;
            public string imageNote; // texto que dice qué dibujo va aquí
        }

        private static readonly Panel[] PANELS = new Panel[]
        {
            new Panel {
                title     = "Una mañana de paseo",
                text      = "Falin, Mia y Kiara viven con la misma familia.\nSon inseparables desde cachorras.\n\nEsta mañana salieron a pasear por los campos\ncerca del gran bosque del norte.",
                bgColor   = new Color(0.15f, 0.28f, 0.10f),
                imageNote = "🎨 [Dibujo: las 3 perritas corriendo por un campo soleado]"
            },
            new Panel {
                title     = "La pelota perdida",
                text      = "¡Por aquí! ¡Puedo olerla! — dijo Mia.\n¡Yo llegaré primero! — gritó Falin.\n¡Esperen! ¡No se alejen tanto! — advirtió Kiara.\n\nLa pelota había caído dentro del bosque.",
                bgColor   = new Color(0.12f, 0.22f, 0.09f),
                imageNote = "🎨 [Dibujo: Falin corriendo, Mia olfateando, Kiara detrás]"
            },
            new Panel {
                title     = "El círculo de hongos",
                text      = "En el centro del bosque encontraron un claro.\n\nUn tronco hueco rodeado por un círculo\nperfecto de hongos brillantes.\n\nLa pelota estaba al otro lado.",
                bgColor   = new Color(0.06f, 0.14f, 0.14f),
                imageNote = "🎨 [Dibujo: el círculo de hongos brillando en la oscuridad]"
            },
            new Panel {
                title     = "La caída",
                text      = "Falin entró primero. Mia la siguió.\nKiara fue tras ellas.\n\nCuando las tres cruzaron el tronco...\nel círculo se iluminó y el suelo desapareció.",
                bgColor   = new Color(0.04f, 0.06f, 0.18f),
                imageNote = "🎨 [Dibujo: las 3 perritas cayendo en espiral de luz]"
            },
            new Panel {
                title     = "¡Kiara!",
                text      = "Despertaron en el Bosque del Eco.\nUn mundo mágico que reacciona a las emociones.\n\nLas raíces del Zarzal Sombrío emergieron...\ny Kiara quedó atrapada para proteger a sus hermanas.",
                bgColor   = new Color(0.18f, 0.04f, 0.04f),
                imageNote = "🎨 [Dibujo: Kiara capturada por raíces oscuras]"
            },
            new Panel {
                title     = "Maya aparece",
                text      = "\"Este bosque hace crecer las grietas entre quienes llegan.\"\n\"Kiara todavía está cerca, pero ninguna\npuede alcanzarla sola.\"\n\"Deben trabajar juntas.\"",
                bgColor   = new Color(0.10f, 0.06f, 0.22f),
                imageNote = "🎨 [Dibujo: Maya el hada flotando frente a Falin y Mia]"
            },
        };

        private int    _panelIdx = 0;
        private bool   _typing   = false;
        private string _fullText = "";
        private Coroutine _typeRoutine;

        // UI refs
        private Text  _titleTxt;
        private Text  _bodyTxt;
        private Text  _imgNoteTxt;
        private Text  _counterTxt;
        private Image _bgPanel;
        private Image _mainBG;

        private void Start()
        {
            _titleTxt   = FindUI<Text>("Story_Title");
            _bodyTxt    = FindUI<Text>("Story_Body");
            _imgNoteTxt = FindUI<Text>("Story_ImageNote");
            _counterTxt = FindUI<Text>("Story_Counter");
            _bgPanel    = FindUI<Image>("Story_BgPanel");
            _mainBG     = FindUI<Image>("Story_MainBG");

            var skipBtn = FindUI<Button>("Btn_Skip");
            skipBtn?.onClick.AddListener(GoToHub);

            ShowPanel(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E)
                || Input.GetKeyDown(KeyCode.Return))
                Advance();
        }

        private void Advance()
        {
            if (_typing)
            {
                if (_typeRoutine != null) StopCoroutine(_typeRoutine);
                _typing = false;
                if (_bodyTxt != null) _bodyTxt.text = _fullText;
                return;
            }
            _panelIdx++;
            if (_panelIdx >= PANELS.Length) GoToHub();
            else ShowPanel(_panelIdx);
        }

        private void ShowPanel(int idx)
        {
            var p = PANELS[idx];
            if (_titleTxt   != null) _titleTxt.text   = p.title;
            if (_imgNoteTxt != null) _imgNoteTxt.text  = p.imageNote;
            if (_counterTxt != null) _counterTxt.text  = $"{idx + 1}  /  {PANELS.Length}";
            if (_bgPanel    != null) _bgPanel.color    = p.bgColor;
            if (_mainBG     != null) _mainBG.color     = new Color(p.bgColor.r * 0.5f, p.bgColor.g * 0.5f, p.bgColor.b * 0.5f);

            _fullText = p.text;
            if (_bodyTxt != null) _bodyTxt.text = "";
            if (_typeRoutine != null) StopCoroutine(_typeRoutine);
            _typeRoutine = StartCoroutine(TypeText());
        }

        private IEnumerator TypeText()
        {
            _typing = true;
            string cur = "";
            foreach (char c in _fullText)
            {
                cur += c;
                if (_bodyTxt != null) _bodyTxt.text = cur;
                yield return new WaitForSeconds(0.022f);
            }
            _typing = false;
        }

        private void GoToHub()
        {
            string next = SceneExists("Hub") ? "Hub" : "Level_01_Bosque";
            SceneManager.LoadScene(next);
        }

        private bool SceneExists(string n)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                if (SceneUtility.GetScenePathByBuildIndex(i).Contains(n)) return true;
            return false;
        }

        private T FindUI<T>(string n) where T : Component
            => GameObject.Find(n)?.GetComponent<T>();
    }
}
