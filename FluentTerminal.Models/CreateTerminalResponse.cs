namespace FluentTerminal.Models
{
    public class CreateTerminalResponse
    {
        public int Id { get; set; }
        public bool Success { get; set; }
        public string WebSocketUrl { get; set; }
        public string Error { get; set; }
        public string ShellExecutableName { get; set; }
    }
}