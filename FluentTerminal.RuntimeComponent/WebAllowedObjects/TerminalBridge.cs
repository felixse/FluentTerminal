using FluentTerminal.RuntimeComponent.Enums;
using FluentTerminal.RuntimeComponent.Interfaces;
using System;
using System.Text;
using System.Threading.Tasks;
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
            _terminalEventListener.OnOutput += OnOutput;
            _terminalEventListener.OnPaste += OnPaste;
        }

        private void OnPaste(object sender, string e)
        {
            Task.Factory.StartNew(() => Paste?.Invoke(this, e));
        }

        private void OnOutput(object sender, object e)
        {
            Task.Factory.StartNew(() => Output?.Invoke(this, e));
        }

        public event EventHandler<object> Output;
        public event EventHandler<string> Paste;

        public void InputReceived(string message)
        {
            _terminalEventListener?.OnInput(Encoding.UTF8.GetBytes(message));
        }

        public void BinaryReceived(string binary)
        {
            _terminalEventListener?.OnInput(Encoding.UTF8.GetBytes(binary));
        }

        public void Initialized()
        {
            _terminalEventListener.OnInitialized();
        }

        public void DisposalPrepare()
        {
            _terminalEventListener.OnOutput -= OnOutput;
            _terminalEventListener.OnPaste -= OnPaste;
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

        public void NotifyRightClick(int x, int y, bool hasSelection, string hoveredUri)
        {
            _terminalEventListener?.OnMouseClick(MouseButton.Right, x, y, hasSelection, hoveredUri);
        }

        public void NotifyMiddleClick(int x, int y, bool hasSelection, string hoveredUri)
        {
            _terminalEventListener?.OnMouseClick(MouseButton.Middle, x, y, hasSelection, hoveredUri);
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