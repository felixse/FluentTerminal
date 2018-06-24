using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using static winpty.WinPty;

namespace FluentTerminal.SystemTray.Services
{
    public class TerminalSession : IDisposable
    {
        private ManualResetEventSlim _connectedEvent;
        private bool _disposedValue;
        private IntPtr _handle;
        private Stream _stdin;
        private Stream _stdout;
        private NamedPipeServerStream _outServer;

        public TerminalSession(CreateTerminalRequest request)
        {
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
                    throw new Exception(winpty_error_msg(errorHandle).ToString());
                }

                string exe = request.Profile.Location;
                string args = $"\"{exe}\" {request.Profile.Arguments}";
                string cwd = GetWorkingDirectory(request.Profile);
                spawnConfigHandle = winpty_spawn_config_new(WINPTY_SPAWN_FLAG_AUTO_SHUTDOWN, exe, args, cwd, null, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_msg(errorHandle).ToString());
                }

                _stdin = CreatePipe(winpty_conin_name(_handle), PipeDirection.Out);
                _stdout = CreatePipe(winpty_conout_name(_handle), PipeDirection.In);

                if (!winpty_spawn(_handle, spawnConfigHandle, out IntPtr process, out IntPtr thread, out int procError, out errorHandle))
                {
                    throw new Exception($"Failed to start the shell process. Please check your shell settings.\nTried to start: {args}");
                }

                ShellExecutableName = Path.GetFileNameWithoutExtension(exe);
            }
            finally
            {
                winpty_config_free(configHandle);
                winpty_spawn_config_free(spawnConfigHandle);
                winpty_error_free(errorHandle);
            }

            var port = Utilities.GetAvailablePort();

            if (port == null)
            {
                throw new Exception("no port available");
            }

            Id = port.Value;
            _connectedEvent = new ManualResetEventSlim(false);

            PipeName = @"LOCAL\" + Guid.NewGuid().ToString();

            _outServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.None);

            StreamReader reader = new StreamReader(_outServer);



            ListenToStdOut();
            ListenToNamedPipe();
        }

        ~TerminalSession()
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

        public int Id { get; }

        public string PipeName { get; }

        public string ShellExecutableName { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Resize(TerminalSize size)
        {
            var errorHandle = IntPtr.Zero;
            try
            {
                winpty_set_size(_handle, size.Columns, size.Rows, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_msg(errorHandle).ToString());
                }
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

        private void ListenToNamedPipe()
        {
            new NotificationService().ShowNotification("DEBUG", "wating now");
            _outServer.WaitForConnection();
            new NotificationService().ShowNotification("DEBUG", "connected");

            Task.Run(async () =>
            {
                //_connectedEvent.Wait();
                var reader = new StreamReader(_outServer);
                var writer = new StreamWriter(_stdin);

                do
                {
                    var offset = 0;
                    var buffer = new char[1024];
                    var readChars = await reader.ReadAsync(buffer, offset, buffer.Length - offset);
                    if (readChars > 0)
                    {
                        writer.Write(buffer, 0, readChars);
                    }
                }
                while (!reader.EndOfStream);
                _stdin.Close();
            });
        }

        private void ListenToStdOut()
        {
            Task.Run(async () =>
            {
                var reader = new StreamReader(_stdout);
                var writer = new StreamWriter(_outServer);

                do
                {
                    var offset = 0;
                    var buffer = new char[1024];
                    var readChars = await reader.ReadAsync(buffer, offset, buffer.Length - offset);
                    if (readChars > 0)
                    {
                        writer.Write(buffer, 0, readChars);
                    }
                }
                while (!reader.EndOfStream);
                _outServer.Close();
            });
        }
    }
}