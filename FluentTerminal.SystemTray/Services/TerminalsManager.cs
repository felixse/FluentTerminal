using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using FluentTerminal.SystemTray.Services.ConPty;
using FluentTerminal.SystemTray.Services.WinPty;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluentTerminal.SystemTray.Services
{
    public class TerminalsManager
    {
        private readonly Dictionary<int, ITerminalSession> _terminals = new Dictionary<int, ITerminalSession>();
        private readonly ISettingsService _settingsService;

        public event EventHandler<DisplayTerminalOutputRequest> DisplayOutputRequested;

        public event EventHandler<int> TerminalExited;

        public TerminalsManager(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void DisplayTerminalOutput(int terminalId, string output)
        {
            DisplayOutputRequested?.Invoke(this, new DisplayTerminalOutputRequest
            {
                TerminalId = terminalId,
                Output = output
            });
        }

        public CreateTerminalResponse CreateTerminal(CreateTerminalRequest request)
        {
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

        public void WriteText(int id, string text)
        {
            if (_terminals.TryGetValue(id, out ITerminalSession terminal))
            {
                terminal.WriteText(text);
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
                terminal.Dispose();
            }
        }

        private void OnTerminalConnectionClosed(object sender, System.EventArgs e)
        {
            if (sender is ITerminalSession terminal)
            {
                _terminals.Remove(terminal.Id);
                terminal.Dispose();
                TerminalExited?.Invoke(this, terminal.Id);
            }
        }
    }
}