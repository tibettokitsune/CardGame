using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IPlayerPresenter
    {
        ReactiveCollection<CardEntity> PlayerHand { get; }
        ReactiveCollection<CardEntity> PlayerEquipment { get; }
    }
}