using Game.Scripts.Gameplay.DataLayer;
using Game.Scripts.Gameplay.DataLayer.Battle;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public class DataLayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerDataProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyDataProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<BattleDataProvider>().AsSingle();
        }
    }
}
