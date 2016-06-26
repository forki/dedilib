using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace DediLib
{
    public class ClientPool<T> : IDisposable where T : class
    {
        private readonly Func<T> _createInstance;
        private readonly Action<T> _freeAction;
        private readonly CounterSignal _counterSignal;
        private readonly InterlockedBoolean _disposed = new InterlockedBoolean();

        private readonly ConcurrentQueue<T> _idleQueue = new ConcurrentQueue<T>();
        private readonly ConcurrentDictionary<T, object> _busy = new ConcurrentDictionary<T, object>();

        public ClientPool(int maxNumberOfClients)
            : this(maxNumberOfClients, Activator.CreateInstance<T>, t => GetDefaultDispose(t))
        {
        }

        public ClientPool(int maxNumberOfClients, Func<T> createInstance)
            : this(maxNumberOfClients, createInstance, t => GetDefaultDispose(t))
        {
        }

        private static Action<T> GetDefaultDispose(T client)
        {
            var disposable = client as IDisposable;
            if (disposable != null) return t => disposable.Dispose();
            return null;
        }

        public ClientPool(int maxNumberOfClients, Func<T> createInstance, Action<T> freeAction)
        {
            if (createInstance == null) throw new ArgumentNullException("createInstance");

            _createInstance = createInstance;
            _freeAction = freeAction;

            if (maxNumberOfClients == 0) maxNumberOfClients = Int32.MaxValue;
            _counterSignal = new CounterSignal(maxNumberOfClients, 0);
        }

        private readonly object _creationLock = new object();

        public T Aquire()
        {
            return Aquire(TimeSpan.FromSeconds(30));
        }

        public T Aquire(TimeSpan timeout)
        {
            T client;
            if (!TryAquire(timeout, out client))
                throw new TimeoutException();
            return client;
        }

        public bool TryAquire(TimeSpan timeout, out T client)
        {
            client = TryDequeue();
            if (client != null) return true;

            var sw = Stopwatch.StartNew();
            if (!Monitor.TryEnter(_creationLock, timeout))
                return false;
            try
            {
                while (_counterSignal.IsSet)
                {
                    if (sw.Elapsed > timeout)
                        return false;

                    Thread.Sleep(1);

                    client = TryDequeue();
                    if (client != null)
                        return true;
                }

                client = _createInstance();
                _counterSignal.Increment();
                _busy.TryAdd(client, null);
                return true;
            }
            finally
            {
                Monitor.Exit(_creationLock);
            }
        }

        private T TryDequeue()
        {
            if (_disposed.Value)
                throw new ObjectDisposedException(typeof(ClientPool<T>).Name);

            T client;
            if (_idleQueue.TryDequeue(out client))
            {
                _busy.TryAdd(client, null);
                return client;
            }
            return null;
        }

        public bool Return(T client)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (_disposed.Value)
                throw new ObjectDisposedException(typeof(ClientPool<T>).Name);

            object oldValue;
            if (_busy.TryRemove(client, out oldValue))
            {
                _idleQueue.Enqueue(client);
                return true;
            }

            return false;
        }

        public bool DiscardBusyClient(T client)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (_disposed.Value)
                throw new ObjectDisposedException(typeof(ClientPool<T>).Name);

            object oldValue;
            if (_busy.TryRemove(client, out oldValue))
            {
                _counterSignal.Decrement();
                if (_freeAction != null) _freeAction(client);
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            if (_disposed.Set(true)) return;

            foreach (var item in _busy.Keys)
            {
                try { DiscardBusyClient(item); }
                catch { }
            }

            if (_freeAction != null)
            {
                T queueItem;
                while (_idleQueue.TryDequeue(out queueItem))
                {
                    try { _freeAction(queueItem); }
                    catch { }
                }
            }
        }
    }
}
