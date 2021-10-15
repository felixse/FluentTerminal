using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static winpty.WinPty;

namespace FluentTerminal.SystemTray.Services.WinPty
{
    public static class ProcessApi
    {
        [DllImport("kernel32.dll")]
        public static extern int GetProcessId(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetExitCodeProcess(IntPtr hProcess, out uint ExitCode);
    }

    internal sealed class BufferedReader : IDisposable
    {
        private const int MaxTotalDelayMilliseconds = 100;
        private const int WaitPeriodMilliseconds = 30;
        private const int NearReadsPeriodMilliseconds = 20;
        private const int NearReadsBufferingTrigger = 3;
        private const int BufferSize = 204800;

        private readonly object _lock = new object();

        private readonly Stream _stream;
        private readonly Action<byte[]> _callback;
        private readonly bool _enableBuffer;

        private bool _disposed;
        private bool _paused;

        private byte[] _buffer;
        private int _bufferIndex;
        private DateTime _lastRead;
        private DateTime _sendingDeadline;
        private DateTime _scheduledSend;
        private int _nearReadingsCount;
        private Task _sendingTask;

        internal BufferedReader(Stream stream, Action<byte[]> callback, bool enableBuffer)
        {
            _stream = stream;
            _callback = callback;
            _enableBuffer = enableBuffer;

            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Task.Factory.StartNew(ReadingLoop, TaskCreationOptions.LongRunning);
        }

        internal void SetPaused(bool value)
        {
            lock (_lock) _paused = value;
        }

        public void Dispose()
        {
            lock (_lock) _disposed = true;
        }

