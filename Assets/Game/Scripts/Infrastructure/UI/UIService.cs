using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        
        private readonly Dictionary<string, UIScreen> _loadedScreens = new();
        private readonly Dictionary<UILayer, List<UIScreen>> _activeScreens = new();
        private readonly Dictionary<UILayer, Transform> _layerRoots = new();
        private readonly List<string> _history = new();
        private readonly Transform _uiRoot;

        public UIService(Transform uiRoot)
        {
            _uiRoot = uiRoot;
            InitializeLayerRoots();
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // Preload any necessary UI elements here if needed
            await Task.CompletedTask;
        }

        private void InitializeLayerRoots()
        {
            foreach (UILayer layer in System.Enum.GetValues(typeof(UILayer)))
            {
                var layerGO = new GameObject(layer.ToString());
                layerGO.transform.SetParent(_uiRoot);
                var rectTransform = layerGO.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.localPosition = Vector3.zero;
                _layerRoots[layer] = layerGO.transform;
            }
        }

        private Type GetScreenType(string typeName)
        {
            // Попробуем стандартный способ
            var type = Type.GetType(typeName);
            if (type != null) return type;
    
            // Если не найдено, поищем во всех сборках
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null) return type;
            }
    
            throw new Exception($"Screen type {typeName} not found in any loaded assembly");
        }
        
        public async Task<UIScreen> ShowScreen(string id)
        {
            var config = _configService.Get<UIDataConfig>(id);
            if (config == null) 
                throw new System.Exception($"UI config not found for id: {id}");

            // Получаем тип экрана из конфига
            var screenType = GetScreenType(config.ScreenType);
            if (screenType == null || !typeof(UIScreen).IsAssignableFrom(screenType))
                throw new System.Exception($"Invalid screen type in config for id: {id}");

            // Остальная логика остается прежней, но с использованием Type
            if (!_loadedScreens.TryGetValue(id, out var prefab))
            {
                var request = Resources.LoadAsync<UIScreen>(config.PrefabPath);
                await request;
                prefab = request.asset as UIScreen;
            
                if (prefab == null)
                    throw new System.Exception($"Failed to load UI prefab at path: {config.PrefabPath}");

                _loadedScreens[id] = prefab;
            }

            // Используем модифицированную фабрику
            var screen = _screensFactory.Create(prefab, _layerRoots[config.Layer], screenType);
            screen.name = $"{id}_Screen";

            await screen.ShowAsync();

            if (!_activeScreens.ContainsKey(config.Layer))
                _activeScreens.Add(config.Layer, new List<UIScreen>());

            _activeScreens[config.Layer].Add(screen);
            _history.Add(id);

            return screen;
        }

        public async Task HideScreen(string id)
        {
            foreach (var (layer, screens) in _activeScreens)
            {
                var screen = screens.FirstOrDefault(s => s.name.StartsWith(id));
                if (screen != null)
                {
                    await screen.HideAsync();
                    screens.Remove(screen);
                    Object.Destroy(screen.gameObject);
                    _history.Remove(id);
                    return;
                }
            }
        }

        public async Task HideScreensOnLayer(UILayer layer)
        {
            if (_activeScreens.TryGetValue(layer, out var screens))
            {
                foreach (var screen in screens.ToList())
                {
                    await screen.HideAsync();
                    Object.Destroy(screen.gameObject);
                    _history.RemoveAll(x => screen.name.StartsWith(x));
                }
                screens.Clear();
            }
        }

        public async Task HideTopScreen()
        {
            if (_history.Count == 0) return;

            var lastId = _history[^1];
            await HideScreen(lastId);
        }

        public Task Clear()
        {
            foreach (var (layer, screens) in _activeScreens)
            {
                foreach (var screen in screens)
                {
                    Object.Destroy(screen.gameObject);
                }
                screens.Clear();
            }

            _activeScreens.Clear();
            _history.Clear();
            return Task.CompletedTask;
        }

        public TC GetActiveScreen<TC>() where TC : UIScreen
        {
            if (_history.Count == 0) return null;

            var lastId = _history[^1];
            foreach (var screens in _activeScreens.Values)
            {
                var screen = screens.FirstOrDefault(s => s.name.StartsWith(lastId));
                if (screen != null && screen is TC typedScreen)
                {
                    return typedScreen;
                }
            }
            return null;
        }
    }
}