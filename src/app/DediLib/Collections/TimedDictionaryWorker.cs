using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DediLib.Collections
{
    internal static class TimedDictionaryWorker
    {
        internal static readonly ConcurrentDictionary<ITimedDictionary, DateTime> TimedDictionaries = new ConcurrentDictionary<ITimedDictionary, DateTime>();
        internal static int TimedDictionartiesCount;
        private static CancellableTask _cleanUpTask;

        public static Action<ITimedDictionary, Exception> OnCleanUpException = (td, ex) =>
        {
            Debug.WriteLine(ex.GetType() + ": " + ex.Message);
            Debug.WriteLine(ex.StackTrace);
        };

        public static void Register(ITimedDictionary timedDictionary)
        {
            if (timedDictionary == null) throw new ArgumentNullException(nameof(timedDictionary));
            if (!TimedDictionaries.TryAdd(timedDictionary, DateTime.UtcNow)) return;

            if (Interlocked.Increment(ref TimedDictionartiesCount) != 1) return;

            var cleanUpTask = new CancellableTask(CleanUp);
            if (Interlocked.CompareExchange(ref _cleanUpTask, cleanUpTask, null) == null)
            {
                cleanUpTask.Start();
            }
        }

        public static void Unregister(ITimedDictionary timedDictionary)
        {
            if (timedDictionary == null) throw new ArgumentNullException(nameof(timedDictionary));

            DateTime dummy;
            if (!TimedDictionaries.TryRemove(timedDictionary, out dummy)) return;

            if (Interlocked.Decrement(ref TimedDictionartiesCount) != 0) return;

            var cleanUpTask = Interlocked.Exchange(ref _cleanUpTask, null);
            cleanUpTask?.Cancel();
        }

        static void CleanUp(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var jobStarted = sw.ElapsedMilliseconds;

                        foreach (var pair in TimedDictionaries)
                        {
                            var timedDictionary = pair.Key;
                            var lastCleanUp = pair.Value;

                            try
                            {
                                var now = DateTime.UtcNow;
                                if ((now - lastCleanUp) > timedDictionary.CleanUpPeriod)
                                {
                                    TimedDictionaries[timedDictionary] = now;
                                    timedDictionary.CleanUp();
                                }
                            }
                            catch (ThreadAbortException)
                            {
                                break;
                            }
                            catch (ThreadInterruptedException)
                            {
                                break;
                            }
                            catch (Exception ex)
                            {
                                OnCleanUpException(timedDictionary, ex);
                            }
                        }

                        var elapsed = sw.ElapsedMilliseconds - jobStarted;
                        var waitPeriod = 1000 - elapsed;
                        if (waitPeriod > 0)
                            cancellationToken.WaitHandle.WaitOne((int)waitPeriod);
                    }
                    catch (ThreadAbortException)
                    {
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        sealed class CancellableTask : IDisposable
        {
            private readonly CancellationTokenSource _cancellationTokenSource;
            private readonly CancellationToken _cancellationToken;
            private readonly Task _task;

            public CancellableTask(Action<CancellationToken> action)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                _task = new Task(() => action(_cancellationToken), _cancellationToken, TaskCreationOptions.LongRunning);
            }

            public void Start()
            {
                _task.Start();
            }

            public void Cancel()
            {
                _cancellationTokenSource.Cancel();
            }

            public void Dispose()
            {
                Cancel();
            }
        }
    }
}
