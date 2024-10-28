using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Infrastructure
{
    public class ConfigService : DataStorage<BaseConfigData>
    {
        private const string filePath = "";
        public async UniTask Initialize()
        {
            var content = await GetConfigData();
            var dictionary = content.ToDictionary(x => x.Id, x => (BaseConfigData)x);
            Load(dictionary);
            Debug.Log($"[CONFIG]: {Dictionary.Count} config loaded");
        }

        private async UniTask<IConfigEntity[]> GetConfigData()
        {
            var json = await Resources.LoadAsync(filePath) as TextAsset;
            
            return await Task.FromResult(JsonConvert.DeserializeObject<IConfigEntity[]>(json.text, App.JsonSettings));
        }
    }
}