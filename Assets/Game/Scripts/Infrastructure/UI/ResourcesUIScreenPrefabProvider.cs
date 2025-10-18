using System.Threading;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;

namespace Game.Scripts.Infrastructure.UI
{
    public class ResourcesUIScreenPrefabProvider : IUIScreenPrefabProvider
    {
        public async Task<GameObject> LoadPrefabAsync(UIDataConfig config, CancellationToken cancellationToken = default)
        {
            if (config == null)
            {
                return null;
            }

            var request = Resources.LoadAsync<GameObject>(config.PrefabPath);

            while (!request.isDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            if (request.asset is not GameObject prefab)
            {
                throw new System.Exception($"Failed to load UI prefab at path: {config.PrefabPath}");
            }

            return prefab;
        }
    }
}
