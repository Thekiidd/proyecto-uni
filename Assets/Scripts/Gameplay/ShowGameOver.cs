using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Evento programado por PlayerDeath cuando el jugador se queda sin vidas.
    /// Muestra la pantalla de Game Over.
    /// </summary>
    public class ShowGameOver : Simulation.Event<ShowGameOver>
    {
        public override void Execute()
        {
            if (GameHUD.Instance != null)
            {
                GameHUD.Instance.ShowGameOver();
            }
            else if (GameOverUI.Instance != null)
            {
                GameOverUI.Instance.Show();
            }
            else
            {
                // Fallback: volver al menú directo
                SceneManager.LoadScene(GameData.MainMenuScene);
            }
        }
    }
}
