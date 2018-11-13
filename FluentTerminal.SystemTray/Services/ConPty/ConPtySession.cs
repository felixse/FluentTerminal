using System;
using System.IO;
using System.Threading.Tasks;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;

namespace FluentTerminal.SystemTray.Services.ConPty
{
    public class ConPtySession : ITerminalSession
    {
        private TerminalsManager _terminalsManager;
        private Terminal _terminal;

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
            _terminalsManager = terminalsManager;

            var port = Utilities.GetAvailablePort();

            if (port == null)
            {
                throw new Exception("no port available");
            }

            Id = port.Value;
            ShellExecutableName = Path.GetFileNameWithoutExtension(request.Profile.Location);

            _terminal = new Terminal();
            _terminal.OutputReady += _terminal_OutputReady;
            Task.Run(() => _terminal.Start(request.Profile.Location, request.Size.Columns, request.Size.Rows));
        }

        private void _terminal_OutputReady(object sender, EventArgs e)
        {
            ListenToStdOut();
        }

        private void ListenToStdOut()
        {
            Task.Run(async () =>
            {
                var reader = new StreamReader(_terminal.ConsoleOutStream);

                do
                {
                    var offset = 0;
                    var buffer = new char[1024];
                    var readChars = await reader.ReadAsync(buffer, offset, buffer.Length - offset).ConfigureAwait(false);

                    if (readChars > 0)
                    {
                        var output = new String(buffer, 0, readChars);
                        _terminalsManager.DisplayTerminalOutput(Id, output);
                    }
                }
                while (!reader.EndOfStream);
                Close();
            });


        }

        public void WriteText(string text)
        {
            _terminal.WriteToPseudoConsole(text);
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
            Dispose(true);
        }
        #endregion
    }
}
