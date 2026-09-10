using UnityEngine;
using Platformer.Core;
using Platformer.Dialogue;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Objeto coleccionable/pista narrativa en el nivel (ej. la bandana roja del amigo perdido).
    /// Al tocarlo, se reproduce un sonido, se guarda el progreso seguro
    /// y se muestra una viñeta de diálogo.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StoryClueTrigger : MonoBehaviour
    {
        [Header("Datos de la Pista")]
        public string clueName = "Bandana Roja de Toby";
        [TextArea(2, 4)]
        public string dialogueText = "¡Miren! ¡Es la bandana roja de Toby prendida en este arbusto! ¡El rastro sigue hacia las ruinas antiguas!";

        [Header("Efectos Visuales")]
        public float bobbingSpeed = 2.5f;
        public float bobbingHeight = 0.15f;

        private Vector3 startPos;
        private bool triggered = false;

        private void Start()
        {
            startPos = transform.position;
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void Update()
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            transform.position = new Vector3(startPos.x, newY, startPos.z);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (triggered) return;
            if (collision.CompareTag("Player") || collision.GetComponent<PlayerController>() != null)
            {
                triggered = true;
                Debug.Log($"[StoryClueTrigger] ¡Pista encontrada: {clueName}!");

                if (GameData.Instance != null)
                {
                    GameData.Instance.AddScore(500);
                    GameData.Instance.SaveSecureProgress();
                }

                var dialogueUI = FindAnyObjectByType<DialogueUI>();
                if (dialogueUI != null)
                {
                    var clueDialogue = ScriptableObject.CreateInstance<StoryDialogueData>();
                    clueDialogue.dialogueTitle = "Pista Encontrada";
                    clueDialogue.nextSceneOnComplete = "Village_Scene";
                    clueDialogue.lines = new StoryLine[]
                    {
                        new StoryLine
                        {
                            speakerName = "Compañero",
                            text = dialogueText,
                            typingSpeed = 0.025f
                        },
                        new StoryLine
                        {
                            speakerName = "Equipo",
                            text = "¡Vamos rápido! Toby no puede estar lejos. El poder de la amistad nos guiará.",
                            typingSpeed = 0.025f
                        }
                    };
                    dialogueUI.StartDialogue(clueDialogue);
                }
            }
        }
    }
}
