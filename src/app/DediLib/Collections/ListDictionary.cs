using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;

namespace DediLib.Collections
{
    /// <summary>
    /// Dictionary with multiple values per key (not thread-safe)
    /// </summary>
    /// <typeparam name="TKey">key type</typeparam>
    /// <typeparam name="TValue">value type</typeparam>
    public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        readonly Dictionary<TKey, TValue> _singleItems;
        readonly Dictionary<TKey, List<TValue>> _multiItems;

        int _valuesCount;
        readonly List<TKey> _allKeys;
        readonly List<TValue> _allValues;
        bool _updateKeysList = true;
        bool _updateValuesList = true;

        /// <summary>
        /// Dictionary is not read-only
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Constructor
        /// </summary>
        [TargetedPatchingOptOut("")]
        public ListDictionary()
            : this(100)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">initial capacity</param>
        [TargetedPatchingOptOut("")]
        public ListDictionary(int capacity)
        {
            _singleItems = new Dictionary<TKey, TValue>(capacity);
            _multiItems = new Dictionary<TKey, List<TValue>>(capacity);

            _allKeys = new List<TKey>(capacity);
            _allValues = new List<TValue>(capacity);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keyComparer">key comparer</param>
        [TargetedPatchingOptOut("")]
        public ListDictionary(IEqualityComparer<TKey> keyComparer)
        {
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));

            _singleItems = new Dictionary<TKey, TValue>(keyComparer);
            _multiItems = new Dictionary<TKey, List<TValue>>(keyComparer);

            _allKeys = new List<TKey>();
            _allValues = new List<TValue>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="capacity">initial capacity</param>
        /// <param name="keyComparer">key comparer</param>
        [TargetedPatchingOptOut("")]
        public ListDictionary(int capacity, IEqualityComparer<TKey> keyComparer)
        {
            if (keyComparer == null) throw new ArgumentNullException(nameof(keyComparer));

            _singleItems = new Dictionary<TKey, TValue>(capacity, keyComparer);
            _multiItems = new Dictionary<TKey, List<TValue>>(capacity, keyComparer);

            _allKeys = new List<TKey>(capacity);
            _allValues = new List<TValue>(capacity);
        }

        /// <summary>
        /// Add key/value pair to IndexDictionary
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        [TargetedPatchingOptOut("")]
        public void Add(TKey key, TValue value)
        {
            TValue existingItem;
            List<TValue> existingList;

            if (_singleItems.TryGetValue(key, out existingItem))
            {
                // convert single item to list
                existingList = new List<TValue>();
                _multiItems.Add(key, existingList);
                _singleItems.Remove(key);
                existingList.Add(existingItem);
                existingList.Add(value);
                if (!_updateValuesList) _allValues.Add(value);
                if (_valuesCount >= 0) _valuesCount++;
                return;
            }

            if (_multiItems.TryGetValue(key, out existingList))
            {
                existingList.Add(value);
                if (!_updateValuesList) _allValues.Add(value);
                if (_valuesCount >= 0) _valuesCount++;
                return;
            }

            // add new single item
            _singleItems.Add(key, value);
            if (_valuesCount >= 0) _valuesCount++;
            if (!_updateKeysList) _allKeys.Add(key);
            if (!_updateValuesList) _allValues.Add(value);
        }

        /// <summary>
        /// Add multiple key/value pairs to IndexDictionary
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="values">values</param>
        [TargetedPatchingOptOut("")]
        public void AddMany(TKey key, ICollection<TValue> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            var count = values.Count;
            if (count == 0) return;
            if (count == 1)
            {
                Add(key, values.First());
                return;
            }

            TValue existingItem;
            List<TValue> existingList;

            if (_singleItems.TryGetValue(key, out existingItem))
            {
                // convert single item to list
                existingList = new List<TValue>();
                _multiItems.Add(key, existingList);
                _singleItems.Remove(key);
                existingList.Add(existingItem);
                existingList.AddRange(values);
                if (!_updateValuesList) _allValues.AddRange(values);
                if (_valuesCount >= 0) _valuesCount += count;
                return;
            }

            if (!_multiItems.TryGetValue(key, out existingList))
            {
                existingList = new List<TValue>();
                _multiItems[key] = existingList;
            }

            // add items to existing list
            existingList.AddRange(values);
            if (!_updateValuesList) _allValues.AddRange(values);
            if (_valuesCount >= 0) _valuesCount += count;
        }

