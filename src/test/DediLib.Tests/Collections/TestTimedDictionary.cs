using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestTimedDictionary
    {
        [Test]
        public void Constructor_DefaultExpiry()
        {
            using (var timedDictionary = new TimedDictionary<object, object>(TimeSpan.FromSeconds(123)))
            {
                Assert.AreEqual(TimeSpan.FromSeconds(123), timedDictionary.DefaultExpiry);
            }
        }

        [Test]
        public void Constructor_DefaultExpiry_CleanUpPeriod()
        {
            var defaultExpiry = TimeSpan.FromSeconds(123);
            var cleanUpPeriod = TimeSpan.FromSeconds(234);

            using (var timedDictionary = new TimedDictionary<object, object>(defaultExpiry, cleanUpPeriod))
            {
                Assert.AreEqual(defaultExpiry, timedDictionary.DefaultExpiry);
                Assert.AreEqual(cleanUpPeriod, timedDictionary.CleanUpPeriod);
            }
        }

        [Test]
        public void Constructor_DefaultExpiry_ConcurrencyLevel_Capacity()
        {
            using (var timedDictionary = new TimedDictionary<object, object>(TimeSpan.FromSeconds(123), 5, 6))
            {
                Assert.AreEqual(TimeSpan.FromSeconds(123), timedDictionary.DefaultExpiry);
            }
        }

        [Test]
        public void AddOrUpdate_value_called_first_creates_value()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                var value = timedDictionary.AddOrUpdate("test", "value", (k, v) => "value2");

                Assert.AreEqual("value", value);
                Assert.AreEqual("value", timedDictionary["test"]);
            }
        }

        [Test]
        public void AddOrUpdate_value_called_twice_updates_value()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.AddOrUpdate("test", "value", (k, v) => "value2");
                var value = timedDictionary.AddOrUpdate("test", "value", (k, v) => "value2");

                Assert.AreEqual("value2", value);
                Assert.AreEqual("value2", timedDictionary["test"]);
            }
        }

        [Test]
        public void AddOrUpdate_value_UpdateAccessTime_true()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.AddOrUpdate("test", "value", (k, v) => v);

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.AddOrUpdate("test", "", (k, v) => v);
                Assert.Greater(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void AddOrUpdate_value_UpdateAccessTime_false()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.AddOrUpdate("test", "value", (k, v) => v);

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.AddOrUpdate("test", "", (k, v) => v, false);
                Assert.AreEqual(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void AddOrUpdate_factory_UpdateAccessTime_true()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.AddOrUpdate("test", k => "value", (k, v) => v);

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.AddOrUpdate("test", k => "value", (k, v) => v);
                Assert.Greater(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void AddOrUpdate_factory_UpdateAccessTime_false()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.AddOrUpdate("test", k => "value", (k, v) => v);

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.AddOrUpdate("test", k => "value", (k, v) => v, false);
                Assert.AreEqual(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void GetOrAdd_value_called_first_creates_value()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                var value = timedDictionary.GetOrAdd("test", "value");

                Assert.AreEqual("value", value);
                Assert.AreEqual("value", timedDictionary["test"]);
            }
        }

        [Test]
        public void GetOrAdd_value_called_twice_creates_value_only_once()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.GetOrAdd("test", "value");
                var value = timedDictionary.GetOrAdd("test", "value2");

                Assert.AreEqual("value", value);
                Assert.AreEqual("value", timedDictionary["test"]);
            }
        }

        [Test]
        public void GetOrAdd_value_UpdateAccessTime_true()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.GetOrAdd("test", "value");

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.GetOrAdd("test", "");
                Assert.Greater(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void GetOrAdd_value_UpdateAccessTime_false()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.GetOrAdd("test", "value");

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.GetOrAdd("test", "", false);
                Assert.AreEqual(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void GetOrAdd_factory_UpdateAccessTime_true()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.GetOrAdd("test", k => "value");

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.GetOrAdd("test", k => "value");
                Assert.Greater(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void GetOrAdd_factory_UpdateAccessTime_false()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary.GetOrAdd("test", k => "value");

                TimedValue<object> timedValue;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out timedValue));

                var lastAccessed = timedValue.LastAccessUtc;
                Thread.Sleep(15);

                timedDictionary.GetOrAdd("test", k => "value", false);
                Assert.AreEqual(timedValue.LastAccessUtc, lastAccessed);
            }
        }

        [Test]
        public void Indexer_TryGetValue()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary["test"] = 1;

                object value;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out value));
            }
        }

        [Test]
        public void Indexer_TryRemove_TryGetValue()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                timedDictionary["test"] = 1;

                Assert.IsTrue(timedDictionary.TryRemove("test"));

                object value;
                Assert.IsFalse(timedDictionary.TryGetValue("test", out value));
            }
        }

        [Test]
        public void TryRemove_not_existing_returns_false()
        {
            using (var timedDictionary = new TimedDictionary<string, object>())
            {
                Assert.IsFalse(timedDictionary.TryRemove("test"));
            }
        }

        [Test]
        public void Indexer_TryGetValue_StringComparer_InvariantCultureIgnoreCase()
        {
            using (var timedDictionary = new TimedDictionary<string, object>(1, 1, StringComparer.InvariantCultureIgnoreCase))
            {
                timedDictionary["Test"] = 1;

                object value;
                Assert.IsTrue(timedDictionary.TryGetValue("test", out value));
                Assert.AreEqual(1, timedDictionary["test"]);
            }
        }

        [Test]
        public void DefaultExpiry_Indexer_StringComparer_InvariantCultureIgnoreCase()
        {
            var defaultExpiry = TimeSpan.FromSeconds(123);

            using (var timedDictionary = new TimedDictionary<string, object>(defaultExpiry, 1, 1, StringComparer.InvariantCultureIgnoreCase))
            {
                timedDictionary["Test"] = 1;

                Assert.AreEqual(defaultExpiry, timedDictionary.DefaultExpiry);
                Assert.AreEqual(1, timedDictionary["test"]);
            }
        }

        [Test]
        public void CleanUp_HasOneExpiredItem_CountIsOne()
        {
            using (var timedDictionary = new TimedDictionary<int, int>(TimeSpan.MaxValue))
            {
                timedDictionary.TryAdd(1, 1);
                timedDictionary.CleanUp();
                Assert.AreEqual(1, timedDictionary.Count);
            }
        }

        [Test]
        public void CleanUp_HasOneExpiredItem_CountIsZero()
        {
            using (var timedDictionary = new TimedDictionary<int, int>(TimeSpan.FromMilliseconds(0)))
            {
                timedDictionary.TryAdd(1, 1);
                Thread.Sleep(1);
                timedDictionary.CleanUp();
                Assert.AreEqual(0, timedDictionary.Count);
            }
        }

        [Test]
        public void CleanUp_HasOneExpiredItemWaitForAutomaticCleanUp_CountIsZero()
        {
            using (var timedDictionary = new TimedDictionary<int, int>(TimeSpan.FromMilliseconds(0)))
            {
                timedDictionary.TryAdd(1, 1);

                bool cancelled = false;
                using (var task = Task.Factory.StartNew(() => { while (timedDictionary.Count > 0 && !cancelled) Thread.Sleep(0); }, TaskCreationOptions.LongRunning))
                {
                    task.Wait(1100);
                    cancelled = true;
                    task.Wait();
                }

                Assert.AreEqual(0, timedDictionary.Count);
            }
        }

        [Test]
        public void ConcurrencyTest()
        {
            var exceptions = new List<Exception>();
            TimedDictionaryWorker.OnCleanUpException += (td, ex) => exceptions.Add(ex);

            var sw = Stopwatch.StartNew();
            using (var timedDictionary = new TimedDictionary<int, byte>(TimeSpan.Zero))
            {
                var added = 0;
                while (sw.ElapsedMilliseconds < 2000)
                {
                    if (timedDictionary.TryAdd(added, 0))
                        added++;
                }
                Assert.Less(timedDictionary.Count, added);
                Assert.AreEqual(0, exceptions.Count);
            }
        }
    }
    // ReSharper restore InconsistentNaming
}
