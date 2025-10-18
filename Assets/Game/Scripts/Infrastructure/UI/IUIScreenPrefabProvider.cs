using System.Threading;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;
using UnityEngine;

namespace Game.Scripts.Infrastructure.UI
{
    public interface IUIScreenPrefabProvider
    {
        Task<GameObject> LoadPrefabAsync(UIDataConfig config, CancellationToken cancellationToken = default);
    }
}
