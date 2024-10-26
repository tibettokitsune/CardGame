using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class BootInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameManager>().AsSingle();
            Container.BindInterfacesTo<SceneManagementService>().AsSingle();
        }
    }
}