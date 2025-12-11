using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.UIContracts
{
    public class CardViewData
    {
        private static readonly IReadOnlyList<StatModifier> EmptyStatModifiers = Array.Empty<StatModifier>();

        protected readonly IBaseCard Card;
        private readonly ICardWithStatModifiers _cardWithStatModifiers;

        public string Id => Card.ID;
        public CardKind Kind => Card.Kind;
        public string Name => Card.Name;
        public string Description => Card.Description;
        public string MainLayer => Card.MainLayer;
        public string BackgroundLayer => Card.BackgroundLayer;
        public string TypeId => Card.TypeId;
        public IReadOnlyList<StatModifier> StatModifiers => _cardWithStatModifiers?.StatModifiers ?? EmptyStatModifiers;

        public CardViewData(IBaseCard card)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            _cardWithStatModifiers = card as ICardWithStatModifiers;
        }
    }

    public class EquipmentCardViewData : CardViewData
    {
        private static readonly IReadOnlyList<AppearanceOverride> EmptyAppearanceOverrides = Array.Empty<AppearanceOverride>();

        private readonly IEquipmentCard _equipmentCard;

        public EquipmentSlot Slot => _equipmentCard?.Equipment?.Slot ?? EquipmentSlot.None;
        public string EquipmentDescription => _equipmentCard?.Equipment?.Description ?? FormatStatDescription();
        public IReadOnlyList<AppearanceOverride> Overrides { get; }

        public EquipmentCardViewData(IBaseCard card) : base(card)
        {
            _equipmentCard = card as IEquipmentCard
                             ?? throw new ArgumentException("Card must contain equipment data", nameof(card));
            Overrides = _equipmentCard.Equipment?.Overrides ?? EmptyAppearanceOverrides;
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

    public class DoorCardViewData : CardViewData
    {
        public DoorCardViewData(IBaseCard card) : base(card)
        {
        }
    }

    public class MonsterCardViewData : DoorCardViewData
    {
        private readonly IMonsterCard _monsterCard;

        public MonsterParameters Parameters => _monsterCard.Parameters;
        public string ViewId => _monsterCard.ViewId;

        public MonsterCardViewData(IBaseCard card) : base(card)
        {
            _monsterCard = card as IMonsterCard
                           ?? throw new ArgumentException("Card must contain monster data", nameof(card));
        }
    }
}
