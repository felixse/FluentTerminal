using FluentTerminal.RuntimeComponent.Enums;

namespace FluentTerminal.RuntimeComponent.Interfaces
{
    public interface IxtermEventListener
    {
        void OnTerminalResized(int columns, int rows);

        void OnTitleChanged(string title);

        void OnKeyboardCommand(string command);

        void OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection);

        void OnSelectionChanged(string selection);
    }
}