using System.Threading.Tasks;

namespace Game.Scripts.UI
{
    public interface IUIService<T>
    {
        Task<TC> ShowScreen<TC>(string id) where TC : T;
        Task Clear();
    }
}