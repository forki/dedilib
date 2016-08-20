using System;
using System.Collections.Generic;

namespace DediLib.Collections
{
    public static class EnumerableExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> itemsToAdd)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (itemsToAdd == null)
            {
                return;
            }

            foreach (T item in itemsToAdd)
            {
                collection.Add(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }
    }
}
