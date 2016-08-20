using System;
using System.Collections.Generic;

namespace DediLib.Collections
{
    /// <summary>
    /// Dictionary to encapsulate a 1:1 map for fast lookups in both directions
    /// </summary>
    public class TwoWayDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        private readonly Dictionary<TValue, TKey> _reverseDict;

        public TwoWayDictionary()
        {
            _reverseDict = new Dictionary<TValue, TKey>();
        }

        public TwoWayDictionary(IEqualityComparer<TKey> keyComparer)
            : base(keyComparer)
        {
            _reverseDict = new Dictionary<TValue, TKey>();
        }

        public TwoWayDictionary(IEqualityComparer<TValue> valueComparer)
        {
            _reverseDict = new Dictionary<TValue, TKey>(valueComparer);
        }

        public TwoWayDictionary(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            : base(keyComparer)
        {
            _reverseDict = new Dictionary<TValue, TKey>(valueComparer);
        }

        public TwoWayDictionary(IDictionary<TKey, TValue> dictionary)
            : base(dictionary)
        {
            _reverseDict = new Dictionary<TValue, TKey>(dictionary.Count);
            foreach (var pair in dictionary)
                _reverseDict.Add(pair.Value, pair.Key);
        }

        public TwoWayDictionary(int capacity)
            : base(capacity)
        {
            _reverseDict = new Dictionary<TValue, TKey>(capacity);
        }

        public new void Clear()
        {
            base.Clear();
            _reverseDict.Clear();
        }

        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            try
            {
                _reverseDict.Add(value, key);
            }
            catch
            {
                base.Remove(key);
                throw;
            }
        }

        public void Set(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            TValue previousValue;
            if (TryGetValue(key, out previousValue))
                _reverseDict.Remove(previousValue);

            base[key] = value;
            _reverseDict[value] = key;
        }

        public new bool ContainsValue(TValue value)
        {
            return _reverseDict.ContainsKey(value);
        }

        public new bool Remove(TKey key)
        {
            TValue value;
            if (!TryGetValue(key, out value)) return false;
            base.Remove(key);
            _reverseDict.Remove(value);
            return true;
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return _reverseDict.TryGetValue(value, out key);
        }

        public TKey GetKey(TValue value)
        {
            TKey key;
            if (!_reverseDict.TryGetValue(value, out key))
                throw new KeyNotFoundException();
            return key;
        }

        public new TValue this[TKey key]
        {
            get { return base[key]; }
            set
            {
                TValue valLeft;
                if (!TryGetValue(key, out valLeft))
                {
                    Add(key, value);
                    return;
                }

                var removed = _reverseDict.Remove(valLeft);
                try
                {
                    _reverseDict.Add(value, key);
                }
                catch (ArgumentException ex)
                {
                    if (removed) _reverseDict.Add(valLeft, key);
                    throw new ArgumentException("New value is already set to a different key. Only unique 1:1 mappings are allowed.", ex);
                }
                base[key] = value;
            }
        }
    }
}
