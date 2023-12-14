using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure
{
    public abstract class RuntimeCollection<T> : ScriptableObject
    {
        [SerializeField]
        private List<T> _items = new();

        public Action<T> ItemAdded;

        public Action<T> ItemRemoved;

        public void Add(T item) {
            if (_items.Contains(item))
                return;

            _items.Add(item);
            ItemAdded?.Invoke(item);
        }

        public void Remove(T item) {
            if (!_items.Contains(item))
                return;

            _items.Remove(item);
            ItemRemoved?.Invoke(item);
        }
    }
}
