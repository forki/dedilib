using System;
using System.Collections.Generic;
using System.Linq;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class HashSetStringExtensions_When_clone
    {
        [Test]
        public void If_empty_Then_cloned_hashSet_is_empty()
        {
            var hashSet = new HashSet<int>();

            var cloned = hashSet.Clone();

            Assert.That(cloned, Is.Empty);
        }

        [Test]
        public void If_cloning_small_hashSet_Then_cloned_hashSet_items_are_equivalent_to_initial_hashSet()
        {
            var hashSet = new HashSet<int>(Enumerable.Range(1, 10));

            var cloned = hashSet.Clone();

            Assert.That(cloned, Is.EquivalentTo(hashSet));
        }

        [Test]
        public void If_cloning_large_hashSet_Then_cloned_hashSet_items_are_equivalent_to_initial_hashSet()
        {
            var hashSet = new HashSet<int>(Enumerable.Range(1, 10000));

            var cloned = hashSet.Clone();

            Assert.That(cloned, Is.EquivalentTo(hashSet));
        }

        [Test]
        public void If_cloning_small_hashSet_Then_cloned_hashSet_is_using_same_comparer()
        {
            var hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "A", "B" }; 

            var cloned = hashSet.Clone();

            Assert.That(cloned.Contains("a"), Is.True);
        }

        [Test]
        public void If_cloning_large_hashSet_Then_cloned_hashSet_is_using_same_comparer()
        {
            var hashSet = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "A" };
            Enumerable.Range(1, 10000).Select(x => x.ToString()).ToList().ForEach(x => hashSet.Add(x));

            var cloned = hashSet.Clone();

            Assert.That(cloned.Contains("a"), Is.True);
        }
    }
}
