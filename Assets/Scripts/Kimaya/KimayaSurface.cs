using UnityEngine;

namespace Kimaya
{
    /// <summary>
    /// Componente de superficie para plataformas estilo Jump King.
    /// Define cómo reacciona el jugador al pisar o deslizarse sobre el bloque.
    /// </summary>
    public class KimayaSurface : MonoBehaviour
    {
        [Header("Tipo de Superficie Jump King")]
        public SurfaceType surfaceType = SurfaceType.Normal;

        [Header("Ajustes Físicos Personalizados (Opcional)")]
        [Tooltip("Fricción en hielo (Jump King original = 0.1)")]
        public float iceFriction = 0.1f;

        [Tooltip("Multiplicador de velocidad al caminar en arena (Jump King original = 0.25)")]
        public float sandMoveMultiplier = 0.25f;

        [Tooltip("Multiplicador de fuerza de salto en arena (Jump King original = 0.5)")]
        public float sandJumpMultiplier = 0.5f;
    }
}
