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
            if (token != null && token.tokenCollectAudio != null)
            {
                AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);
            }

            if (token == null) return;

            if (token.tokenType == TokenInstance.DogTokenType.DogBiscuit)
            {
                // Galleta canina: restaura 1 vida al jugador
                if (player != null && player.health != null)
                {
                    player.health.Increment();
                }
                if (GameData.Instance != null)
                {
                    GameData.Instance.AddScore(250);
                }
            }
            else if (token.tokenType == TokenInstance.DogTokenType.TennisBall)
            {
                // Pelota de tenis secreta
                if (GameData.Instance != null)
                {
                    GameData.Instance.AddScore(500);
                }
                Platformer.UI.GameHUD.RegisterTennisBallCollected();
            }
            else
            {
                // Hueso dorado común: +1 moneda/hueso y puntos
                if (GameData.Instance != null)
                {
                    GameData.Instance.AddCoins(1);
                    GameData.Instance.AddScore(100);
                }
            }
        }
    }
}
