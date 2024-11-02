using Game.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class GameManager : IInitializable
    {
        private readonly ISceneManagementService _sceneManagementService;
        private readonly LoadingScreen _loadingScreen;
        private readonly SceneConfig _sceneConfig;

        public GameManager(ISceneManagementService sceneManagementService,
            LoadingScreen loadingScreen,
            SceneConfig sceneConfig)
        {
            _sceneManagementService = sceneManagementService;
            _loadingScreen = loadingScreen;
            _sceneConfig = sceneConfig;
        }

        public async void Initialize()
        {
            Debug.Log("Init");
            var loading = _sceneManagementService.LoadScene("Menu");
            await _loadingScreen.Loading(loading);
        }
    }
}
