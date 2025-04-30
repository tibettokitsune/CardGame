using System;
using Game.Scripts.Gameplay.Lobby.Deck;

namespace Game.Scripts.Infrastructure
{
    public class GameplayEventBus : IGameplayNotifier, IGameplayReceiver
    {
        public event Action<IBaseCard, HandCardView> OnCardGenerated;

        public void NotifyAddCardToHand(IBaseCard card, HandCardView handCardView) =>
            OnCardGenerated?.Invoke(card, handCardView);
    }

    public interface IGameplayNotifier
    {
        void NotifyAddCardToHand(IBaseCard card, HandCardView handCardView);
    }
    
    public interface IGameplayReceiver
    {
        event Action<IBaseCard, HandCardView> OnCardGenerated;
    }
}