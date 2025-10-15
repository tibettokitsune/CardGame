using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using Newtonsoft.Json;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Newtonsoft.Json.Linq;

namespace Game.Scripts.Infrastructure.Helpers
{
    public static class DataSaveHelper
    {
        public static async Task LoadJsonData(Dictionary<string, BaseConfig> source)
        {
            var request = Resources.LoadAsync<TextAsset>("configs");
            await UniTask.WaitUntil(() => request.isDone);
            var jsonFile = request.asset as TextAsset;

            if (!jsonFile)
            {
                Debug.LogError("Не удалось загрузить JSON файл.");
                return;
            }

            var data = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(jsonFile.text);

            if (data == null)
            {
                Debug.LogWarning("Конфигурационный файл пустой или содержит неверные данные.");
                return;
            }

            foreach (var pair in data)
            {
                var id = pair.Key;
                var payload = pair.Value;

                if (payload == null)
                {
                    Debug.LogWarning($"Конфигурация с id \"{id}\" имеет пустое значение и будет пропущена.");
                    continue;
                }

                var typeName = payload.Value<string>(nameof(BaseConfig.ConfigType));
                var configType = ResolveConfigType(typeName);

                if (configType == null)
                {
                    Debug.LogWarning($"Тип конфигурации \"{typeName}\" не найден. Конфигурация с id \"{id}\" будет пропущена.");
                    continue;
                }

                var config = (BaseConfig)payload.ToObject(configType);

                if (config == null)
                {
                    Debug.LogWarning($"Не удалось десериализовать конфигурацию с id \"{id}\".");
                    continue;
                }

                config.ConfigType = configType.Name;

                if (string.IsNullOrWhiteSpace(config.Id))
                {
                    config.Id = id;
                }

                if (source.TryAdd(id, config))
                {
                    Debug.Log($"{config.ConfigType} {id} loaded");
                    continue;
                }

                Debug.LogWarning($"Конфигурация с id \"{id}\" уже существует и будет пропущена.");
            }
        }
        
        public static async void PatchSourceData(Dictionary<string, BaseConfig> source)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            var data = new Dictionary<string, BaseConfig>();

            foreach (var pair in source)
            {
                var id = pair.Key;
                var config = pair.Value;

                if (config == null)
                {
                    Debug.LogWarning($"Конфигурация с id \"{id}\" имеет пустое значение и будет пропущена.");
                    continue;
                }

                config.ConfigType = config.GetType().Name;

                if (string.IsNullOrWhiteSpace(config.Id))
                {
                    config.Id = id;
                }

                data[id] = config;
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

        private static readonly Dictionary<string, Type> ConfigTypesCache = new();

        private static Type ResolveConfigType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            if (ConfigTypesCache.TryGetValue(typeName, out var cached))
            {
                return cached;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    var loadedTypes = new List<Type>();

                    foreach (var candidate in exception.Types)
                    {
                        if (candidate != null)
                        {
                            loadedTypes.Add(candidate);
                        }
                    }

                    types = loadedTypes.ToArray();
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !typeof(BaseConfig).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (string.Equals(type.Name, typeName, StringComparison.Ordinal) ||
                        string.Equals(type.FullName, typeName, StringComparison.Ordinal))
                    {
                        CacheType(type);
                        return type;
                    }
                }
            }

            return null;
        }

        private static void CacheType(Type type)
        {
            if (type == null)
            {
                return;
            }

            if (!ConfigTypesCache.ContainsKey(type.Name))
            {
                ConfigTypesCache[type.Name] = type;
            }

            if (!ConfigTypesCache.ContainsKey(type.FullName))
            {
                ConfigTypesCache[type.FullName] = type;
            }
        }
    }
}
