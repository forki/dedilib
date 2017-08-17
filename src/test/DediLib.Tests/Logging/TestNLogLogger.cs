using System;
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

        [Test]
        public void Trace()
        {
            var sut = CreateSut();

            sut.Trace("logText");
        }

        [Test]
        public void Debug()
        {
            var sut = CreateSut();

            sut.Debug("logText");
        }

        [Test]
        public void Info()
        {
            var sut = CreateSut();

            sut.Info("logText");
        }

        [Test]
        public void Warning()
        {
            var sut = CreateSut();

            sut.Warning("logText");
        }

        [Test]
        public void Error()
        {
            var sut = CreateSut();

            sut.Error(new Exception());
        }

        [Test]
        public void ErrorWithText()
        {
            var sut = CreateSut();

            sut.Error(new Exception(), "logText");
        }

        [Test]
        public void Fatal()
        {
            var sut = CreateSut();

            sut.Fatal(new Exception());
        }

        [Test]
        public void FatalWithText()
        {
            var sut = CreateSut();

            sut.Fatal(new Exception(), "logText");
        }
        private NLogLogger CreateSut()
        {
            LogManager.EnableLogging();
            return new NLogLogger(null);
        }
    }
}
