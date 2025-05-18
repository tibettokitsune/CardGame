using DG.Tweening;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.UI
{
    public class PlayerHandScreen : UIScreen
    {
        [Inject] private IPlayerPresenter _playerPresenter;
        [Inject] private HandCardFactory _handCardFactory;
        [SerializeField] public Transform cardsContainer;
        [SerializeField] public Transform fromPos;
        [SerializeField] public HandLayoutGroup group;
        
        private void OnEnable()
        {
            //TODO: recieve deck changes
            // _receiver.OnCardGenerated += HandleIncrease;
            _playerPresenter.PlayerHand.ObserveAdd().Subscribe(OnChange).AddTo(this);
            _playerPresenter.PlayerHand.ObserveRemove().Subscribe(OnChange).AddTo(this);
        }
        
        private void OnChange(CollectionAddEvent<CardEntity> collectionAddEvent)
        {
            var view = _handCardFactory.Create();
            var card = collectionAddEvent.Value;
            view.Setup(card.Name, card.Description, card.MainLayer, card.BackgroundLayer);
            view.transform.SetParent(cardsContainer);
            group.SetLayoutHorizontal();
        }
        
        private void OnChange(CollectionRemoveEvent<CardEntity> collectionAddEvent)
        {
            var view = _handCardFactory.Create();
            var card = collectionAddEvent.Value;
            view.Setup(card.Name, card.Description, card.MainLayer, card.BackgroundLayer);
            view.transform.SetParent(cardsContainer);
            group.SetLayoutHorizontal();
        }
        
        //
        // private void OnDisable()
        // {
        //     // _receiver.OnCardGenerated -= HandleIncrease;
        // }
    }
}