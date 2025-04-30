using Game.Scripts.UI;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby.Deck
{
    public class HandCardFactory : PlaceholderFactory<HandCardView>
    {
        private readonly DiContainer _container;
        private readonly HandCardView _handCardPrefab;

        public HandCardFactory(DiContainer container, HandCardView handCardPrefab)
        {
            _container = container;
            _handCardPrefab = handCardPrefab;
        }

        public HandCardView Create()
        {
            return _container.InstantiatePrefabForComponent<HandCardView>(_handCardPrefab);
        }
    }
}