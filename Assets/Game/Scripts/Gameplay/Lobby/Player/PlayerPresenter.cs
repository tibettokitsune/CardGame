using System.Threading.Tasks;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IPlayerPresenter
    {
        Task FillStartHand();
    }

    public class PlayerPresenter : IPlayerPresenter
    {
        private readonly IDeckPresenter _deckPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private const int StartDoorsLimit = 4;
        private const int StartTreasuresLimit = 4;

        public PlayerPresenter(IDeckPresenter deckPresenter,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _playerDataProvider = playerDataProvider;
        }

        public async Task FillStartHand()
        {
            await FillStartDoors();
            await FillStartTreasures();
        }

        private async Task FillStartDoors()
        {
            for (var i = 0; i < StartDoorsLimit; i++)
            {
                _playerDataProvider.ClaimCard(await _deckPresenter.ClaimRandomCardFromDeck(CardType.Door));
            }
        }

        private async Task FillStartTreasures()
        {
            for (var i = 0; i < StartTreasuresLimit; i++)
            {
                _playerDataProvider.ClaimCard(await _deckPresenter.ClaimRandomCardFromDeck(CardType.Treasure));
            }
        }
    }
}