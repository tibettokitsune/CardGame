using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Scripts.Infrastructure.UI
{
    public class UIService : IUIService, IAsyncInitializable
    {
        [Inject] private IConfigService _configService;
        [Inject] private UIScreenFactory _screensFactory;
        [Inject] private IUIScreenPrefabProvider _prefabProvider;

        private readonly Dictionary<string, GameObject> _loadedPrefabs = new();
        private readonly Dictionary<UILayer, List<UIScreen>> _activeScreens = new();
        private readonly Dictionary<UIScreen, UILayer> _screenLayers = new();
        private readonly List<UIScreen> _history = new();
        private readonly Dictionary<UILayer, Transform> _layerRoots = new();
        private readonly Transform _uiRoot;

        public UIService(Transform uiRoot)
        {
            _uiRoot = uiRoot;
            InitializeLayerRoots();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
        }

        public async UniTask<TScreen> ShowAsync<TScreen>(CancellationToken cancellationToken = default)
            where TScreen : class
        {
            var metadata = UIScreenMetadataCache.For<TScreen>();
            var config = ResolveConfig(metadata.ConfigId, metadata.ScreenType);

            if (config.HideOtherScreensOnLayer)
            {
                await HideLayerAsync(config.Layer, cancellationToken);
            }

            var prefab = await GetOrLoadPrefabAsync(metadata.ConfigId, config, cancellationToken);
            var parent = GetLayerRoot(config.Layer);
            var screen = _screensFactory.Create(prefab, parent, metadata.ScreenType);
            screen.name = $"{metadata.ConfigId}_{metadata.ScreenType.Name}";

            await screen.ShowAsync();

            RegisterScreen(config.Layer, screen);
            if (screen is not TScreen typedScreen)
            {
                throw new InvalidOperationException(
                    $"Screen instance of type {metadata.ScreenType.FullName} does not implement {typeof(TScreen).FullName}.");
            }

            return typedScreen;
        }

        public async UniTask HideAsync<TScreen>(CancellationToken cancellationToken = default)
            where TScreen : class
        {
            var instance = FindInstance(typeof(TScreen));
            if (instance != null)
            {
                await HideAsync(instance, cancellationToken);
            }
        }

        public async Task HideAsync(UIScreen screen, CancellationToken cancellationToken = default)
        {
            if (screen == null)
                return;

            if (!_screenLayers.TryGetValue(screen, out var layer))
                return;

            await screen.HideAsync();

            if (_activeScreens.TryGetValue(layer, out var screens))
            {
                screens.Remove(screen);
                if (screens.Count == 0)
                {
                    _activeScreens.Remove(layer);
                }
            }

            _screenLayers.Remove(screen);
            _history.Remove(screen);
            Object.Destroy(screen.gameObject);
        }

        public async Task HideLayerAsync(UILayer layer, CancellationToken cancellationToken = default)
        {
            if (!_activeScreens.TryGetValue(layer, out var screens) || screens.Count == 0)
                return;

            var copy = screens.ToArray();
            foreach (var screen in copy)
            {
                await HideAsync(screen, cancellationToken);
            }
        }

        public async Task HideTopAsync(CancellationToken cancellationToken = default)
        {
            if (_history.Count == 0)
                return;

            var screen = _history[^1];
            await HideAsync(screen, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            if (_history.Count == 0)
                return;

            var copy = _history.ToArray();
            var hideTasks = new List<Task>(copy.Length);

            foreach (var screen in copy)
            {
                cancellationToken.ThrowIfCancellationRequested();
                hideTasks.Add(HideAsync(screen, cancellationToken));
            }

            await Task.WhenAll(hideTasks);

            _activeScreens.Clear();
            _screenLayers.Clear();
            _history.Clear();
        }

        public TScreen GetActiveScreen<TScreen>() where TScreen : class
        {
            for (var i = _history.Count - 1; i >= 0; i--)
            {
                if (_history[i] is TScreen typed)
                    return typed;
            }

            return null;
        }

        private void InitializeLayerRoots()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerGO = new GameObject(layer.ToString());
                var rectTransform = layerGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localPosition = Vector3.zero;

                layerGO.transform.SetParent(_uiRoot, false);
                _layerRoots[layer] = layerGO.transform;
            }
        }

        private UIDataConfig ResolveConfig(string configId, Type screenType)
        {
            var config = _configService.Get<UIDataConfig>(configId);
            if (config == null)
            {
                throw new InvalidOperationException(
                    $"UI config '{configId}' not found for screen {screenType.FullName}.");
            }

            return config;
        }

        private async Task<GameObject> GetOrLoadPrefabAsync(string configId, UIDataConfig config,
            CancellationToken cancellationToken)
        {
            if (_loadedPrefabs.TryGetValue(configId, out var prefab))
            {
                return prefab;
            }

            prefab = await _prefabProvider.LoadPrefabAsync(config, cancellationToken);
            if (prefab == null)
            {
                throw new InvalidOperationException(
                    $"Prefab for config '{configId}' returned null (path: {config.PrefabPath}).");
            }

            _loadedPrefabs[configId] = prefab;
            return prefab;
        }

        private void RegisterScreen(UILayer layer, UIScreen screen)
        {
            if (!_activeScreens.TryGetValue(layer, out var screens))
            {
                screens = new List<UIScreen>();
                _activeScreens[layer] = screens;
            }

            screens.Add(screen);
            _screenLayers[screen] = layer;
            _history.Add(screen);
        }

        private UIScreen FindInstance(Type screenType)
        {
            for (var i = _history.Count - 1; i >= 0; i--)
            {
                var screen = _history[i];
                if (screenType.IsInstanceOfType(screen))
                {
                    return screen;
                }
            }

            return null;
        }

        private Transform GetLayerRoot(UILayer layer)
        {
            if (_layerRoots.TryGetValue(layer, out var root))
                return root;

            InitializeLayerRoots();
            return _layerRoots[layer];
        }
    }
}
