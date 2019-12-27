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

        private BufferedReader _reader;

        public byte Id { get; private set; }

        public string ShellExecutableName { get; private set; }

        public event EventHandler<int> ConnectionClosed;

        public void Close()
        {
            _reader?.Dispose();

            ConnectionClosed?.Invoke(this, _terminal.ExitCode);
        }

        public void Resize(TerminalSize size)
        {
            _terminal?.Resize(size.Columns, size.Rows);
        }

        public void Start(CreateTerminalRequest request, TerminalsManager terminalsManager)
        {
            Id = request.Id;
            _terminalsManager = terminalsManager;

            ShellExecutableName = Path.GetFileNameWithoutExtension(request.Profile.Location);
            var cwd = GetWorkingDirectory(request.Profile);

            var args = !string.IsNullOrWhiteSpace(request.Profile.Location)
                ? $"\"{request.Profile.Location}\" {request.Profile.Arguments}"
                : request.Profile.Arguments;

            _terminal = new Terminal();
            _terminal.OutputReady += _terminal_OutputReady;
            _terminal.Exited += _terminal_Exited;

            Task.Factory.StartNew(() => _terminal.Start(args, cwd,
                terminalsManager.GetDefaultEnvironmentVariableString(request.Profile.EnvironmentVariables),
                request.Size.Columns, request.Size.Rows));
        }

        private void _terminal_Exited(object sender, EventArgs e)
        {
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
            if (_reader != null)
                return;

            _reader = new BufferedReader(_terminal.ConsoleOutStream,
                bytes => _terminalsManager.DisplayTerminalOutput(Id, bytes));
        }

        public void Write(byte[] data)
        {
            _terminal?.WriteToPseudoConsole(data);
        }

        public void Pause(bool value)
        {
            _reader?.SetPaused(value);
        }

        #region IDisposable Support

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _terminal?.Dispose();
                }

                _disposed = true;

                _reader?.Dispose();
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