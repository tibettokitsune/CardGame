using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Scripts.Gameplay.ViewsLayer.Base
{
    public class DynamicUIContainer<TData, TElement> : DynamicUIContainerBase
        where TElement : MonoBehaviour
    {
        [SerializeField] private TElement _prefab;
        [SerializeField] private Transform _container;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly Dictionary<object, TElement> _spawnedElements = new Dictionary<object, TElement>();

        private ReactiveCollection<TData> _boundCollection;
        private ReactiveDictionary<object, TData> _boundDictionary;

        private Func<TData, TElement, IDisposable> _elementInitializer;
        private Action<TElement> _onElementCreated;

        public void Bind(
            ReactiveCollection<TData> collection,
            Func<TData, TElement, IDisposable> elementInitializer,
            Action<TElement> onElementCreated = null)
        {
            Unbind();
            _boundCollection = collection;
            _elementInitializer = elementInitializer;
            _onElementCreated = onElementCreated;

            InitializeCollection();
        }

        public void Bind<TKey>(
            ReactiveDictionary<TKey, TData> dictionary,
            Func<TData, TElement, IDisposable> elementInitializer,
            Action<TElement> onElementCreated = null)
        {
            Unbind();
            _boundDictionary = new ReactiveDictionary<object, TData>();
            foreach (var kvp in dictionary)
            {
                _boundDictionary.Add(kvp.Key, kvp.Value);
            }

            _elementInitializer = elementInitializer;
            _onElementCreated = onElementCreated;

            InitializeDictionary(dictionary);
        }

        public override void Initialize()
        {
            // Не используется напрямую, вызывается через InitializeCollection / InitializeDictionary
        }

        private void InitializeCollection()
        {
            for (var i = 0; i < _boundCollection.Count; i++)
            {
                OnItemAdded(_boundCollection[i], i);
            }

            _boundCollection.ObserveAdd()
                .Subscribe(e => OnItemAdded(e.Value, e.Index))
                .AddTo(_disposables);

            _boundCollection.ObserveRemove()
                .Subscribe(e => OnItemRemoved(e.Value, e.Index))
                .AddTo(_disposables);

            _boundCollection.ObserveReplace()
                .Subscribe(e => OnItemReplaced(e.OldValue, e.NewValue, e.Index))
                .AddTo(_disposables);

            _boundCollection.ObserveReset()
                .Subscribe(_ => OnCollectionReset())
                .AddTo(_disposables);
        }

        private void InitializeDictionary<TKey>(ReactiveDictionary<TKey, TData> dict)
        {
            foreach (var kvp in dict)
            {
                OnItemAddedToDictionary(kvp.Key, kvp.Value);
            }

            dict.ObserveAdd()
                .Subscribe(e => OnItemAddedToDictionary(e.Key, e.Value))
                .AddTo(_disposables);

            dict.ObserveRemove()
                .Subscribe(e => OnItemRemovedFromDictionary(e.Key, e.Value))
                .AddTo(_disposables);

            dict.ObserveReplace()
                .Subscribe(e => OnItemReplacedInDictionary(e.Key, e.OldValue, e.NewValue))
                .AddTo(_disposables);

            dict.ObserveReset()
                .Subscribe(_ => OnCollectionReset())
                .AddTo(_disposables);
        }

        public override void Clear()
        {
            Unbind();
        }

        public void Unbind()
        {
            _disposables.Clear();
            ClearAllElements();
            _boundCollection = null;
            _boundDictionary = null;
        }

        private async void OnItemAdded(TData item, int index)
        {
            var element = Instantiate(_prefab, _container);
            element.transform.SetSiblingIndex(index);

            if (_elementInitializer != null)
            {
                var disposable = _elementInitializer(item, element);
                if (disposable != null) disposable.AddTo(_disposables);
            }

            _onElementCreated?.Invoke(element);
            _spawnedElements.Add(index, element);

            ReindexElementsFrom(index + 1);
            await UniTask.Yield();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container.transform as RectTransform);
        }

        private void OnItemRemoved(TData item, int index)
        {
            if (_spawnedElements.TryGetValue(index, out var element))
            {
                Destroy(element.gameObject);
                _spawnedElements.Remove(index);
                ReindexElementsFrom(index);
            }
        }

        private void OnItemReplaced(TData oldItem, TData newItem, int index)
        {
            if (_spawnedElements.TryGetValue(index, out var element))
            {
                _elementInitializer?.Invoke(newItem, element);
            }
        }

        private void OnItemAddedToDictionary(object key, TData value)
        {
            var element = Instantiate(_prefab, _container);

            if (_elementInitializer != null)
            {
                var disposable = _elementInitializer(value, element);
                if (disposable != null) disposable.AddTo(_disposables);
            }

            _onElementCreated?.Invoke(element);
            _spawnedElements.Add(key, element);

            LayoutRebuilder.ForceRebuildLayoutImmediate(_container.transform as RectTransform);
        }

        private void OnItemRemovedFromDictionary(object key, TData value)
        {
            if (_spawnedElements.TryGetValue(key, out var element))
            {
                Destroy(element.gameObject);
                _spawnedElements.Remove(key);
            }
        }

        private void OnItemReplacedInDictionary(object key, TData oldValue, TData newValue)
        {
            if (_spawnedElements.TryGetValue(key, out var element))
            {
                _elementInitializer?.Invoke(newValue, element);
            }
        }

        private void OnCollectionReset()
        {
            ClearAllElements();
        }

        private void ClearAllElements()
        {
            foreach (var element in _spawnedElements.Values)
            {
                if (element != null)
                {
                    Destroy(element.gameObject);
                }
            }

            _spawnedElements.Clear();
        }

        private void ReindexElementsFrom(int startIndex)
        {
            var toReindex = new List<KeyValuePair<object, TElement>>();

            foreach (var kvp in _spawnedElements)
            {
                if (kvp.Key is int index && index >= startIndex)
                {
                    toReindex.Add(kvp);
                }
            }

            foreach (var kvp in toReindex)
            {
                _spawnedElements.Remove(kvp.Key);
            }

            foreach (var kvp in toReindex)
            {
                var newIndex = (int)kvp.Key - 1;
                _spawnedElements.Add(newIndex, kvp.Value);
                kvp.Value.transform.SetSiblingIndex(newIndex);
            }
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
