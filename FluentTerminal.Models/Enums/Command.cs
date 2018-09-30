using System.ComponentModel;

namespace FluentTerminal.Models.Enums
{
    public enum Command
    {
        [Description("Toggle window")]
        ToggleWindow,

        [Description("Next tab")]
        NextTab,

        [Description("Previous tab")]
        PreviousTab,

        [Description("New tab")]
        NewTab,

        [Description("Configurable new tab")]
        ConfigurableNewTab,

        [Description("Close tab")]
        CloseTab,

        [Description("New window")]
        NewWindow,

        [Description("Show settings")]
        ShowSettings,

        [Description("Copy")]
        Copy,

        [Description("Paste")]
        Paste,

        [Description("Search")]
        Search,

        [Description("Toggle Fullscreen")]
        ToggleFullScreen,

        [Description("Select all")]
        SelectAll,

        [Description("Clear")]
        Clear,

        [Description("Switch to Terminal 1")]
        SwitchToTerm1,

        [Description("Switch to Terminal 2")]
        SwitchToTerm2,

        [Description("Switch to Terminal 3")]
        SwitchToTerm3,

        [Description("Switch to Terminal 4")]
        SwitchToTerm4,

        [Description("Switch to Terminal 5")]
        SwitchToTerm5,

        [Description("Switch to Terminal 6")]
        SwitchToTerm6,

        [Description("Switch to Terminal 7")]
        SwitchToTerm7,

        [Description("Switch to Terminal 8")]
        SwitchToTerm8,

        [Description("Switch to Terminal 9")]
        SwitchToTerm9
    }
}