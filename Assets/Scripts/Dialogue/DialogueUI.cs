using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

namespace Platformer.Dialogue
{
    public class DialogueUI : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Elements")]
        public GameObject dialoguePanel;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueContentText;
        public Image portraitImage;
        public GameObject continueIndicator;
        public Button nextButton;
        public Button skipButton;
        public Button screenClickButton;

        [Header("Story Dialogue Data")]
        public StoryDialogueData currentDialogue;

        private int _currentIndex = 0;
        private bool _isTyping = false;
        private Coroutine _typingCoroutine;
        private string _fullTargetText = "";

        private void Start()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextClicked);
            if (skipButton != null)
                skipButton.onClick.AddListener(OnSkipClicked);
            if (screenClickButton != null)
                screenClickButton.onClick.AddListener(OnNextClicked);

            if (currentDialogue != null && currentDialogue.lines != null && currentDialogue.lines.Length > 0)
            {
                StartDialogue(currentDialogue);
            }
        }

        private void Update()
        {
            if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

            bool advancePressed = false;

            // 1. Keyboard / Gamepad New Input System
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.eKey.wasPressedThisFrame)
                    advancePressed = true;
            }

            if (UnityEngine.InputSystem.Gamepad.current != null)
            {
                if (UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame)
                    advancePressed = true;
            }

            // 2. Mouse click anywhere via New Input System
            if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Verify we're not clicking on the skip button (EventSystem handles button events)
                if (EventSystem.current == null || !IsPointerOverSkipButton())
                {
                    advancePressed = true;
                }
            }

            // 3. Legacy Input fallback
            try
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current == null || !IsPointerOverSkipButton())
                        advancePressed = true;
                }
            }
            catch {}

            if (advancePressed)
            {
                OnNextClicked();
            }
        }

        private bool IsPointerOverSkipButton()
        {
            if (skipButton == null || EventSystem.current == null) return false;
            var curSelected = EventSystem.current.currentSelectedGameObject;
            return curSelected == skipButton.gameObject;
        }

        // Mouse click on the dialogue panel or attached graphic
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnNextClicked();
            }
        }

        public void StartDialogue(StoryDialogueData data)
        {
            currentDialogue = data;
            _currentIndex = 0;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
            ShowLine(_currentIndex);
        }

        private void ShowLine(int index)
        {
            if (currentDialogue == null || index >= currentDialogue.lines.Length)
            {
                EndDialogue();
                return;
            }

            var line = currentDialogue.lines[index];

            if (speakerNameText != null)
                speakerNameText.text = line.speakerName;

            if (portraitImage != null)
            {
                if (line.speakerPortrait != null)
                {
                    portraitImage.gameObject.SetActive(true);
                    portraitImage.sprite = line.speakerPortrait;
                }
                else
                {
                    portraitImage.gameObject.SetActive(false);
                }
            }

            if (continueIndicator != null)
                continueIndicator.SetActive(false);

            _fullTargetText = line.text;
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeText(line.text, line.typingSpeed));
        }

        private IEnumerator TypeText(string target, float speed)
        {
            _isTyping = true;
            dialogueContentText.text = "";

            foreach (char c in target)
            {
                dialogueContentText.text += c;
                yield return new WaitForSeconds(speed > 0 ? speed : 0.025f);
            }

            _isTyping = false;
            if (continueIndicator != null)
                continueIndicator.SetActive(true);
        }

        public void OnNextClicked()
        {
            if (_isTyping)
            {
                // Finish typing immediately
                if (_typingCoroutine != null)
                    StopCoroutine(_typingCoroutine);
                dialogueContentText.text = _fullTargetText;
                _isTyping = false;
                if (continueIndicator != null)
                    continueIndicator.SetActive(true);
            }
            else
            {
                // Advance to next line
                _currentIndex++;
                ShowLine(_currentIndex);
            }
        }

        public void OnSkipClicked()
        {
            EndDialogue();
        }

        public void EndDialogue()
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            string targetScene = "Village_Scene";
            if (currentDialogue != null && !string.IsNullOrEmpty(currentDialogue.nextSceneOnComplete))
            {
                targetScene = currentDialogue.nextSceneOnComplete.Trim();
                if (targetScene == "VillageMap" || string.IsNullOrEmpty(targetScene))
                    targetScene = "Village_Scene";
            }

            Debug.Log("[DialogueUI] Diálogo finalizado. Cargando escena: " + targetScene);

            if (Platformer.Core.SceneLoader.Instance != null)
            {
                if (targetScene == "Village_Scene" || targetScene == Platformer.Core.GameData.VillageScene)
                    Platformer.Core.SceneLoader.Instance.LoadVillage();
                else
                    SceneManager.LoadScene(targetScene);
            }
            else
            {
                SceneManager.LoadScene(targetScene);
            }
        }
    }
}
