using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// Cámara suave cenital para seguir al perro dentro de los límites del pueblo.
    /// </summary>
    public class TopDownCameraFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothSpeed = 5f;
        public Vector2 minBounds = new Vector2(-15f, -15f);
        public Vector2 maxBounds = new Vector2(15f, 15f);

        private void Start()
        {
            if (target == null)
            {
                var dog = FindAnyObjectByType<TopDownDogController>();
                if (dog != null) target = dog.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                var dog = FindAnyObjectByType<TopDownDogController>();
                if (dog != null) target = dog.transform;
                return;
            }

            Vector3 desiredPos = new Vector3(target.position.x, target.position.y, -10f);

            // Limitar a los bordes del pueblo
            desiredPos.x = Mathf.Clamp(desiredPos.x, minBounds.x, maxBounds.x);
            desiredPos.y = Mathf.Clamp(desiredPos.y, minBounds.y, maxBounds.y);

            transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        }
    }
}
