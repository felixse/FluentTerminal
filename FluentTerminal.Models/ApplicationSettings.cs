using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class ApplicationSettings
    {
        public bool ConfirmClosingTabs { get; set; }
        public bool ConfirmClosingWindows { get; set; }
        public bool UnderlineSelectedTab { get; set; }
        public bool UnderlineInactiveTabs { get; set; }
        public NewTerminalLocation NewTerminalLocation { get; set; }
        public TabsPosition TabsPosition { get; set; }
        public bool CopyOnSelect { get; set; }
        public MouseAction MouseMiddleClickAction { get; set; }
        public MouseAction MouseRightClickAction { get; set; }
        public bool AlwaysShowTabs { get; set; }
    }
}