using System;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class TestCounterSignal
    {
        [Test]
        public void initial_value_greater_equal_one_IsSet_false()
        {
            var counterSignal = new CounterSignal(1);
            Assert.IsFalse(counterSignal.IsSet);
        }

        [Test]
        public void initial_value_greater_equal_one_initial_value_one_IsSet_false()
        {
            var counterSignal = new CounterSignal(1, 1);
            Assert.IsTrue(counterSignal.IsSet);
        }

        [Test]
        public void initial_value_signal_IsSet_true()
        {
            var counterSignal = new CounterSignal(1, 1);
            Assert.IsTrue(counterSignal.IsSet);
        }

        [Test]
        public void initial_value_signal_set_Wait()
        {
            var counterSignal = new CounterSignal(1, 1);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void initial_value_signal_set_Wait_TimeSpan()
        {
            var counterSignal = new CounterSignal(1, 1);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void initial_value_signal_not_set_IsSet_false()
        {
            var counterSignal = new CounterSignal(2, 1);
            Assert.IsFalse(counterSignal.IsSet);
        }

        [Test]
        public void initial_value_signal_not_set_Wait_CancellationToken()
        {
            var counterSignal = new CounterSignal(2, 1);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(15)).Token;
            Assert.Throws<OperationCanceledException>(() => counterSignal.Wait(cancellationToken));
        }

        [Test]
        public void initial_value_signal_not_set_Wait_TimeSpan_CancellationToken_timed_out()
        {
            var counterSignal = new CounterSignal(2, 1);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;
            Assert.IsFalse(counterSignal.Wait(TimeSpan.FromMilliseconds(15), cancellationToken));
        }

        [Test]
        public void initial_value_signal_not_set_Wait_TimeSpan_CancellationToken_cancelled()
        {
            var counterSignal = new CounterSignal(2, 1);
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromMilliseconds(15)).Token;
            Assert.Throws<OperationCanceledException>(() => counterSignal.Wait(TimeSpan.FromSeconds(1), cancellationToken));
        }

        [Test]
        public void initial_value_signal_not_set_Wait_TimeSpan_false()
        {
            var counterSignal = new CounterSignal(2, 1);
            Assert.IsFalse(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_increment_signal_IsSet_true()
        {
            var counterSignal = new CounterSignal(2, 1);
            counterSignal.Increment();
            Assert.IsTrue(counterSignal.IsSet);
        }

        [Test]
        public void counter_add_signal_IsSet_true()
        {
            var counterSignal = new CounterSignal(10, 1);
            counterSignal.Add(100);
            Assert.IsTrue(counterSignal.IsSet);
            Assert.IsTrue(counterSignal.Wait(TimeSpan.Zero));
        }

        [Test]
        public void counter_add_signal_IsSet_false()
        {
            var counterSignal = new CounterSignal(10, 0);
            counterSignal.Add(9);
            Assert.IsFalse(counterSignal.IsSet);
        }

        [Test]
        public void counter_increment_then_decrement_signal_IsSet_false()
        {
            var counterSignal = new CounterSignal(2, 1);
            counterSignal.Increment();
            counterSignal.Decrement();
            Assert.IsFalse(counterSignal.IsSet);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void CurrentValue_is_initial_value_same_after_ctor(int initialValue)
        {
            var counterSignal = new CounterSignal(0, initialValue);
            Assert.AreEqual(initialValue, counterSignal.CurrentValue);
        }

        [Test]
        public void CurrentValue_set_value()
        {
            var counterSignal = new CounterSignal(0) { CurrentValue = 1234 };
            Assert.AreEqual(1234, counterSignal.CurrentValue);
        }

        [Test]
        public void CurrentValue_incremented_on_Increment()
        {
            var counterSignal = new CounterSignal(0, 1);
            counterSignal.Increment();
            Assert.AreEqual(2, counterSignal.CurrentValue);
        }

        [Test]
        public void CurrentValue_decremented_on_Decrement()
        {
            var counterSignal = new CounterSignal(0, 2);
            counterSignal.Decrement();
            Assert.AreEqual(1, counterSignal.CurrentValue);
        }

        [Explicit]
        [Test]
        public void benchmark_get_value()
        {
            var counterSignal = new CounterSignal(1, 0);

            const int iterations = 10000000;
            var value = false;

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                value |= counterSignal.Wait(TimeSpan.Zero);
            }
            sw.Stop();

            if (value) Console.WriteLine(); // prevent too aggressive optimization

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }

        [Explicit]
        [Test]
        public void benchmark_increment_value()
        {
            var counterSignal = new CounterSignal(1, 0);

            const int iterations = 10000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                counterSignal.Increment();
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }

        [Explicit]
        [Test]
        public void benchmark_add_value()
        {
            var counterSignal = new CounterSignal(1, 0);

            const int iterations = 10000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                counterSignal.Add(1);
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, (decimal)iterations / sw.ElapsedMilliseconds * 1000m);
        }
    }
}
