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
            
            Container.BindInterfacesAndSelfTo<GameplayFlowPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<DeckPresenter>().AsSingle();
            
            // State effects
            Container.BindInterfacesAndSelfTo<ShowInitialGameplayUiEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<FillStartHandEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadPrepareSceneEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<PrepareRoundTimerEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<CleanupPreparePlayerEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<TakeEventCardFlowEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnloadEventCardScenesEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadBattleSceneEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<UnloadBattleSceneEffect>().AsSingle();
            Container.BindInterfacesAndSelfTo<FinishTakeEventCardStateUseCase>().AsSingle();

            // States
            Container.BindInterfacesAndSelfTo<FirstEnterInGameState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PreparePlayerState>().AsSingle();
            Container.BindInterfacesAndSelfTo<TakeEventCardState>().AsSingle();
            Container.BindInterfacesAndSelfTo<BattleState>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerPresenter>().AsSingle();
        }
    }
}
