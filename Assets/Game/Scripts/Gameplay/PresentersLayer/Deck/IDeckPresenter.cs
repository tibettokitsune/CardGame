using System.Threading.Tasks;
using Game.Scripts.Gameplay.DataLayer.Models;

namespace Game.Scripts.Gameplay.PresentersLayer.Deck
{
    public interface IDeckPresenter
    {
        Task<string> TakeEquipmentCard();
        Task<string> TakeDoorCard();
        BaseCard GetCardById(string cardId);
    }
}
