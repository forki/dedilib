using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DediLib.Collections
{
    [Serializable]
    public class TimedDictionary<TKey, TValue> : ITimedDictionary, IDictionary<TKey, TValue>, IDisposable
    {
        private readonly TimeSpan _overrideDefaultExpiry = TimeSpan.FromMilliseconds(-1);
        private readonly ConcurrentDictionary<TKey, TimedValue<TValue>> _dict;

        public TimeSpan DefaultExpiry { get; set; }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (!_dict.TryAdd(item.Key, new TimedValue<TValue>(item.Value, DefaultExpiry)))
                throw new ArgumentException("Duplicate key");
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            TimedValue<TValue> timedValue;
            if (!TryGetValue(item.Key, out timedValue))
                return false;

            return Equals(item.Value, timedValue.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var item in _dict)
            {
                array[i++] = new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TimedValue<TValue> timedValue;
            if (!TryGetValue(item.Key, out timedValue))
                return false;

            return Equals(item.Value, timedValue.Value);
        }

        public int Count => _dict.Count;
        public bool IsReadOnly => false;

        /// <summary>
        /// Period when to clean up expired values (however granularity >1 sec)
        /// </summary>
        public TimeSpan CleanUpPeriod { get; set; }

        #region Dispose

        private bool _disposed;
        private readonly object _disposeLock = new object();

        protected virtual void Dispose(bool disposing)
        {
            lock (_disposeLock)
            {
                if (!_disposed)
                {
                    TimedDictionaryWorker.Unregister(this);

                    _disposed = true;
                    if (disposing) GC.SuppressFinalize(this);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();
        }

        ~TimedDictionary()
        {
            Dispose(false);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public TimedDictionary()
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>();

            CleanUpPeriod = TimeSpan.FromSeconds(1);
            TimedDictionaryWorker.Register(this);
        }

        public TimedDictionary(TimeSpan defaultExpiry)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>();

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, TimeSpan cleanUpPeriod)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>();

            Initialize(defaultExpiry, cleanUpPeriod);
        }

        public TimedDictionary(int concurrencyLevel, int capacity)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, capacity);

            Initialize(_overrideDefaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, int concurrencyLevel, int capacity)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, capacity);

            Initialize(defaultExpiry);
        }

        public TimedDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, capacity, comparer);

            Initialize(TimeSpan.FromMilliseconds(-1));
        }

        public TimedDictionary(TimeSpan defaultExpiry, int concurrencyLevel, int capacity,
            IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, capacity, comparer);

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _dict =
                new ConcurrentDictionary<TKey, TimedValue<TValue>>(
                    collection.Select(
                        x =>
                            new KeyValuePair<TKey, TimedValue<TValue>>(x.Key,
                                new TimedValue<TValue>(x.Value, defaultExpiry))));

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> comparer)
        {
            _dict =
                new ConcurrentDictionary<TKey, TimedValue<TValue>>(
                    collection.Select(
                        x =>
                            new KeyValuePair<TKey, TimedValue<TValue>>(x.Key,
                                new TimedValue<TValue>(x.Value, defaultExpiry))), comparer);

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, int concurrencyLevel,
            IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel,
                collection.Select(
                    x =>
                        new KeyValuePair<TKey, TimedValue<TValue>>(x.Key, new TimedValue<TValue>(x.Value, defaultExpiry))),
                comparer);

            Initialize(defaultExpiry);
        }

        private void Initialize(TimeSpan defaultExpiry)
        {
            var cleanUpPeriod =
                defaultExpiry == _overrideDefaultExpiry
                    ? TimeSpan.FromSeconds(1)
                    : TimeSpan.FromSeconds(defaultExpiry.TotalSeconds / 10);

            if (cleanUpPeriod > TimeSpan.FromMinutes(1))
                cleanUpPeriod = TimeSpan.FromMinutes(1);

            Initialize(defaultExpiry, cleanUpPeriod);
        }

        private void Initialize(TimeSpan defaultExpiry, TimeSpan cleanUpPeriod)
        {
            CleanUpPeriod = cleanUpPeriod;
            DefaultExpiry = defaultExpiry;
            TimedDictionaryWorker.Register(this);
        }

        private readonly object _cleanUpLock = new object();

        public void CleanUp()
        {
            lock (_cleanUpLock)
            {
                foreach (var pair in _dict)
                    if (DateTime.UtcNow - pair.Value.LastAccessUtc > pair.Value.Expiry)
                        TryRemove(pair.Key);
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return TryAdd(key, value, DefaultExpiry);
        }

        public bool TryAdd(TKey key, TValue value, TimeSpan expires)
        {
            return _dict.TryAdd(key, new TimedValue<TValue>(value, expires));
        }

        public bool TryRemove(TKey key)
        {
            TValue value;
            return TryRemove(key, out value);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            TimedValue<TValue> timedValue;
            if (!_dict.TryRemove(key, out timedValue))
            {
                value = default(TValue);
                return false;
            }
            value = timedValue.Value;
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value, bool updateAccessTime = true)
        {
            TimedValue<TValue> timedValue;
            if (!_dict.TryGetValue(key, out timedValue))
            {
                value = default(TValue);
                return false;
            }

            if (updateAccessTime) timedValue.UpdateAccessTime();
            value = timedValue.Value;
            return true;
        }

        public bool TryGetValue(TKey key, out TimedValue<TValue> value, bool updateAccessTime = true)
        {
            TimedValue<TValue> timedValue;
            if (!_dict.TryGetValue(key, out timedValue))
            {
                value = null;
                return false;
            }

            if (updateAccessTime) timedValue.UpdateAccessTime();
            value = timedValue;
            return true;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, bool updateAccessTime = true)
        {
            return GetOrAdd(key, valueFactory, DefaultExpiry, updateAccessTime);
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory, TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.GetOrAdd(key, k => new TimedValue<TValue>(valueFactory(k), expires));
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public TValue GetOrAdd(TKey key, TValue value, bool updateAccessTime = true)
        {
            return GetOrAdd(key, value, DefaultExpiry, updateAccessTime);
        }

        public TValue GetOrAdd(TKey key, TValue value, TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.GetOrAdd(key, new TimedValue<TValue>(value, expires));
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory,
            bool updateAccessTime = true)
        {
            return AddOrUpdate(key, addValue, updateValueFactory, DefaultExpiry, updateAccessTime);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory,
            TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.AddOrUpdate(key, new TimedValue<TValue>(addValue, expires), (k, v) =>
            {
                v.Value = updateValueFactory(k, v.Value);
                return v;
            });
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory, bool updateAccessTime = true)
        {
            return AddOrUpdate(key, addValueFactory, updateValueFactory, DefaultExpiry, updateAccessTime);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory, TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.AddOrUpdate(key, k => new TimedValue<TValue>(addValueFactory(k), expires), (k, v) =>
            {
                v.Value = updateValueFactory(k, v.Value);
                return v;
            });
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            if (!_dict.TryAdd(key, new TimedValue<TValue>(value, DefaultExpiry)))
                throw new ArgumentException("Duplicate key");
        }

        public bool Remove(TKey key)
        {
            TimedValue<TValue> timedValue;
            return _dict.TryRemove(key, out timedValue);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            TimedValue<TValue> timedValue;
            if (!_dict.TryGetValue(key, out timedValue))
            {
                value = default(TValue);
                return false;
            }

            value = timedValue.Value;
            return true;
        }

        public TValue this[TKey key]
        {
            get { return _dict[key].Value; }
            set { _dict[key] = new TimedValue<TValue>(value, DefaultExpiry); }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dict.Keys;

        public ICollection<TValue> Values
            => new CollectionWrapper<TimedValue<TValue>, TValue>(_dict.Values, tv => tv.Value,
                v => new TimedValue<TValue>(v, DefaultExpiry));
    }
}
