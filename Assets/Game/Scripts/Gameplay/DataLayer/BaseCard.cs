using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer
{
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string Name => _config.Name;
        public string Description => _config.Description;
        public CardLayerDataConfig MainLayer { get; private set; }
        public CardLayerDataConfig BackgroundLayer { get; private set; }
        public CardType CardType { get; }

        public BaseCard(CardDataConfig config, 
            CardLayerDataConfig mainLayer, 
            CardLayerDataConfig backgroundLayer,
            CardType cardType)
        {
            _config = config;
            CardType = cardType;
            MainLayer = mainLayer;
            BackgroundLayer = backgroundLayer;
        }
    }
}