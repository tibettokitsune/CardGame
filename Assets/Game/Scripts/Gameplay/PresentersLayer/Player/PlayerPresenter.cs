using System;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.Player;
using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IFillStartHandUseCase
    {
        Task Execute();
    }

    public interface IPlayerPresenter
    {
        ReactiveCollection<CardEntity> PlayerHand { get; }
    }


    public class PlayerPresenter : IPlayerPresenter, IFillStartHandUseCase, IDisposable
    {
        private readonly IDeckPresenter _deckPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private const int StartDoorsLimit = 4;
        private const int StartTreasuresLimit = 4;

        private readonly CompositeDisposable _disposables = new();
        public ReactiveCollection<CardEntity> PlayerHand { get; } = new();

        public PlayerPresenter(IDeckPresenter deckPresenter,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _playerDataProvider = playerDataProvider;
            _playerDataProvider.PlayersHand.ObserveAdd().Subscribe(OnCollectionChange).AddTo(_disposables);
            _playerDataProvider.PlayersHand.ObserveRemove().Subscribe(OnCollectionChange).AddTo(_disposables);
        }

        private void OnCollectionChange(CollectionAddEvent<string> collectionAddEvent)
        {
            PlayerHand.Add(new CardEntity( _deckPresenter.GetCardById(collectionAddEvent.Value)));
        }

        private void OnCollectionChange(CollectionRemoveEvent<string> collectionRemoveEvent)
        {
            PlayerHand.Remove(new CardEntity( _deckPresenter.GetCardById(collectionRemoveEvent.Value)));
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
            var cardId = await _deckPresenter.ClaimRandomCardFromDeck(cardType);
            await _playerDataProvider.ClaimCard(cardId);
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

        public void Dispose()
        {
            PlayerHand?.Dispose();
        }
    }
}