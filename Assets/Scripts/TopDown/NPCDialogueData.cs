using System.Collections.Generic;
using UnityEngine;

namespace Platformer.TopDown
{
    /// <summary>
    /// Datos de diálogo para un NPC con múltiples fases y reacciones.
    /// </summary>
    [CreateAssetMenu(fileName = "NPC_Dialogue", menuName = "Dockia/NPC Dialogue")]
    public class NPCDialogueData : ScriptableObject
    {
        [Header("Identidad del NPC")]
        public string npcName = "Aldeano";
        public Color nameColor = new Color(1f, 0.85f, 0.4f);
        public string profession = "Aldeano";

        [Header("Saludos (primera vez que hablas)")]
        [TextArea(2, 4)]
        public string[] greetings = new string[]
        {
            "¡Hola! No te había visto antes por aquí.",
            "¡Bienvenido, forastero!"
        };

        [Header("Diálogos Generales")]
        [TextArea(2, 4)]
        public string[] generalLines = new string[]
        {
            "¿Cómo puedo ayudarte?",
            "Hace buen tiempo hoy, ¿verdad?"
        };

        [Header("Historia / Lore del mundo")]
        [TextArea(2, 4)]
        public string[] loreLines = new string[]
        {
            "Esta aldea existe desde hace 300 años.",
            "Los cristales de luz son nuestra fuente de energía."
        };

        [Header("Consejos de juego")]
        [TextArea(2, 4)]
        public string[] tipLines = new string[]
        {
            "Presiona E cerca de objetos para interactuar.",
            "Puedes entrar a las casas por sus puertas."
        };

        [Header("Reacción a peligro (si hay amenaza activa)")]
        [TextArea(2, 4)]
        public string[] dangerLines = new string[]
        {
            "¡Ten cuidado ahí afuera!",
            "Se escuchan ruidos extraños en el bosque..."
        };

        [Header("Despedida")]
        [TextArea(1, 2)]
        public string[] farewells = new string[]
        {
            "¡Hasta luego! ¡Cuídate!",
            "¡Buen viaje, valiente!"
        };

        [Header("¿Tiene misión disponible?")]
        public bool hasQuest = false;
        [TextArea(2, 4)]
        public string questOffer = "";
        [TextArea(1, 2)]
        public string questActiveReminder = "";
        [TextArea(2, 3)]
        public string questComplete = "";
        public int questRewardCoins = 0;
    }

    /// <summary>
    /// Diálogo de conversación multi-turno para el sistema de novela visual rápido.
    /// </summary>
    [System.Serializable]
    public class ConversationSequence
    {
        [TextArea(2, 4)]
        public string[] lines;
        public string speakerName;
    }
}
