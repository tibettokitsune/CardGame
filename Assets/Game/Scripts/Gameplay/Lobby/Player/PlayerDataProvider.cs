using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.Lobby.Deck;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IPlayerDataProvider
    {
        Task ClaimCard(IBaseCard claimRandomCardFromDeck);
    }

    public class PlayerDataProvider : IPlayerDataProvider
    {
        private readonly List<IBaseCard> _playersHand = new();

        public Task ClaimCard(IBaseCard card)
        {
            _playersHand.Add(card);
            return Task.CompletedTask;
        }
    }
}