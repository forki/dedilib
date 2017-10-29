using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class LinqExtensionsTests
    {
        [Test]
        public void AsList_InputIsNull_EmptyList()
        {
            Assert.AreEqual(new List<int>(), ((IEnumerable<int>)null).AsList());
        }

        [Test]
        public void AsList_InputIsEmptyList_SameReference()
        {
            var list = new List<int>();
            Assert.IsTrue(ReferenceEquals(list, list.AsList()));
        }

        [Test]
        public void AsList_InputIsEnumerable_List()
        {
            var list = new Dictionary<int, int>();
            Assert.IsInstanceOf<List<KeyValuePair<int, int>>>(list.AsList());
        }

        [Test]
        public void Split_InputIsNull_EmptyListOfList()
        {
            Assert.AreEqual(new List<List<int>>(), ((IEnumerable<int>)null).Split(1).ToList());
        }

        [Test]
        public void Split_EnumerableHasLessItemsThanBatchSize_OneListWithOneListOfAllElements()
        {
            Assert.AreEqual(new List<List<int>> { new List<int> { 1, 2 } }, new List<int> { 1, 2 }.Split(3).ToList());
        }

        [Test]
        public void Split_EnumerableHasSameNumberOfItemsAsBatchSize_OneListWithOneListOfAllElements()
        {
            Assert.AreEqual(new List<List<int>> { new List<int> { 1, 2 } }, new List<int> { 1, 2 }.Split(2).ToList());
        }

        [Test]
        public void Split_EnumerableHasOneMoreItemThanBatchSize_SplitIntoMultipleLists()
        {
            Assert.AreEqual(new List<int> { 1, 2 }, new List<int> { 1, 2, 3 }.Split(2).ElementAt(0));
            Assert.AreEqual(new List<int> { 3 }, new List<int> { 1, 2, 3 }.Split(2).ElementAt(1));
        }
    }
}
