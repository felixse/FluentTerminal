using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using FluentTerminal.SystemTray.Services.WinPty;

namespace FluentTerminal.SystemTray.Services.ConPty
{
    public class ConPtySession : ITerminalSession
    {
        private Terminal _terminal;

        private BufferedReader _reader;
        private bool _enableBuffer;

        public byte Id { get; private set; }

        public string ShellExecutableName { get; private set; }

        public event EventHandler<int> ConnectionClosed;

        public event EventHandler<TerminalOutput> OutputReceived;

        public void Close()
        {
            _reader?.Dispose();

            ConnectionClosed?.Invoke(this, _terminal.ExitCode);
        }

        public void Resize(TerminalSize size)
        {
            _terminal?.Resize(size.Columns, size.Rows);
        }

        public void Start(CreateTerminalRequest request)
        {
            _enableBuffer = false; // request.Profile.UseBuffer;

            _reader?.Dispose();
            _reader = null;

            Id = request.Id;

            ShellExecutableName = Path.GetFileNameWithoutExtension(request.Profile.Location);
            var cwd = GetWorkingDirectory(request.Profile);

            var args = !string.IsNullOrWhiteSpace(request.Profile.Location)
                ? $"\"{request.Profile.Location}\" {request.Profile.Arguments}"
                : request.Profile.Arguments;

            _terminal = new Terminal();
            _terminal.OutputReady += _terminal_OutputReady;
            _terminal.Exited += _terminal_Exited;

            var env = GetDefaultEnvironmentVariableString(request.Profile.EnvironmentVariables);

            Task.Factory.StartNew(() => _terminal.Start(args, cwd, env,
                request.Size.Columns, request.Size.Rows));
        }

        public string GetDefaultEnvironmentVariableString(Dictionary<string, string> additionalVariables)
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            environmentVariables["TERM_PROGRAM"] = "FluentTerminal";
            //environmentVariables["TERM_PROGRAM_VERSION"] = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

            if (additionalVariables != null)
            {
                foreach (var kvp in additionalVariables)
                {
                    environmentVariables[kvp.Key] = kvp.Value;
                }
            }

            var builder = new StringBuilder();

            foreach (DictionaryEntry item in environmentVariables)
            {
                builder.Append(item.Key).Append("=").Append(item.Value).Append("\0");
            }
            builder.Append('\0');

            return builder.ToString();
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
            if (_reader == null)
            {
                _reader = new BufferedReader(_terminal.ConsoleOutStream,
                    bytes => OutputReceived?.Invoke(this, new TerminalOutput {  Data = bytes }), _enableBuffer);
            }
        }

        public void Write(byte[] data)
        {
            _terminal?.WriteToPseudoConsole(data);
        }

        public void Pause(bool value)
        {
            _reader?.SetPaused(value);
        }

        ~ConPtySession()
        {
            Dispose(false);
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
            _terminal.OutputReady -= _terminal_OutputReady;
            Dispose(true);
        }

        #endregion IDisposable Support
    }
}