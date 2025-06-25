using Game.Scripts.Gameplay.Lobby.Player;
using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IPlayerPresenter
    {
        ReactiveCollection<CardEntity> PlayerHand { get; }
        ReactiveCollection<EquipmentCardEntity> PlayerEquipment { get; }
        ReactiveCollection<StatEntity> PlayerStats { get; }
    }
}