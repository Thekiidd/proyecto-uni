using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Evento programado por PlayerDeath cuando el jugador se queda sin vidas.
    /// Muestra la pantalla de Game Over.
    /// </summary>
    public class ShowGameOver : Simulation.Event<ShowGameOver>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.Show();
            }
            else
            {
                // Fallback: volver al menú directo
                if (SceneLoader.Instance != null)
                    SceneLoader.Instance.LoadMainMenu();
            }
        }
    }
}
