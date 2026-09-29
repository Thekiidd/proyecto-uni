using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Cámara de plataformas que sigue al personaje activo con suavizado.
    /// Tiene límites de mapa configurables.
    /// </summary>
    public class KimayaCameraController : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        public float smoothSpeed = 6f;
        public Vector3 offset = new Vector3(0f, 1.5f, -10f);

        [Header("Límites del nivel")]
        public bool useBounds = true;
        public float minX = -50f;
        public float maxX =  50f;
        public float minY = -10f;
        public float maxY =  20f;

        [Header("Look Ahead (mira hacia donde camina)")]
        public float lookAheadDistance = 2f;
        public float lookAheadSpeed    = 3f;

        private float _lookAheadX = 0f;
        private Camera _cam;
        private float _halfW;
        private float _halfH;

        private void Awake()
        {
            _cam  = GetComponent<Camera>();
            _halfH = _cam.orthographicSize;
            _halfW = _halfH * _cam.aspect;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;

        private void LateUpdate()
        {
            if (target == null) return;

            // Look ahead
            float vx = 0f;
            var pc = target.GetComponent<PlayerController2D>();
            if (pc != null) vx = pc.VelocityX;
            _lookAheadX = Mathf.Lerp(_lookAheadX,
                Mathf.Sign(vx) * lookAheadDistance * (Mathf.Abs(vx) > 0.5f ? 1f : 0f),
                Time.deltaTime * lookAheadSpeed);

            Vector3 desired = target.position + offset + new Vector3(_lookAheadX, 0f, 0f);

            if (useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, minX + _halfW, maxX - _halfW);
                desired.y = Mathf.Clamp(desired.y, minY + _halfH, maxY - _halfH);
            }

            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }
    }
}
