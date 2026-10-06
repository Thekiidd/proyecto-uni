using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Kimaya
{
    /// <summary>
    /// Mueve a Falin y Mia en el Hub isométrico con WASD / flechas.
    /// TAB cambia el personaje activo.
    /// </summary>
    public class HubCharacterController : MonoBehaviour
    {
        [Header("Personajes")]
        public GameObject falinGo;
        public GameObject miaGo;

        [Header("Movimiento")]
        public float moveSpeed = 4f;

        [Header("Controles")]
        public KeyCode switchKey = KeyCode.Tab;

        private bool    _falinActive = true;
        private GameObject _active;
        private Text    _charLabel;
        private float   _t = 0f;

        private void Start()
        {
            _active     = falinGo;
            _charLabel  = GameObject.Find("ActiveChar")?.GetComponent<Text>();
            UpdateVisuals();
        }

        private void Update()
        {
            _t += Time.deltaTime;

            // Cambio de personaje
            if (Input.GetKeyDown(switchKey))
            {
                _falinActive = !_falinActive;
                _active = _falinActive ? falinGo : miaGo;
                UpdateVisuals();
            }

            if (_active == null) return;

            // Dirección isométrica (cámara a 30° en X)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // Transformar a espacio isométrico
            Vector3 dir = new Vector3(h, 0f, v).normalized;
            var rb = _active.GetComponent<Rigidbody>();
            if (rb != null)
                rb.MovePosition(_active.transform.position + dir * moveSpeed * Time.deltaTime);

            // Flip sprite según dirección
            var sr = _active.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && Mathf.Abs(h) > 0.1f)
                sr.flipX = h < 0;

            // Animar luciérnagas del hub (si hay)
            AnimateFairies();
        }

        private void UpdateVisuals()
        {
            // Color del personaje activo
            if (falinGo != null)
            {
                var sr = falinGo.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = _falinActive
                    ? new Color(0.9f, 0.75f, 0.3f)
                    : new Color(0.9f, 0.75f, 0.3f, 0.5f);
            }
            if (miaGo != null)
            {
                var sr = miaGo.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.color = !_falinActive
                    ? new Color(0.4f, 0.8f, 1f)
                    : new Color(0.4f, 0.8f, 1f, 0.5f);
            }
            if (_charLabel != null)
                _charLabel.text = $"▶ {(_falinActive ? "Falin" : "Mia")}";
        }

        private void AnimateFairies()
        {
            // Pulso sutil en los portales
            var portals = FindObjectsByType<HubPortal>(FindObjectsInactive.Exclude);
            foreach (var p in portals)
                p.AnimatePulse(_t);
        }
    }

    /// <summary>
    /// Portal en el Hub — al acercarse muestra "E: Entrar".
    /// Al presionar E carga la escena de la misión.
    /// </summary>
    public class HubPortal : MonoBehaviour
    {
        [Header("Config")]
        public string targetScene = "";
        public bool   isLocked    = false;
        public Color  portalColor = Color.cyan;
        public string labelText   = "";

        private bool   _playerNear  = false;
        private GameObject _msgPanel;
        private Text   _msgText;
        private float  _pulseT = 0f;

        private void Start()
        {
            _msgPanel = GameObject.Find("PortalMsg");
            _msgText  = _msgPanel?.GetComponentInChildren<Text>();
        }

        public void AnimatePulse(float t)
        {
            _pulseT = t;
            // Pulsa el anillo principal
            var rings = GetComponentsInChildren<MeshRenderer>();
            foreach (var r in rings)
            {
                if (isLocked) continue;
                float pulse = 0.8f + 0.2f * Mathf.Sin(t * 2.5f);
                r.material.color = new Color(
                    portalColor.r * pulse,
                    portalColor.g * pulse,
                    portalColor.b * pulse);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNear = true;
            if (_msgPanel != null) _msgPanel.SetActive(true);
            if (_msgText  != null)
                _msgText.text = isLocked
                    ? "🔒 Esta misión estará disponible pronto"
                    : $"[E] {labelText.Split('\n')[0]}";
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _playerNear = false;
            if (_msgPanel != null) _msgPanel.SetActive(false);
        }

        private void Update()
        {
            if (!_playerNear || isLocked) return;
            if (!Input.GetKeyDown(KeyCode.E)) return;
            if (string.IsNullOrEmpty(targetScene)) return;
            SceneManager.LoadScene(targetScene);
        }
    }

    /// <summary>
    /// Maya en el Hub — dice consejos al acercarse.
    /// </summary>
    public class HubMayaDialogue : MonoBehaviour
    {
        private static readonly string[] HINTS = new[]
        {
            "Bienvenidas al Bosque del Eco. Kiara te necesita.",
            "Usa TAB para cambiar entre Falin y Mia.",
            "Falin puede empujar cajas. Mia puede pasar por espacios pequeños.",
            "Presiona Q con Mia para usar su olfato y revelar caminos ocultos.",
            "El Portal azul lleva a la primera misión. ¡Rescata a Kiara!",
            "Mantén SHIFT con Falin y presiona saltar para un salto impulsado."
        };

        private bool   _shown   = false;
        private int    _hintIdx = 0;
        private bool   _near    = false;
        private Text   _msgText;
        private GameObject _msgPanel;

        private void Start()
        {
            _msgPanel = GameObject.Find("PortalMsg");
            _msgText  = _msgPanel?.GetComponentInChildren<Text>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _near = true;
            ShowHint();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            _near = false;
            if (_msgPanel != null) _msgPanel.SetActive(false);
        }

        private void Update()
        {
            if (!_near) return;
            if (Input.GetKeyDown(KeyCode.E))
            {
                _hintIdx = (_hintIdx + 1) % HINTS.Length;
                ShowHint();
            }
        }

        private void ShowHint()
        {
            if (_msgPanel != null) _msgPanel.SetActive(true);
            if (_msgText  != null) _msgText.text = $"🧚 Maya: {HINTS[_hintIdx]}\n[E] Siguiente consejo";
        }
    }

    /// <summary>
    /// Manager general del Hub.
    /// </summary>
    public class HubManager : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 1f;

            // Asegurar que el cuadro de mensaje esté oculto al inicio
            var msg = GameObject.Find("PortalMsg");
            if (msg != null) msg.SetActive(false);

            // Orientar sprites a la cámara (billboard)
            StartCoroutine(BillboardLoop());
        }

        private IEnumerator BillboardLoop()
        {
            var cam = Camera.main;
            while (true)
            {
                if (cam != null)
                {
                    var sprites = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude);
                    foreach (var sr in sprites)
                        if (sr.GetComponentInParent<HubCharacterController>() != null
                            || sr.name == "Sprite")
                            sr.transform.LookAt(
                                sr.transform.position + cam.transform.rotation * Vector3.forward,
                                cam.transform.rotation * Vector3.up);
                }
                yield return new WaitForSeconds(0.05f);
            }
        }
    }
}
