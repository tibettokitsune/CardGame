using Game.Scripts.Gameplay.Lobby;
using Game.Scripts.Gameplay.Lobby.Player;
using Game.Scripts.Gameplay.PresentersLayer.Deck;
using Game.Scripts.Gameplay.PresentersLayer.GameStates;
using Game.Scripts.Gameplay.PresentersLayer.Player;
using Game.Scripts.Infrastructure.UI;
using Zenject;

namespace Game.Scripts.Gameplay.PresentersLayer
{
    public class PresentersLayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            //todo: core service 
            Container.Resolve<UIScreenFactory>().SetGameplayContainer(Container);
            
            Container.BindInterfacesAndSelfTo<LobbyPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeckPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<FirstEnterInGameState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreparePlayerState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPresenter>().AsSingle();
        }
    }
}