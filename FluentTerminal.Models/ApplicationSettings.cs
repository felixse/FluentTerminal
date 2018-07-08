using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class ApplicationSettings
    {
        public bool ConfirmClosingTabs { get; set; }
        public bool ConfirmClosingWindows { get; set; }
        public bool UnderlineSelectedTab { get; set; }
        public NewTerminalLocation NewTerminalLocation { get; set; }
        public TabsPosition TabsPosition { get; set; }
    }
}