        private async Task ReadingLoop()
        {
            while (true)
            {
                // Allow CPU to jump between TerminalSessions' ReadingLoop Tasks 
                await Task.Delay(1).ConfigureAwait(false);

                bool paused;

                lock (_lock)
                {
                    if (_disposed) return;

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

                if (!_enableBuffer)
                {
                    // Buffering disabled. Just send.

                    _buffer = currentBuffer;
                    _bufferIndex = read;

                    SendBuffer();

                    continue;
                }

                var now = DateTime.UtcNow;

                lock (_lock)
                {
                    if (_buffer != null)
                    {
                        // We're already in buffered mode

                        if (_bufferIndex + read > BufferSize)
                        {
                            // No room in the buffer. Have to flush it.
                            SendBuffer();

                            _buffer = currentBuffer;
                            _bufferIndex = 0;

                            _scheduledSend = now.AddMilliseconds(WaitPeriodMilliseconds);
                            _sendingDeadline = now.AddMilliseconds(MaxTotalDelayMilliseconds);
                        }
                        else
                        {
                            // Copy to existing buffer
                            Buffer.BlockCopy(currentBuffer, 0, _buffer, _bufferIndex, read);
                            _bufferIndex += read;

                            _scheduledSend = now.AddMilliseconds(WaitPeriodMilliseconds);
                        }

                        if (now.Subtract(_lastRead).TotalMilliseconds < NearReadsPeriodMilliseconds)
                        {
                            // We should stop buffered mode
                            SendBuffer();
                        }

                        _lastRead = now;

                        continue;
                    }

                    if (now.Subtract(_lastRead).TotalMilliseconds < NearReadsPeriodMilliseconds)
                    {
                        _nearReadingsCount++;
                    }
                    else
                    {
                        _nearReadingsCount = 0;
                    }

                    _lastRead = now;

                    if (_nearReadingsCount >= NearReadsBufferingTrigger)
                    {
                        // We should enter buffered mode
                        _buffer = currentBuffer;
                        _bufferIndex = 0;
                        _sendingDeadline = now.AddMilliseconds(MaxTotalDelayMilliseconds);
                        _scheduledSend = now.AddMilliseconds(WaitPeriodMilliseconds);

                        if (_sendingTask == null) _sendingTask = SendAsync();

                        _nearReadingsCount = 0;

                        continue;
                    }

                    // Not in buffering mode. Just send.
                    _buffer = currentBuffer;
                    _bufferIndex = read;

                    SendBuffer();
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
                    if (_buffer == null)
                    {
                        _sendingTask = null;

                        return;
                    }

                    if (_paused)
                    {
                        sleep = TimeSpan.FromMilliseconds(WaitPeriodMilliseconds);
                    }
                    else
                    {
                        sleep = _scheduledSend < _sendingDeadline
                            ? _scheduledSend.Subtract(DateTime.UtcNow)
                            : _sendingDeadline.Subtract(DateTime.UtcNow);

                        if (sleep.TotalMilliseconds < 5)
                        {
                            // Time to send
                            SendBuffer();

                            _sendingTask = null;

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

    public class WinPtySession : ITerminalSession
    {
        private bool _disposed;
        private IntPtr _handle;
        private Stream _stdin;
        private Stream _stdout;
        private Process _shellProcess;
        private BufferedReader _reader;
        private bool _enableBuffer;

        public void Start(CreateTerminalRequest request)
        {
            _enableBuffer = false; // request.Profile.UseBuffer;

            Id = request.Id;

            var configHandle = IntPtr.Zero;
            var spawnConfigHandle = IntPtr.Zero;
            var errorHandle = IntPtr.Zero;

            try
            {
                configHandle = winpty_config_new(WINPTY_FLAG_COLOR_ESCAPES, out errorHandle);
                winpty_config_set_initial_size(configHandle, request.Size.Columns, request.Size.Rows);

                _handle = winpty_open(configHandle, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_msg(errorHandle));
                }

                var cwd = GetWorkingDirectory(request.Profile);
                var args = request.Profile.Arguments;

                if (!string.IsNullOrWhiteSpace(request.Profile.Location))
                {
                    args = $"\"{request.Profile.Location}\" {args}";
                }

                string env = ""; // terminalsManager.GetDefaultEnvironmentVariableString(request.Profile.EnvironmentVariables) todo
                spawnConfigHandle = winpty_spawn_config_new(WINPTY_SPAWN_FLAG_AUTO_SHUTDOWN, request.Profile.Location, args, cwd, env, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_msg(errorHandle));
                }

                _stdin = CreatePipe(winpty_conin_name(_handle), PipeDirection.Out);
                _stdout = CreatePipe(winpty_conout_name(_handle), PipeDirection.In);

                if (!winpty_spawn(_handle, spawnConfigHandle, out IntPtr process, out IntPtr thread, out int procError, out errorHandle))
                {
                    throw new Exception($@"Failed to start the shell process. Please check your shell settings.{Environment.NewLine}Tried to start: {request.Profile.Location} ""{request.Profile.Arguments}""");
                }

                var shellProcessId = ProcessApi.GetProcessId(process);
                _shellProcess = Process.GetProcessById(shellProcessId);
                //_shellProcess.EnableRaisingEvents = true;
                //_shellProcess.Exited += _shellProcess_Exited;

                if (!string.IsNullOrWhiteSpace(request.Profile.Location))
                {
                    ShellExecutableName = Path.GetFileNameWithoutExtension(request.Profile.Location);
                }
                else
                {
                    ShellExecutableName = request.Profile.Arguments.Split(' ')[0];
                }
            }
            finally
            {
                winpty_config_free(configHandle);
                winpty_spawn_config_free(spawnConfigHandle);
                winpty_error_free(errorHandle);
            }

            _reader = new BufferedReader(_stdout, bytes => OutputReceived?.Invoke(this, new TerminalOutput { Data = bytes }),
                _enableBuffer);
        }

        private void _shellProcess_Exited(object sender, EventArgs e)
        {
            Close();
        }

        ~WinPtySession()
        {
            Dispose(false);
        }

        private string GetWorkingDirectory(ShellProfile configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.WorkingDirectory) || !Directory.Exists(configuration.WorkingDirectory))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            return configuration.WorkingDirectory;
        }

        public event EventHandler<int> ConnectionClosed;

        public event EventHandler<TerminalOutput> OutputReceived;

        public byte Id { get; private set; }

        public string ShellExecutableName { get; private set; }

        public void Dispose()
        {
            _shellProcess.Exited -= _shellProcess_Exited;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            _reader?.Dispose();

            int exitCode = -1;
            if (_shellProcess != null && _shellProcess.HasExited)
            {
                exitCode = _shellProcess.ExitCode;
            }
            ConnectionClosed?.Invoke(this, exitCode);
        }

        public void Write(byte[] data)
        {
            _stdin.Write(data, 0, data.Length);
        }

        public void Resize(TerminalSize size)
        {
            var errorHandle = IntPtr.Zero;
            try
            {
                winpty_set_size(_handle, size.Columns, size.Rows, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_msg(errorHandle));
                }
            }
            finally
            {
                winpty_error_free(errorHandle);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _stdin?.Dispose();
                    _stdout?.Dispose();
                }

                winpty_free(_handle);

                _reader?.Dispose();

                _disposed = true;
            }
        }

        private Stream CreatePipe(string pipeName, PipeDirection direction)
        {
            string serverName = ".";
            if (pipeName.StartsWith("\\"))
            {
                int slash3 = pipeName.IndexOf('\\', 2);
                if (slash3 != -1)
                {
                    serverName = pipeName.Substring(2, slash3 - 2);
                }
                int slash4 = pipeName.IndexOf('\\', slash3 + 1);
                if (slash4 != -1)
                {
                    pipeName = pipeName.Substring(slash4 + 1);
                }
            }

            var pipe = new NamedPipeClientStream(serverName, pipeName, direction);
            pipe.Connect();
            return pipe;
        }

        public void Pause(bool value)
        {
            _reader?.SetPaused(value);
        }
    }
}