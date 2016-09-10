using System;
using System.Runtime;
using System.Threading;

namespace DediLib
{
    /// <summary>
    /// Counter that can be waited on for reaching a certain value
    /// </summary>
    public class CounterSignal
    {
        private long _counter;
        private readonly long _signalGreaterOrEqual;

        private readonly ManualResetEventSlim _counterSignal = new ManualResetEventSlim();

        /// <summary>
        /// Determines if signal is set (meaning CurrentValue >= signalGreaterOrEqual)
        /// </summary>
        public bool IsSet => _counterSignal.IsSet;

        /// <summary>
        /// Current counter value
        /// </summary>
        public long CurrentValue
        {
            get { return _counter; }
            set { _counter = value; }
        }

        public CounterSignal(long signalGreaterOrEqual)
            : this(signalGreaterOrEqual, 0)
        {
        }

        public CounterSignal(long signalGreaterOrEqual, long initialCounterValue)
        {
            _counter = initialCounterValue;
            _signalGreaterOrEqual = signalGreaterOrEqual;

            if (initialCounterValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
        }

        /// <summary>
        /// Add a value to counter
        /// </summary>
        /// <returns>new value after adding to the current value</returns>
        [TargetedPatchingOptOut("")]
        public long Add(long value)
        {
            var newValue = Interlocked.Add(ref _counter, value);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset(); 
            return newValue;
        }

        /// <summary>
        /// Increment counter value
        /// </summary>
        /// <returns>new value after incrementing the current value</returns>
        [TargetedPatchingOptOut("")]
        public long Increment()
        {
            var newValue = Interlocked.Increment(ref _counter);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
            return newValue;
        }

        /// <summary>
        /// Decrement counter value
        /// </summary>
        /// <returns>new value after decrementing the current value</returns>
        [TargetedPatchingOptOut("")]
        public long Decrement()
        {
            var newValue = Interlocked.Decrement(ref _counter);
            if (newValue >= _signalGreaterOrEqual) _counterSignal.Set();
            else _counterSignal.Reset();
            return newValue;
        }

        [TargetedPatchingOptOut("")]
        public bool Wait()
        {
            return Wait(TimeSpan.FromMilliseconds(-1));
        }

        [TargetedPatchingOptOut("")]
        public bool Wait(TimeSpan timeout)
        {
            return _counterSignal.Wait(timeout);
        }

        [TargetedPatchingOptOut("")]
        public void Wait(CancellationToken cancellationToken)
        {
            _counterSignal.Wait(cancellationToken);
        }

        [TargetedPatchingOptOut("")]
        public bool Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            return _counterSignal.Wait(timeout, cancellationToken);
        }
    }
}
