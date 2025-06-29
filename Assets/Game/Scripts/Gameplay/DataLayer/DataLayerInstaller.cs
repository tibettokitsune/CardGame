using Game.Scripts.Gameplay.DataLayer;
using Zenject;

namespace Game.Scripts.Gameplay.Lobby.Player
{
    public class DataLayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<PlayerDataProvider>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyDataProvider>().AsSingle();
        }
    }
}