using Game.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

namespace Game.Scripts.Infrastructure
{
    public interface ISceneManagementService
    {
        AsyncOperation LoadScene(string scene);
        AsyncOperation UnloadScene(string scene);
        void LoadScenes(string[] scenes);
        void SelectActiveScene(string scene);
    }

    public class SceneManagementService : ISceneManagementService
    {
        private string[] _currentScenes = { "Menu" };

        private readonly LoadingScreen _loadingScreen;
        public SceneManagementService(LoadingScreen loadingScreen)
        {
            _loadingScreen = loadingScreen;
        }

        public AsyncOperation LoadScene(string scene)
        {
            return SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }

        public AsyncOperation UnloadScene(string scene)
        {
            return SceneManager.UnloadSceneAsync(scene);
        }

        public void SelectActiveScene(string scene)
        {
            Scene targetScene = SceneManager.GetSceneByName(scene);

            if (targetScene.IsValid() && targetScene.isLoaded)
                SceneManager.SetActiveScene(targetScene);
            else
                Debug.LogWarning($"The scene {scene} is not loaded or is invalid");
        }

        public async void LoadScenes(string[] scenes)
        {

            foreach (var scene in _currentScenes)
                await UnLoadSceneProcess(scene);

            foreach (var scene in scenes)
                await LoadSceneProcess(scene);

            _currentScenes = scenes;
        }

        private async Task UnLoadSceneProcess(string scene)
        {
            var loading = UnloadScene(scene);
            await _loadingScreen.Loading(loading);
        }

        private async Task LoadSceneProcess(string scene)
        {
            var loading = LoadScene(scene);
            await _loadingScreen.Loading(loading);
        }
    }
}