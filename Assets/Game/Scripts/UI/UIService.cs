using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;
using Zenject;

namespace Game.Scripts.UI
{
    public class UIService : IUIService<UIScreen>, IAsyncInitializable
    {
        [Inject] private IConfigService<BaseConfig> configService;
        [Inject] private UIScreen.Factory screensFactory;
        private readonly Dictionary<string, UIScreen> _loadedScreens = new();
        private readonly Transform _uiRoot;

        private readonly Dictionary<UILayer, List<UIScreen>> _activeScreens = new();
        private readonly List<string> _history = new();

        public UIService(Transform uiRoot)
        {
            _uiRoot = uiRoot;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("Initializing UI start");
            Debug.Log("Initializing UI finished");
        }

        public async Task<TC> ShowScreen<TC>(string id) where TC : UIScreen
        {
            var config = configService.Get<UIDataConfig>(id);

            if (!_loadedScreens.TryGetValue(id, out var screen))
            {
                var loadedRes = await Resources.LoadAsync<TC>(config.PrefabPath) as TC;
                screen = screensFactory.Create(loadedRes);
            }
            else
            {
                screen = screensFactory.Create(screen);
            }

            screen.transform.SetParent(_uiRoot);
            screen.gameObject.SetActive(true);
            if (!_activeScreens.ContainsKey(config.Layer))
                _activeScreens.Add(config.Layer, new List<UIScreen>());
            _activeScreens[config.Layer].Add(screen);
            _history.Add(id);

            return (TC) screen;
        }

        public Task Clear()
        {
            foreach (var (layer, screens) in _activeScreens)
            {
                foreach (var screen in screens)
                {
                    GameObject.Destroy(screen.gameObject);
                }
            }

            _activeScreens.Clear();
            _history.Clear();
            return Task.CompletedTask;
        }
    }
}