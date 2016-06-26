using System;
using System.IO;
using DediLib.IO;
using NUnit.Framework;

namespace DediLib.Tests.IO
{
    [TestFixture]
    public class TestStreamSplitter
    {
        [Test]
        public void Constructor_null_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new StreamSplitter(null));
        }

        [Test]
        public void CanRead_false()
        {
            var stream = new StreamSplitter(new MemoryStream());
            Assert.IsFalse(stream.CanRead);
        }

        [Test]
        public void Constructor_PrimaryStreamNotWritable_ThrowsException()
        {
            var notWritableStream = new MemoryStream(new byte[] {}, false);
            Assert.Throws<InvalidOperationException>(() => new StreamSplitter(notWritableStream));
        }

        [Test]
        public void Constructor_OtherStreamNotWritable_ThrowsException()
        {
            var notWritableStream = new MemoryStream(new byte[] { }, false);
            Assert.Throws<InvalidOperationException>(() => new StreamSplitter(new MemoryStream(), notWritableStream));
        }

        [Test]
        public void Constructor_AllStreamsOk_NoException()
        {
            new StreamSplitter(new MemoryStream(), new MemoryStream());
        }

        [Test]
        public void SetPosition_TwoStreams_BothAtSamePositionAfterSeek()
        {
            var subStream1 = new MemoryStream(new byte[1000]);
            var subStream2 = new MemoryStream(new byte[1000]);

            subStream1.Position = 1;
            subStream2.Position = 2;

            var stream = new StreamSplitter(subStream1, subStream2)
            {
                Position = 100
            };

            Assert.AreEqual(100, subStream1.Position);
            Assert.AreEqual(100, subStream2.Position);
        }

        [Test]
        public void RelativeSeek_TwoStreams_PositionsRelativelyMoved()
        {
            var subStream1 = new MemoryStream(new byte[1000]);
            var subStream2 = new MemoryStream(new byte[1000]);

            subStream1.Position = 1;
            subStream2.Position = 2;

            var stream = new StreamSplitter(subStream1, subStream2);
            stream.Seek(100, SeekOrigin.Current);

            Assert.AreEqual(101, subStream1.Position);
            Assert.AreEqual(102, subStream2.Position);
        }
    }
}
