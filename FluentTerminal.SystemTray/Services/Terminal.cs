using Fleck;
using FluentTerminal.SystemTray.DataTransferObjects;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static winpty.WinPty;

namespace FluentTerminal.SystemTray.Services
{
    public class Terminal : IDisposable
    {
        private IntPtr _handle;
        private Stream _stdin;
        private Stream _stdout;
        private bool _disposedValue;
        private ManualResetEventSlim _connectedEvent;

        private IWebSocketConnection _webSocket;

        public int Id { get; }

        public string WebSocketUrl { get; }

        public event EventHandler ConnectionClosed;

        public Terminal(TerminalOptions options)
        {
            var configHandle = IntPtr.Zero;
            var spawnConfigHandle = IntPtr.Zero;
            var errorHandle = IntPtr.Zero;

            try
            {
                configHandle = winpty_config_new(WINPTY_FLAG_COLOR_ESCAPES, out errorHandle);
                winpty_config_set_initial_size(configHandle, options.Columns, options.Rows);

                _handle = winpty_open(configHandle, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_code(errorHandle).ToString());
                }

                string exe = @"C:\windows\system32\WindowsPowerShell\v1.0\powershell.exe";
                string args = "";
                string cwd = @"C:\";
                spawnConfigHandle = winpty_spawn_config_new(WINPTY_SPAWN_FLAG_AUTO_SHUTDOWN, exe, args, cwd, null, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_code(errorHandle).ToString());
                }

                _stdin = CreatePipe(winpty_conin_name(_handle), PipeDirection.Out);
                _stdout = CreatePipe(winpty_conout_name(_handle), PipeDirection.In);

                if (!winpty_spawn(_handle, spawnConfigHandle, out IntPtr process, out IntPtr thread, out int procError, out errorHandle))
                {
                    throw new Exception(winpty_error_code(errorHandle).ToString());
                }
            }
            finally
            {
                winpty_config_free(configHandle);
                winpty_spawn_config_free(spawnConfigHandle);
                winpty_error_free(errorHandle);
            }


            var port = Utilities.GetAvailablePort(49151);

            if (port == null)
            {
                throw new Exception("no port available");
            }

            Id = port.Value;
            _connectedEvent = new ManualResetEventSlim(false);
            WebSocketUrl = "ws://127.0.0.1:" + port;
            var webSocketServer = new WebSocketServer(WebSocketUrl);
            webSocketServer.Start(socket =>
            {
                _webSocket = socket;
                socket.OnOpen = () =>
                {
                    Console.WriteLine("open");
                    _connectedEvent.Set();
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("closing");
                    ConnectionClosed?.Invoke(this, EventArgs.Empty);
                };
                socket.OnMessage = message =>
                {
                    var bytes = Encoding.ASCII.GetBytes(message);
                    _stdin.Write(bytes, 0, bytes.Length);
                };
            });

            ListenToStdOut();
        }

        public void Resize(int cols, int rows)
        {
            var errorHandle = IntPtr.Zero;
            try
            {
                winpty_set_size(_handle, cols, rows, out errorHandle);
                if (errorHandle != IntPtr.Zero)
                {
                    throw new Exception(winpty_error_code(errorHandle).ToString());
                }
            }
            finally
            {
                winpty_error_free(errorHandle);
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
            Task.Run(async () =>
            {
                _connectedEvent.Wait();
                var reader = new StreamReader(_stdout);
                do
                {
                    var offset = 0;
                    var buffer = new char[1024];
                    var readChars = await reader.ReadAsync(buffer, offset, buffer.Length - offset);
                    if (readChars > 0)
                    {
                        await _webSocket.Send(new String(buffer));
                    }
                }
                while (!reader.EndOfStream);
            });
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

        ~Terminal()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
