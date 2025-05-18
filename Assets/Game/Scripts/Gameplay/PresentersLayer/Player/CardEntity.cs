using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class CardEntity
    {
        private IBaseCard _card;

        public string ID => _card.ID;
        public string Name => _card.Name;
        public string Description => _card.Description;
        public CardLayerDataConfig MainLayer => _card.MainLayer;
        public CardLayerDataConfig BackgroundLayer => _card.BackgroundLayer;

        public CardEntity(IBaseCard card)
        {
            _card = card;
        }
    }
}