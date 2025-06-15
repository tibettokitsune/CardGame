using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public interface IPlayerDataProvider
    {
        public ReactiveCollection<string> PlayersHand { get; }
        Task ClaimCard(string cardId);
        bool IsPlayerCanEquipCard(string cardId);
        Task EquipCard(string cardId);
    }

    public class PlayerDataProvider : IPlayerDataProvider
    {
        public ReactiveCollection<string> PlayersHand { get; } = new();
        public ReactiveCollection<string> Equipment { get; } = new();

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
            Equipment.Add(cardId);
            return Task.CompletedTask;
        }
    }
}