        /// <summary>
        /// Add key/value pair to IndexDictionary
        /// </summary>
        /// <param name="item">key/value pair</param>
        [TargetedPatchingOptOut("")]
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Adds multiple key/value pair values at once
        /// </summary>
        /// <param name="range">collection of values to add</param>
        [TargetedPatchingOptOut("")]
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> range)
        {
            foreach (var item in range)
                Add(item.Key, item.Value);
        }

        /// <summary>
        /// Remove all values for a specific key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>true, if values have been removed</returns>
        [TargetedPatchingOptOut("")]
        public bool Remove(TKey key)
        {
            var removed = _singleItems.Remove(key);
            if (!removed)
            {
                removed = _multiItems.Remove(key);
                if (!removed) return false;

                _valuesCount = -1; // to be updated if needed
                _updateKeysList = true;
                _updateValuesList = true;
            }
            else
            {
                if (_valuesCount >= 0) _valuesCount--;
                _updateKeysList = true;
                _updateValuesList = true;
            }
            return true;
        }

        /// <summary>
        /// Remove a key and value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>true, if key/value pair has been removed</returns>
        [TargetedPatchingOptOut("")]
        public bool Remove(TKey key, TValue value)
        {
            TValue existingItem;
            bool removed;

            if (!_singleItems.TryGetValue(key, out existingItem))
            {
                List<TValue> existingList;
                if (!_multiItems.TryGetValue(key, out existingList)) return false; // item not found

                removed = existingList.Remove(value);
                if (!removed) return false;

                if (_valuesCount >= 0) _valuesCount--;
                if (existingList.Count != 1) return true;

                // convert multi item to single item
                _singleItems[key] = existingList.First();
                _multiItems.Remove(key);
                _updateValuesList = true; // values list is to be updated
                return true;
            }

            // check if single value matches the value to be removed)
            if (!Equals(existingItem, value)) return false;

            removed = _singleItems.Remove(key);
            if (removed && _valuesCount >= 0) _valuesCount--;
            return removed;
        }

        /// <summary>
        /// Remove a key/value pair
        /// </summary>
        /// <param name="item">key/value pair</param>
        /// <returns>true, if key/value pair has been removed</returns>
        [TargetedPatchingOptOut("")]
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key, item.Value);
        }

        /// <summary>
        /// Check if dictionary contains a key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>true, if dictionary contains the specific key</returns>
        [TargetedPatchingOptOut("")]
        public bool ContainsKey(TKey key)
        {
            return _singleItems.ContainsKey(key) || _multiItems.ContainsKey(key);
        }

        /// <summary>
        /// Check if dictionary contains a key/value pair
        /// </summary>
        /// <param name="item">key</param>
        /// <returns>true, if dictionary contains the specific key</returns>
        [TargetedPatchingOptOut("")]
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TValue existingItem;

            if (_singleItems.TryGetValue(item.Key, out existingItem))
                return _singleItems.Contains(item);

            List<TValue> existingList;
            return _multiItems.TryGetValue(item.Key, out existingList) && existingList.Contains(item.Value);
        }

        /// <summary>
        /// Gets a list of keys
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                if (_updateKeysList) UpdateAllKeys();
                return new ReadOnlyCollection<TKey>(_allKeys);
            }
        }

        /// <summary>
        /// Try to get the first single value of a specific key
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns></returns>
        [TargetedPatchingOptOut("")]
        public bool TryGetValue(TKey key, out TValue value)
        {
            TValue existingItem;
            List<TValue> existingList;

            // check if there is a single item
            if (_singleItems.TryGetValue(key, out existingItem))
            {
                value = existingItem;
                return true;
            }

            // check if there are multiple items
            if (!_multiItems.TryGetValue(key, out existingList))
            {
                // item not found
                value = default(TValue);
                return false;
            }

            value = existingList.First();
            return true;
        }

        /// <summary>
        /// Gets a list of all values
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                if (_updateValuesList) UpdateAllValues();
                return new ReadOnlyCollection<TValue>(_allValues);
            }
        }

        /// <summary>
        /// Indexer
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        /// <exception cref="KeyNotFoundException">The given key was not present in the dictionary.</exception>
        public TValue this[TKey key]
        {
            get
            {
                TValue itemValue;
                if (TryGetValue(key, out itemValue)) return itemValue;
                throw new KeyNotFoundException("The given key was not present in the dictionary.");
            }
            set
            {
                TValue existingItem;
                List<TValue> existingList;

                // check if there is a single item
                if (_singleItems.TryGetValue(key, out existingItem))
                {
                    _singleItems[key] = value;
                    _updateValuesList = true; // to be updated if needed
                    return;
                }

                // check if there are multiple items
                if (_multiItems.TryGetValue(key, out existingList))
                {
                    // overwrite multiple items with single item
                    if (_valuesCount >= 0) _valuesCount -= existingList.Count - 1;
                    _multiItems.Remove(key);
                    _singleItems[key] = value;
                    _updateValuesList = true; // to be updated if needed
                    return;
                }

                // add single item
                _singleItems.Add(key, value);
                if (!_updateKeysList) _allKeys.Add(key);
                if (!_updateValuesList) _allValues.Add(value);
                if (_valuesCount >= 0) _valuesCount++;
            }
        }

        /// <summary>
        /// Clear collection
        /// </summary>
        [TargetedPatchingOptOut("")]
        public void Clear()
        {
            _multiItems.Clear();
            _singleItems.Clear();
            _allKeys.Clear();
            _allValues.Clear();
            _valuesCount = 0;
        }

        /// <summary>
        /// Copy key/pair values to 
        /// </summary>
        /// <param name="array">array to copy to</param>
        /// <param name="arrayIndex">array index to start with</param>
        [TargetedPatchingOptOut("")]
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int index = arrayIndex;
            foreach (var pair in this)
                array[index++] = pair;
        }

        /// <summary>
        /// Gets the number of values in dictionary
        /// </summary>
        public int Count
        {
            get
            {
                if (_valuesCount < 0) _valuesCount = GetValuesCount();
                return _valuesCount;
            }
        }

        /// <summary>
        /// Get all values for specific key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>collection of values</returns>
        [TargetedPatchingOptOut("")]
        public IEnumerable<TValue> GetValues(TKey key)
        {
            TValue existingItem;
            List<TValue> existingList;

            if (_singleItems.TryGetValue(key, out existingItem))
            {
                yield return existingItem;
                yield break;
            }

            if (!_multiItems.TryGetValue(key, out existingList))
            {
                // no items found for key
                yield break;
            }

            foreach (var value in existingList)
                yield return value;
        }

        /// <summary>
        /// Update values count
        /// </summary>
        /// <returns>number of values in dictionary</returns>
        [TargetedPatchingOptOut("")]
        private int GetValuesCount()
        {
            var count = _singleItems.Count;
            foreach (var list in _multiItems.Values)
                count += list.Count;
            return count;
        }

        /// <summary>
        /// Update internal list of all keys
        /// </summary>
        [TargetedPatchingOptOut("")]
        private void UpdateAllKeys()
        {
            _allKeys.Clear();
            _allKeys.AddRange(_singleItems.Keys);
            _allKeys.AddRange(_multiItems.Keys);
        }

        /// <summary>
        /// Update internal list of all values
        /// </summary>
        [TargetedPatchingOptOut("")]
        private void UpdateAllValues()
        {
            _allValues.Clear();
            _allValues.AddRange(_singleItems.Values);

            foreach (var list in _multiItems)
                foreach (var item in list.Value)
                    _allValues.Add(item);
        }

        /// <summary>
        /// Get generic enumerator for all KeyValuePairs of dictionary
        /// </summary>
        /// <returns>Generic enumerator for all KeyValuePairs of dictionary</returns>
        [TargetedPatchingOptOut("")]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in _singleItems)
                yield return item;

            foreach (var item in _multiItems)
                foreach (var value in item.Value)
                    yield return new KeyValuePair<TKey, TValue>(item.Key, value);
        }

        /// <summary>
        /// Get enumerator interface for all KeyValuePairs of dictionary
        /// </summary>
        /// <returns>Enumerator interface for all KeyValuePairs of dictionary</returns>
        [TargetedPatchingOptOut("")]
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
