using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using Game.Scripts.Infrastructure.Helpers;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Scripts.Infrastructure.Configs
{
    public class ConfigService : IConfigService<BaseConfig>, IAsyncInitializable
    {
        private readonly Dictionary<string, BaseConfig> _dictionary = new();

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("Config service start initialization");
            await DataSaveHelper.LoadJsonData(_dictionary);
            Debug.Log("Config service initialized");
            await Task.CompletedTask;
        }

        public T Get<T>(string id) where T : BaseConfig
        {
            if (_dictionary.TryGetValue(id, out var value) && value is T t)
                return t;
            Debug.LogWarning($"'Couldn't find {typeof(T).Name} id={id}");
            return default;
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