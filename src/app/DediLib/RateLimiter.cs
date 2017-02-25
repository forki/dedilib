using System;
using System.Threading;
using System.Threading.Tasks;

namespace DediLib
{
    public class RateLimiter
    {
        private static readonly TimeSpan Infinity = TimeSpan.FromMilliseconds(-1);

        private readonly SemaphoreSlim _softSemaphore;
        private readonly SemaphoreSlim _hardSemaphore;
        private readonly TimeSpan _hardLimitTimeout;

        public RateLimiter(int softLimit, int hardLimit)
            : this(softLimit, hardLimit, Infinity)
        {
        }

        public RateLimiter(int softLimit, int hardLimit, TimeSpan hardLimitTimeout)
        {
            if (softLimit <= 0) throw new ArgumentOutOfRangeException(nameof(softLimit));
            if (hardLimit <= 0) throw new ArgumentOutOfRangeException(nameof(hardLimit));
            if (hardLimitTimeout != Infinity && hardLimitTimeout <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(hardLimitTimeout));
            }

            _hardLimitTimeout = hardLimitTimeout;
            if (softLimit > hardLimit)
            {
                throw new ArgumentException(nameof(hardLimit),
                    $"hardLimit ({hardLimit}) must be larger than softLimit ({softLimit})");
            }

            _softSemaphore = new SemaphoreSlim(softLimit, softLimit);
            _hardSemaphore = new SemaphoreSlim(hardLimit, hardLimit);
        }

        public Task<bool> RateLimit(Func<Task> func)
        {
            return RateLimit(func, Infinity, CancellationToken.None);
        }

        public Task<bool> RateLimit(Func<Task> func, CancellationToken cancellationToken)
        {
            return RateLimit(func, Infinity, cancellationToken);
        }

        public async Task<bool> RateLimit(Func<Task> func, TimeSpan timeSpan, CancellationToken cancellationToken)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (_hardSemaphore.CurrentCount == 0)
            {
                return false;
            }

            var success = await _hardSemaphore.WaitAsync(_hardLimitTimeout, cancellationToken).ConfigureAwait(false);
            if (!success)
            {
                return false;
            }

            try
            {
                if (await _softSemaphore.WaitAsync(timeSpan, cancellationToken).ConfigureAwait(false))
                {
                    try
                    {
                        await func().ConfigureAwait(false);
                        return true;
                    }
                    finally
                    {
                        _softSemaphore.Release();
                    }
                }
            }
            finally
            {
                _hardSemaphore.Release();
            }

            return false;
        }
    }
}
