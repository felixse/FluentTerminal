using FluentTerminal.RuntimeComponent.Enums;
using FluentTerminal.RuntimeComponent.Interfaces;
using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Core;

namespace FluentTerminal.RuntimeComponent.WebAllowedObjects
{
    internal static class EventDispatcher
    {
        public static async void Dispatch(Action action)
        {
            if (CoreWindow.GetForCurrentThread()?.Dispatcher is { } dispatcher)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
            }
            else
            {
                await Task.Factory.StartNew(action);
            }
        }
    }

    [AllowForWeb]
    public sealed class TerminalBridge
    {
        private IxtermEventListener _terminalEventListener;

        public TerminalBridge(IxtermEventListener terminalEventListener)
        {
            _terminalEventListener = terminalEventListener;
            _terminalEventListener.OnOutput += _terminalEventListener_OnOutput;
        }

        private void _terminalEventListener_OnOutput(object sender, object e)
        {
            EventDispatcher.Dispatch(() => Output?.Invoke(this, e));
        }

        public event EventHandler<object> Output;

        public void InputReceived(string message)
        {
            _terminalEventListener?.OnInput(message);
        }

        public void Initialized()
        {
            _terminalEventListener.OnInitialized();
        }

        public void DisposalPrepare()
        {
            _terminalEventListener.OnOutput -= _terminalEventListener_OnOutput;
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