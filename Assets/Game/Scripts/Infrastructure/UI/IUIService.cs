using System.Threading;
using System.Threading.Tasks;
using Game.Scripts.Infrastructure.Configs.Configs;

namespace Game.Scripts.UI
{
    public interface IUIService
    {
        Task<TScreen> ShowAsync<TScreen>(CancellationToken cancellationToken = default)
            where TScreen : class;

        Task HideAsync<TScreen>(CancellationToken cancellationToken = default)
            where TScreen : class;

        Task HideAsync(UIScreen screen, CancellationToken cancellationToken = default);

        Task HideLayerAsync(UILayer layer, CancellationToken cancellationToken = default);

        Task HideTopAsync(CancellationToken cancellationToken = default);

        Task ClearAsync(CancellationToken cancellationToken = default);

        TScreen GetActiveScreen<TScreen>() where TScreen : class;
    }
}
