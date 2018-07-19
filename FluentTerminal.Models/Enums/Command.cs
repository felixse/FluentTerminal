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
        Clear
    }
}