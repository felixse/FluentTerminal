using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using FluentTerminal.SystemTray.Services.ConPty;
using FluentTerminal.SystemTray.Services.WinPty;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Windows.ApplicationModel;

namespace FluentTerminal.SystemTray.Services
{
    public class TerminalsManager
    {
        private readonly Dictionary<int, ITerminalSession> _terminals = new Dictionary<int, ITerminalSession>();
        private readonly ISettingsService _settingsService;

        public event EventHandler<DisplayTerminalOutputRequest> DisplayOutputRequested;

        public event EventHandler<TerminalExitStatus> TerminalExited;

        public TerminalsManager(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void DisplayTerminalOutput(int terminalId, byte[] output)
        {
            DisplayOutputRequested?.Invoke(this, new DisplayTerminalOutputRequest
            {
                TerminalId = terminalId,
                Output = output
            });
        }

        public CreateTerminalResponse CreateTerminal(CreateTerminalRequest request)
        {
            if (_terminals.ContainsKey(request.Id))
            {
                // App terminated without cleaning up, removing orphaned sessions
                foreach (var item in _terminals.Values)
                {
                    item.Dispose();
                }
                _terminals.Clear();
            }

            string error = request.Profile?.CheckIfMosh();

            if (!string.IsNullOrEmpty(error))
                return new CreateTerminalResponse
                    {Error = error, ShellExecutableName = request.Profile.Location, Success = false};

            ITerminalSession terminal = null;
            try
            {
                if (request.SessionType == SessionType.WinPty)
                {
                    terminal = new WinPtySession();
                }
                else if (request.SessionType == SessionType.ConPty)
                {
                    terminal = new ConPtySession();
                }
                terminal.Start(request, this);
            }
            catch (Exception e)
            {
                return new CreateTerminalResponse { Error = e.ToString() };
            }

            terminal.ConnectionClosed += OnTerminalConnectionClosed;
            _terminals.Add(terminal.Id, terminal);
            return new CreateTerminalResponse
            {
                Success = true,
                ShellExecutableName = terminal.ShellExecutableName
            };
        }

        public void Write(int id, byte[] data)
        {
            if (_terminals.TryGetValue(id, out ITerminalSession terminal))
            {
                terminal.Write(data);
            }
        }

        public void ResizeTerminal(int id, TerminalSize size)
        {
            if (_terminals.TryGetValue(id, out ITerminalSession terminal))
            {
                terminal.Resize(size);
            }
            else
            {
                Debug.WriteLine($"ResizeTerminal: terminal with id '{id}' was not found");
            }
        }

        public void CloseTerminal(int id)
        {
            if (_terminals.TryGetValue(id, out ITerminalSession terminal))
            {
                _terminals.Remove(terminal.Id);
                terminal.Close();
            }
        }

        public string GetDefaultEnvironmentVariableString(Dictionary<string, string> additionalVariables)
        {
            var environmentVariables = Environment.GetEnvironmentVariables();
            environmentVariables["TERM"] = "xterm-256color";
            environmentVariables["TERM_PROGRAM"] = "FluentTerminal";
            environmentVariables["TERM_PROGRAM_VERSION"] = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";

            if (additionalVariables != null)
                foreach (var kvp in additionalVariables)
                    environmentVariables[kvp.Key] = kvp.Value;

            var builder = new StringBuilder();

            foreach (DictionaryEntry item in environmentVariables)
            {
                builder.Append(item.Key).Append("=").Append(item.Value).Append("\0");
            }
            builder.Append('\0');

            return builder.ToString();
        }

        private void OnTerminalConnectionClosed(object sender, int exitcode)
        {
            if (sender is ITerminalSession terminal)
            {
                _terminals.Remove(terminal.Id);
                terminal.Dispose();
                TerminalExited?.Invoke(this, new TerminalExitStatus(terminal.Id, exitcode));
            }
        }
    }
}
