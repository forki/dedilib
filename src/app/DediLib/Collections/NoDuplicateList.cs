using System.Collections;
using System.Collections.Generic;

namespace DediLib.Collections
{
    public class NoDuplicateList<T> : IList<T>
    {
        private List<T> _list;
        private HashSet<T> _hashSet;

        public int Count => _hashSet.Count;
        public int Capacity => _list.Capacity;

        public bool IsReadOnly => false;

        public NoDuplicateList()
        {
            Clear();
        }

        public NoDuplicateList(int capacity)
        {
            CreateWithCapacity(capacity);
        }

        public bool Add(T item)
        {
            var result = _hashSet.Add(item);
            if (result) _list.Add(item);
            return result;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Add(item);
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public void Clear()
        {
            _list = new List<T>();
            _hashSet = new HashSet<T>();
        }

        private void CreateWithCapacity(int capacity)
        {
            _list = new List<T>(capacity);
            _hashSet = new HashSet<T>();
        }

        public bool Contains(T item)
        {
            return _hashSet.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            if (!Contains(item)) return -1;
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            if (!_hashSet.Add(item)) return;
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            var oldValue = _list[index];
            _hashSet.Remove(oldValue);
            _list.RemoveAt(index);
        }

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
