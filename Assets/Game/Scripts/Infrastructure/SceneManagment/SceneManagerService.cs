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
    public class SceneManagerService : IAsyncInitializable, ISceneManagerService
    {
        private Dictionary<SceneLayer, HashSet<string>> _scenes = new();
        [Inject] private LoadingScreen _loadingScreen;

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log($"Initialize SceneManagerService start");
            Debug.Log($"Initialize SceneManagerService finished");
        }

        public async Task LoadScene(string sceneName, SceneLayer layer, bool isActivateAfterLoad = false)
        {
            if (!_scenes.ContainsKey(layer))
            {
                _scenes.Add(layer, new HashSet<string>() {});
            }

            _scenes.TryGetValue(layer, out var layerScenes);
            using var loadingScreen = _loadingScreen.Show();
            if (layerScenes.Contains(sceneName))
            {
                await SceneManager.UnloadSceneAsync(sceneName);
                _scenes.Remove(layer);
            }

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (isActivateAfterLoad)
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));


            if (!_scenes[layer].Contains(sceneName))
                _scenes[layer].Add(sceneName);
        }

        public async Task UnloadScene(string sceneName, SceneLayer layer)
        {
            await SceneManager.UnloadSceneAsync(sceneName);
            _scenes[layer].Remove(sceneName);
        }
    }
}