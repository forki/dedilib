using System.Linq;
using DediLib.Collections;
using NUnit.Framework;

// ReSharper disable UseCollectionCountProperty
namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestNoDuplicateList
    {
        [Test]
        public void Add_single_item_count_one()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void Add_same_item_twice_count_one()
        {
            var list = new NoDuplicateList<string> { "test", "test" };

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void Add_two_different_items_count_two()
        {
            var list = new NoDuplicateList<string> { "test1", "test2" };

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(2, list.Count());
        }

        [Test]
        public void Insert_single_item_count_one()
        {
            var list = new NoDuplicateList<string>();
            list.Insert(0, "test");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void Insert_same_item_twice_count_one()
        {
            var list = new NoDuplicateList<string>();
            list.Insert(0, "test");
            list.Insert(0, "test");

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void Capacity()
        {
            var list = new NoDuplicateList<string>(1234);

            Assert.AreEqual(1234, list.Capacity);
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Count());
        }

        [Test]
        public void Contains_single_item_exists()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.IsTrue(list.Contains("test"));
        }

        [Test]
        public void Contains_single_item_doesnt_exist()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.IsFalse(list.Contains("other"));
        }

        [Test]
        public void IndexOf_existing_item()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.AreEqual(0, list.IndexOf("test"));
        }

        [Test]
        public void IndexOf_non_existing_item()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.AreEqual(-1, list.IndexOf("other"));
        }

        [Test]
        public void Indexer_get_item()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.AreEqual("test", list[0]);
        }

        [Test]
        public void Indexer_set_item()
        {
            var list = new NoDuplicateList<string> { "test" };
            list[0] = "new";

            Assert.IsFalse(list.Contains("test"));
            Assert.IsTrue(list.Contains("new"));
            Assert.AreEqual("new", list[0]);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void preserves_order()
        {
            var list = new NoDuplicateList<int>();
            list.AddRange(Enumerable.Range(1, 100));

            var actual = list.ToList();
            var expected = Enumerable.Range(1, 100).ToList();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Clear()
        {
            var list = new NoDuplicateList<string> { "test" };
            list.Clear();

            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Count());
            Assert.IsFalse(list.Contains("test"));
        }

        [Test]
        public void Remove_non_existing_item()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.IsFalse(list.Remove("other"));
            Assert.IsTrue(list.Contains("test"));
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(1, list.Count());
        }

        [Test]
        public void Remove_single_item()
        {
            var list = new NoDuplicateList<string> { "test" };

            Assert.IsTrue(list.Remove("test"));
            Assert.IsFalse(list.Contains("test"));
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Count());
        }

        [Test]
        public void Remove_single_item_at_position()
        {
            var list = new NoDuplicateList<string> { "test" };

            list.RemoveAt(0);
            Assert.IsFalse(list.Contains("test"));
            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(0, list.Count());
        }
    }
}
