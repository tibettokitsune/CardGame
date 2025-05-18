using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer;
using Game.Scripts.Gameplay.ViewsLayer;
using UnityEngine;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby
{
    public class LobbyInstaller : MonoInstaller
    {
        [SerializeField] private HandCardView cardViewPrefab;

        public override void InstallBindings()
        {
            Container.Install<DataLayerInstaller>();
            Container.Install<PresentersLayerInstaller>();
            Container.Install<ViewsLayerInstaller>();
        }
    }
}