using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.Gameplay.DataLayer.Models
{
    public static class CardTypeUtils
    {
        private const string TreasureConfigType = "treasurecardconfig";
        private const string DoorConfigType = "doorcardconfig";
        private const string EventConfigType = "eventcardconfig";

        public static string Normalize(string typeId)
        {
            return string.IsNullOrWhiteSpace(typeId)
                ? string.Empty
                : typeId.Trim().ToLowerInvariant();
        }

        public static bool IsEquipment(string typeId)
        {
            var normalized = Normalize(typeId);
            return normalized is CardTypeIds.Equipment or CardTypeIds.Treasure or TreasureConfigType;
        }

        public static bool IsEvent(string typeId)
        {
            var normalized = Normalize(typeId);
            return normalized is CardTypeIds.Event or EventConfigType;
        }

        public static bool IsDoor(string typeId)
        {
            var normalized = Normalize(typeId);
            return normalized is CardTypeIds.Door or DoorConfigType;
        }
    }
}
