using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Desplazamiento continuo estilo Jump King para capas de nubes, niebla o partículas de fondo.
    /// </summary>
    public class KimayaJumpKingScroller : MonoBehaviour
    {
        [Header("Velocidad de Desplazamiento")]
        public float scrollSpeed = 0.5f;
        public float resetThreshold = 20f;
        public float loopWidth = 40f;

        private void Update()
        {
            transform.position += Vector3.right * scrollSpeed * Time.deltaTime;

            if (scrollSpeed > 0 && transform.position.x > resetThreshold)
            {
                transform.position -= new Vector3(loopWidth, 0f, 0f);
            }
            else if (scrollSpeed < 0 && transform.position.x < -resetThreshold)
            {
                transform.position += new Vector3(loopWidth, 0f, 0f);
            }
        }
    }
}
