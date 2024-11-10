using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class GameManager : IGameManager , IInitializable
    {
        private readonly ISceneManagementService _sceneManagementService;
        private readonly IGameStateDataProvider _dataProvider;
        private readonly LoadingScreen _loadingScreen;

        public GameManager(ISceneManagementService sceneManagementService, 
            IGameStateDataProvider dataProvider,
            LoadingScreen loadingScreen)
        {
            _sceneManagementService = sceneManagementService;
            _dataProvider = dataProvider;
            _loadingScreen = loadingScreen;
        }

        public async void Initialize()
        {
            Debug.Log("Init");

            GoToMenu();
        }

        public async void  GoToMenu()
        {
            var op = _sceneManagementService.LoadScene("Menu");
            await _loadingScreen.Loading(op);
            _dataProvider.GameState.Value = GameState.MainMenu;
        }

        public async void GoToGame()
        {
            var op = _sceneManagementService.UnloadScene("Menu");
            await _loadingScreen.Loading(op);
            op = _sceneManagementService.LoadScene("Game");
            await _loadingScreen.Loading(op);
            _dataProvider.GameState.Value = GameState.Game;
        }
    }

    public interface IGameManager
    {

        void GoToMenu();

        void GoToGame();
    }
}
