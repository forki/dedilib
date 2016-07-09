using System.Diagnostics;
using DediLib.Logging;
using NLog;
using NUnit.Framework;

namespace DediLib.Tests.Logging
{
    [TestFixture]
    public class TestNLogLogger
    {
        [Test]
        public void Constructor()
        {
            CreateSut();
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark()
        {
            var sut = CreateSut();

            const int iterations = 1000000;
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < iterations; i++)
            {
                sut.Debug("");
            }
            sw.Stop();

            Assert.Inconclusive("{0} ({1:N0} ops/sec)", sw.Elapsed, iterations / sw.Elapsed.TotalMilliseconds * 1000);
        }

        private NLogLogger CreateSut()
        {
            LogManager.EnableLogging();
            return new NLogLogger(null);
        }
    }
}
