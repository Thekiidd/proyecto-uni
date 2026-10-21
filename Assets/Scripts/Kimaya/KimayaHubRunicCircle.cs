using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Gestiona el Círculo Rúnico central del Bosque del Eco y el estado de cierre del MVP (KIM-01, KIM-02, KIM-15, KIM-16, KIM-17).
    /// - 4 Runas: Runa 1 (Sendero de las Raíces), Runas 2, 3 y 4 (Regiones pendientes).
    /// - Al rescatar a Kiara, se activan las 3 hermanas reunidas en el Hub (Falin, Mia y Kiara).
    /// - La primera runa se ilumina de forma permanente y se muestra el mensaje de finalización del MVP.
    /// </summary>
    public class KimayaHubRunicCircle : MonoBehaviour
    {
        public static KimayaHubRunicCircle Instance { get; private set; }

        [Header("Referencias de Personajes")]
        public GameObject kiaraObject;
        public Sprite kiaraSprite;

        [Header("Estado de Runas")]
        public bool rune1Active = false;
        public bool rune2Active = false;
        public bool rune3Active = false;
        public bool rune4Active = false;

        [Header("Colores de Runas")]
        public Color runeActiveColor = new Color(0.2f, 1f, 0.5f, 0.95f); // Verde esmeralda resplandeciente
        public Color runeInactiveColor = new Color(0.35f, 0.35f, 0.45f, 0.35f); // Apagada / sombría

        private bool _showCompletionModal = false;
        private Camera _mainCam;

        private static GUIStyle _modalBoxStyle;
        private static GUIStyle _titleStyle;
        private static GUIStyle _bodyStyle;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _hudRuneStyle;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _mainCam = Camera.main;

            // Verificar si Kiara ya fue rescatada (KIM-15 & KIM-16)
            bool kiaraRescued = PlayerPrefs.GetInt("KiaraRescued", 0) == 1;
            rune1Active = kiaraRescued;

            if (kiaraRescued)
            {
                SetupReunitedSisters();

                // Mostrar el mensaje de cierre del MVP si es la primera vez que vuelve
                bool hasSeenEnding = PlayerPrefs.GetInt("HasSeenMVPEnding", 0) == 1;
                if (!hasSeenEnding)
                {
                    _showCompletionModal = true;
                    PlayerPrefs.SetInt("HasSeenMVPEnding", 1);
                    PlayerPrefs.Save();
                }
            }
        }

        private void SetupReunitedSisters()
        {
            if (kiaraObject == null)
            {
                kiaraObject = GameObject.Find("Kiara_GoldenRetriever");
            }

            if (kiaraObject == null)
            {
                // Crear a Kiara dinámicamente si no existe en la escena
                kiaraObject = new GameObject("Kiara_GoldenRetriever");
                kiaraObject.transform.position = new Vector3(0f, -0.65f, 0f);
                var sr = kiaraObject.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 510;

                // Cargar sprite de Kiara
                if (kiaraSprite == null)
                {
                    var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                    foreach (var s in allSprites)
                    {
                        if (s.name.Contains("Kiara") || s.name.Contains("kiara"))
                        {
                            kiaraSprite = s;
                            break;
                        }
                    }
                }
                if (kiaraSprite != null) sr.sprite = kiaraSprite;
            }
            else
            {
                kiaraObject.SetActive(true);
            }

            Debug.Log("[KimayaHubRunicCircle] 🌟 ¡Las 3 hermanas (Falin, Mia y Kiara) están reunidas en el Bosque del Eco! Runa 1 activada.");
        }

        private void Update()
        {
            if (_showCompletionModal && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)))
            {
                _showCompletionModal = false;
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // Dibujar el estado de las 4 runas en la esquina superior izquierda (KIM-01, KIM-02, KIM-16)
            DrawRunicCircleHUD();

            // Si está activo el modal de felicitación de cierre del MVP (KIM-17)
            if (_showCompletionModal)
            {
                DrawMVPCompletionModal();
            }
        }

        private void DrawRunicCircleHUD()
        {
            float hudX = 16f;
            float hudY = 16f;
            float hudW = 260f;
            float hudH = 78f;

            GUI.backgroundColor = new Color(0.08f, 0.12f, 0.18f, 0.88f);
            GUI.Box(new Rect(hudX, hudY, hudW, hudH), GUIContent.none);

            GUI.Label(new Rect(hudX + 10f, hudY + 8f, hudW - 20f, 20f), "<b>🌀 Círculo Mágico (4 Runas)</b>", _hudRuneStyle);

            string r1 = rune1Active ? "<color=#33FF77>● Runa 1 (Raíces): ACTIVA</color>" : "<color=#778899>○ Runa 1: Inactiva</color>";
            string r2 = "<color=#778899>○ Runa 2: Inactiva</color>";
            string r3 = "<color=#778899>○ Runa 3: Inactiva</color>";
            string r4 = "<color=#778899>○ Runa 4: Inactiva</color>";

            string line1 = $"{r1}   {r2}";
            string line2 = $"{r3}   {r4}";

            GUI.Label(new Rect(hudX + 10f, hudY + 30f, hudW - 20f, 20f), line1, _hudRuneStyle);
            GUI.Label(new Rect(hudX + 10f, hudY + 50f, hudW - 20f, 20f), line2, _hudRuneStyle);
        }

        private void DrawMVPCompletionModal()
        {
            float modalW = Mathf.Min(660f, Screen.width - 30f);
            float modalH = 340f;
            float modalX = (Screen.width - modalW) * 0.5f;
            float modalY = (Screen.height - modalH) * 0.5f;

            GUI.Box(new Rect(modalX, modalY, modalW, modalH), GUIContent.none, _modalBoxStyle);

            // Título KIM-17
            GUI.Label(new Rect(modalX, modalY + 20f, modalW, 32f), "🌟 ¡DEMOSTRACIÓN DEL MVP COMPLETADA! 🌟", _titleStyle);

            // Contenido obligatorio KIM-17
            string content = "<b>¡Kiara ha sido rescatada exitosamente!</b>\n\n" +
                             "• <b>Primera Runa Restaurada:</b> La energía del Sendero de las Raíces fluye de nuevo por el círculo central.\n" +
                             "• <b>Tres Regiones Pendientes:</b> Aún restan 3 regiones místicas del reino que continúan selladas por la oscuridad.\n" +
                             "• <b>Las Tres Hermanas Unidas:</b> Falin, Mia y Kiara están a salvo juntas en el Bosque del Eco.\n\n" +
                             "<i>¡La aventura de Fairytopia continuará posteriormente en las siguientes fases del proyecto!</i>";

            GUI.Label(new Rect(modalX + 25f, modalY + 65f, modalW - 50f, 190f), content, _bodyStyle);

            // Botón de continuar
            if (GUI.Button(new Rect(modalX + (modalW - 220f) * 0.5f, modalY + modalH - 52f, 220f, 38f), "✨ Explorar el Bosque [E / Space]", _buttonStyle))
            {
                _showCompletionModal = false;
            }
        }

        public void ReplayDemo()
        {
            PlayerPrefs.DeleteKey("KiaraRescued");
            PlayerPrefs.DeleteKey("FirstRuneActive");
            PlayerPrefs.DeleteKey("HasSeenMVPEnding");
            PlayerPrefs.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private static void InitStyles()
        {
            if (_modalBoxStyle != null) return;

            Texture2D boxTex = new Texture2D(1, 1);
            boxTex.SetPixel(0, 0, new Color(0.06f, 0.10f, 0.16f, 0.98f));
            boxTex.Apply();

            _modalBoxStyle = new GUIStyle();
            _modalBoxStyle.normal.background = boxTex;

            _titleStyle = new GUIStyle();
            _titleStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);
            _titleStyle.fontSize = 18;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.alignment = TextAnchor.MiddleCenter;

            _bodyStyle = new GUIStyle();
            _bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 1f);
            _bodyStyle.fontSize = 13;
            _bodyStyle.wordWrap = true;

            Texture2D btnTex = new Texture2D(1, 1);
            btnTex.SetPixel(0, 0, new Color(0.18f, 0.65f, 0.35f, 1f));
            btnTex.Apply();

            _buttonStyle = new GUIStyle();
            _buttonStyle.normal.background = btnTex;
            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 14;
            _buttonStyle.fontStyle = FontStyle.Bold;
            _buttonStyle.alignment = TextAnchor.MiddleCenter;

            _hudRuneStyle = new GUIStyle();
            _hudRuneStyle.normal.textColor = Color.white;
            _hudRuneStyle.fontSize = 11;
        }
    }
}
