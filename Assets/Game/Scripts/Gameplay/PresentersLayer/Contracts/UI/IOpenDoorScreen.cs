using System.Threading.Tasks;
using Game.Scripts.Gameplay.PresentersLayer.Player;

namespace Game.Scripts.Gameplay.PresentersLayer.Contracts.UI
{
    public interface IOpenDoorScreen
    {
        Task ShowDoorCard(CardEntity cardEntity);
    }
}
