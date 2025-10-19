using UniRx;

namespace Game.Scripts.Gameplay.DataLayer
{
    public enum LobbyState
    {
        PrepareToRound,
        TakeEventCard,
        Battle
    }

    public interface ILobbyDataProvider
    {
        ReactiveProperty<LobbyState> LobbyState { get; }
    }

    public class LobbyDataProvider : ILobbyDataProvider
    {
        public ReactiveProperty<LobbyState> LobbyState { get; } = new ReactiveProperty<LobbyState>();

        public LobbyDataProvider()
        {
        }
    }
}
