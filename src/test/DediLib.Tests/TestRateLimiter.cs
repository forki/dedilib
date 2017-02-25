using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class TestRateLimiter
    {
        private Func<Task> _func;

        private bool _taskHasRun;

        [SetUp]
        public void SetUp()
        {
            _func = () =>
            {
                _taskHasRun = true;
                return Task.FromResult(0);
            };
        }

        [Test]
        public void If_arguments_are_invalid_Then_throw()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new RateLimiter(1, 1, TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => new RateLimiter(2, 1));
        }

        [Test]
        public void If_soft_limit_is_not_exhausted_Then_do_not_rate_limit_and_run()
        {
            var rateLimiter = new RateLimiter(1, 1);

            var sw = Stopwatch.StartNew();
            var result = rateLimiter.RateLimit(_func).Result;
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(15));
            Assert.That(result, Is.True);
            Assert.That(_taskHasRun, Is.True);
        }

        [Test]
        public void If_soft_limit_is_exhausted_Then_rate_limit_and_run()
        {
            var rateLimiter = new RateLimiter(1, 2);

            rateLimiter.RateLimit(() => Task.Delay(20));

            var sw = Stopwatch.StartNew();
            var result = rateLimiter.RateLimit(_func).Result;
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan(10));
            Assert.That(result, Is.True);
            Assert.That(_taskHasRun, Is.True);
        }

        [Test]
        public void If_hard_limit_is_exhausted_Then_do_not_rate_limit_and_do_not_run()
        {
            var rateLimiter = new RateLimiter(1, 2);

            rateLimiter.RateLimit(() => Task.Delay(20));
            rateLimiter.RateLimit(() => Task.Delay(0));

            var sw = Stopwatch.StartNew();
            var result = rateLimiter.RateLimit(_func).Result;
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(20));
            Assert.That(result, Is.False);
            Assert.That(_taskHasRun, Is.False);
        }

        [Test]
        public void If_hard_limit_is_exceeded_Then_do_not_rate_limit_and_do_not_run()
        {
            var rateLimiter = new RateLimiter(1, 2);

            rateLimiter.RateLimit(() => Task.Delay(100));
            rateLimiter.RateLimit(() => Task.Delay(0));

            var sw = Stopwatch.StartNew();
            Task.WaitAll(Enumerable.Range(0, 100).Select(x => rateLimiter.RateLimit(_func)).ToArray());
            sw.Stop();

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(120));
            Assert.That(_taskHasRun, Is.False);
        }
    }
}
