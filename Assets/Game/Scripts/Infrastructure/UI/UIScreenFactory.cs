using System;
using Game.Scripts.UI;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Scripts.Infrastructure.UI
{
    public class UIScreenFactory : PlaceholderFactory<UIScreen, Transform, Type, UIScreen>
    {
        private readonly DiContainer _bootContainer;
        private DiContainer _gameplayContainer;

        public UIScreenFactory(DiContainer bootContainer)
        {
            _bootContainer = bootContainer;
        }

        public UIScreen Create(UIScreen prefab, Transform parent, Type screenType)
        {
            var container = _gameplayContainer ?? _bootContainer;

            var gameObj = container.InstantiatePrefab(prefab.gameObject, parent);
            var instance = (UIScreen) gameObj.GetComponent(screenType);
            if (instance == null)
            {
                Object.Destroy(gameObj);
                throw new InvalidOperationException(
                    $"Prefab {prefab.name} doesn't contain component of type {screenType.Name}");
            }

            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            container.Inject(instance);
            return instance;
        }

        public void SetGameplayContainer(DiContainer gameplayContainer)
        {
            _gameplayContainer = gameplayContainer;
        }
    }
}