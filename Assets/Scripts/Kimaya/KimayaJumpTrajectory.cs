using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Visualizador de parábolas de salto de Kimaya Jump King en el Scene View.
    /// Permite diseñar y colocar plataformas con precisión matemática viendo el alcance real del perro.
    /// </summary>
    [ExecuteAlways]
    public class KimayaJumpTrajectory : MonoBehaviour
    {
        [Header("Ajustes de Visualización")]
        public bool showTrajectories = true;
        public int trajectorySteps = 40;
        public float timeStep = 0.035f;

        [Header("Referencias")]
        public KimayaJumpKingController controller;

        private void OnDrawGizmos()
        {
            if (!showTrajectories) return;
            if (controller == null) controller = GetComponent<KimayaJumpKingController>();

            Vector2 startPos = transform.position;
            float grav = 2.4f * 9.81f; // Gravedad calculada de Kimaya
            float minV = 7.0f;
            float maxV = 17.5f;
            float midV = (minV + maxV) * 0.5f;
            float hSpeed = 7.0f;

            // 1. Salto Mínimo (Verde) hacia derecha e izquierda
            DrawArc(startPos, new Vector2(hSpeed, minV), grav, Color.green);
            DrawArc(startPos, new Vector2(-hSpeed, minV), grav, Color.green);

            // 2. Salto Medio (Amarillo)
            DrawArc(startPos, new Vector2(hSpeed, midV), grav, Color.yellow);
            DrawArc(startPos, new Vector2(-hSpeed, midV), grav, Color.yellow);

            // 3. Salto Máximo Diagonal (Rojo)
            DrawArc(startPos, new Vector2(hSpeed, maxV), grav, new Color(1f, 0.3f, 0.3f));
            DrawArc(startPos, new Vector2(-hSpeed, maxV), grav, new Color(1f, 0.3f, 0.3f));

            // 4. Salto Máximo Vertical puro (Cian)
            DrawArc(startPos, new Vector2(0f, maxV), grav, Color.cyan);
        }

        private void DrawArc(Vector2 origin, Vector2 velocity, float gravity, Color color)
        {
            Gizmos.color = color;
            Vector2 current = origin;
            for (int i = 1; i <= trajectorySteps; i++)
            {
                float t = i * timeStep;
                Vector2 next = origin + new Vector2(velocity.x * t, velocity.y * t - 0.5f * gravity * t * t);
                Gizmos.DrawLine(current, next);
                current = next;
            }
            // Dibujar punto de impacto estimado al nivel del origen
            Gizmos.DrawWireSphere(current, 0.15f);
        }
    }
}
