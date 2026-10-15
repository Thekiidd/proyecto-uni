using System.Collections;
using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Cámara fija por pantallas estilo Jump King.
    /// La cámara encuadra una pantalla fija de tamaño predeterminado (ej. 16x10 unidades).
    /// Cuando el jugador cruza el techo de la pantalla actual, transiciona hacia arriba.
    /// Si el jugador cae por el suelo, transiciona hacia abajo inmediatamente o sigue su caída.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class JumpKingScreenCamera : MonoBehaviour
    {
        [Header("Objetivo")]
        public Transform target;

        [Header("Dimensiones de Pantalla")]
        public float screenHeight = 10f;
        public float screenWidth = 16f;
        public float transitionSpeed = 15f;

        [Header("Piso Base")]
        public float baseWorldY = 0f;

        public int CurrentScreenIndex { get; private set; } = 0;
        public int MaxScreenReached { get; private set; } = 0;

        private Camera _cam;
        private Vector3 _targetCameraPos;
        private bool _isTransitioning = false;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _targetCameraPos = transform.position;
        }

        private void Start()
        {
            if (target != null)
            {
                RecalculateScreenIndex(instant: true);
            }
        }

        [Tooltip("Si es true, la cámara cambia de pantalla al instante como en Jump King original")]
        public bool instantScreenTransition = true;

        private void LateUpdate()
        {
            if (target == null) return;

            // Calcular en qué pantalla está el jugador según su coordenada Y
            float playerY = target.position.y;
            int newScreenIndex = Mathf.FloorToInt((playerY - baseWorldY) / screenHeight);
            if (newScreenIndex < 0) newScreenIndex = 0;

            bool screenChanged = (newScreenIndex != CurrentScreenIndex);

            if (screenChanged)
            {
                CurrentScreenIndex = newScreenIndex;
                if (CurrentScreenIndex > MaxScreenReached)
                {
                    MaxScreenReached = CurrentScreenIndex;
                }
            }

            // La posición Y deseada para la cámara es el centro de la pantalla actual
            float desiredY = baseWorldY + (CurrentScreenIndex * screenHeight) + (screenHeight * 0.5f);
            _targetCameraPos = new Vector3(transform.position.x, desiredY, transform.position.z);

            // Transición instantánea (Jump King) o suave
            if (instantScreenTransition && screenChanged)
            {
                transform.position = _targetCameraPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, _targetCameraPos, Time.deltaTime * transitionSpeed);
            }
        }

        public void RecalculateScreenIndex(bool instant)
        {
            if (target == null) return;
            float playerY = target.position.y;
            CurrentScreenIndex = Mathf.FloorToInt((playerY - baseWorldY) / screenHeight);
            if (CurrentScreenIndex < 0) CurrentScreenIndex = 0;

            float desiredY = baseWorldY + (CurrentScreenIndex * screenHeight) + (screenHeight * 0.5f);
            _targetCameraPos = new Vector3(transform.position.x, desiredY, transform.position.z);

            if (instant)
            {
                transform.position = _targetCameraPos;
            }
        }

        private void OnDrawGizmos()
        {
            // Dibujar las divisiones de pantallas en el editor
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
            float startY = baseWorldY;
            for (int i = 0; i < 20; i++)
            {
                float cy = startY + (i * screenHeight) + (screenHeight * 0.5f);
                Gizmos.DrawWireCube(new Vector3(transform.position.x, cy, 0), new Vector3(screenWidth, screenHeight, 0.1f));
            }
        }
    }
}
