using System;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.Player;
using ModestTree;
using UniRx;
using UnityEngine;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IFillStartHandUseCase
    {
        Task Execute();
    }

    public enum EquipCardResult
    {
        Equipped, 
        Failed
    }
    
    public interface IEquipCardUseCase
    {
        Task<EquipCardResult> Execute(string cardId);
    }

    public interface IPlayerPresenter
    {
        ReactiveCollection<CardEntity> PlayerHand { get; }
        ReactiveCollection<CardEntity> PlayerEquipment { get; }
    }


    public class PlayerPresenter : IPlayerPresenter, 
        IFillStartHandUseCase, 
        IEquipCardUseCase,
        IDisposable
    {
        private readonly IDeckPresenter _deckPresenter;
        private readonly IPlayerDataProvider _playerDataProvider;
        private const int StartDoorsLimit = 4;
        private const int StartTreasuresLimit = 4;

        private readonly CompositeDisposable _disposables = new();
        public ReactiveCollection<CardEntity> PlayerHand { get; } = new();
        public ReactiveCollection<CardEntity> PlayerEquipment { get; } = new();

        public PlayerPresenter(IDeckPresenter deckPresenter,
            IPlayerDataProvider playerDataProvider)
        {
            _deckPresenter = deckPresenter;
            _playerDataProvider = playerDataProvider;
            _playerDataProvider.PlayersHand.ObserveAdd().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersHand.ObserveRemove().Subscribe(OnHandChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveAdd().Subscribe(OnEquipmentChange).AddTo(_disposables);
            _playerDataProvider.PlayersEquipment.ObserveRemove().Subscribe(OnEquipmentChange).AddTo(_disposables);
        }

        private void OnHandChange(CollectionAddEvent<string> collectionAddEvent)
        {
            PlayerHand.Add(new CardEntity( _deckPresenter.GetCardById(collectionAddEvent.Value)));
        }

        private void OnHandChange(CollectionRemoveEvent<string> collectionRemoveEvent)
        {
            PlayerHand.Remove(PlayerHand.First(x => x.ID == collectionRemoveEvent.Value));
        }
        
        private void OnEquipmentChange(CollectionAddEvent<string> collectionAddEvent)
        {
            PlayerEquipment.Add(new CardEntity( _deckPresenter.GetCardById(collectionAddEvent.Value)));
        }

        private void OnEquipmentChange(CollectionRemoveEvent<string> collectionRemoveEvent)
        {
            PlayerEquipment.Remove(new CardEntity( _deckPresenter.GetCardById(collectionRemoveEvent.Value)));
        }

        #region usecases
        
        async Task<EquipCardResult> IEquipCardUseCase.Execute(string cardId)
        {
            if (_playerDataProvider.IsPlayerCanEquipCard(cardId))
            {
                await _playerDataProvider.EquipCard(cardId);
                return EquipCardResult.Equipped;
            }
            return EquipCardResult.Failed;
        }

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