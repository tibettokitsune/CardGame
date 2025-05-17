using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer
{
    public enum CardType
    {
        Door,
        Treasure
    }
    public interface IBaseCard
    {
        string Name { get; }
        string Description { get; }
        public CardLayerDataConfig MainLayer { get; }
        public CardLayerDataConfig BackgroundLayer { get; }
    }
}