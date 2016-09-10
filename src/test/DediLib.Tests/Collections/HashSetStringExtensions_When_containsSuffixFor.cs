using System.Collections.Generic;
using DediLib.Collections;
using NUnit.Framework;

namespace DediLib.Tests.Collections
{
    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class HashSetStringExtensions_When_containsSuffixFor
    {
        [Test]
        public void If_text_is_null_Then_return_false()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor(null, '.'), Is.False);
        }

        [Test]
        public void If_text_is_blank_Then_return_false()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor("", '.'), Is.False);
        }

        [Test]
        public void If_text_is_delimiter_Then_return_false()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor(".", '.'), Is.False);
        }

        [Test]
        public void If_hashSet_empty_and_long_text_Then_return_false()
        {
            var hashSet = new HashSet<string>();
            Assert.That(hashSet.ContainsSuffixFor("FirstSection_is_extracted_from_RawUrl.it", '.'), Is.False);
        }

        [Test]
        public void If_hashSet_contains_blank_text_and_text_is_blank_Then_return_false()
        {
            var hashSet = new HashSet<string> { "" };
            Assert.That(hashSet.ContainsSuffixFor("", '.'), Is.False);
        }

        [Test]
        public void If_hashSet_contains_text_and_text_is_exactly_the_same_Then_return_true()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor("FirstSection_is_extracted_from_RawUrl.it", '.'), Is.True);
        }

        [Test]
        public void If_hashSet_contains_text_and_text_is_shorter_than_hashSet_text_Then_return_false()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor("it", '.'), Is.False);
        }

        [Test]
        public void If_hashSet_contains_text_and_text_is_suffix_for_containing_text_Then_return_true()
        {
            var hashSet = new HashSet<string> { "FirstSection_is_extracted_from_RawUrl.it" };
            Assert.That(hashSet.ContainsSuffixFor("www.FirstSection_is_extracted_from_RawUrl.it", '.'), Is.True);
        }

        [Test]
        public void If_delimiter_does_not_match_Then_return_false()
        {
            var hashSet = new HashSet<string> { "wwwtest.it" };
            Assert.That(hashSet.ContainsSuffixFor("bla.wwwtest.it", ','), Is.False);
        }
    }
}
