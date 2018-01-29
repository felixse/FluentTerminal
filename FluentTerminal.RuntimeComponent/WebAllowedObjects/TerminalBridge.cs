using FluentTerminal.RuntimeComponent.Interfaces;
using Windows.Foundation.Metadata;

namespace FluentTerminal.RuntimeComponent.WebAllowedObjects
{
    [AllowForWeb]
    public sealed class TerminalBridge
    {
        private readonly ITerminalEventListener _terminalEventListener;

        public TerminalBridge(ITerminalEventListener terminalEventListener)
        {
            _terminalEventListener = terminalEventListener;
        }

        public void NotifySizeChanged(int columns, int rows)
        {
            _terminalEventListener.OnTerminalResized(columns, rows);
        }

        public void NotifyTitleChanged(string title)
        {
            _terminalEventListener.OnTitleChanged(title);
        }
    }
}
