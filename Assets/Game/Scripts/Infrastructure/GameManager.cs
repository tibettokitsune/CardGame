using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class GameManager : IInitializable
    {
        private readonly ISceneManagementService _sceneManagementService;
        private readonly LoadingScreen _loadingScreen;

        public GameManager(ISceneManagementService sceneManagementService, 
            LoadingScreen loadingScreen)
        {
            _sceneManagementService = sceneManagementService;
            _loadingScreen = loadingScreen;
        }

        public async void Initialize()
        {
            Debug.Log("Init");

            var loading = _sceneManagementService.LoadScene("Menu");
            await _loadingScreen.Loading(loading);
        }

    }
}
