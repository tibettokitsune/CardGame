using System;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.UIContracts;
using UniRx;

namespace Game.Scripts.Gameplay.PresentersLayer.Player
{
    public interface IPlayerPresenter
    {
        ReactiveCollection<CardViewData> PlayerHand { get; }
        ReactiveCollection<EquipmentCardViewData> PlayerEquipment { get; }
        ReactiveDictionary<string ,StatEntity> PlayerStats { get; }
        ReactiveProperty<DoorCardViewData> CurrentDoor { get; }
    }
}
