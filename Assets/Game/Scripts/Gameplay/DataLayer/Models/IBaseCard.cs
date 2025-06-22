using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public enum CardType
    {
        Door,
        Treasure
    }
    public interface IBaseCard
    {
        string ID { get; }
        string Name { get; }
        string Description { get; }
        public string MainLayer { get; }
        public string BackgroundLayer { get; }
        public Dictionary<string, string> MetaData { get; }
    }
}