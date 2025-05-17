using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Scripts.Infrastructure.Helpers
{
    public static class DataSaveHelper
    {
        public static async Task LoadJsonData(Dictionary<string, BaseConfig> source)
        {
            var request = Resources.LoadAsync<TextAsset>($"configs");
            await UniTask.WaitUntil(() => request.isDone);
            var jsonFile = request.asset as TextAsset;

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