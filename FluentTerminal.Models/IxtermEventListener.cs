using FluentTerminal.Model.Enums;
using System;

namespace FluentTerminal.Model
{
    public interface IxtermEventListener
    {
        void OnTerminalResized(int columns, int rows);

        void OnTitleChanged(string title);

        void OnKeyboardCommand(string command);

        void OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection, string hoveredUri);

        void OnSelectionChanged(string selection);

        void OnError(string error);

        void OnInput(byte[] data);

        void OnInitialized();

        event EventHandler<object> OnOutput;
        event EventHandler<string> OnPaste;
        event EventHandler<string> OnSessionRestart;
    }
}