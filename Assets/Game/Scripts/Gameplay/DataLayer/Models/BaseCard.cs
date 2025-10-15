using System;
using System.Collections.Generic;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public class BaseCard : IBaseCard
    {
        private readonly CardDataConfig _config;

        public string ID => _config.Id;
        public CardKind Kind => _config.Kind;
        public string Name => _config.Name;
        public string Description => _config.Description;
        public string MainLayer => _config.MainLayerId;
        public string BackgroundLayer => _config.BackgroundLayerId;
        public EquipmentConfig Equipment => _config.Equipment;
        public IReadOnlyList<StatModifier> StatModifiers { get; }

        public BaseCard(CardDataConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            StatModifiers = _config.StatModifiers ?? new List<StatModifier>();
        }
    }
}
