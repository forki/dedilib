using System;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class DistinctConcurrentQueueTests
    {
        private DistinctConcurrentQueue<string> _queue;

        [SetUp]
        public void SetUp()
        {
            _queue = new DistinctConcurrentQueue<string>(StringComparer.OrdinalIgnoreCase);
        }

        [Test]
        public void IsEmpty_Empty_True()
        {
            var isEmpty = _queue.IsEmpty;

            Assert.IsTrue(isEmpty);
        }

        [Test]
        public void IsEmpty_NotEmpty_False()
        {
            _queue.Enqueue("");

            var isEmpty = _queue.IsEmpty;

            Assert.IsFalse(isEmpty);
        }

        [Test]
        public void Enqueue_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _queue.Enqueue(null));
        }

        [Test]
        public void Enqueue_Value_True()
        {
            var enqueued = _queue.Enqueue("value");

            Assert.IsTrue(enqueued);
        }

        [Test]
        public void Enqueue_EqualValueTwice_False()
        {
            _queue.Enqueue("Value");

            var enqueued = _queue.Enqueue("value");

            Assert.IsFalse(enqueued);
        }

        [Test]
        public void Dequeue_NotExisting_False()
        {
            var dequeued = _queue.TryDequeue(out var _);

            Assert.IsFalse(dequeued);
        }

        [Test]
        public void Dequeue_Existing_True()
        {
            _queue.Enqueue("value");

            var dequeued = _queue.TryDequeue(out var value);

            Assert.IsTrue(dequeued);
            Assert.AreEqual("value", value);
        }

        [Test]
        public void EnqueueDequeue_Multiple_Works()
        {
            _queue.Enqueue("value1");
            _queue.Enqueue("Value1");
            _queue.Enqueue("VALUE1");
            _queue.Enqueue("Value2");
            _queue.Enqueue("value2");

            _queue.TryDequeue(out var value);
            Assert.AreEqual("value1", value);
            _queue.TryDequeue(out value);
            Assert.AreEqual("Value2", value);
            Assert.IsTrue(_queue.IsEmpty);

            _queue.Enqueue("value3");
            _queue.TryDequeue(out value);
            Assert.AreEqual("value3", value);
            Assert.IsTrue(_queue.IsEmpty);
        }
    }
}
