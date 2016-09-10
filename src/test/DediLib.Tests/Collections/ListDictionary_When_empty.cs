using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class ListDictionary_When_empty
    {
        private ListDictionary<int, int> _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ListDictionary<int, int>();
        }

        [Test]
        public void Then_count_and_values_match()
        {
            Assert.That(_sut.IsReadOnly, Is.False);

            Assert.That(_sut.Count, Is.EqualTo(0));
            Assert.That(_sut.ContainsKey(1), Is.False);
            Assert.That(_sut.GetValues(1), Is.Empty);
            Assert.That(_sut.Keys, Is.Empty);
            Assert.That(_sut.Keys.IsReadOnly, Is.True);
            Assert.That(_sut.Values, Is.Empty);
            Assert.That(_sut.Values.IsReadOnly, Is.True);
        }
    }
}
