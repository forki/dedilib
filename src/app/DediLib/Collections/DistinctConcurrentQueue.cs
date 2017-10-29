using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DediLib.Collections
{
    public class DistinctConcurrentQueue<T>
    {
        private readonly ConcurrentDictionary<T, object> _dictionary;
        private readonly ConcurrentQueue<T> _queue;

        public DistinctConcurrentQueue()
        {
            _queue = new ConcurrentQueue<T>();
            _dictionary = new ConcurrentDictionary<T, object>();
        }

        public DistinctConcurrentQueue(IEqualityComparer<T> comparer)
        {
            _queue = new ConcurrentQueue<T>();
            _dictionary = new ConcurrentDictionary<T, object>(comparer);
        }

        public bool IsEmpty => _queue.IsEmpty;

        public bool Enqueue(T item)
        {
            if (!_dictionary.TryAdd(item, null))
            {
                return false;
            }
            _queue.Enqueue(item);
            return true;
        }

        public bool TryDequeue(out T item)
        {
            if (_queue.TryDequeue(out item))
            {
                object dummy;
                _dictionary.TryRemove(item, out dummy);
                return true;
            }
            return false;
        }
    }
}
