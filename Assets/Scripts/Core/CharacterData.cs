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
        public Sprite[] idleFrames; // Frames para animación idle base (respiración tranquila)
        public Sprite[] runFrames;  // Frames para animación al correr
        public Sprite[] walkFrames; // Frames para animación al caminar
        public Sprite[] deathFrames; // Frames para animación de caída/muerte (lying-down)
        public Sprite[] hurtFrames;  // Frames para animación de impacto
        public Sprite[] scratchFrames; // Rascándose la orejita
        public Sprite[] stretchFrames; // Estirándose como perrito
        public Sprite[] lickFrames;    // Lamiéndose la patita (lick 1)
        public Sprite[] lick2Frames;   // Lamiéndose el pecho / hocico (lick 2)
        public Sprite[] barkFrames;    // Ladrido amistoso
        public Sprite[] sitFrames;     // Sentado atento como buen perro
        public Sprite[] layDownFrames; // Acostarse en el suelo y levantarse
        public Sprite[] sleepFrames;   // Durmiendo hecho bolita zZz
    }
}
