using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente de rescate final en la cumbre del Sendero de las Raíces (KIM-14).
    /// Muestra a Kiara atrapada por el Zarzal Sombrío.
    /// Al aproximarse el jugador, las sombras del Zarzal se disipan, Kiara queda libre
    /// y se activa el diálogo de rescate y la victoria del MVP.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaAltarGoal : MonoBehaviour
    {
        [Header("Configuración de Meta y Rescate")]
        [Tooltip("Mensaje de éxito que se muestra en el panel de victoria")]
        public string victoryMessage = "¡Has alcanzado la cumbre del Sendero de las Raíces y rescatado a Kiara!";

        [Tooltip("Congelar físicas del jugador al llegar a la cima para evitar caídas")]
        public bool freezePlayerOnVictory = true;

        [Header("Efectos Opcionales")]
        public ParticleSystem victoryAura;

        private bool _isGoalReached = false;
        private Collider2D _triggerCollider;
        private Transform _zarzalTr;

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider2D>();
            _triggerCollider.isTrigger = true;

            // Desactivar el sprite del altar en el objeto raíz para dejar solo a Kiara y el Zarzal
            var rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                rootSr.enabled = false;
            }
        }

        private void Start()
        {
            EnsureKiaraAndZarzalVisuals();
        }

        private void EnsureKiaraAndZarzalVisuals()
        {
            // 1. NPC Kiara en la cumbre
            Transform kiaraTr = transform.Find("Kiara_Summit");
            if (kiaraTr == null)
            {
                var kiaraGo = new GameObject("Kiara_Summit");
                kiaraGo.transform.SetParent(transform);
                kiaraGo.transform.localPosition = new Vector3(0f, 0.15f, 0f);
                kiaraGo.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
                var sr = kiaraGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 505;

                Sprite kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_south");
                if (kiaraSprite == null) kiaraSprite = Resources.Load<Sprite>("NPCs/Kiara/Kiara_east");
                if (kiaraSprite != null) sr.sprite = kiaraSprite;
            }

            // 2. Zarzal Sombrío enredando a Kiara
            _zarzalTr = transform.Find("Zarzal_Trap");
            if (_zarzalTr == null)
            {
                var zarzalGo = new GameObject("Zarzal_Trap");
                zarzalGo.transform.SetParent(transform);
                zarzalGo.transform.localPosition = new Vector3(0.50f, 0.25f, 0f);
                zarzalGo.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
                var sr = zarzalGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 515;
                sr.color = new Color(0.9f, 0.45f, 1f, 0.95f); // Tinte sombrío de energía oscura

                Sprite zarzalSprite = Resources.Load<Sprite>("NPCs/Zarzal/Zarzal_Sombrio");
                if (zarzalSprite != null) sr.sprite = zarzalSprite;
                _zarzalTr = zarzalGo.transform;
            }
        }

        private void Update()
        {
            if (_isGoalReached) return;

            // Animación suave de respiración / pulso amenazante del Zarzal Sombrío
            if (_zarzalTr != null && _zarzalTr.gameObject.activeSelf)
            {
                float pulse = 1.0f + 0.05f * Mathf.Sin(Time.time * 3.5f);
                _zarzalTr.localScale = new Vector3(1.25f * pulse, 1.25f * pulse, 1f);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isGoalReached) return;

            var player = other.GetComponent<KimayaJumpKingController>();
            if (player == null && !other.CompareTag("Player") && !other.name.Contains("Player"))
            {
                return;
            }

            _isGoalReached = true;
            if (_triggerCollider != null) _triggerCollider.enabled = false;

            if (victoryAura != null)
            {
                victoryAura.Play();
            }

            // Liberar a Kiara: El Zarzal Sombrío se desvanece
            if (_zarzalTr != null)
            {
                _zarzalTr.gameObject.SetActive(false);
            }

            // Sonido de victoria
            if (KimayaAudio.Instance != null)
            {
                KimayaAudio.Instance.PlaySFX(KimayaAudio.SFX.Victory);
            }

            // Disparar condición de éxito en el Manager
            if (KimayaJumpKingManager.Instance != null)
            {
                KimayaJumpKingManager.Instance.TriggerVictory(victoryMessage);
            }
            else
            {
                Debug.Log($"[KimayaAltarGoal] ¡Kiara rescatada! {victoryMessage}");
            }
        }

        public void ResetGoal()
        {
            _isGoalReached = false;
            if (_triggerCollider != null) _triggerCollider.enabled = true;
            if (_zarzalTr != null) _zarzalTr.gameObject.SetActive(true);
        }
    }
}
