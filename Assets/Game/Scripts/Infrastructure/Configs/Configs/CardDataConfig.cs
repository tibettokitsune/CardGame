using System;
using System.Collections.Generic;
using System.Numerics;

namespace Game.Scripts.Infrastructure.Configs.Configs
{
    [Serializable]
    public class SublayerData
    {
        public string SpritePath { get; set; }
        public Vector3 Offset { get; set; }
        public Vector3 Scale { get; set; }
        public Vector3 Rotation { get; set; }
    }

    [Serializable]
    public class CardLayerDataConfig : BaseConfig
    {
        public List<SublayerData> Layers { get; set; }
    }

    [Serializable]
    public class StatModifier
    {
        public PlayerStat Stat { get; set; }
        public float Value { get; set; }
    }

    [Serializable]
    public class AppearanceOverride
    {
        public string Item { get; set; }
        public int Index { get; set; }
    }

    [Serializable]
    public class EquipmentConfig
    {
        public EquipmentSlot Slot { get; set; } = EquipmentSlot.None;
        public string Description { get; set; }
        public List<AppearanceOverride> Overrides { get; set; } = new();
    }

    [Serializable]
    public class CardDataConfig : BaseConfig
    {
        public CardKind Kind { get; set; }
        public string MainLayerId { get; set; }
        public string BackgroundLayerId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    [Serializable]
    public class CardWithStatModifiersConfig : CardDataConfig
    {
        public List<StatModifier> StatModifiers { get; set; } = new();
    }

    [Serializable]
    public class TreasureCardConfig : CardWithStatModifiersConfig
    {
        public EquipmentConfig Equipment { get; set; } = new();
    }

    [Serializable]
    public class DoorCardConfig : CardWithStatModifiersConfig
    {
    }

    [Serializable]
    public class EventCardConfig : CardWithStatModifiersConfig
    {
        public bool IsPersistent { get; set; }
    }
}
