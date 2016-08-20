using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DediLib.Collections
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> createFunc)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            TValue result;
            if (dictionary.TryGetValue(key, out result))
                return result;

            result = createFunc(key);
            dictionary[key] = result;
            return result;
        }

        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class
        {
            if (dictionary == null) return null;
            if (key == null) return null;

            TValue value;
            if (dictionary.TryGetValue(key, out value))
                return value;

            return null;
        }

        public static bool TryRemove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue dummy;
            return dictionary.TryRemove(key, out dummy);
        }
    }
}
