using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UniRx;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IPlayerDataProvider
    {
        ReactiveCollection<string> PlayersHand { get; }
        ReactiveCollection<string> PlayersEquipment { get; }
        ReactiveDictionary<PlayerStat, float> PlayersStats { get; }
        Task ClaimCard(string cardId);
        bool IsPlayerCanEquipCard(string cardId);
        Task EquipCard(string cardId);
    }

    public class PlayerDataProvider : IPlayerDataProvider
    {
        public ReactiveCollection<string> PlayersHand { get; } = new();
        public ReactiveCollection<string> PlayersEquipment { get; } = new();
        public ReactiveDictionary<PlayerStat, float> PlayersStats { get; private set; }

        private readonly IConfigService _configService;

        private readonly Dictionary<PlayerStat, float> _defaultStats;

        public PlayerDataProvider(IConfigService configService)
        {
            _configService = configService;
            _defaultStats = new Dictionary<PlayerStat, float>()
            {
                {PlayerStat.Health, 100f},
                {PlayerStat.Attack, 1f},
                {PlayerStat.Defend, 0f},
                {PlayerStat.Luck, 10f},
                {PlayerStat.Agility, 5f},
                {PlayerStat.Intelligence, 5f},
                {PlayerStat.Strength, 5f}
            };
            PlayersStats = new ReactiveDictionary<PlayerStat, float>(_defaultStats);
        }

        public Task ClaimCard(string cardId)
        {
            PlayersHand.Add(cardId);
            return Task.CompletedTask;
        }

        public bool IsPlayerCanEquipCard(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                return false;

            var card = _configService.Get<CardDataConfig>(cardId) as TreasureCardConfig;
            if (card == null)
                return false;

            var equipment = card.Equipment;
            return equipment != null && equipment.Slot != EquipmentSlot.None;
        }

        public Task EquipCard(string cardId)
        {
            var card = _configService.Get<CardDataConfig>(cardId);
            if (card is not TreasureCardConfig treasureCard)
                return Task.CompletedTask;

            RemoveOccupiedSlot(treasureCard);
            EquipSlot(treasureCard);
            return Task.CompletedTask;
        }

        private void RemoveOccupiedSlot(TreasureCardConfig card)
        {
            if (card?.Equipment == null || card.Equipment.Slot == EquipmentSlot.None)
                return;

            var equipmentToReplace = PlayersEquipment
                .Select(eq => _configService.Get<CardDataConfig>(eq) as TreasureCardConfig)
                .FirstOrDefault(eqConfig => eqConfig?.Equipment != null &&
                                            eqConfig.Equipment.Slot == card.Equipment.Slot);

            if (equipmentToReplace != null)
                TakeOffEquip(equipmentToReplace);
        }

        private void TakeOffEquip(TreasureCardConfig eqCard)
        {
            PlayersEquipment.Remove(eqCard.Id);
            PlayersHand.Add(eqCard.Id);

            foreach (var modifier in eqCard.StatModifiers ?? Enumerable.Empty<StatModifier>())
                PlayersStats[modifier.Stat] -= modifier.Value;
        }

        private void EquipSlot(TreasureCardConfig card)
        {
            PlayersEquipment.Add(card.Id);
            var index = PlayersHand.IndexOf(PlayersHand.First(x => x.Equals(card.Id)));
            PlayersHand.RemoveAt(index);

            foreach (var modifier in card.StatModifiers ?? Enumerable.Empty<StatModifier>())
                PlayersStats[modifier.Stat] += modifier.Value;
        }
    }
}
