using Game.Scripts.UI;
using Zenject;

namespace Game.Scripts.Infrastructure.UI
{
    public class ScreensFactory : IFactory<UnityEngine.Object, UIScreen>
    {
        readonly DiContainer _container;

        public ScreensFactory(DiContainer container)
        {
            _container = container;
        }

        public UIScreen Create(UnityEngine.Object prefab)
        {
            return _container.InstantiatePrefabForComponent<UIScreen>(prefab);
        }
    }
}