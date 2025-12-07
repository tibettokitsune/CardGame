namespace Game.Scripts.Infrastructure.Configs.Configs
{
    public enum CardKind
    {
        Treasure,
        Door,
        Event
    }

    public enum EquipmentSlot
    {
        None = 0,
        Head,
        Chest,
        Hands,
        Legs
    }

    public enum PlayerStat
    {
        Health,
        Attack,
        Defend,
        Agility,
        Strength,
        Intelligence,
        Luck
    }

    public static class CardTypeIds
    {
        public const string Equipment = "equipment";
        public const string Effect = "effect";
        public const string Monster = "monster";
        public const string Event = "event";

        // Compatibility fallbacks for existing deck separation
        public const string Treasure = "treasure";
        public const string Door = "door";
    }
}
