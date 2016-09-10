using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class HashSetDictionary_When_addMany
    {
        private HashSetDictionary<int, int> _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new HashSetDictionary<int, int>();
        }

        [Test]
        public void If_addMany_with_key_and_no_values_Then_count_and_values_match()
        {
            _sut.AddMany(1, new int[0]);

            Assert.That(_sut.Count, Is.EqualTo(0));
            Assert.That(_sut.ContainsKey(0), Is.False);
            Assert.That(_sut.GetValues(1), Is.Empty);
            Assert.That(_sut.GetValuesAsHashSet(1), Is.Empty);
            Assert.That(_sut.Keys, Is.Empty);
            Assert.That(_sut.Values, Is.Empty);
        }

        [Test]
        public void If_addMany_with_same_key_and_same_values_Then_count_and_values_match()
        {
            _sut.AddMany(1, new[] { 1000, 1000 });

            Assert.That(_sut.Count, Is.EqualTo(1));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(_sut.GetValuesAsHashSet(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000 }));
        }

        [Test]
        public void If_addMany_with_same_key_and_different_values_Then_count_and_values_match()
        {
            _sut.AddMany(1, new[] { 1000, 2000 });

            Assert.That(_sut.Count, Is.EqualTo(2));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000 }));
            Assert.That(_sut.GetValuesAsHashSet(1), Is.EquivalentTo(new[] { 1000, 2000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }

        [Test]
        public void If_addMany_twice_with_same_key_and_different_values_Then_count_and_values_match()
        {
            _sut.AddMany(1, new[] { 1000 });
            _sut.AddMany(1, new[] { 2000, 3000 });

            Assert.That(_sut.Count, Is.EqualTo(3));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000, 3000 }));
            Assert.That(_sut.GetValuesAsHashSet(1), Is.EquivalentTo(new[] { 1000, 2000, 3000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000, 2000, 3000 }));
        }
    }
}
