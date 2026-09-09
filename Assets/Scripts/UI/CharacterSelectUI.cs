using Platformer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// Carrusel de selección de personajes en el menú principal.
    /// Se auto-inicializa al activarse el panel.
    /// </summary>
    public class CharacterSelectUI : MonoBehaviour
    {
        [Header("Personajes disponibles")]
        public CharacterData[] characters;

        [Header("UI Elements")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI specialText;

        [Header("Navegación")]
        [SerializeField] private Button btnLeft;
        [SerializeField] private Button btnRight;

        private int _currentIndex = 0;
        private bool _initialized = false;

        // ── Se llama cada vez que el panel se activa (SetActive true) ──
        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            // Asignar listeners siempre (por si se reactive el panel)
            if (btnLeft  != null) { btnLeft.onClick.RemoveAllListeners();  btnLeft.onClick.AddListener(PreviousCharacter); }
            if (btnRight != null) { btnRight.onClick.RemoveAllListeners(); btnRight.onClick.AddListener(NextCharacter); }

#if UNITY_EDITOR
            if (characters == null || characters.Length < 6)
            {
                var guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Characters" });
                if (guids.Length > 0)
                {
                    var list = new System.Collections.Generic.List<CharacterData>();
                    foreach (var g in guids)
                    {
                        var path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                        var cd = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                        if (cd != null && !list.Contains(cd)) list.Add(cd);
                    }
                    if (list.Count > 0) characters = list.ToArray();
                }
            }
#endif

            if (characters == null || characters.Length == 0)
            {
                Debug.LogError("[CharacterSelectUI] ¡FALTA asignar personajes en el Inspector! " +
                               "Ve a Tools > Animate Dog Characters.");
                return;
            }

            _currentIndex = 0;
            _initialized  = true;
            UpdateDisplay();
            Debug.Log($"[CharacterSelectUI] Iniciado con {characters.Length} personajes.");
        }

        private void PreviousCharacter()
        {
            if (!_initialized || characters == null || characters.Length == 0) return;
            _currentIndex = (_currentIndex - 1 + characters.Length) % characters.Length;
            UpdateDisplay();
        }

        private void NextCharacter()
        {
            if (!_initialized || characters == null || characters.Length == 0) return;
            _currentIndex = (_currentIndex + 1) % characters.Length;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (characters == null || characters.Length == 0) return;
            CharacterData c = characters[_currentIndex];
            if (c == null)
            {
                Debug.LogError($"[CharacterSelectUI] El personaje en índice {_currentIndex} es null. " +
                               "Revisa el array Characters en el Inspector.");
                return;
            }

            if (portraitImage != null)
            {
                portraitImage.color = Color.white;
                if (c.portrait != null)
                    portraitImage.sprite = c.portrait;
                else if (c.idleSprite != null)
                    portraitImage.sprite = c.idleSprite;
            }

            if (statsText == null)
            {
                statsText = transform.Find("Stats")?.GetComponent<TextMeshProUGUI>();
            }

            if (nameText != null)        nameText.text = c.characterName;
            if (statsText != null)
            {
                if (descriptionText != null) descriptionText.text = c.description;
                statsText.text = $"Velocidad: {c.moveSpeed:F0}  |  Salto: {c.jumpStrength:F0}  |  Vidas: {c.startingLives}";
            }
            else if (descriptionText != null)
            {
                descriptionText.text = $"{c.description}\n<size=20>Vel: {c.moveSpeed:F0}  |  Salto: {c.jumpStrength:F0}  |  Vidas: {c.startingLives}</size>";
            }
            if (specialText != null)
            {
                string special = "Normal";
                if (c.hasDoubleJump)           special = "Doble Salto";
                else if (c.hasDash)            special = "Impulso Dash";
                else if (c.startingLives > 3)  special = $"{c.startingLives} Vidas";
                specialText.text = $"Especial: {special}";
            }
        }

        public CharacterData GetSelectedCharacter()
        {
            if (!_initialized || characters == null || characters.Length == 0) return null;
            return characters[_currentIndex];
        }
    }
}
