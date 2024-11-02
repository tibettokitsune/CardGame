using Zenject;

namespace Game.Scripts.Infrastructure
{
    public class ProjectInstaller : MonoInstaller
    {
        public SceneConfig sceneConfig;
        public override void InstallBindings()
        {
            Container.BindInstance(sceneConfig).AsSingle();
        }
    }
}
