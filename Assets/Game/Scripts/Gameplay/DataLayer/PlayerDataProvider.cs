using System.Collections.Generic;
using System.Threading.Tasks;
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
            PlayersEquipment.Add(cardId);
            PlayersHand.RemoveAt(PlayersHand.IndexOf(cardId));
            return Task.CompletedTask;
        }
    }
}