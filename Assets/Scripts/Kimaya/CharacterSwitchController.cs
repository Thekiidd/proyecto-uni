using UnityEngine;
using UnityEngine.Events;

namespace Kimaya
{
    /// <summary>
    /// Maneja el cambio entre Falin y Mia con TAB.
    /// Desactiva el control del personaje inactivo pero mantiene su física.
    /// </summary>
    public class CharacterSwitchController : MonoBehaviour
    {
        [Header("Personajes")]
        public PlayerController2D falin;
        public PlayerController2D mia;

        [Header("Controles")]
        public KeyCode switchKey = KeyCode.Tab;

        [Header("Visual")]
        public Color falinActiveColor   = new Color(0.9f, 0.7f, 0.3f, 1f);  // Dorado
        public Color miaActiveColor     = new Color(0.4f, 0.8f, 1f, 1f);    // Azul
        public Color inactiveColor      = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        [Header("Eventos")]
        public UnityEvent<bool> onSwitch; // true = Falin activa

        private bool _falinActive = true;
        private Camera _cam;
        private KimayaCameraController _camCtrl;

        public bool FalinActive => _falinActive;
        public PlayerController2D ActivePlayer => _falinActive ? falin : mia;

        private void Start()
        {
            _cam    = Camera.main;
            _camCtrl = _cam?.GetComponent<KimayaCameraController>();

            ActivateFalin();
        }

        private void Update()
        {
            if (Input.GetKeyDown(switchKey))
                SwitchCharacter();
        }

        public void SwitchCharacter()
        {
            _falinActive = !_falinActive;

            if (_falinActive) ActivateFalin();
            else              ActivateMia();

            onSwitch?.Invoke(_falinActive);
        }

        private void ActivateFalin()
        {
            _falinActive = true;
            falin.SetActive(true);
            mia.SetActive(false);

            SetSpriteColor(falin, falinActiveColor);
            SetSpriteColor(mia, inactiveColor);

            if (_camCtrl != null) _camCtrl.SetTarget(falin.transform);
        }

        private void ActivateMia()
        {
            _falinActive = false;
            falin.SetActive(false);
            mia.SetActive(true);

            SetSpriteColor(falin, inactiveColor);
            SetSpriteColor(mia, miaActiveColor);

            if (_camCtrl != null) _camCtrl.SetTarget(mia.transform);
        }

        private void SetSpriteColor(PlayerController2D player, Color color)
        {
            var sr = player.GetComponent<SpriteRenderer>();
            if (sr == null) sr = player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = color;
        }

        /// <summary>Llamado por la UI — muestra qué personaje está activo.</summary>
        public string ActiveName => _falinActive ? "Falin" : "Mia";
    }
}
