using Zenject;

namespace Game.Scripts.Gameplay.Lobby
{
    public class LobbyInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LobbyPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeckPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<FirstEnterInGameState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreparePlayerState>().AsSingle();
        }
    }
}