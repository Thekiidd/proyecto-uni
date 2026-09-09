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

                if (player.animator != null && player.animator.enabled)
                {
                    if (HasParameter(player.animator, "hurt")) player.animator.SetTrigger("hurt");
                    if (HasParameter(player.animator, "dead")) player.animator.SetBool("dead", true);
                }

                player.TriggerDeathAnimation();

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

        private bool HasParameter(Animator animator, string paramName)
        {
            if (animator == null) return false;
            foreach (var p in animator.parameters)
            {
                if (p.name == paramName) return true;
            }
            return false;
        }
    }
}
