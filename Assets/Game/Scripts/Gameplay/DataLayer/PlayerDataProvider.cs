using System.Collections.Generic;
using System.Threading.Tasks;
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
            ProcessEquipment(cardId);
            PlayersEquipment.Add(cardId);
            PlayersHand.RemoveAt(PlayersHand.IndexOf(cardId));
            return Task.CompletedTask;
        }

        private void ProcessEquipment(string cardId)
        {
            throw new System.NotImplementedException();
        }
    }
}