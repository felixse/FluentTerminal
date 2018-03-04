using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class ApplicationSettings
    {
        public bool ConfirmClosingTabs { get; set; }
        public bool ConfirmClosingWindows { get; set; }
        public NewTerminalLocation NewTerminalLocation { get; set; }
    }
}
