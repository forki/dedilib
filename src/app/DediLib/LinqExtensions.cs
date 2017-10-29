using System.Collections.Generic;

// ReSharper disable CheckNamespace
namespace System.Linq
// ReSharper restore CheckNamespace
{
    public static class LinqExtensions
    {
        /// <summary>
        /// Tries to cast an enumerable into a list if possible, otherwise
        /// it creates a list of enumerable values (using '.ToList()')
        /// </summary>
        /// <typeparam name="T">Item type for enumerable</typeparam>
        /// <param name="enumerable">Enumerable collection of items</param>
        /// <returns>List of items</returns>
        public static IList<T> AsList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) return new List<T>();
            if (enumerable is IList<T> list) return list;
            return enumerable.ToList();
        }

        /// <summary>
        /// Calls an action for each item in an enumerable collection
        /// </summary>
        /// <typeparam name="T">Item type for enumerable</typeparam>
        /// <param name="enumerable">Enumerable collection of items</param>
        /// <param name="action">Action to perform on each item</param>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) return;
            if (action == null) return;

            foreach (var item in enumerable)
                action(item);
        }

        /// <summary>
        /// Gets the element with the minimum value for a given selector and 
        /// returns the element rather than the maximum value
        /// </summary>
        public static T MinElementOrDefault<T, TU>(this IEnumerable<T> enumerable, Func<T, TU> selector) where TU : IComparable
        {
            var maxElement = default(T);
            if (enumerable == null) return maxElement;
            using (var enumerator = enumerable.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return maxElement;
                maxElement = enumerator.Current;
                var maxValue = selector(enumerator.Current);

                while (enumerator.MoveNext())
                {
                    var currValue = selector(enumerator.Current);
                    if (currValue.CompareTo(maxValue) >= 0) continue;

                    maxElement = enumerator.Current;
                    maxValue = currValue;
                }
            }
            return maxElement;
        }

        /// <summary>
        /// Gets the element with the maximum value for a given selector and 
        /// returns the element rather than the maximum value
        /// </summary>
        public static T MaxElementOrDefault<T, TU>(this IEnumerable<T> enumerable, Func<T, TU> selector) where TU : IComparable
        {
            var maxElement = default(T);
            if (enumerable == null) return maxElement;
            using (var enumerator = enumerable.GetEnumerator())
            {
                if (!enumerator.MoveNext()) return maxElement;
                maxElement = enumerator.Current;
                var maxValue = selector(enumerator.Current);

                while (enumerator.MoveNext())
                {
                    var currValue = selector(enumerator.Current);
                    if (currValue.CompareTo(maxValue) <= 0) continue;

                    maxElement = enumerator.Current;
                    maxValue = currValue;
                }
            }
            return maxElement;
        }

        /// <summary>
        /// Splits a large enumerable into multiple lists (ensuring maximum number of elements)
        /// </summary>
        /// <typeparam name="T">type of objects in enumerable</typeparam>
        /// <param name="objects">enumerble of objects</param>
        /// <param name="batchSize">maximum number of elements in each list (has to be greater than 0)</param>
        /// <returns>one or more lists of objects</returns>
        public static IEnumerable<List<T>> Split<T>(this IEnumerable<T> objects, int batchSize)
        {
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, "batchSize must be greater than 0");
            if (objects == null) yield break;

            var list = new List<T>(batchSize);
            foreach (var obj in objects)
            {
                list.Add(obj);
                if (list.Count < batchSize) continue;

                yield return list;
                list = new List<T>(batchSize);
            }

            if (list.Count > 0) yield return list;
        }

        public static IEnumerable<List<T>> Partition<T>(this IList<T> source, int size)
        {
            for (var i = 0; i < Math.Ceiling(source.Count / (double)size); i++)
                yield return new List<T>(source.Skip(size * i).Take(size));
        }
    }
}
