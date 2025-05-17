using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Game.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIScreen : MonoBehaviour
    {
        public virtual Task ShowAsync()
        {
            gameObject.SetActive(true);
            return Task.CompletedTask;
        }

        public virtual Task HideAsync()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }
        
        public class Factory : PlaceholderFactory<UnityEngine.Object, UIScreen>
        {
        }
    }
    
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