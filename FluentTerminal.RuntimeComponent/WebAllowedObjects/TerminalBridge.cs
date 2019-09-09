using FluentTerminal.RuntimeComponent.Enums;
using FluentTerminal.RuntimeComponent.Interfaces;
using Windows.Foundation.Metadata;

namespace FluentTerminal.RuntimeComponent.WebAllowedObjects
{
    [AllowForWeb]
    public sealed class TerminalBridge
    {
        private IxtermEventListener _terminalEventListener;

        public TerminalBridge(IxtermEventListener terminalEventListener)
        {
            _terminalEventListener = terminalEventListener;
        }

        public void DisposalPrepare()
        {
            _terminalEventListener = null;
        }

        public void NotifySizeChanged(int columns, int rows)
        {
            _terminalEventListener?.OnTerminalResized(columns, rows);
        }

        public void NotifyTitleChanged(string title)
        {
            _terminalEventListener?.OnTitleChanged(title);
        }

        public void InvokeCommand(string command)
        {
            _terminalEventListener?.OnKeyboardCommand(command);
        }

        public void NotifyRightClick(int x, int y, bool hasSelection)
        {
            _terminalEventListener?.OnMouseClick(MouseButton.Right, x, y, hasSelection);
        }

        public void NotifyMiddleClick(int x, int y, bool hasSelection)
        {
            _terminalEventListener?.OnMouseClick(MouseButton.Middle, x, y, hasSelection);
        }

        public void NotifySelectionChanged(string selection)
        {
            _terminalEventListener?.OnSelectionChanged(selection);
        }

        public void ReportError(string error)
        {
            _terminalEventListener?.OnError(error);
        }
    }
}