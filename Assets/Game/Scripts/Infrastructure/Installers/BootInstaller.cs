using Game.Scripts.UI;
using Zenject;

namespace Game.Scripts.Infrastructure.Installers
{
    public class BootInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            InstallDataLayer();
            InstallLogic();
            InstallUI();
        }

        private void InstallUI()
        {
            Container.Bind<LoadingScreen>().FromComponentInHierarchy(true).AsSingle();
        }

        private void InstallLogic()
        {
            Container.BindInterfacesTo<SceneManagementService>().AsSingle();
            Container.BindInterfacesTo<GameManager>().AsSingle();
        }

        private void InstallDataLayer()
        {
            Container.BindInterfacesTo<GameStateDataProvider>().AsSingle();
        }
    }
}