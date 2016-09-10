using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class ListDictionary_When_add
    {
        private ListDictionary<int, int> _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ListDictionary<int, int>();
        }

        [Test]
        public void If_adding_single_item_Then_expected_count_and_values_match()
        {
            _sut.Add(1, 1000);

            Assert.That(_sut.Count, Is.EqualTo(1));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000 }));
        }

        [Test]
        public void If_adding_two_items_with_same_key_and_value_Then_expected_count_and_values_match()
        {
            _sut.Add(1, 1000);
            _sut.Add(1, 1000);

            Assert.That(_sut.Count, Is.EqualTo(2));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 1000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000, 1000 }));
        }

        [Test]
        public void If_adding_two_items_with_same_key_Then_expected_count_and_values_match()
        {
            _sut.Add(1, 1000);
            _sut.Add(1, 2000);

            Assert.That(_sut.Count, Is.EqualTo(2));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000, 2000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }

        [Test]
        public void If_adding_two_items_with_different_keys_and_different_values_Then_expected_count_and_values_match()
        {
            _sut.Add(1, 1000);
            _sut.Add(2, 2000);

            Assert.That(_sut.Count, Is.EqualTo(2));
            Assert.That(_sut.ContainsKey(1), Is.True);
            Assert.That(_sut.ContainsKey(2), Is.True);
            Assert.That(_sut.GetValues(1), Is.EquivalentTo(new[] { 1000 }));
            Assert.That(_sut.GetValues(2), Is.EquivalentTo(new[] { 2000 }));
            Assert.That(_sut.Keys, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(_sut.Values, Is.EquivalentTo(new[] { 1000, 2000 }));
        }
    }
}
