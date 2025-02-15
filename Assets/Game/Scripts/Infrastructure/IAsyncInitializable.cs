using System.Threading;
using System.Threading.Tasks;

namespace Game.Scripts.Infrastructure
{
    public interface IAsyncInitializable
    {
        Task InitializeAsync(CancellationToken cancellationToken = default);
    }
}