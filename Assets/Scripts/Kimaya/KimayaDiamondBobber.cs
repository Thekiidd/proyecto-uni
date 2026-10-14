using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador suave para el diamante de selección flotante sobre la cabeza del perro activo.
    /// Realiza una oscilación vertical suave (bobbing) y pulso sutil de brillo.
    /// </summary>
    public class KimayaDiamondBobber : MonoBehaviour
    {
        public float baseY = 0.85f;
        public float bobHeight = 0.07f;
        public float bobSpeed = 4.2f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.localPosition;
            if (baseY == 0f) baseY = _startPos.y;
        }

        private void Update()
        {
            float newY = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(_startPos.x, newY, _startPos.z);
        }
    }
}
