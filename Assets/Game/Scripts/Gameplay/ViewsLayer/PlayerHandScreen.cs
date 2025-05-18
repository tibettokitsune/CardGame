using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class PlayerHandScreen : UIScreen
    {
        [SerializeField] private HandCardView cardPrefab;
        [Inject] private IPlayerPresenter _playerPresenter;
        [SerializeField] public Transform cardsContainer;
        [SerializeField] public HandLayoutGroup group;
        
        private void OnEnable()
        {
            _playerPresenter.PlayerHand.ObserveAdd().Subscribe(OnChange).AddTo(this);
            _playerPresenter.PlayerHand.ObserveRemove().Subscribe(OnChange).AddTo(this);
        }
        
        private void OnChange(CollectionAddEvent<CardEntity> collectionAddEvent)
        {
            var view = Instantiate(cardPrefab, cardsContainer);
            var card = collectionAddEvent.Value;
            view.Setup(card.Name, card.Description, card.MainLayer, card.BackgroundLayer);
            view.transform.SetParent(cardsContainer);
            group.SetLayoutHorizontal();
        }
        
        private void OnChange(CollectionRemoveEvent<CardEntity> collectionAddEvent)
        {
        }
        
        //
        // private void OnDisable()
        // {
        //     // _receiver.OnCardGenerated -= HandleIncrease;
        // }
    }
}