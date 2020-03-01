using FluentTerminal.RuntimeComponent.Enums;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FluentTerminal.RuntimeComponent.Interfaces
{
    public interface IxtermEventListener
    {
        void OnTerminalResized(int columns, int rows);

        void OnTitleChanged(string title);

        void OnKeyboardCommand(string command);

        void OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection, string hoveredUri);

        void OnSelectionChanged(string selection);

        void OnError(string error);

        void OnInput([ReadOnlyArray]byte[] data);

        void OnInitialized();

        event EventHandler<object> OnOutput;
    }
}