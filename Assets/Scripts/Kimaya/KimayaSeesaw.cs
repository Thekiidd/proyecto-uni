using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Mecanismo de balancín (Seesaw) estilo Fireboy & Watergirl.
    /// Utiliza física 2D (HingeJoint2D) para inclinarse suavemente
    /// cuando el jugador o una caja se posiciona sobre alguno de sus extremos.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class KimayaSeesaw : MonoBehaviour
    {
        [Header("Configuración del Balancín")]
        [Tooltip("Ángulo máximo de inclinación hacia arriba o abajo en grados.")]
        public float maxAngle = 26f;

        [Tooltip("Fuerza de retorno suave al centro cuando no hay nadie encima.")]
        public float returnSpeed = 0.5f;

        [Tooltip("Amortiguación para evitar vibraciones bruscas.")]
        public float angularDrag = 3f;

        private Rigidbody2D _rb;
        private HingeJoint2D _joint;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.mass = 4f;
            _rb.angularDamping = angularDrag; // Unity 6 API (o angularDrag)

            // Configurar o buscar el HingeJoint2D
            _joint = GetComponent<HingeJoint2D>();
            if (_joint == null)
                _joint = gameObject.AddComponent<HingeJoint2D>();

            _joint.useLimits = true;
            var limits = _joint.limits;
            limits.min = -maxAngle;
            limits.max = maxAngle;
            _joint.limits = limits;
        }

        private void FixedUpdate()
        {
            // Si nadie está tocando el balancín, tiende suavemente hacia el centro
            if (returnSpeed > 0f && Mathf.Abs(_rb.linearVelocity.y) < 0.05f)
            {
                float currentAngle = transform.eulerAngles.z;
                if (currentAngle > 180f) currentAngle -= 360f;

                if (Mathf.Abs(currentAngle) > 0.5f)
                {
                    _rb.AddTorque(-Mathf.Sign(currentAngle) * returnSpeed);
                }
            }
        }
    }
}
