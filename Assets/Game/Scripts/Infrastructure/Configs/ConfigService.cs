using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
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
            await LoadJsonData(_dictionary);
            Debug.Log("Config service initialized");
            await Task.CompletedTask;
        }

        public static async Task LoadJsonData(Dictionary<string, BaseConfig> source)
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
                    source.TryAdd(config.Id, config);
                    Debug.Log($"UI config {config.Id} loaded");
                }

                foreach (var config in data.CardLayersConfigs)
                {
                    source.TryAdd(config.Id, config);
                    Debug.Log($"Card layer config {config.Id} loaded");
                }

                foreach (var config in data.CardConfigs)
                {
                    source.TryAdd(config.Id, config);
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

        public static async void PatchSourceData(Dictionary<string, BaseConfig> source)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            var data = new ConfigsWrapper
            {
                UIConfigs = new List<UIDataConfig>(),
                CardLayersConfigs = new List<CardLayerDataConfig>(),
                CardConfigs = new List<CardDataConfig>()
            };

            foreach (var config in source.Values)
            {
                switch (config)
                {
                    case UIDataConfig ui:
                        data.UIConfigs.Add(ui);
                        break;
                    case CardLayerDataConfig layer:
                        data.CardLayersConfigs.Add(layer);
                        break;
                    case CardDataConfig card:
                        data.CardConfigs.Add(card);
                        break;
                    default:
                        Debug.LogWarning($"Неизвестный тип конфигурации: {config.GetType()}");
                        break;
                }
            }

            string json = JsonConvert.SerializeObject(data, settings);

#if UNITY_EDITOR
            string resourcePath = Path.Combine(Application.dataPath, "Resources/configs.json");

            try
            {
                File.WriteAllText(resourcePath, json);
                Debug.Log($"Конфигурации успешно сохранены в {resourcePath}");

                // Обновим ассеты
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка при сохранении конфигураций: {ex.Message}");
            }
#else
        Debug.LogWarning("Сохранение в Resources запрещено во время выполнения вне редактора. Используйте Application.persistentDataPath вместо этого.");
#endif
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