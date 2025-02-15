using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Game.Scripts.Infrastructure.Configs
{
    public interface IBaseConfig
    {
        string Id { get; }
    }
    
    [Serializable]
    public abstract class BaseConfig : IBaseConfig
    {
        public string Id { get; set; }
    }

    public interface IConfigService<T>
    {
        TC Get<TC>(string id) where TC : T;
    }

    public class ConfigService : IConfigService<BaseConfig>, IAsyncInitializable
    {
        private const string ScriptableConfigsPath = "Configs/ConfigsContainer";
        private Dictionary<string, BaseConfig> _dictionary = new();
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("Config service start initialization");
            await LoadScriptableConfigs();
            await Task.Delay(3000);
            Debug.Log("Config service initialized");
            await Task.CompletedTask;
        }

        private async Task LoadScriptableConfigs()
        {
            var container = await Resources.LoadAsync<ScriptableConfigsContainer>(ScriptableConfigsPath) as ScriptableConfigsContainer;
            await container.LoadUIConfigs(_dictionary);
        }

        public T Get<T>(string id) where T : BaseConfig
        {
            if (_dictionary.TryGetValue(id, out var value) && value is T t)
                return t;
            Debug.LogWarning($"'Couldn't find {typeof(T).Name} id={id}");
            return default;
        }
    }

    public enum UILayer
    {
        Window, Popup
    }

    public class UIDataConfig : BaseConfig
    {
        public string PrefabPath;
        public UILayer Layer;
        
    }
}