using System.Threading.Tasks;
namespace Game.Scripts.UIContracts
{
    public interface IOpenDoorScreen
    {
        Task ShowDoorCard(DoorCardViewData cardEntity);
    }
}
