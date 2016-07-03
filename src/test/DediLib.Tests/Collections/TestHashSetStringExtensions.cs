using System.Collections.Generic;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    [TestFixture]
    public class TestHashSetStringExtensions
    {
        [Test]
        public void ContainsSuffixFor_Null_False()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsFalse(hashSet.ContainsSuffixFor(null, '.'));
        }

        [Test]
        public void ContainsSuffixFor_Blank_False()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsFalse(hashSet.ContainsSuffixFor("", '.'));
        }

        [Test]
        public void ContainsSuffixFor_Delimiter_False()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsFalse(hashSet.ContainsSuffixFor(".", '.'));
        }

        [Test]
        public void ContainsSuffixFor_EmptyHashSet_False()
        {
            var hashSet = new HashSet<string>();
            Assert.IsFalse(hashSet.ContainsSuffixFor("FirstSection_is_extracted_from_RawUrl.it", '.'));
        }

        [Test]
        public void ContainsSuffixFor_BlankHashSetEmpty_False()
        {
            var hashSet = new HashSet<string> { "" };
            Assert.IsFalse(hashSet.ContainsSuffixFor("", '.'));
        }

        [Test]
        public void ContainsSuffixFor_ExactString_True()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsTrue(hashSet.ContainsSuffixFor("FirstSection_is_extracted_from_RawUrl.it", '.'));
        }

        [Test]
        public void ContainsSuffixFor_SuffixTooShort_False()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsFalse(hashSet.ContainsSuffixFor("it", '.'));
        }

        [Test]
        public void ContainsSuffixFor_SuffixMatches_True()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsTrue(hashSet.ContainsSuffixFor("www.FirstSection_is_extracted_from_RawUrl.it", '.'));
        }

        [Test]
        public void ContainsSuffixFor_SuffixMatchesWithoutDelimiter_False()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.IsFalse(hashSet.ContainsSuffixFor("wwwtest.it", '.'));
        }
    }
}
