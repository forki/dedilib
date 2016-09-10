using System.Diagnostics;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [Category("Benchmark")]
    [Explicit]
    [TestFixture]
    public class ListDictionary_When_benchmarking
    {
        private const int Count = 10000000;

        // ReSharper disable CollectionNeverQueried.Local
        [Test]
        public void Then_adding_multi_int_keys_are_benchmarked()
        {
            var sut = new ListDictionary<int, int>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < Count; i++)
            {
                sut.Add(i, i);
            }
            sw.Stop();

            var opsPerSec = Count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{Count} iterations of Add multiple keys and value, {sw.Elapsed} ({opsPerSec:N0} ops/sec)");
        }

        [Category("Benchmark")]
        [Test]
        public void Then_adding_single_int_keys_are_benchmarked()
        {
            var sut = new ListDictionary<int, int>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < Count; i++)
            {
                sut.Add(1, i);
            }
            sw.Stop();

            var opsPerSec = Count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{Count} iterations of Add multiple keys and value, {sw.Elapsed} ({opsPerSec:N0} ops/sec)");
        }
    }
}
