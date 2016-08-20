using System.Diagnostics;
using DediLib.Collections;
using NUnit.Framework;

// ReSharper disable UseObjectOrCollectionInitializer
namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestListDictionary
    {
        [Test]
        public void Empty_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            Assert.That(sut.IsReadOnly, Is.False);

            Assert.That(sut.Count, Is.EqualTo(0));
            Assert.That(sut.ContainsKey(1), Is.False);
            Assert.That(sut.GetValues(1), Is.Empty);
            Assert.That(sut.Keys, Is.Empty);
            Assert.That(sut.Values, Is.Empty);
        }

        [Test]
        public void Add_SingleKeySingleValue_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.Add(1, 1000);

            Assert.That(sut.Count, Is.EqualTo(1));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000 }));
        }

        [Test]
        public void Add_SingleKeySingleValueTwice_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.Add(1, 1000);
            sut.Add(1, 1000);

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 1000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 1000 }));
        }

        [Test]
        public void Add_SingleKeyTwoValues_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.Add(1, 1000);
            sut.Add(1, 2000);

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }

        [Test]
        public void Add_TwoKeysSingleValueEach_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.Add(1, 1000);
            sut.Add(2, 2000);

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.ContainsKey(2), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(sut.GetValues(2), Is.EquivalentTo(new[] { 2000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }

        [Test]
        public void AddMany_SingleKeySingleValueTwice_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.AddMany(1, new[] { 1000, 1000 });

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 1000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 1000 }));
        }

        [Test]
        public void AddMany_SingleKeyTwoValues_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.AddMany(1, new[] { 1000, 2000 });

            Assert.That(sut.Count, Is.EqualTo(2));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }

        [Test]
        public void AddMany_SingleKeyThreeValues_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            sut.AddMany(1, new[] { 1000 });
            sut.AddMany(1, new[] { 2000, 3000 });

            Assert.That(sut.Count, Is.EqualTo(3));
            Assert.That(sut.ContainsKey(1), Is.True);
            Assert.That(sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000, 3000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 1000, 2000, 3000 }));
        }

        [Test]
        public void Remove_NonExistingValue_CountAndValues()
        {
            var sut = new ListDictionary<int, int>();

            var result = sut.Remove(1);

            Assert.That(result, Is.False);

            Assert.That(sut.Count, Is.EqualTo(0));
            Assert.That(sut.ContainsKey(1), Is.False);
            Assert.That(sut.GetValues(1), Is.Empty);
            Assert.That(sut.Keys, Is.Empty);
            Assert.That(sut.Values, Is.Empty);
        }

        [Test]
        public void Remove_SingleKeySingleValue_CountAndValues()
        {
            var sut = new ListDictionary<int, int> { { 1, 1000 } };

            var result = sut.Remove(1);

            Assert.That(result, Is.True);

            Assert.That(sut.Count, Is.EqualTo(0));
            Assert.That(sut.ContainsKey(1), Is.False);
            Assert.That(sut.GetValues(1), Is.Empty);
            Assert.That(sut.Keys, Is.Empty);
            Assert.That(sut.Values, Is.Empty);
        }

        [Test]
        public void Remove_SingleKeyTwoValues_CountAndValues()
        {
            var sut = new ListDictionary<int, int> { { 1, 1000 }, { 1, 2000 } };

            var result = sut.Remove(1);

            Assert.That(result, Is.True);

            Assert.That(sut.Count, Is.EqualTo(0));
            Assert.That(sut.ContainsKey(1), Is.False);
            Assert.That(sut.GetValues(1), Is.Empty);
            Assert.That(sut.Keys, Is.Empty);
            Assert.That(sut.Values, Is.Empty);
        }

        [Test]
        public void Remove_TwoKeysSingleValueEach_CountAndValues()
        {
            var sut = new ListDictionary<int, int> { { 1, 1000 }, { 2, 2000 } };

            var result = sut.Remove(1);

            Assert.That(result, Is.True);

            Assert.That(sut.Count, Is.EqualTo(1));
            Assert.That(sut.ContainsKey(1), Is.False);
            Assert.That(sut.ContainsKey(2), Is.True);
            Assert.That(sut.GetValues(1), Is.Empty);
            Assert.That(sut.GetValues(2), Is.EquivalentTo(new[] { 2000 }));
            Assert.That(sut.Keys, Is.EquivalentTo(new[] { 2 }));
            Assert.That(sut.Values, Is.EquivalentTo(new[] { 2000 }));
        }

        // ReSharper disable CollectionNeverQueried.Local
        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark_AddMultipleKeys()
        {
            var count = 10000000;

            var sut = new ListDictionary<int, int>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                sut.Add(i, i);
            }
            sw.Stop();

            var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{count} iterations of Add multiple keys and value, {sw.Elapsed} ({opsPerSec.ToString("N0")} ops/sec)");
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark_AddSingleKeys()
        {
            var count = 10000000;

            var sut = new ListDictionary<int, int>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                sut.Add(1, i);
            }
            sw.Stop();

            var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
            Assert.Inconclusive($"{count} iterations of Add multiple keys and value, {sw.Elapsed} ({opsPerSec.ToString("N0")} ops/sec)");
        }
    }
}
