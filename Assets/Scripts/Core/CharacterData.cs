using UnityEngine;

namespace Platformer.Core
{
    /// <summary>
    /// ScriptableObject que define las estadísticas y apariencia de un personaje jugable.
    /// Crear desde: Assets > Create > Platformer > Character Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Character", menuName = "Platformer/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identidad")]
        public string characterName = "Runner";
        [TextArea(2, 4)]
        public string description = "Un personaje veloz y ágil.";
        public Sprite portrait;          // Imagen del personaje para el menú
        public RuntimeAnimatorController animatorController; // Animaciones propias

        [Header("Estadísticas de Movimiento")]
        [Range(1f, 20f)] public float moveSpeed = 7f;
        [Range(1f, 30f)] public float jumpStrength = 14f;
        [Range(1f, 5f)]  public float gravityModifier = 3f;

        [Header("Habilidades")]
        public bool hasDoubleJump = false;
        public bool hasDash = false;
        [Range(0f, 20f)] public float dashSpeed = 0f;

        [Header("Vidas")]
        [Range(1, 10)] public int startingLives = 3;

        [Header("Visual")]
        public Color characterColor = Color.white;
        public Sprite idleSprite;   // Sprite que reemplaza al personaje en juego
        public Sprite runSprite;    // Sprite al correr (opcional)

    }
}
