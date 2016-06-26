using DediLib.IO;
using NUnit.Framework;

namespace DediLib.Tests.IO
{
    [TestFixture]
    public class TestFileNameCleaner
    {
        [Test]
        public void ReplaceInvalidPathChars()
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual("abc_", fileNameCleaner.ReplaceInvalidPathChars("abc|", "_"));
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("abc|", "abc")]
        [TestCase("abc>", "abc")]
        [TestCase("abc?", "abc?")]
        [TestCase("abc*", "abc*")]
        [TestCase(@"C:\Temp", @"C:\Temp")]
        public void Remove_InvalidPathChars(string fileName, string expectedOutput)
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual(expectedOutput, fileNameCleaner.ReplaceInvalidPathChars(fileName));
        }

        [Test]
        public void ReplaceInvalidFileChars()
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual("abc_", fileNameCleaner.ReplaceInvalidFileChars("abc*", "_"));
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("abc|", "abc")]
        [TestCase("abc>", "abc")]
        [TestCase("abc?", "abc")]
        [TestCase("abc*", "abc")]
        [TestCase(@"C:\Temp", "CTemp")]
        public void Remove_InvalidFileChars(string fileName, string expectedOutput)
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual(expectedOutput, fileNameCleaner.ReplaceInvalidFileChars(fileName));
        }

        [Test]
        public void ReplaceAllInvalidChars()
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual("abc_", fileNameCleaner.ReplaceAllInvalidChars("abc*", "_"));
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("abc", "abc")]
        [TestCase("abc|", "abc")]
        [TestCase("abc>", "abc")]
        [TestCase("abc?", "abc")]
        [TestCase("abc*", "abc")]
        [TestCase(@"C:\Temp", "CTemp")]
        public void Remove_AllInvalidChars(string fileName, string expectedOutput)
        {
            var fileNameCleaner = new FileNameCleaner();
            Assert.AreEqual(expectedOutput, fileNameCleaner.ReplaceAllInvalidChars(fileName));
        }
    }
}
