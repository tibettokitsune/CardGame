using UniRx;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public enum GameState
    {
        MainMenu, WaitingForPlayers, Game
    }
    public interface IGameStateDataProvider
    {
        public ReactiveProperty<GameState> GameState { get; set; }
    }
    public class GameStateDataProvider : IGameStateDataProvider, IInitializable
    {
        public ReactiveProperty<GameState> GameState { get;  set; } = new();
        public void Initialize()
        {
            
        }
    }
}