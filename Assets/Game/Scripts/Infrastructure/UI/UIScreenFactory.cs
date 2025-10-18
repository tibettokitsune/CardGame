using Game.Scripts.UI;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Game.Scripts.Infrastructure.UI
{
    public class UIScreenFactory
    {
        private readonly DiContainer _bootContainer;
        private DiContainer _gameplayContainer;

        public UIScreenFactory(DiContainer bootContainer)
        {
            _bootContainer = bootContainer;
        }

        public UIScreen Create(GameObject prefab, Transform parent, System.Type screenType)
        {
            var container = _gameplayContainer ?? _bootContainer;

            var gameObject = container.InstantiatePrefab(prefab, parent);
            var instance = gameObject.GetComponent(screenType) as UIScreen;
            if (instance == null)
            {
                Object.Destroy(gameObject);
                throw new System.InvalidOperationException(
                    $"Prefab {prefab.name} does not contain component of type {screenType.FullName}.");
            }

            ApplyDefaultLayout(instance.transform);
            container.Inject(instance);
            return instance;
        }

        public void SetGameplayContainer(DiContainer gameplayContainer)
        {
            _gameplayContainer = gameplayContainer;
        }

        private static void ApplyDefaultLayout(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            if (transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.localRotation = Quaternion.identity;
            }
        }
    }
}
