using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Se dispara cuando el jugador entra a la VictoryZone.
    /// Muestra la UI de nivel completado y avanza al siguiente nivel.
    /// </summary>
    public class PlayerEnteredVictoryZone : Simulation.Event<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            // Animación de victoria
            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;

            // Agregar puntos por completar el nivel
            if (GameData.Instance != null)
                GameData.Instance.AddScore(500);

            // Mostrar UI de nivel completado
            if (LevelCompleteUI.Instance != null)
            {
                LevelCompleteUI.Instance.Show();
            }
            else
            {
                // Fallback: si no hay UI, cargar siguiente nivel directo
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadNextLevel();
            }
        }
    }
}
