using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Configs
{
    
    public class ConfigService : IConfigService, IAsyncInitializable
    {
        private readonly Dictionary<string, BaseConfig> _configs = new();

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("Config service start initialization");
            await DataSaveHelper.LoadJsonData(_configs);
            Debug.Log("Config service initialized");
            await Task.CompletedTask;
        }
        public T Get<T>(string id) where T : BaseConfig
        {
            if (_configs.TryGetValue(id, out var config) && config is T typedConfig)
                return typedConfig;

            Debug.LogWarning($"Config not found: Type={typeof(T).Name}, ID={id}");
            return default;
        }

        public IEnumerable<T> GetAll<T>() where T : BaseConfig
        {
            return _configs.Values.OfType<T>();
        }
    }
}

[Serializable]
public class ConfigsWrapper
{
    public List<UIDataConfig> UIConfigs { get; set; }
    public List<CardDataConfig> CardConfigs { get; set; }
    public List<CardLayerDataConfig> CardLayersConfigs { get; set; }
}
