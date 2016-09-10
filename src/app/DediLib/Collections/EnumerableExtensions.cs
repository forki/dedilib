using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace DediLib.Collections
{
    public static class EnumerableExtensions
    {
        [TargetedPatchingOptOut("")]
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

        [TargetedPatchingOptOut("")]
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }
        }

        [TargetedPatchingOptOut("")]
        public static async Task<List<T>> ToListAsync<T>(this IEnumerable<T> items)
        {
            return await ToListAsync(items, item => Task.FromResult(true)).ConfigureAwait(false);
        }

        [TargetedPatchingOptOut("")]
        public static async Task<List<T>> ToListAsync<T>(this IEnumerable<T> items, Func<T, Task<bool>> selector)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            var itemList = items.ToList();
            var taskList = itemList.Select(selector).ToList();

            await Task.WhenAll(taskList).ConfigureAwait(false);

            return Enumerable.Range(0, itemList.Count)
                .Where(i => taskList[i].Result)
                .Select(i => itemList[i])
                .ToList();
        }
    }
}
