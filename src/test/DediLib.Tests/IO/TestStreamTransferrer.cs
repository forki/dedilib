using System.IO;
using System.Text;
using DediLib.IO;
using NUnit.Framework;

namespace DediLib.Tests.IO
{
    [TestFixture]
    public class TestStreamTransferrer
    {
        [Test]
        public void Transfer_bufsize_smaller_content()
        {
            const string content = "Hello World";

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var output = new MemoryStream())
            using (var transferrer = new StreamTransferrer(input, output, bufSize: 2))
            {
                transferrer.Transfer().Wait();

                Assert.AreEqual(content, Encoding.UTF8.GetString(output.ToArray()));
            }
        }

        [Test]
        public void Transfer_bufsize_larger_content()
        {
            const string content = "Hello World";

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var output = new MemoryStream())
            using (var transferrer = new StreamTransferrer(input, output, bufSize: 1024))
            {
                transferrer.Transfer().Wait();

                Assert.AreEqual(content, Encoding.UTF8.GetString(output.ToArray()));
            }
        }

        [Test]
        public void Transfer_AutoCloseStreams_false()
        {
            const string content = "Hello World";

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var output = new MemoryStream())
            {
                using (var transferrer = new StreamTransferrer(input, output, false))
                {
                    transferrer.Transfer().Wait();
                }

                Assert.IsTrue(input.CanRead || input.CanSeek || input.CanWrite);
                Assert.IsTrue(output.CanRead || output.CanSeek || output.CanWrite);
            }
        }

        [Test]
        public void Transfer_AutoCloseStreams_true()
        {
            const string content = "Hello World";

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var output = new MemoryStream())
            {
                using (var transferrer = new StreamTransferrer(input, output))
                {
                    transferrer.Transfer().Wait();
                }

                Assert.IsFalse(input.CanRead || input.CanSeek || input.CanWrite);
                Assert.IsFalse(output.CanRead || output.CanSeek || output.CanWrite);
            }
        }

        [Test]
        public void Transfer_multiple_streams()
        {
            const string content = "Hello World";

            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var output1 = new MemoryStream())
            using (var output2 = new MemoryStream())
            using (var transferrer = new StreamTransferrer(input, new [] { output1, output2 }))
            {
                transferrer.Transfer().Wait();

                Assert.AreEqual(content, Encoding.UTF8.GetString(output1.ToArray()));
                Assert.AreEqual(content, Encoding.UTF8.GetString(output2.ToArray()));
            }
        }
    }
}
