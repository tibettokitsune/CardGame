using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace Game.Scripts.Gameplay.ViewsLayer
{
    public class DynamicUIContainer<TData, TElement> : DynamicUIContainerBase
        where TElement : MonoBehaviour
    {
        [SerializeField] private TElement _prefab;
        [SerializeField] private Transform _container;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly Dictionary<int, TElement> _spawnedElements = new Dictionary<int, TElement>();

        private ReactiveCollection<TData> _boundCollection;
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

            Initialize();
        }

        public override void Initialize()
        {
            if (_boundCollection == null) return;

            // Обработка существующих элементов
            for (var i = 0; i < _boundCollection.Count; i++)
            {
                OnItemAdded(_boundCollection[i], i);
            }

            // Подписка на изменения коллекции
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

        public override void Clear()
        {
            Unbind();
        }

        public void Unbind()
        {
            _disposables.Clear();
            ClearAllElements();
            _boundCollection = null;
        }

        private void OnItemAdded(TData item, int index)
        {
            var element = Instantiate(_prefab, _container);
            element.transform.SetSiblingIndex(index);

            var disposables = new CompositeDisposable();
            if (_elementInitializer != null)
            {
                var initDisposable = _elementInitializer(item, element);
                if (initDisposable != null)
                {
                    disposables.Add(initDisposable);
                }
            }

            _onElementCreated?.Invoke(element);

            _spawnedElements.Add(index, element);

            // Обновляем индексы для последующих элементов
            ReindexElementsFrom(index + 1);
        }

        private void OnItemRemoved(TData item, int index)
        {
            if (_spawnedElements.TryGetValue(index, out var element))
            {
                Destroy(element.gameObject);
                _spawnedElements.Remove(index);

                // Обновляем индексы для последующих элементов
                ReindexElementsFrom(index);
            }
        }

        private void OnItemReplaced(TData oldItem, TData newItem, int index)
        {
            if (_spawnedElements.TryGetValue(index, out var element))
            {
                // Обновляем элемент с новыми данными
                if (_elementInitializer != null)
                {
                    _elementInitializer(newItem, element);
                }
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
            var toReindex = new List<KeyValuePair<int, TElement>>();

            // Собираем элементы для переиндексации
            foreach (var kvp in _spawnedElements)
            {
                if (kvp.Key >= startIndex)
                {
                    toReindex.Add(kvp);
                }
            }

            // Удаляем старые индексы
            foreach (var kvp in toReindex)
            {
                _spawnedElements.Remove(kvp.Key);
            }

            // Добавляем с новыми индексами
            foreach (var kvp in toReindex)
            {
                var newIndex = kvp.Key - 1;
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