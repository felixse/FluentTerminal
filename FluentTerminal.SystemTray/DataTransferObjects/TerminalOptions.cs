namespace FluentTerminal.SystemTray.DataTransferObjects
{
    public class TerminalOptions
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string PWD { get; set; }
        public string ShellExecutable { get; set; }
    }
}
