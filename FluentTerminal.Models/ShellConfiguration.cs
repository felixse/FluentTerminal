using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class ShellConfiguration
    {
        public string Arguments { get; set; }
        public ShellType Shell { get; set; }
        public string CustomShellLocation { get; set; }
        public string WorkingDirectory { get; set; }
    }
}
