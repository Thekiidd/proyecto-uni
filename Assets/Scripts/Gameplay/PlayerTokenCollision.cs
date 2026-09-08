using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Se dispara cuando el jugador colisiona con un token.
    /// Ahora también agrega puntos al GameData.
    /// </summary>
    public class PlayerTokenCollision : Simulation.Event<PlayerTokenCollision>
    {
        public PlayerController player;
        public TokenInstance token;

        PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public override void Execute()
        {
            AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);

            // Agregar 100 puntos por cada token
            if (GameData.Instance != null)
                GameData.Instance.AddScore(100);
        }
    }
}
