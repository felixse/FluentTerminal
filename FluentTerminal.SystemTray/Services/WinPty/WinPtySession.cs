using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using FluentTerminal.SystemTray.Native;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using static winpty.WinPty;

namespace FluentTerminal.SystemTray.Services.WinPty
{
    public class WinPtySession : IDisposable, ITerminalSession
    {
        private bool _disposedValue;
        private IntPtr _handle;
        private Stream _stdin;
        private Stream _stdout;
        private TerminalsManager _terminalsManager;
        private Process _shellProcess;
        private bool _exited;
        private TerminalSize _terminalSize;

        public void Start(CreateTerminalRequest request, TerminalsManager terminalsManager)
        {
            Id = request.Id;
            _terminalsManager = terminalsManager;
            _terminalSize = request.Size;

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

                spawnConfigHandle = winpty_spawn_config_new(WINPTY_SPAWN_FLAG_AUTO_SHUTDOWN, request.Profile.Location, args, cwd, terminalsManager.GetDefaultEnvironmentVariableString(), out errorHandle);
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
                _shellProcess.EnableRaisingEvents = true;
                _shellProcess.Exited += _shellProcess_Exited;

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

            ListenToStdOut();
        }

        private void _shellProcess_Exited(object sender, EventArgs e)
        {
            Close();
            _exited = true;
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

        public event EventHandler ConnectionClosed;

        public int Id { get; private set; }

        public string ShellExecutableName { get; private set; }

        public void Dispose()
        {
            _shellProcess.Exited -= _shellProcess_Exited;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
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
                _terminalSize = size;
            }
            finally
            {
                winpty_error_free(errorHandle);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _stdin?.Dispose();
                    _stdout?.Dispose();
                }

                winpty_free(_handle);

                _disposedValue = true;
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

        private void ListenToStdOut()
        {
            Task.Factory.StartNew(async () =>
            {
                using (var reader = new StreamReader(_stdout))
                {
                    do
                    {
                        var buffer = new byte[Math.Max(1024, _terminalSize.Columns * _terminalSize.Rows * 4)];
                        var readBytes = await _stdout.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                        var read = new byte[readBytes];
                        Buffer.BlockCopy(buffer, 0, read, 0, readBytes);

                        if (readBytes > 0)
                        {
                            _terminalsManager.DisplayTerminalOutput(Id, read);
                        }
                    }
                    while (!_exited);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}