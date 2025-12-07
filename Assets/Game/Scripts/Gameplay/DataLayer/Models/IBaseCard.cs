using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public interface IBaseCard
    {
        string ID { get; }
        string TypeId { get; }
        CardKind Kind { get; }
        string Name { get; }
        string Description { get; }
        string MainLayer { get; }
        string BackgroundLayer { get; }
    }

    public interface ICardWithStatModifiers : IBaseCard
    {
        IReadOnlyList<StatModifier> StatModifiers { get; }
    }

    public interface IEquipmentCard : ICardWithStatModifiers
    {
        EquipmentConfig Equipment { get; }
    }

    public interface IMonsterCard : IBaseCard
    {
        MonsterParameters Parameters { get; }
        string ViewId { get; }
    }
}
