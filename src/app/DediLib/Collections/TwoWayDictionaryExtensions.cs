using System;
using System.Collections.Generic;
using System.Runtime;

namespace DediLib.Collections
{
    public static class TwoWayDictionaryExtensions
    {
        [TargetedPatchingOptOut("")]
        public static TwoWayDictionary<TKey, TValue> ToTwoWayDictionary<TKey, TValue, T>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
            if (valueSelector == null) throw new ArgumentNullException(nameof(valueSelector));

            var result = new TwoWayDictionary<TKey, TValue>();
            foreach (var item in enumerable)
            {
                result[keySelector(item)] = valueSelector(item);
            }
            return result;
        }

        [TargetedPatchingOptOut("")]
        public static TwoWayDictionary<TKey, TValue> ToTwoWayDictionary<TKey, TValue>(this IEnumerable<TValue> enumerable, Func<TValue, TKey> keySelector)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            var result = new TwoWayDictionary<TKey, TValue>();
            foreach (var item in enumerable)
            {
                result[keySelector(item)] = item;
            }
            return result;
        }
    }
}