using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Infrastructure;
using UnityEngine;
using Zenject;

namespace Game.Scripts.UI
{
    public class PlayerHandScreen : UIScreen
    {
        [Inject] private IGameplayReceiver _receiver;
        [SerializeField] public Transform cardsContainer;
        [SerializeField] public Transform fromPos;
        [SerializeField] public HandLayoutGroup group;

        private void OnEnable()
        {
            _receiver.OnCardGenerated += HandleIncrease;
        }

        private async void HandleIncrease(IBaseCard card, HandCardView view)
        {
            view.transform.SetParent(cardsContainer.parent);
            view.transform.position = fromPos.position;
            
            var rt = (RectTransform) view.transform;
            var tween = rt.DOMove(cardsContainer.position, .5f).SetEase(Ease.OutSine);
            await tween.AsyncWaitForCompletion();
            view.transform.SetParent(cardsContainer);
            group.SetLayoutHorizontal();
            view.EnableGrouping();
        }

        private void OnDisable()
        {
            _receiver.OnCardGenerated -= HandleIncrease;
        }
    }
}