using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace DediLib.Collections
{
    public sealed class BucketQueue<T>
    {
        private readonly int _numberOfBuckets;

        private readonly ConcurrentQueue<T>[] _buckets;

        private int _position;

        public BucketQueue(int numberOfBuckets)
        {
            if (numberOfBuckets <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfBuckets));

            _numberOfBuckets = numberOfBuckets;
            _buckets = Enumerable.Range(0, _numberOfBuckets).Select(i => new ConcurrentQueue<T>()).ToArray();
        }

        public bool TryDequeue(out T value)
        {
            var initialIndex = Interlocked.Increment(ref _position) % _numberOfBuckets;
            var index = initialIndex;
            do
            {
                var bucket = _buckets[index % _numberOfBuckets];
                if (bucket.TryDequeue(out value))
                {
                    return true;
                }
                index = ++index % _numberOfBuckets;

            } while (index != initialIndex);

            return false;
        }

        public void Enqueue(int hash, T value)
        {
            var bucketIndex = hash % _numberOfBuckets;
            _buckets[bucketIndex].Enqueue(value);
        }
    }
}
