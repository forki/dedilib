using System;
using System.Collections.Generic;
using System.Linq;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class ListDictionary_When_remove
    {
        private ListDictionary<string, string> _sut;

        private ICollection<string> _keys;

        private ICollection<string> _values;

        [SetUp]
        public void SetUp()
        {
            _sut = new ListDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            _sut.AddMany("key1", new[] { "value" });
            _sut.AddMany("key2", new[] { "value1", "value2" });
            _sut.AddMany("key3", new[] { "value", "value" });

            _keys = _sut.Keys.ToList();
            _values = _sut.Values.ToList();
        }

        [Test]
        public void If_remove_non_existing_key_Then_collection_is_unchanged()
        {
            var result = _sut.Remove("non-existing-key");

            AssertCollectionUnchanged(result);
        }

        [Test]
        public void If_remove_single_key_Then_item_is_removed()
        {
            var result = _sut.Remove("Key2");

            Assert.That(result, Is.True);
            Assert.That(_sut.ContainsKey("key2"), Is.False);
        }

        [Test]
        public void If_remove_single_key_with_existing_value_Then_item_is_removed()
        {
            var result = _sut.Remove("Key1", "value");

            Assert.That(result, Is.True);
            Assert.That(_sut.ContainsKey("key1"), Is.False);
        }

        [Test]
        public void If_remove_single_key_with_non_existing_value_Then_item_is_not_removed()
        {
            var result = _sut.Remove("Key1", "non-existing-value");

            AssertCollectionUnchanged(result);
        }

        [Test]
        public void If_remove_multi_key_Then_item_is_removed()
        {
            var result = _sut.Remove("Key1");

            Assert.That(result, Is.True);
            Assert.That(_sut.ContainsKey("key1"), Is.False);
        }

        [Test]
        public void If_remove_multi_key_with_existing_value_Then_item_is_removed()
        {
            var result = _sut.Remove("Key2", "value1");

            Assert.That(result, Is.True);
            Assert.That(_sut.ContainsKey("key2"), Is.True);
            Assert.That(_sut.GetValues("key2"), Is.EquivalentTo(new [] { "value2" }));
        }

        [Test]
        public void If_remove_multi_key_with_non_existing_value_Then_item_is_not_removed()
        {
            var result = _sut.Remove("Key2", "non-existing-value");

            AssertCollectionUnchanged(result);
        }

        private void AssertCollectionUnchanged(bool result)
        {
            Assert.That(result, Is.False);
            Assert.That(_sut.Keys, Is.EquivalentTo(_keys));
            Assert.That(_sut.Values, Is.EquivalentTo(_values));
        }
    }
}
