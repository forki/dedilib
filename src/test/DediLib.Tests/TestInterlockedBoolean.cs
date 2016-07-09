using System;
using System.Diagnostics;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class TestInterlockedBoolean
    {
        [Test]
        public void initial_value_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            Assert.IsFalse(interlockedBoolean.Value);
        }

        [Test]
        public void initial_value_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            Assert.IsTrue(interlockedBoolean.Value);
        }

        [Test]
        public void set_value_true_was_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.Set(true);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void set_value_true_was_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.Set(true);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Test]
        public void CompareExchange_true_was_false_compare_with_false()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.CompareExchange(true, false);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void CompareExchange_true_was_true_compare_with_false()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.CompareExchange(true, false);

            Assert.IsTrue(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Test]
        public void CompareExchange_false_was_false_compare_with_true()
        {
            var interlockedBoolean = new InterlockedBoolean();
            var oldValue = interlockedBoolean.CompareExchange(false, true);

            Assert.IsFalse(interlockedBoolean.Value);
            Assert.IsFalse(oldValue);
        }

        [Test]
        public void CompareExchange_false_was_true_compare_with_true()
        {
            var interlockedBoolean = new InterlockedBoolean(true);
            var oldValue = interlockedBoolean.CompareExchange(false, true);

            Assert.IsFalse(interlockedBoolean.Value);
            Assert.IsTrue(oldValue);
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void benchmark_get_value()
        {
            var interlockedBoolean = new InterlockedBoolean();

            const int iterations = 100000000;
            var value = false;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                value |= interlockedBoolean.Value;
            }
            sw.Stop();

            if (value) Console.WriteLine(); // prevent too aggressive optimization

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, iterations / sw.Elapsed.TotalMilliseconds * 1000);
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void benchmark_set_value()
        {
            var interlockedBoolean = new InterlockedBoolean();

            const int iterations = 100000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                interlockedBoolean.Set(true);
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, iterations / sw.Elapsed.TotalMilliseconds * 1000);
        }
    }
}
