using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class TestClientPool
    {
        [Test]
        public void Maximum_One_Aquire_Single_Object()
        {
            var obj = new object();
            var clientPool = new ClientPool<object>(1, () => obj);

            var aquired = clientPool.Aquire();
            Assert.AreSame(obj, aquired);
        }

        [Test]
        public void Maximum_One_Aquire_Two_Objects_Second_Aquire_Waits_For_Return()
        {
            var obj = new object();
            var clientPool = new ClientPool<object>(1, () => obj);

            object secondAquired = null;
            var sw = new Stopwatch();
            var aquired = clientPool.Aquire();

            var started = new ManualResetEventSlim();
            var stopped = new ManualResetEventSlim();
            Task.Factory.StartNew(() =>
            {
                started.Set();
                sw = Stopwatch.StartNew();
                try
                {
                    secondAquired = clientPool.Aquire(TimeSpan.FromMilliseconds(1000));
                }
                finally
                {
                    sw.Stop();
                    stopped.Set();
                }
            }, TaskCreationOptions.LongRunning);
            
            started.Wait();

            var busyTime = TimeSpan.FromMilliseconds(100);
            Thread.Sleep(busyTime);

            clientPool.Return(aquired);

            stopped.Wait();

            Assert.AreSame(obj, secondAquired);
            Assert.GreaterOrEqual(sw.Elapsed, busyTime);
        }

        [Test]
        public void Maximum_One_Aquire_Two_Objects_Second_Aquire_Waits_For_DiscardBusyObject()
        {
            var obj = new object();
            var clientPool = new ClientPool<object>(1, () => obj);

            object secondAquired = null;
            var sw = new Stopwatch();
            var aquired = clientPool.Aquire();

            var started = new ManualResetEventSlim();
            var stopped = new ManualResetEventSlim();
            Task.Factory.StartNew(() =>
            {
                started.Set();
                sw = Stopwatch.StartNew();
                try
                {
                    secondAquired = clientPool.Aquire(TimeSpan.FromMilliseconds(1000));
                }
                finally
                {
                    sw.Stop();
                    stopped.Set();
                }
            }, TaskCreationOptions.LongRunning);

            started.Wait();

            var busyTime = TimeSpan.FromMilliseconds(100);
            Thread.Sleep(busyTime);

            clientPool.DiscardBusyClient(aquired);

            stopped.Wait();

            Assert.AreSame(obj, secondAquired);
            Assert.GreaterOrEqual(sw.Elapsed, busyTime);
        }

        [Test]
        public void Maximum_One_Aquire_Single_Object_DiscardBusyClient_calls_Free_action()
        {
            var obj = new object();
            object freeObject = null;
            var clientPool = new ClientPool<object>(1, () => obj, x => freeObject = x);

            var aquired = clientPool.Aquire();
            clientPool.DiscardBusyClient(aquired);

            Assert.AreSame(obj, freeObject);
        }
    }
}
