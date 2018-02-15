using System;

namespace FluentTerminal.Models
{
    public class TerminalTheme
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public bool PreInstalled { get; set; }
        public double BackgroundOpacity { get; set; }
        public TerminalColors Colors { get; set; }
    }
}
