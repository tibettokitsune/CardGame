using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UniRx;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IPlayerDataProvider
    {
        public ReactiveCollection<string> PlayersHand { get; }
        public ReactiveCollection<string> PlayersEquipment { get; }
        Task ClaimCard(string cardId);
        bool IsPlayerCanEquipCard(string cardId);
        Task EquipCard(string cardId);
    }

    public class PlayerDataProvider : IPlayerDataProvider
    {
        private readonly IConfigService _configService;

        public PlayerDataProvider(IConfigService configService)
        {
            _configService = configService;
        }

        public ReactiveCollection<string> PlayersHand { get; } = new();
        public ReactiveCollection<string> PlayersEquipment { get; } = new();

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
            EquipSlot(cardId);
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
                PlayersEquipment.Remove(eqCard.Id);
                PlayersHand.Add(eqCard.Id);
                return;
            }
        }

        private void EquipSlot(string cardId)
        {
            PlayersEquipment.Add(cardId);
            var index = PlayersHand.IndexOf(PlayersHand.First(x => x.Equals(cardId)));
            PlayersHand.RemoveAt(index);
        }
    }
}