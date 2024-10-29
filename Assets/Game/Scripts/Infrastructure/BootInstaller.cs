using Game.Scripts.UI;
using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class BootInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LoadingScreen>().FromComponentInHierarchy(true).AsSingle();
            Container.BindInterfacesTo<SceneManagementService>().AsSingle();
            Container.BindInterfacesTo<GameManager>().AsSingle();
        }
    }
}