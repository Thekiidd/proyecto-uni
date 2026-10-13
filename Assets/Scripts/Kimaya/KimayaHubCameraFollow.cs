using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Cámara suave cenital para el Hub de la aldea.
    /// Sigue dinámicamente al perro activo entre Falin y Mia con límites de mapa (clamping).
    /// </summary>
    public class KimayaHubCameraFollow : MonoBehaviour
    {
        public Vector2 minBounds = new Vector2(-12f, -14f);
        public Vector2 maxBounds = new Vector2(12f, 14f);
        public float smoothSpeed = 6.0f;
        public Vector3 offset = new Vector3(0f, 0f, -10f);

        private void LateUpdate()
        {
            Transform target = KimayaHubDuoController.Instance != null 
                ? KimayaHubDuoController.Instance.GetActiveTransform() 
                : null;

            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
            desiredPosition.z = offset.z;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
