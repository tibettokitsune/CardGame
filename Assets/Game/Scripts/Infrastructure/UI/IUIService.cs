using System.Threading.Tasks;

namespace Game.Scripts.UI
{
    public interface IUIService
    {
        Task<UIScreen> ShowScreen(string id);
        Task Clear();
    }
}