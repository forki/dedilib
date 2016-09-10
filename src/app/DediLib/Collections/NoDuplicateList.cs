using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;

namespace DediLib.Collections
{
    public class NoDuplicateList<T> : IList<T>
    {
        private List<T> _list;
        private HashSet<T> _hashSet;

        public int Count => _hashSet.Count;
        public int Capacity => _list.Capacity;

        public bool IsReadOnly => false;

        private readonly Action _clear;

        [TargetedPatchingOptOut("")]
        public NoDuplicateList()
        {
            _clear = () =>
            {
                _list = new List<T>();
                _hashSet = new HashSet<T>();
            };
            Clear();
        }

        [TargetedPatchingOptOut("")]
        public NoDuplicateList(IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null) throw new ArgumentNullException(nameof(equalityComparer));

            _clear = () =>
            {
                _list = new List<T>();
                _hashSet = new HashSet<T>(equalityComparer);
            };
            Clear();
        }

        [TargetedPatchingOptOut("")]
        public NoDuplicateList(int capacity)
        {
            _clear = () =>
            {
                _list = new List<T>(capacity);
                _hashSet = new HashSet<T>();
            };
            Clear();
        }

        [TargetedPatchingOptOut("")]
        public bool Add(T item)
        {
            var result = _hashSet.Add(item);
            if (result) _list.Add(item);
            return result;
        }

        [TargetedPatchingOptOut("")]
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                Add(item);
        }

        [TargetedPatchingOptOut("")]
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        [TargetedPatchingOptOut("")]
        public void Clear()
        {
            _clear();
        }

        [TargetedPatchingOptOut("")]
        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        [TargetedPatchingOptOut("")]
        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        [TargetedPatchingOptOut("")]
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        [TargetedPatchingOptOut("")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [TargetedPatchingOptOut("")]
        public int IndexOf(T item)
        {
            if (!Contains(item)) return -1;
            return _list.IndexOf(item);
        }

        [TargetedPatchingOptOut("")]
        public void Insert(int index, T item)
        {
            if (!_hashSet.Add(item)) return;
            _list.Insert(index, item);
        }

        [TargetedPatchingOptOut("")]
        public void RemoveAt(int index)
        {
            var oldValue = _list[index];
            _hashSet.Remove(oldValue);
            _list.RemoveAt(index);
        }

        [TargetedPatchingOptOut("")]
        public bool Remove(T item)
        {
            if (_hashSet.Remove(item))
                return _list.Remove(item);
            return false;
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                var oldValue = _list[index];
                _hashSet.Remove(oldValue);
                _list[index] = value;
                _hashSet.Add(value);
            }
        }
    }
}
