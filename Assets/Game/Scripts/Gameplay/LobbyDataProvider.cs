using System.Collections.Generic;
using UniRx;

namespace Game.Scripts.Gameplay
{
    public enum LobbyState
    {
        WaitingForPlayers,
        Prepare,
        GameplayEvents
    }

    public enum PlayerState
    {
        WaitingForPlayers,
        Prepare,
        Fight,
    }
    public class LobbyDataProvider : ILobbyDataProvider
    {
        public ReactiveProperty<LobbyState> LobbyState { get; } = new();
        public ReactiveProperty<PlayerState> CurrentPlayerState { get; } = new();
        public string CurrentPlayerId { get; }
        public List<PlayerModel> JoinedPlayers { get; } = new();
    }

    public interface ILobbyDataProvider
    {
        public ReactiveProperty<LobbyState> LobbyState { get; }
        public ReactiveProperty<PlayerState> CurrentPlayerState { get; }
        string CurrentPlayerId { get; }
        List<PlayerModel> JoinedPlayers { get; }
    }
}