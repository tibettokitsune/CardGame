using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public class CardEntity
    {
        private static readonly IReadOnlyList<StatModifier> EmptyStatModifiers = Array.Empty<StatModifier>();

        protected readonly IBaseCard Card;

        public string ID => Card.ID;
        public CardKind Kind => Card.Kind;
        public string Name => Card.Name;
        public string Description => Card.Description;
        public string MainLayer => Card.MainLayer;
        public string BackgroundLayer => Card.BackgroundLayer;
        public IReadOnlyList<StatModifier> StatModifiers => Card.StatModifiers ?? EmptyStatModifiers;

        public CardEntity(IBaseCard card)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
        }
    }

    public class EquipmentCardEntity : CardEntity
    {
        private static readonly IReadOnlyList<AppearanceOverride> EmptyAppearanceOverrides = Array.Empty<AppearanceOverride>();

        public EquipmentSlot Slot => Card.Equipment?.Slot ?? EquipmentSlot.None;
        public string EquipmentDescription => Card.Equipment?.Description ?? FormatStatDescription();
        public IReadOnlyList<AppearanceOverride> Overrides { get; }

        public EquipmentCardEntity(IBaseCard card) : base(card)
        {
            Overrides = Card.Equipment?.Overrides ?? EmptyAppearanceOverrides;
        }

        private string FormatStatDescription()
        {
            if (StatModifiers.Count == 0)
                return string.Empty;

            return string.Join("\n", StatModifiers.Select(modifier =>
            {
                var formattedValue = modifier.Value >= 0 ? $"+{modifier.Value:G}" : modifier.Value.ToString("G");
                return $"{formattedValue} {modifier.Stat}";
            }));
        }
    }

    public class DoorCardEntity : CardEntity
    {
        public DoorCardEntity(IBaseCard card) : base(card)
        {
        }
    }
}
