using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.CodeBase.Infrastructure
{
    public abstract class RuntimeCollection<T> : ScriptableObject, IEnumerable<T>
    {
        [SerializeField] private List<T> _items = new();

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

        public IEnumerator<T> GetEnumerator() => new RuntimeCollectionEnumerator(_items);

        private IEnumerator GetEnumeratorPrivate() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorPrivate();

        private class RuntimeCollectionEnumerator : IEnumerator<T>
        {
            private readonly List<T> _items;

            private int position = -1;

            public RuntimeCollectionEnumerator(List<T> items) =>
                _items = items;

            public T Current => _items[position];

            private object CurrentPrivate => Current;
            object IEnumerator.Current => CurrentPrivate;

            public void Dispose() { }

            public bool MoveNext() {
                position++;
                return (position < _items.Count);
            }

            public void Reset() => position = -1;
        }
    }
}
