using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class ApplicationSettings
    {
        public bool ConfirmClosingTabs { get; set; }
        public bool ConfirmClosingWindows { get; set; }
        public bool UnderlineSelectedTab { get; set; }
        public InactiveTabColorMode InactiveTabColorMode { get; set; }
        public NewTerminalLocation NewTerminalLocation { get; set; }
        public TabsPosition TabsPosition { get; set; }
        public bool CopyOnSelect { get; set; }
        public MouseAction MouseMiddleClickAction { get; set; }
        public MouseAction MouseRightClickAction { get; set; }
        public bool ShowNewOutputIndicator { get; set; }
        public bool EnableTrayIcon { get; set; }
        public bool ShowCustomTitleInTitlebar { get; set; }
        public bool UseMoshByDefault { get; set; }
        public bool AutoFallbackToWindowsUsernameInLinks { get; set; }
        public bool UseQuickSshConnectByDefault { get; set; }
        public bool RTrimCopiedLines { get; set; }
        public bool MuteTerminalBeeps { get; set; }
        public bool EnableLogging { get; set; }
        public bool PrintableOutputOnly { get; set; }
        public string LogDirectoryPath { get; set; }
    }
}