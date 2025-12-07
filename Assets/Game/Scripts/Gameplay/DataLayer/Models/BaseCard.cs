using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string ID => _config.Id;
        public string TypeId { get; }
        public CardKind Kind => _config.Kind;
        public string Name => _config.Name;
        public string Description => _config.Description;
        public string MainLayer => _config.MainLayerId;
        public string BackgroundLayer => _config.BackgroundLayerId;

        public BaseCard(CardDataConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            TypeId = ResolveTypeId(config);
        }

        public static string ResolveTypeId(CardDataConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (!string.IsNullOrWhiteSpace(config.CardTypeId))
                return config.CardTypeId;

            if (!string.IsNullOrWhiteSpace(config.ConfigType))
                return config.ConfigType;

            return config.GetType().Name;
        }
    }

    public class CardWithStatModifiers : BaseCard, ICardWithStatModifiers
    {
        private readonly IReadOnlyList<StatModifier> _statModifiers;

        public IReadOnlyList<StatModifier> StatModifiers => _statModifiers;

        public CardWithStatModifiers(CardWithStatModifiersConfig config) : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.StatModifiers ??= new List<StatModifier>();
            _statModifiers = config.StatModifiers;
        }
    }

    public class EquipmentCard : CardWithStatModifiers, IEquipmentCard
    {
        public EquipmentConfig Equipment { get; }

        public EquipmentCard(TreasureCardConfig config) : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.Equipment ??= new EquipmentConfig();
            config.Equipment.Overrides ??= new List<AppearanceOverride>();
            Equipment = config.Equipment;
        }
    }

    public class EventCard : BaseCard
    {
        public EventCard(EventCardConfig config) : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
        }
    }

    public class MonsterCard : BaseCard, IMonsterCard
    {
        public MonsterParameters Parameters { get; }
        public string ViewId { get; }

        public MonsterCard(MonsterCardConfig config) : base(config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            Parameters = config.Parameters ?? new MonsterParameters();
            ViewId = config.ViewId;
        }
    }
}
