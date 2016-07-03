// based on http://www.codeproject.com/Articles/27436/Remaining-Timer

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DediLib
{
    public class RemainingTimer
    {
        private struct TimedValue
        {
            private readonly long _timeStamp;
            private readonly double _value;

            public TimedValue(long timeStamp, double value)
            {
                _timeStamp = timeStamp;
                _value = value;
            }

            public double Value
            {
                get { return _value; }
            }

            public long TimeStamp
            {
                get { return _timeStamp; }
            }
        }

        private readonly LinkedList<TimedValue> _data; // Collected data
        private readonly Stopwatch _sw; // Elapsed time stopwatch
        private readonly object _syncLock = new object();
        private double _lastCorrelationCoefficient; // AKA r
        private TimeSpan _lastElapsed;
        private double _lastSlope; // AKA m
        private double _lastYint; // AKA b
        private bool _needToRecomputeEstimation; // only if there was a change from the last call.
        private double _targetValue;
        private TimeSpan _windowDuration;

        public RemainingTimer()
        {
            _sw = new Stopwatch();
            _data = new LinkedList<TimedValue>();
            _targetValue = 100.0;
            _windowDuration = new TimeSpan(0, 0, 45); // default window is 45 seconds.
            _needToRecomputeEstimation = true;
            _lastSlope = 0.0;
            _lastYint = 0.0;
            _lastCorrelationCoefficient = 0.0;
        }

        public double TargetValue
        {
            get { return _targetValue; }
            set { _targetValue = value; }
        }

        /// <summary>
        /// Returns the "r" value which can indicate how "good" is the estimation (the closer to 1.0 the better)
        /// </summary>
        public double Correlation
        {
            get { return _lastCorrelationCoefficient; }
        }

        public TimeSpan WindowDuration
        {
            get { return _windowDuration; }
            set { _windowDuration = value; }
        }

        public void Start()
        {
            lock (_syncLock)
            {
                _sw.Start();
            }
        }

        public void Pause()
        {
            lock (_syncLock)
            {
                _sw.Stop();
            }
        }

        public void StopAndReset()
        {
            lock (_syncLock)
            {
                _sw.Stop();
                _data.Clear();
                _needToRecomputeEstimation = true;
                _lastSlope = 0.0;
                _lastYint = 0.0;
                _lastCorrelationCoefficient = 0.0;
                _sw.Reset();
            }
        }

        public void Mark(double value)
        {
            var tv = new TimedValue(_sw.ElapsedMilliseconds, value);
            lock (_syncLock)
            {
                _data.AddFirst(tv);
                _needToRecomputeEstimation = true;
                ClearOutOfWindowData(); // remove out of window data
            }
        }

        public TimeSpan GetRemainingEstimation()
        {
            if (_needToRecomputeEstimation)
            {
                lock (_syncLock)
                {
                    ClearOutOfWindowData();
                    if (_data.Count != 0)
                    {
                        ComputeLinearCoefficients(); // compute linear coefficients using linear regression
                        var remainingMillis = (_targetValue - _lastYint) / _lastSlope; //  y = m*x+b --> (y-b)/m = x

                        if (double.IsNaN(remainingMillis) || double.IsInfinity(remainingMillis))
                            _lastElapsed = TimeSpan.MaxValue; // no data ot invalid data
                        else
                            _lastElapsed = TimeSpan.FromMilliseconds(remainingMillis);

                        _needToRecomputeEstimation = false; // until data list is changed again.
                    }
                }
            }

            if (_lastElapsed != TimeSpan.MaxValue) return _lastElapsed - _sw.Elapsed;

            return TimeSpan.MaxValue;
        }

        public override string ToString()
        {
            var ts = GetRemainingEstimation();

            if (ts == TimeSpan.MaxValue) return "Unavailable";

            var sb = new StringBuilder();
            if (ts.Days != 0)
            {
                sb.Append(ts.Days);
                sb.Append(".");
            }
            sb.Append(ts.Hours.ToString("D2"));
            sb.Append(":");
            sb.Append(ts.Minutes.ToString("D2"));
            sb.Append(":");
            sb.Append(ts.Seconds.ToString("D2"));
            return sb.ToString();
        }

        private void ClearOutOfWindowData()
        {
            lock (_syncLock)
            {
                if (_data.Count == 0) return;
                while (_data.First.Value.TimeStamp - _data.Last.Value.TimeStamp > (long)_windowDuration.TotalMilliseconds)
                {
                    _data.RemoveLast();
                    _needToRecomputeEstimation = true;

                    if (_data.Count == 0) return;
                }
            }
        }

        private void ComputeLinearCoefficients()
        {
            var sumTime = 0.0;
            var sumValue = 0.0;
            var sumValueTime = 0.0;
            var sumTime2 = 0.0;
            var sumValue2 = 0.0;
            var n = 0;

            lock (_syncLock)
            {
                IEnumerator<TimedValue> e = _data.GetEnumerator();
                while (e.MoveNext())
                {
                    double d = e.Current.TimeStamp;
                    var val = e.Current.Value;
                    sumTime += d;
                    sumTime2 += d * d;
                    sumValue += val;
                    sumValue2 += val * val;
                    sumValueTime += val * d;
                    n++;
                }
            }

            if (n == 0)
            {
                // no data at all
                _lastSlope = 0.0;
                _lastYint = 0.0;
                _lastCorrelationCoefficient = 0.0;
                return;
            }

            var sum2Time = sumTime * sumTime;
            var sum2Value = sumValue * sumValue;
            double nDouble = n;

            _lastSlope = ((nDouble * sumValueTime) - (sumTime * sumValue)) / ((nDouble * sumTime2) - sum2Time);
            _lastYint = (sumValue - _lastSlope * sumTime) / nDouble;
            _lastCorrelationCoefficient = ((nDouble * sumValueTime) - (sumTime * sumValue)) / Math.Sqrt(((nDouble * sumTime2) - sum2Time) * ((nDouble * sumValue2) - sum2Value));
        }
    }
}