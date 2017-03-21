using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestBucketQueue
    {
        [TestCase(-1)]
        [TestCase(0)]
        public void Constructor_InvalidBucketSize_Throws(int bucketSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BucketQueue<object>(bucketSize));
        }

        [Test]
        public void Dequeue_NonExistingItem_Works()
        {
            // Arrange
            var bucket = new BucketQueue<string>(1);

            // Act
            string value;
            var result = bucket.TryDequeue(out value);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(value, Is.Null);
        }

        [Test]
        public void Dequeue_SingleItem_Works()
        {
            // Arrange
            var bucket = new BucketQueue<string>(1);
            bucket.Enqueue(0, "test");

            // Act
            string value;
            var result = bucket.TryDequeue(out value);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(value, Is.EqualTo("test"));
        }

        [Test]
        public void Dequeue_MultipleItems_Works()
        {
            // Arrange
            var bucket = new BucketQueue<string>(2);
            bucket.Enqueue(0, "0");
            bucket.Enqueue(1, "1");
            bucket.Enqueue(2, "2");

            // Act / Assert
            string value;
            Assert.That(bucket.TryDequeue(out value), Is.True);
            Assert.That(value, Is.EqualTo("1"));
            Assert.That(bucket.TryDequeue(out value), Is.True);
            Assert.That(value, Is.EqualTo("0"));
            Assert.That(bucket.TryDequeue(out value), Is.True);
            Assert.That(value, Is.EqualTo("2"));
            Assert.That(bucket.TryDequeue(out value), Is.False);
        }

        [Test]
        public void Dequeue_Concurrently_Works()
        {
            // Arrange
            var bucket = new BucketQueue<string>(2);
            var count = 10000;
            var range = Enumerable.Range(0, count).ToList();
            range.ForEach(i => bucket.Enqueue(i, i.ToString()));

            var dequeuedItems = new ConcurrentBag<string>();

            // Act
            Enumerable.Range(0, 2 * count).AsParallel().ForAll(i =>
            {
                string value;
                if (bucket.TryDequeue(out value))
                {
                    dequeuedItems.Add(value);
                }
            });

            // Assert
            var rangeAsString = range.Select(x => x.ToString()).ToList();
            Assert.True(new HashSet<string>(dequeuedItems).SetEquals(rangeAsString));
        }

        [Category("Benchmark")]
        [Explicit]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void Benchmark(int buckets)
        {
            var count = 2000000;
            var bucket = new BucketQueue<string>(buckets);
            var range = Enumerable.Range(0, count).ToList();
            range.ForEach(i => bucket.Enqueue(i, i.ToString()));

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                string value;
                bucket.TryDequeue(out value);
            };
            sw.Stop();

            Assert.Inconclusive("{0} ops/sec", count / sw.Elapsed.Add(TimeSpan.FromTicks(1)).TotalSeconds);
        }
    }
}