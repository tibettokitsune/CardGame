using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Game.Scripts.Infrastructure.SceneManagment
{
    public enum SceneLayer
    {
        GameStage
    }
    public interface ISceneManagerService
    {
        Task LoadScene(string sceneName, SceneLayer layer, bool isActivateAfterLoad = false);
    }
    public class SceneManagerService : IAsyncInitializable, ISceneManagerService
    {
        private Dictionary<SceneLayer, string> _scenes = new();
        [Inject] private LoadingScreen _loadingScreen;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log($"Initialize SceneManagerService start");
            Debug.Log($"Initialize SceneManagerService finished");
        }

        public async Task LoadScene(string sceneName, SceneLayer layer, bool isActivateAfterLoad = false)
        {
            _scenes.TryGetValue(layer, out var sceneToUnload);
            using var loadingScreen = _loadingScreen.Show();
            if (!string.IsNullOrEmpty(sceneToUnload))
            {
                await  SceneManager.UnloadSceneAsync(sceneToUnload);
                _scenes.Remove(layer);
            }
            await  SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if(isActivateAfterLoad)
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            _scenes.Add(layer, sceneName);
        }
    }
}