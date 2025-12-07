using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var assets = new List<TextAsset>();

            // Load any files under Resources/configs/*
            assets.AddRange(Resources.LoadAll<TextAsset>("configs") ?? Array.Empty<TextAsset>());

            // Fallback to legacy single file Resources/configs.json (name "configs")
            var legacyAsset = Resources.Load<TextAsset>("configs");
            if (legacyAsset != null && assets.All(a => a != null && a.name != legacyAsset.name))
                assets.Add(legacyAsset);

            if (assets.Count == 0)
            {
                Debug.LogError("Не удалось загрузить JSON файлы конфигураций.");
                return;
            }

            foreach (var asset in assets)
            {
                if (asset == null)
                    continue;

                var data = JsonConvert.DeserializeObject<Dictionary<string, JObject>>(asset.text);

                if (data == null)
                {
                    Debug.LogWarning($"Конфигурационный файл \"{asset.name}\" пустой или содержит неверные данные.");
                    continue;
                }

                foreach (var pair in data)
                {
                    var id = pair.Key;
                    var payload = pair.Value;

                    if (payload == null)
                    {
                        Debug.LogWarning($"Конфигурация с id \"{id}\" имеет пустое значение и будет пропущена. Файл: {asset.name}");
                        continue;
                    }

                    var typeName = payload.Value<string>(nameof(BaseConfig.ConfigType));
                    var configType = ResolveConfigType(typeName);

                    if (configType == null)
                    {
                        Debug.LogWarning($"Тип конфигурации \"{typeName}\" не найден. Конфигурация с id \"{id}\" будет пропущена. Файл: {asset.name}");
                        continue;
                    }

                    var config = (BaseConfig)payload.ToObject(configType);

                    if (config == null)
                    {
                        Debug.LogWarning($"Не удалось десериализовать конфигурацию с id \"{id}\". Файл: {asset.name}");
                        continue;
                    }

                    config.ConfigType = configType.Name;

                    if (string.IsNullOrWhiteSpace(config.Id))
                    {
                        config.Id = id;
                    }

                    if (source.TryAdd(id, config))
                    {
                        Debug.Log($"{config.ConfigType} {id} loaded from {asset.name}");
                        continue;
                    }

                    Debug.LogWarning($"Конфигурация с id \"{id}\" уже существует и будет пропущена. Файл: {asset.name}");
                }
            }
        }
        
        public static async void PatchSourceData(Dictionary<string, BaseConfig> source)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

#if UNITY_EDITOR
            var resourceFolder = Path.Combine(Application.dataPath, "Resources", "configs");
            Directory.CreateDirectory(resourceFolder);

            var groupedByType = source
                .Where(pair => pair.Value != null)
                .GroupBy(pair => pair.Value.ConfigType ?? pair.Value.GetType().Name);

            foreach (var group in groupedByType)
            {
                var payload = new Dictionary<string, BaseConfig>();

                foreach (var pair in group)
                {
                    var id = pair.Key;
                    var config = pair.Value;

                    config.ConfigType = config.GetType().Name;

                    if (string.IsNullOrWhiteSpace(config.Id))
                    {
                        config.Id = id;
                    }

                    payload[id] = config;
                }

                var json = JsonConvert.SerializeObject(payload, settings);
                var fileName = $"{group.Key}.json";
                var filePath = Path.Combine(resourceFolder, fileName);

                try
                {
                    File.WriteAllText(filePath, json);
                    Debug.Log($"Конфигурации типа {group.Key} сохранены в {filePath}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Ошибка при сохранении конфигураций ({group.Key}): {ex.Message}");
                }
            }

            AssetDatabase.Refresh();
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
