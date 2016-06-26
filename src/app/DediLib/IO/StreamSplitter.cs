using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DediLib.IO
{
    public class StreamSplitter : Stream, IKnowTotalBytesWritten
    {
        private readonly bool _closeOnDispose;

        public Stream PrimaryStream { get; }
        public List<Stream> OtherStreams { get; set; }

        public long TotalBytesWritten { get; private set; }

        public StreamSplitter(Stream primaryStream, params Stream[] otherStreams)
            : this(true, primaryStream, otherStreams)
        {
        }

        public StreamSplitter(bool closeOnDispose, Stream primaryStream, params Stream[] otherStreams)
        {
            if (primaryStream == null) throw new ArgumentNullException(nameof(primaryStream));

            _closeOnDispose = closeOnDispose;
            PrimaryStream = primaryStream;
            if (!primaryStream.CanWrite)
                throw new InvalidOperationException("Primary stream must be writable");

            OtherStreams = new List<Stream>((otherStreams ?? new Stream[0]).Where(x => x != null));
            if (!OtherStreams.All(x => x.CanWrite))
                throw new InvalidOperationException("All other streams must be writable");
        }

        private IEnumerable<Stream> GetAllStreams()
        {
            yield return PrimaryStream;
            foreach (var stream in OtherStreams.Where(x => x != null))
                yield return stream;
        }

        public override void Flush()
        {
            foreach (var stream in GetAllStreams())
                stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw new InvalidOperationException("Not all stream are seekable");

            var result = PrimaryStream.Seek(offset, origin);

            foreach (var stream in OtherStreams.Where(x => x != null))
                stream.Seek(offset, origin);

            return result;
        }

        public override void SetLength(long value)
        {
            foreach (var stream in GetAllStreams())
                stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Stream is not readable");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (var stream in GetAllStreams())
                stream.Write(buffer, offset, count);

            TotalBytesWritten += count;
        }

        public override bool CanRead => false;

        public override bool CanSeek => GetAllStreams().All(x => x.CanSeek);

        public override bool CanWrite => GetAllStreams().All(x => x.CanWrite);

        public override long Length => PrimaryStream.Length;

        public override long Position
        {
            get { return PrimaryStream.Position; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public override void Close()
        {
            var exceptions = new List<Exception>();
            foreach (var stream in GetAllStreams())
            {
                try
                {
                    stream.Close();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        protected override void Dispose(bool disposing)
        {
            if (_closeOnDispose)
                Close();

            base.Dispose(disposing);
        }
    }
}
