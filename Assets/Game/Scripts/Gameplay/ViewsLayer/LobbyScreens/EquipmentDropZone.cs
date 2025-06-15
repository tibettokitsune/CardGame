using Game.Scripts.Gameplay.PresentersLayer.Player;
using Zenject;

namespace Game.Scripts.Gameplay.ViewsLayer.LobbyScreens
{
    public class EquipmentDropZone : DropZone
    {
        [Inject] private IEquipCardUseCase _equipmentUseCase;
        public override void HandleDrop(string cardId)
        {
            base.HandleDrop(cardId);
            _equipmentUseCase.Execute(cardId);
        }
    }
}