using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Scripts.Infrastructure.Configs
{
    [CreateAssetMenu(fileName = "ConfigsContainer", menuName = "Configs/ScriptableContainer", order = 0)]
    public class ScriptableConfigsContainer : ScriptableObject
    {
        public UIViewConfig[] uiConfigs;

        public Task LoadUIConfigs(Dictionary<string, BaseConfig> source)
        {
            foreach (var config in uiConfigs)
            {
                source.TryAdd(config.Id, new UIDataConfig()
                {
                    Id = config.Id,
                    PrefabPath = config.PrefabPath,
                    Layer = config.Layer
                });
                
                Debug.Log($"UI config {config.Id} loaded");
            }
            
            return Task.CompletedTask;
        }
    }
}