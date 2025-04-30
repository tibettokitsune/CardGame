using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Infrastructure;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IFillStartHandUseCase
    {
        Task Execute();
    }


    public class PlayerPresenter : IFillStartHandUseCase
    {
        private readonly IDeckPresenter _deckPresenter;
        private readonly HandCardFactory _handCardFactory;
        private readonly IGameplayNotifier _notifier;
        private readonly IPlayerDataProvider _playerDataProvider;
        private const int StartDoorsLimit = 4;
        private const int StartTreasuresLimit = 4;
        
        private Dictionary<IBaseCard, HandCardView> _handCardViews = new();

        public PlayerPresenter(IDeckPresenter deckPresenter,
            HandCardFactory handCardFactory,
            IGameplayNotifier notifier,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _handCardFactory = handCardFactory;
            _notifier = notifier;
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
            var view = _handCardFactory.Create();
            view.Setup(card.Name, card.Description, card.MainLayer, card.BackgroundLayer);
            _handCardViews.Add(card, view);
            _notifier.NotifyAddCardToHand(card, view);
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