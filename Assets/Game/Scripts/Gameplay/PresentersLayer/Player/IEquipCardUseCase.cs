using System.Threading.Tasks;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IEquipCardUseCase
    {
        Task<EquipCardResult> Execute(string cardId);
    }
}