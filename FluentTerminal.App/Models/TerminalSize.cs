namespace FluentTerminal.App.Models
{
    public class TerminalSize
    {
        public TerminalSize(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
        }

        public int Columns { get; }
        public int Rows { get; }
    }
}