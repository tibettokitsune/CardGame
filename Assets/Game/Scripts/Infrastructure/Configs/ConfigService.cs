using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Configs
{
    public class ConfigService : IConfigService<BaseConfig>, IAsyncInitializable
    {
        private readonly Dictionary<string, BaseConfig> _dictionary = new();

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("Config service start initialization");
            await LoadJsonData();
            Debug.Log("Config service initialized");
            await Task.CompletedTask;
        }

        private async Task LoadJsonData()
        {
            var jsonFile = await Resources.LoadAsync<TextAsset>($"configs") as TextAsset;

            if (jsonFile)
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                var data = JsonConvert.DeserializeObject<ConfigsWrapper>(jsonFile.text, settings);
                foreach (var config in data.UIConfigs)
                {
                    _dictionary.TryAdd(config.Id, config);
                    Debug.Log($"UI config {config.Id} loaded");
                }
                
                foreach (var config in data.CardLayersConfigs)
                {
                    _dictionary.TryAdd(config.Id, config);
                    Debug.Log($"Card layer config {config.Id} loaded");
                }
                
                foreach (var config in data.CardConfigs)
                {
                    _dictionary.TryAdd(config.Id, config);
                    Debug.Log($"Card config {config.Id} loaded");
                }
            }
            else
            {
                Debug.LogError("Не удалось загрузить JSON файл.");
            }
        }

        public T Get<T>(string id) where T : BaseConfig
        {
            if (_dictionary.TryGetValue(id, out var value) && value is T t)
                return t;
            Debug.LogWarning($"'Couldn't find {typeof(T).Name} id={id}");
            return default;
        }
    }

    [Serializable]
    public class ConfigsWrapper
    {
        public List<UIDataConfig> UIConfigs { get; set; }
        public List<CardDataConfig> CardConfigs { get; set; }
        public List<CardLayerDataConfig> CardLayersConfigs { get; set; }
    }
}