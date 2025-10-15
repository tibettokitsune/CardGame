using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;
        private readonly CardWithStatModifiersConfig _statsConfig;
        private readonly TreasureCardConfig _treasureConfig;
        private readonly IReadOnlyList<StatModifier> _statModifiers;

        public string ID => _config.Id;
        public CardKind Kind => _config.Kind;
        public string Name => _config.Name;
        public string Description => _config.Description;
        public string MainLayer => _config.MainLayerId;
        public string BackgroundLayer => _config.BackgroundLayerId;
        public EquipmentConfig Equipment => _treasureConfig?.Equipment;
        public IReadOnlyList<StatModifier> StatModifiers => _statModifiers;

        public BaseCard(CardDataConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _statsConfig = config as CardWithStatModifiersConfig;
            _treasureConfig = config as TreasureCardConfig;

            if (_treasureConfig != null)
            {
                _treasureConfig.Equipment ??= new EquipmentConfig();
                _treasureConfig.Equipment.Overrides ??= new List<AppearanceOverride>();
            }

            if (_statsConfig != null && _statsConfig.StatModifiers == null)
            {
                _statsConfig.StatModifiers = new List<StatModifier>();
            }

            _statModifiers = _statsConfig != null
                ? (IReadOnlyList<StatModifier>)_statsConfig.StatModifiers
                : Array.Empty<StatModifier>();
        }
    }
}
