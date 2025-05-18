using Game.Scripts.Gameplay.Lobby;
using Game.Scripts.Gameplay.Lobby.Deck;
using Game.Scripts.Gameplay.Lobby.GameStates;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Zenject;

namespace Game.Scripts.Gameplay.PresentersLayer
{
    public class PresentersLayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeckPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<FirstEnterInGameState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreparePlayerState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPresenter>().AsSingle();
        }
    }
}