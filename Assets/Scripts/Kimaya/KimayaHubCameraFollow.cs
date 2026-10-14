using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Cámara cinematográfica cenital para el Hub de Kimaya.
    /// Sigue dinámicamente al líder activo (Falin o Mía) manteniendo el mapa 100% encuadrado
    /// sin mostrar jamás bordes vacíos fuera del escenario.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class KimayaHubCameraFollow : MonoBehaviour
    {
        [Header("Mapa y Encuadre")]
        public SpriteRenderer mapRenderer;
        public float smoothSpeed = 6.0f;
        public Vector3 offset = new Vector3(0f, 0f, -10f);

        private Camera _cam;
        private Vector2 _minBounds;
        private Vector2 _maxBounds;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam != null)
            {
                // Color de roca montañosa oscura profunda de fondo (nunca verde)
                _cam.backgroundColor = new Color(0.09f, 0.10f, 0.13f);
            }
        }

        private void Start()
        {
            if (mapRenderer == null)
            {
                var bg = GameObject.Find("Forest_Hub_Background");
                if (bg != null) mapRenderer = bg.GetComponent<SpriteRenderer>();
            }

            RecalculateBounds();
        }

        public void RecalculateBounds()
        {
            if (_cam == null || mapRenderer == null) return;

            Bounds b = mapRenderer.bounds;
            float vertExtent = _cam.orthographicSize;
            float horzExtent = vertExtent * _cam.aspect;

            // Si la relación de aspecto es más ancha que el mapa, ajustar orthographicSize para llenar el ancho
            if (horzExtent > b.extents.x)
            {
                _cam.orthographicSize = Mathf.Max(3.8f, b.extents.x / _cam.aspect);
                vertExtent = _cam.orthographicSize;
                horzExtent = vertExtent * _cam.aspect;
            }

            _minBounds.x = b.min.x + horzExtent;
            _maxBounds.x = b.max.x - horzExtent;
            _minBounds.y = b.min.y + vertExtent;
            _maxBounds.y = b.max.y - vertExtent;

            // Si el mapa es casi del mismo tamaño que la vista, bloquear al centro
            if (_minBounds.x > _maxBounds.x)
            {
                float midX = (b.min.x + b.max.x) * 0.5f;
                _minBounds.x = _maxBounds.x = midX;
            }
            if (_minBounds.y > _maxBounds.y)
            {
                float midY = (b.min.y + b.max.y) * 0.5f;
                _minBounds.y = _maxBounds.y = midY;
            }
        }

        private void LateUpdate()
        {
            Transform target = null;
            if (KimayaHubDuoController.Instance != null)
            {
                target = KimayaHubDuoController.Instance.GetActiveTransform();
            }

            if (target == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) target = p.transform;
            }

            if (target == null) return;

            Vector3 desired = target.position + offset;
            desired.x = Mathf.Clamp(desired.x, _minBounds.x, _maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, _minBounds.y, _maxBounds.y);
            desired.z = offset.z;

            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }
    }
}
