using System;
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
    }
}
