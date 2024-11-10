using System;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class LobbyManager : IInitializable
    {
        [Inject] private IGameStateDataProvider _gameStateDataProvider;
        
        public void Initialize()
        {
            
        }
    }
}