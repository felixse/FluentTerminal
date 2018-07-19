namespace FluentTerminal.RuntimeComponent.Interfaces
{
    public interface ITerminalEventListener
    {
        void OnTerminalResized(int columns, int rows);

        void OnTitleChanged(string title);

        void OnKeyboardCommand(string command);

        void OnRightClick(int x, int y, bool hasSelection);
    }
}