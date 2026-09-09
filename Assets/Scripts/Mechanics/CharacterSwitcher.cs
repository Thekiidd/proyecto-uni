using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// Permite alternar el control y la cámara entre múltiples perros (jugadores) en la escena.
    /// Presiona la tecla TAB o Q para cambiar de perro.
    /// </summary>
    public class CharacterSwitcher : MonoBehaviour
    {
        public static CharacterSwitcher Instance { get; private set; }

        [Header("Lista de Perros / Jugadores en Escena")]
        [Tooltip("Arrastra aquí los GameObjects que tienen PlayerController en el nivel")]
        public List<PlayerController> characters = new List<PlayerController>();

        [Header("Índice de personaje activo")]
        [SerializeField] private int activeIndex = 0;

        [Header("Efectos Visuales de Selección")]
        [Tooltip("Prefab o Sprite opcional que flota sobre la cabeza del perro activo")]
        public GameObject selectionIndicator;
        public Vector3 indicatorOffset = new Vector3(0, 1.2f, 0);

        private readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // Si la lista está vacía, buscamos todos los PlayerControllers en la escena
            if (characters.Count == 0)
            {
                characters.AddRange(FindObjectsByType<PlayerController>(FindObjectsSortMode.None));
            }

            if (characters.Count > 0)
            {
                SetActiveCharacter(activeIndex);
            }
        }

        private void Update()
        {
            // Cambiar con la tecla Tab o Q si hay más de 1 perro
            if (characters.Count > 1)
            {
                if (Keyboard.current != null && (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame))
                {
                    SwitchToNext();
                }
            }

            // Actualizar posición del indicador si existe
            if (selectionIndicator != null && GetCurrentPlayer() != null)
            {
                selectionIndicator.transform.position = GetCurrentPlayer().transform.position + indicatorOffset;
            }
        }

        public void SwitchToNext()
        {
            if (characters.Count <= 1) return;

            int next = (activeIndex + 1) % characters.Count;
            SetActiveCharacter(next);
        }

        public void SetActiveCharacter(int index)
        {
            if (characters.Count == 0) return;

            activeIndex = Mathf.Clamp(index, 0, characters.Count - 1);

            for (int i = 0; i < characters.Count; i++)
            {
                var pc = characters[i];
                if (pc == null) continue;

                bool isActive = (i == activeIndex);
                pc.controlEnabled = isActive;

                // Si no está activo, detenemos cualquier movimiento horizontal residual
                if (!isActive)
                {
                    pc.velocity.x = 0;
                }
            }

            var current = GetCurrentPlayer();
            if (current != null)
            {
                // Actualizar el modelo global para que los eventos de simulación sigan al perro activo
                if (model != null)
                {
                    model.player = current;

                    // Actualizar el objetivo de la cámara Cinemachine
                    if (model.virtualCamera != null)
                    {
                        model.virtualCamera.Follow = current.transform;
                        model.virtualCamera.LookAt = current.transform;
                    }
                }

                Debug.Log($"[CharacterSwitcher] Ahora controlas a: {current.gameObject.name}");
            }
        }

        public PlayerController GetCurrentPlayer()
        {
            if (characters.Count == 0 || activeIndex < 0 || activeIndex >= characters.Count) return null;
            return characters[activeIndex];
        }

        public void RegisterCharacter(PlayerController pc)
        {
            if (pc != null && !characters.Contains(pc))
            {
                characters.Add(pc);
            }
        }
    }
}
