using Game.Scripts.UI;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class GameManager : IInitializable
    {
        private string[] _currentScenes = { "Menu" };

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
            await LoadSceneProcess(_currentScenes[0]);
        }


        public async void LoadScenes(string[] scenes)
        {

            foreach (var scene in _currentScenes)
                await UnLoadSceneProcess(scene);

            foreach (var scene in scenes)
                await LoadSceneProcess(scene);

            _currentScenes = scenes;
        }

        public void SelectActiveScene(string scene) => _sceneManagementService.SelectScene(scene);

        private async Task UnLoadSceneProcess(string scene)
        {
            var loading = _sceneManagementService.UnloadScene(scene);
            await _loadingScreen.Loading(loading);
        }

        private async Task LoadSceneProcess(string scene)
        {
            var loading = _sceneManagementService.LoadScene(scene);
            await _loadingScreen.Loading(loading);
        }


    }
}
