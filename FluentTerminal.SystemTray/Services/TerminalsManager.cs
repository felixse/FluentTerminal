using FluentTerminal.Models;
using FluentTerminal.Models.Requests;
using FluentTerminal.Models.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluentTerminal.SystemTray.Services
{
    public class TerminalsManager
    {
        private Dictionary<int, TerminalSession> _terminals = new Dictionary<int, TerminalSession>();

        public TerminalsManager()
        {

        }

        public CreateTerminalResponse CreateTerminal(CreateTerminalRequest request)
        {
            TerminalSession terminal;
            try
            {
                terminal = new TerminalSession(request);
            }
            catch (Exception e)
            {
                return new CreateTerminalResponse { Error = e.Message };
            }
            
            terminal.ConnectionClosed += OnTerminalConnectionClosed;
            _terminals.Add(terminal.Id, terminal);

            return new CreateTerminalResponse
            {
                Success = true,
                Id = terminal.Id,
                Port = Utilities.GetAvailablePort().Value,
                PipeName = terminal.PipeName,
                ShellExecutableName = terminal.ShellExecutableName
            };
        }

        public void ResizeTerminal(int id, TerminalSize size)
        {
            if (_terminals.TryGetValue(id, out TerminalSession terminal))
            {
                terminal.Resize(size);
            }
            else
            {
                Debug.WriteLine($"ResizeTerminal: terminal with id '{id}' was not found");
            }
        }

        private void OnTerminalConnectionClosed(object sender, System.EventArgs e)
        {
            if (sender is TerminalSession terminal)
            {
                _terminals.Remove(terminal.Id);
                terminal.Dispose();
            }
        }
    }
}
