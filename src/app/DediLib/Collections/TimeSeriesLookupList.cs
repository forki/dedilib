using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace DediLib.Collections
{
    public class TimeSeriesLookupList<T> : IEnumerable<T>
    {
        private readonly Func<T, DateTime> _timestampFunc;
        private readonly List<T> _list;
        private readonly Dictionary<DateTime, int> _perMinuteIndexes = new Dictionary<DateTime, int>();

        private readonly DateTime _minTimestamp = DateTime.MinValue;
        private readonly DateTime _maxTimestamp = DateTime.MaxValue;

        public TimeSeriesLookupList(IEnumerable<T> collection, Func<T, DateTime> timestampFunc)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (timestampFunc == null) throw new ArgumentNullException(nameof(timestampFunc));

            _timestampFunc = timestampFunc;

            _list = collection.OrderBy(timestampFunc).ToList();
            if (_list.Count == 0) return;

            _minTimestamp = timestampFunc(_list[0]);
            _maxTimestamp = timestampFunc(_list[_list.Count - 1]);

            var currentLookupTimestamp = DateTime.MinValue;
            for (int i = 0; i < _list.Count; i++)
            {
                var item = _list[i];
                var timestamp = timestampFunc(item);

                var lookupTimestamp = ReduceTimestampGranularity(timestamp);

                if (currentLookupTimestamp != lookupTimestamp)
                {
                    if (currentLookupTimestamp != DateTime.MinValue)
                    {
                        AddIndexesInBetween(currentLookupTimestamp, lookupTimestamp, i);
                    }
                    _perMinuteIndexes[lookupTimestamp] = i;
                    currentLookupTimestamp = lookupTimestamp;
                }
            }

            var lastTimestamp = _maxTimestamp.AddMinutes(1);
            if (!_perMinuteIndexes.ContainsKey(ReduceTimestampGranularity(lastTimestamp)))
            {
                _perMinuteIndexes[ReduceTimestampGranularity(lastTimestamp)] = _list.Count;
            }
        }

        private void AddIndexesInBetween(DateTime fromTimestamp, DateTime toTimestamp, int index)
        {
            if (fromTimestamp > toTimestamp)
                return;

            for (var minuteToAdd = (int) (toTimestamp - fromTimestamp).TotalMinutes; minuteToAdd > 0; minuteToAdd--)
            {
                _perMinuteIndexes[fromTimestamp.AddMinutes(minuteToAdd)] = index;
            }
        }

        private static readonly T[] Empty = new T[0];

        [TargetedPatchingOptOut("")]
        public IList<T> GetBetween(DateTime fromInclusiveTimestamp, DateTime toExclusiveTimestamp)
        {
            if (fromInclusiveTimestamp >= toExclusiveTimestamp) return Empty;
            if (fromInclusiveTimestamp > _maxTimestamp) return Empty;
            if (toExclusiveTimestamp <= _minTimestamp) return Empty;

            var fromIndex = 0;
            if (fromInclusiveTimestamp >= _minTimestamp)
                fromIndex = _perMinuteIndexes[ReduceTimestampGranularity(fromInclusiveTimestamp)];

            var toIndex = _list.Count;
            if (toExclusiveTimestamp < _maxTimestamp)
                toIndex = _perMinuteIndexes[ReduceTimestampGranularity(toExclusiveTimestamp.AddMinutes(1))];

            if (toIndex >= _list.Count)
                toIndex = _list.Count - 1;

            var result = new List<T>();
            for (var i = fromIndex; i <= toIndex; i++)
            {
                var item = _list[i];
                var timestamp = _timestampFunc(item);
                if (timestamp < fromInclusiveTimestamp) continue;
                if (timestamp >= toExclusiveTimestamp) return result;

                result.Add(item);
            }
            return result;
        }

        private DateTime ReduceTimestampGranularity(DateTime timestamp)
        {
            return new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0);
        }

        public int Count => _list.Count;

        public T this[int index] => _list[index];

        [TargetedPatchingOptOut("")]
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        [TargetedPatchingOptOut("")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
