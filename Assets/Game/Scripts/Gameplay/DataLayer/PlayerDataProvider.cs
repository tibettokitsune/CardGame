using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UniRx;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    internal static class DataParser
    {
        public static List<(PlayerStat, float)> ParseStats(string input)
        {
            var result = new List<(PlayerStat, float)>();
            var statPairs = input.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in statPairs)
            {
                var parts = pair.Split(',', StringSplitOptions.RemoveEmptyEntries);
                string typePart = null;
                string valuePart = null;

                foreach (var part in parts)
                {
                    var kv = part.Split('=');
                    if (kv.Length != 2)
                        continue;

                    if (kv[0].Trim().Equals("type", StringComparison.OrdinalIgnoreCase))
                        typePart = kv[1].Trim();

                    else if (kv[0].Trim().Equals("Value", StringComparison.OrdinalIgnoreCase))
                        valuePart = kv[1].Trim();
                }

                if (Enum.TryParse<PlayerStat>(typePart, out var stat) && float.TryParse(valuePart, out var value))
                {
                    result.Add((stat, value));
                }
                else
                {
                    throw new FormatException($"Invalid stat or value in: '{pair}'");
                }
            }

            return result;
        }
    }

    [Serializable]
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

    public interface IPlayerDataProvider
    {
        public ReactiveCollection<string> PlayersHand { get; }
        public ReactiveCollection<string> PlayersEquipment { get; }
        public ReactiveDictionary<PlayerStat, float> PlayersStats { get; }
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
            return true;
        }

        public Task EquipCard(string cardId)
        {
            var card = _configService.Get<CardDataConfig>(cardId);
            RemoveOccupiedSlot(card);
            EquipSlot(card);
            return Task.CompletedTask;
        }
        
        private void RemoveOccupiedSlot(CardDataConfig card)
        {
            if (!card.MetaDataDictionary.TryGetValue(MetaDataKeys.Equipment, out var equipmentSlot))
                return;
            foreach (var eq in PlayersEquipment)
            {
                var eqCard = _configService.Get<CardDataConfig>(eq);
                var equippedSlot = eqCard.MetaDataDictionary[MetaDataKeys.Equipment];
                if (!equippedSlot.Equals(equipmentSlot)) continue;
                TakeOffEquip(eqCard);
                return;
            }
        }

        private void TakeOffEquip(CardDataConfig eqCard)
        {
            PlayersEquipment.Remove(eqCard.Id);
            PlayersHand.Add(eqCard.Id);
            
            
            if (!eqCard.MetaDataDictionary.TryGetValue(MetaDataKeys.Stats, out var statsData))
                return;
            
            var stats = DataParser.ParseStats(statsData);
            
            foreach (var (stat, value) in stats)
            {
                PlayersStats[stat] -= value;
            }
        }

        private void EquipSlot(CardDataConfig card)
        {
            PlayersEquipment.Add(card.Id);
            var index = PlayersHand.IndexOf(PlayersHand.First(x => x.Equals(card.Id)));
            PlayersHand.RemoveAt(index);
            
            if (!card.MetaDataDictionary.TryGetValue(MetaDataKeys.Stats, out var statsData))
                return;
            
            var stats = DataParser.ParseStats(statsData);
            
            foreach (var (stat, value) in stats)
            {
                PlayersStats[stat] += value;
            }
        }
    }
}