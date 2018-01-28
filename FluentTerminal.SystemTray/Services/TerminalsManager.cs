using FluentTerminal.SystemTray.DataTransferObjects;
using System.Collections.Generic;
using System.Diagnostics;

namespace FluentTerminal.SystemTray.Services
{
    public class TerminalsManager
    {
        private Dictionary<int, Terminal> _terminals = new Dictionary<int, Terminal>();

        public TerminalsManager()
        {

        }

        public string CreateTerminal(TerminalOptions options)
        {
            var terminal = new Terminal(options);
            terminal.ConnectionClosed += OnTerminalConnectionClosed;
            _terminals.Add(terminal.Id, terminal);
            return terminal.WebSocketUrl;
        }

        public void ResizeTerminal(int id, int cols, int rows)
        {
            if (_terminals.TryGetValue(id, out Terminal terminal))
            {
                terminal.Resize(cols, rows);
            }
            else
            {
                Debug.WriteLine($"ResizeTerminal: terminal with id '{id}' was not found");
            }
        }

        private void OnTerminalConnectionClosed(object sender, System.EventArgs e)
        {
            if (sender is Terminal terminal)
            {
                _terminals.Remove(terminal.Id);
                terminal.Dispose();
            }
        }
    }
}
