using System;
using System.IO;
using System.Threading.Tasks;

namespace FluentTerminal.SystemTray
{
    internal sealed class BufferedReader : IDisposable
    {
        private const int MaxTotalDelayMilliseconds = 200;
        private const int WaitPeriodMilliseconds = 50;
        private const int NearReadsPeriodMilliseconds = WaitPeriodMilliseconds;
        private const int NearReadsBufferingTrigger = 3;
        private const int BufferSize = 204800;

        private readonly object _lock = new object();

        private readonly Stream _stream;
        private readonly Action<byte[]> _callback;

        private bool _disposed;
        private bool _paused;

        private byte[] _buffer;
        private int _bufferIndex;
        private DateTime _lastRead;
        private DateTime _sendingDeadline;
        private DateTime _scheduledSend;
        private int _nearReadingsCount;

        internal BufferedReader(Stream stream, Action<byte[]> callback)
        {
            _stream = stream;
            _callback = callback;

            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Task.Factory.StartNew(ReadingLoop, TaskCreationOptions.LongRunning);
        }

        internal void SetPaused(bool value)
        {
            lock (_lock)
                _paused = value;
        }

        public void Dispose()
        {
            lock (_buffer)
            {
                _disposed = true;
            }
        }

        private async Task ReadingLoop()
        {
            while (true)
            {
                bool paused;

                lock (_lock)
                {
                    if (_disposed)
                        return;

                    paused = _paused;
                }

                if (paused)
                {
                    await Task.Delay(WaitPeriodMilliseconds).ConfigureAwait(false);

                    continue;
                }

                var currentBuffer = new byte[BufferSize];

                int read;

                try
                {
                    read = await _stream.ReadAsync(currentBuffer, 0, currentBuffer.Length).ConfigureAwait(false);
                }
                catch
                {
                    read = 0;
                }

                if (read < 1)
                {
                    // Expected to happen only when terminal is closed.
                    // Probably not recoverable, but we'll wait anyway 'till disposed.
                    await Task.Delay(50).ConfigureAwait(false);

                    continue;
                }

                var now = DateTime.UtcNow;

                lock (_lock)
                {
                    if (_buffer != null)
                    {
                        // We're already in buffered mode

                        _lastRead = now;

                        if (_bufferIndex + read > BufferSize)
                        {
                            // No room in the buffer. Have to flush it.
                            SendBuffer();

                            _buffer = currentBuffer;
                            _bufferIndex = 0;
                            _sendingDeadline = now.AddMilliseconds(MaxTotalDelayMilliseconds);
                        }
                        else
                        {
                            // Copy to existing buffer
                            Buffer.BlockCopy(currentBuffer, 0, _buffer, _bufferIndex, read);

                            _scheduledSend = now.AddMilliseconds(WaitPeriodMilliseconds);
                        }

                        continue;
                    }

                    if (now.Subtract(_lastRead).TotalMilliseconds < NearReadsPeriodMilliseconds)
                        _nearReadingsCount++;
                    else
                        _nearReadingsCount = 0;

                    _lastRead = now;

                    if (_nearReadingsCount >= NearReadsBufferingTrigger)
                    {
                        // We should enter buffered mode
                        _buffer = currentBuffer;
                        _bufferIndex = 0;
                        _sendingDeadline = now.AddMilliseconds(MaxTotalDelayMilliseconds);
                        _scheduledSend = now.AddMilliseconds(WaitPeriodMilliseconds);

                        // ReSharper disable once AssignmentIsFullyDiscarded
                        _ = SendAsync();

                        _nearReadingsCount = 0;

                        continue;
                    }
                }

                // Not in buffering mode. Just send
                if (read == BufferSize)
                    _callback(currentBuffer);
                else
                {
                    var newBuff = new byte[read];

                    Buffer.BlockCopy(currentBuffer, 0, newBuff, 0, read);

                    _callback(newBuff);
                }
            }
        }

        private async Task SendAsync()
        {
            // Just to release the calling thread asap
            await Task.Delay(5).ConfigureAwait(false);

            while (true)
            {
                TimeSpan sleep;

                lock (_lock)
                {
                    if (_paused)
                        sleep = TimeSpan.FromMilliseconds(WaitPeriodMilliseconds);
                    else
                    {
                        sleep = _scheduledSend < _sendingDeadline
                            ? _scheduledSend.Subtract(DateTime.UtcNow)
                            : _sendingDeadline.Subtract(DateTime.UtcNow);

                        if (sleep.TotalMilliseconds < 5)
                        {
                            // Time to send
                            SendBuffer();

                            return;
                        }
                    }
                }

                await Task.Delay(sleep).ConfigureAwait(false);
            }
        }

        // Has to be called from a locked code!
        private void SendBuffer()
        {
            if (_bufferIndex == _buffer.Length)
            {
                _callback(_buffer);
            }
            else
            {
                var newBuffer = new byte[_bufferIndex];

                Buffer.BlockCopy(_buffer, 0, newBuffer, 0, _bufferIndex);

                _callback(newBuffer);
            }

            _buffer = null;
            _bufferIndex = 0;
        }
    }
}
