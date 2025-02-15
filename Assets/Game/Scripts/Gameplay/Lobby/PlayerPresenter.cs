using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Scripts.Gameplay.Lobby
{
    public class PlayerPresenter : IPlayerPresenter
    {
        private List<IBaseCard> _playersHand;
        private readonly IDeckPresenter _deckPresenter;
        private const int StartDoorsLimit = 4;
        private const int StartTreasuresLimit = 4;

        public PlayerPresenter(IDeckPresenter deckPresenter)
        {
            _deckPresenter = deckPresenter;
        }

        public async Task FillStartHand()
        {
            _playersHand = new List<IBaseCard>();
            await FillStartDoors();
            await FillStartTreasures();
        }

        private async Task FillStartDoors()
        {
            for (var i = 0; i < StartDoorsLimit; i++)
            {
                _playersHand.Add(await _deckPresenter.ClaimRandomCardFromDeck(CardType.Door));
            }
        }
        
        private async Task FillStartTreasures()
        {
            for (var i = 0; i < StartTreasuresLimit; i++)
            {
                _playersHand.Add(await _deckPresenter.ClaimRandomCardFromDeck(CardType.Treasure));
            }
        }
    }

    public interface IPlayerPresenter
    {
        Task FillStartHand();
    }
}