using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Animador suave para el diamante de selección flotante sobre la cabeza del perro activo.
    /// Flota limpiamente por encima de las orejas y cabeza en todas las direcciones (sur, norte, este, oeste),
    /// sin tapar el hocico, ojos o cara del personaje.
    /// </summary>
    public class KimayaDiamondBobber : MonoBehaviour
    {
        [Header("Posición y Flotación")]
        public float baseY = 2.05f;
        public float bobHeight = 0.05f;
        public float bobSpeed = 3.6f;

        [Header("Escala y Apariencia")]
        public float targetScale = 0.27f;

        private SpriteRenderer _sr;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();

            // Ajuste calibrado de altura y escala según el perrito padre (unificado a 36 PPU en las 4 direcciones)
            if (transform.parent != null && transform.parent.name.ToLower().Contains("mia"))
            {
                baseY = 1.52f;
                targetScale = 0.24f;
            }
            else if (transform.parent != null && transform.parent.name.ToLower().Contains("falin"))
            {
                baseY = 2.05f;
                targetScale = 0.27f;
            }

            transform.localScale = new Vector3(targetScale, targetScale, 1f);
            transform.localPosition = new Vector3(0f, baseY, 0f);

            if (_sr != null)
            {
                _sr.sortingOrder = 950;
            }
        }

        private void Update()
        {
            // Oscilación suave arriba y abajo
            float newY = baseY + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = new Vector3(0f, newY, 0f);
            transform.localScale = new Vector3(targetScale, targetScale, 1f);

            // Pulso sutil de brillo respetando los colores naturales de la gema (esmeralda / zafiro)
            if (_sr != null)
            {
                float pulse = 0.90f + 0.10f * Mathf.Sin(Time.time * 4f);
                _sr.color = new Color(1f, 1f, 1f, pulse);
            }
        }
    }
}
