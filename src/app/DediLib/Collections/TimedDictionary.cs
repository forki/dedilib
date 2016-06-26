using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DediLib.Collections
{
    [Serializable]
    public class TimedDictionary<TKey, TValue> : ITimedDictionary, IDisposable
    {
        private readonly TimeSpan _overrideDefaultExpiry = TimeSpan.FromMilliseconds(-1);
        private readonly ConcurrentDictionary<TKey, TimedValue<TValue>> _dict;

        public TimeSpan DefaultExpiry { get; set; }

        public int Count { get { return _dict.Count; } }

        public IEnumerable<TKey> Keys { get { return _dict.Keys; } }

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

        ~TimedDictionary()
        {
            Dispose(false);
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

        public TimedDictionary(TimeSpan defaultExpiry, int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, capacity, comparer);

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(collection.Select(x => new KeyValuePair<TKey, TimedValue<TValue>>(x.Key, new TimedValue<TValue>(x.Value, defaultExpiry))));

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(collection.Select(x => new KeyValuePair<TKey, TimedValue<TValue>>(x.Key, new TimedValue<TValue>(x.Value, defaultExpiry))), comparer);

            Initialize(defaultExpiry);
        }

        public TimedDictionary(TimeSpan defaultExpiry, int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
        {
            _dict = new ConcurrentDictionary<TKey, TimedValue<TValue>>(concurrencyLevel, collection.Select(x => new KeyValuePair<TKey, TimedValue<TValue>>(x.Key, new TimedValue<TValue>(x.Value, defaultExpiry))), comparer);

            Initialize(defaultExpiry);
        }

        private void Initialize(TimeSpan defaultExpiry)
        {
            var cleanUpPeriod =
                defaultExpiry == _overrideDefaultExpiry ?
                TimeSpan.FromSeconds(1) :
                TimeSpan.FromSeconds(defaultExpiry.TotalSeconds / 10);

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
                    if ((DateTime.UtcNow - pair.Value.LastAccessUtc) > pair.Value.Expiry)
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

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, bool updateAccessTime = true)
        {
            return AddOrUpdate(key, addValue, updateValueFactory, DefaultExpiry, updateAccessTime);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.AddOrUpdate(key, new TimedValue<TValue>(addValue, expires), (k, v) => { v.Value = updateValueFactory(k, v.Value); return v; });
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, bool updateAccessTime = true)
        {
            return AddOrUpdate(key, addValueFactory, updateValueFactory, DefaultExpiry, updateAccessTime);
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan expires, bool updateAccessTime = true)
        {
            var timedValue = _dict.AddOrUpdate(key, k => new TimedValue<TValue>(addValueFactory(k), expires), (k, v) => { v.Value = updateValueFactory(k, v.Value); return v; });
            if (updateAccessTime) timedValue.UpdateAccessTime();
            return timedValue.Value;
        }

        public TValue this[TKey key]
        {
            get { return _dict[key].Value; }
            set { _dict[key] = new TimedValue<TValue>(value, DefaultExpiry); }
        }
    }
}
