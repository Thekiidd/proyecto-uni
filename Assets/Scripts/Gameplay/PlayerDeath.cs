using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Se dispara cuando el jugador muere.
    /// Descuenta una vida; si no quedan vidas muestra Game Over.
    /// </summary>
    public class PlayerDeath : Simulation.Event<PlayerDeath>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            var player = model.player;
            if (player.health.IsAlive)
            {
                player.health.Die();
                model.virtualCamera.Follow = null;
                model.virtualCamera.LookAt = null;
                player.controlEnabled = false;

                if (player.audioSource && player.ouchAudio)
                    player.audioSource.PlayOneShot(player.ouchAudio);

                player.animator.SetTrigger("hurt");
                player.animator.SetBool("dead", true);

                // Descontar una vida
                if (GameData.Instance != null)
                {
                    GameData.Instance.LoseLife();

                    if (!GameData.Instance.HasLivesLeft())
                    {
                        // Sin vidas: Game Over
                        Simulation.Schedule<ShowGameOver>(2);
                        return;
                    }
                }

                // Aún hay vidas: respawnear
                Simulation.Schedule<PlayerSpawn>(2);
            }
        }
    }
}
