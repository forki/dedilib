using System;
using System.Diagnostics;
using System.Linq;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestTimeSeriesLookupList
    {
        [Test]
        public void Constructor_NullCollection_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesLookupList<DateTime>(null, t => t));
        }

        [Test]
        public void Constructor_NullTimestampFunc_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new TimeSeriesLookupList<DateTime>(new DateTime[0], null));
        }

        [Test]
        public void Count()
        {
            var list = new DateTime[1];
            var sut = new TimeSeriesLookupList<DateTime>(list, t => t);
            Assert.That(sut.Count, Is.EqualTo(1));
        }

        [Test]
        public void Indexer_Ordered()
        {
            var value1 = DateTime.UtcNow;
            var value2 = DateTime.UtcNow.AddSeconds(-1);

            var sut = new TimeSeriesLookupList<DateTime>(new[] { value1, value2 }, t => t);
            Assert.That(sut[0], Is.EqualTo(value2));
            Assert.That(sut[1], Is.EqualTo(value1));
        }

        [Test]
        public void GetEnumerator_Ordered()
        {
            var value1 = DateTime.UtcNow;
            var value2 = DateTime.UtcNow.AddSeconds(-1);

            var sut = new TimeSeriesLookupList<DateTime>(new[] { value1, value2 }, t => t);
            Assert.That(sut, Is.EqualTo(new [] { value2, value1 }));
        }

        [Test]
        public void GetBetween_RangeOutsideLeft_Empty()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 2500).Select(x => timestamp.AddSeconds(x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp.AddSeconds(-2), timestamp.AddSeconds(-1));
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBetween_RangeOutsideRight_Empty()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 2500).Select(x => timestamp.AddSeconds(-x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp.AddSeconds(1), timestamp.AddSeconds(2));
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBetween_RangeFromInclusive_Included()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 2500).Select(x => timestamp.AddSeconds(-x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp, timestamp.AddMinutes(1));
            Assert.That(result, Is.EqualTo(new[] { timestamp }));
        }

        [Test]
        public void GetBetween_RangeToExclusive_Excluded()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 2500).Select(x => timestamp.AddSeconds(x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp.AddMinutes(-1), timestamp);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetBetween_RangeInside_Range()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 2500).Select(x => timestamp.AddSeconds(x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp.AddMinutes(1), timestamp.AddMinutes(2));
            Assert.That(result.Count, Is.EqualTo(60));
        }

        [Test]
        public void GetBetween_RangeInsideWithDuplicates_IncludeDuplicates()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 20).Select(x => timestamp.AddMinutes(x)).ToList();
            values.AddRange(Enumerable.Range(0, 20).Select(x => timestamp.AddMinutes(x)));

            var sut = new TimeSeriesLookupList<DateTime>(values.ToList(), t => t);

            var result = sut.GetBetween(timestamp, timestamp.AddSeconds(1));
            Assert.That(result, Is.EqualTo(new [] { timestamp, timestamp }));
        }

        [Test]
        public void GetBetween_Dates_Empty()
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, 200).Select(x => timestamp.AddDays(x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var result = sut.GetBetween(timestamp.AddDays(1), timestamp.AddDays(2));
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Category("Benchmark")]
        [Explicit]
        [TestCase(1000000, 10)]
        [TestCase(100000, 100)]
        [TestCase(10000, 1000)]
        public void Benchmark(int count, int samplesPerWindow)
        {
            var timestamp = DateTime.UtcNow;
            var values = Enumerable.Range(0, count).Select(x => timestamp.AddSeconds(x));

            var sut = new TimeSeriesLookupList<DateTime>(values, t => t);

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var result = sut.GetBetween(timestamp, timestamp.AddSeconds(samplesPerWindow)).Count;
            }
            sw.Stop();

            var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{count} iterations with {samplesPerWindow} samples per window, {sw.Elapsed} ({opsPerSec.ToString("N0")} ops/sec)");
        }
    }
}
