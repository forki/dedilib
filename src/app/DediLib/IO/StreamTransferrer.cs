using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DediLib.IO
{
    public class StreamTransferrer : IDisposable
    {
        private readonly Stream _inputStream;
        private readonly List<Stream> _outputStreams = new List<Stream>();
        private readonly bool _autoClose;
        private readonly int _bufSize;

        public StreamTransferrer(Stream inputStream, Stream outputStream, bool autoClose = true, int bufSize = 1024 * 1024)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            _inputStream = inputStream;
            _outputStreams.Add(outputStream);
            _autoClose = autoClose;
            _bufSize = bufSize;
        }

        public StreamTransferrer(Stream inputStream, IEnumerable<Stream> outputStreams, bool autoClose = true, int bufSize = 1024 * 1024)
        {
            if (inputStream == null) throw new ArgumentNullException(nameof(inputStream));
            if (outputStreams == null) throw new ArgumentNullException(nameof(outputStreams));

            _inputStream = inputStream;
            _outputStreams.AddRange(outputStreams);
            _autoClose = autoClose;
            _bufSize = bufSize;
        }

        public async Task Transfer(params Action<byte[], int>[] hooks)
        {
            await Transfer(CancellationToken.None, hooks).ConfigureAwait(false);
        }

        public async Task Transfer(CancellationToken cancellationToken, params Action<byte[], int>[] hooks)
        {
            var buf = new byte[_bufSize];

            int read;
            var tasks = new Task[_outputStreams.Count];
            while ((read = await _inputStream.ReadAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (hooks != null)
                {
                    foreach (var hook in hooks)
                        hook(buf, read);
                }

                for (var i = 0; i < _outputStreams.Count; i++)
                {
                    tasks[i] = _outputStreams[i].WriteAsync(buf, 0, read, cancellationToken);
                }
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_autoClose)
                    {
                        _inputStream?.Dispose();
                        if (_outputStreams != null)
                        {
                            foreach (var outputStream in _outputStreams)
                                outputStream.Dispose();
                        }
                    }
                }

                _disposed = true;
            }
        }

        ~StreamTransferrer()
        {
            Dispose(false);
        }

        private bool _disposed;
    }
}
