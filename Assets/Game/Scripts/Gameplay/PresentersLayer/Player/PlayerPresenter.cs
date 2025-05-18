using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.Player;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IFillStartHandUseCase
    {
        Task Execute();
    }
    
    public interface IPlayerPresenter
    {
        
    }


    public class PlayerPresenter : IPlayerPresenter, IFillStartHandUseCase
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

        #region usecases

        async Task IFillStartHandUseCase.Execute()
        {
            await Task.Delay(100);
            await FillStartDoors();
            await FillStartTreasures();
        }

        private async Task AddRandomCardByType(CardType cardType)
        {
            var card = await _deckPresenter.ClaimRandomCardFromDeck(cardType);
            await _playerDataProvider.ClaimCard(card);
            // var view = _handCardFactory.Create();
            // view.Setup(card.Name, card.Description, card.MainLayer, card.BackgroundLayer);
            // _handCardViews.Add(card, view);
            //TODO: notify gameplay bus about changes
            // _notifier.NotifyAddCardToHand(card, view);
        }

        private async Task FillStartDoors()
        {
            for (var i = 0; i < StartDoorsLimit; i++)
            {
                await AddRandomCardByType(CardType.Door);
                await Task.Delay(100);
            }
        }

        private async Task FillStartTreasures()
        {
            for (var i = 0; i < StartTreasuresLimit; i++)
            {
                await AddRandomCardByType(CardType.Treasure);
                await Task.Delay(100);
            }
        }

        #endregion
    }
}