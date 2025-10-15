using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public interface IBaseCard
    {
        string ID { get; }
        CardKind Kind { get; }
        string Name { get; }
        string Description { get; }
        string MainLayer { get; }
        string BackgroundLayer { get; }
        EquipmentConfig Equipment { get; }
        IReadOnlyList<StatModifier> StatModifiers { get; }
    }
}
