using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FluentTerminal.SystemTray.Services.ConPty
{
    public class ConPtySession : ITerminalSession
    {
        private TerminalsManager _terminalsManager;
        private Terminal _terminal;
        private bool _exited;

        public int Id { get; private set; }

        public string ShellExecutableName { get; private set; }

        public event EventHandler ConnectionClosed;

        public void Close()
        {
            ConnectionClosed?.Invoke(this, EventArgs.Empty);
        }

        public void Resize(TerminalSize size)
        {
            _terminal.Resize(size.Columns, size.Rows);
        }

        public void Start(CreateTerminalRequest request, TerminalsManager terminalsManager)
        {
            Id = request.Id;
            _terminalsManager = terminalsManager;

            ShellExecutableName = Path.GetFileNameWithoutExtension(request.Profile.Location);
            var cwd = GetWorkingDirectory(request.Profile);

            var args = string.Empty;
            if (!string.IsNullOrWhiteSpace(request.Profile.Location))
            {
                args = $"\"{request.Profile.Location}\" {request.Profile.Arguments}";
            }
            else
            {
                args = request.Profile.Arguments;
            }

            _terminal = new Terminal();
            _terminal.OutputReady += _terminal_OutputReady;
            _terminal.Exited += _terminal_Exited;
            Task.Run(() => _terminal.Start(args, cwd, request.Size.Columns, request.Size.Rows));
        }

        private void _terminal_Exited(object sender, EventArgs e)
        {
            _exited = true;
            Close();
        }

        private string GetWorkingDirectory(ShellProfile configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.WorkingDirectory) || !Directory.Exists(configuration.WorkingDirectory))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            return configuration.WorkingDirectory;
        }

        private void _terminal_OutputReady(object sender, EventArgs e)
        {
            ListenToStdOut();
        }

        private void ListenToStdOut()
        {
            Task.Factory.StartNew(async () =>
            {
                using (var reader = new StreamReader(_terminal.ConsoleOutStream))
                {
                    do
                    {
                        var buffer = new byte[1024];
                        var readBytes = await _terminal.ConsoleOutStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
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

        public void Write(byte[] data)
        {
            _terminal.WriteToPseudoConsole(data);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _terminal?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            _terminal.Exited -= _terminal_Exited;
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}