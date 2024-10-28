using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Game.Scripts.Infrastructure
{
    public abstract class DataStorage<T> : IDataStorage<T>
    {
        private Dictionary<string, T> _dictionary = new();
        private readonly Dictionary<Type, object> _allCache = new();

        public Dictionary<string, T> Dictionary => _dictionary;

        protected void Load(Dictionary<string, T> dictionary)
        {
            _allCache.Clear();
            _dictionary = dictionary;

            System.Diagnostics.Debug.Assert(_dictionary != null, nameof(_dictionary) + " != null");

            foreach (var pair in _dictionary)
                AddToCache(pair.Value);
        }

        protected void Load(string text) =>
            Load(JsonConvert.DeserializeObject<Dictionary<string, T>>(text, App.JsonSettings));

        public TC Get<TC>(string id) where TC : T
        {
            if (_dictionary.TryGetValue(id, out var value) && value is TC t)
                return t;
            Debug.LogWarning($"'Couldn't find {typeof(TC).Name} id={id}");
            return default;
        }

        public IReadOnlyList<TC> Get<TC>() where TC : T =>
            _allCache.TryGetValue(typeof(TC), out var r) ? (IReadOnlyList<TC>)r : ArraySegment<TC>.Empty;

        protected TC Add<TC>(TC data, string id) where TC : T
        {
            _dictionary.Add(id, data);

            AddToCache(data);

            return data;
        }

        private void AddToCache(T data)
        {
            void AddToTypedCache(Type type)
            {
                if (!_allCache.TryGetValue(type, out var temp))
                    _allCache.Add(type, temp = Activator.CreateInstance(typeof(List<>).MakeGenericType(type)));
                ((IList)temp).Add(data);
            }

            var t = data.GetType();
            while (t is { IsClass: true })
            {
                AddToTypedCache(t);
                t = t.BaseType;
            }
        }

        public IEnumerator<T> GetEnumerator() => _dictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}