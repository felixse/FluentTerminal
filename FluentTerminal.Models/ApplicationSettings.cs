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
        public bool TabWindowCascadingAppMenu { get; set; }
        public TabsPosition TabsPosition { get; set; }
        public bool CopyOnSelect { get; set; }
        public MouseAction MouseMiddleClickAction { get; set; }
        public MouseAction MouseRightClickAction { get; set; }
        public bool ShowNewOutputIndicator { get; set; }
        public bool EnableTrayIcon { get; set; }
        public bool ShowCustomTitleInTitlebar { get; set; }
        public bool UseMoshByDefault { get; set; }
        public bool AutoFallbackToWindowsUsernameInLinks { get; set; }
        public bool RTrimCopiedLines { get; set; }
        public bool MuteTerminalBeeps { get; set; }
        public bool EnableLogging { get; set; }
        public bool PrintableOutputOnly { get; set; }
        public string LogDirectoryPath { get; set; }
        public bool UseConPty { get; set; }
        public bool ShowTextCopied { get; set; }

        public ApplicationSettings Clone() => new ApplicationSettings
        {
            ConfirmClosingTabs = ConfirmClosingTabs,
            ConfirmClosingWindows = ConfirmClosingWindows,
            UnderlineSelectedTab = UnderlineSelectedTab,
            InactiveTabColorMode = InactiveTabColorMode,
            NewTerminalLocation = NewTerminalLocation,
            TabWindowCascadingAppMenu = TabWindowCascadingAppMenu,
            TabsPosition = TabsPosition,
            CopyOnSelect = CopyOnSelect,
            MouseMiddleClickAction = MouseMiddleClickAction,
            MouseRightClickAction = MouseRightClickAction,
            ShowNewOutputIndicator = ShowNewOutputIndicator,
            EnableTrayIcon = EnableTrayIcon,
            ShowCustomTitleInTitlebar = ShowCustomTitleInTitlebar,
            UseMoshByDefault = UseMoshByDefault,
            AutoFallbackToWindowsUsernameInLinks = AutoFallbackToWindowsUsernameInLinks,
            RTrimCopiedLines = RTrimCopiedLines,
            MuteTerminalBeeps = MuteTerminalBeeps,
            EnableLogging = EnableLogging,
            PrintableOutputOnly = PrintableOutputOnly,
            LogDirectoryPath = LogDirectoryPath,
            UseConPty = UseConPty,
            ShowTextCopied = ShowTextCopied
        };
    }
}