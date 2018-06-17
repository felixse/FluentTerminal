using System.ComponentModel;

namespace FluentTerminal.Models.Enums
{
    public enum Command
    {
        // Explicitly assign the values of the enums, to ensure that we don't end up with them in weird places
        // in the integer spectrum. This allows us to reserver 1024 and up for shell profile shortcuts safely.

        [Description("Toggle window")]
        ToggleWindow = 0,

        [Description("Next tab")]
        NextTab = 1,

        [Description("Previous tab")]
        PreviousTab = 2,

        [Description("New tab")]
        NewTab = 3,

        [Description("Configurable new tab")]
        ConfigurableNewTab = 4,

        [Description("Close tab")]
        CloseTab = 5,

        [Description("New window")]
        NewWindow = 6,

        [Description("Show settings")]
        ShowSettings = 7,

        [Description("Copy")]
        Copy = 8,

        [Description("Paste")]
        Paste = 9,

        [Description("Search")]
        Search = 10,

        [Description("Toggle Fullscreen")]
        ToggleFullScreen = 11,

        [Description("Select all")]
        SelectAll = 12,

        [Description("Clear")]
        Clear = 13,

        [Description("ShellProfileShortcuts")]
        ShellProfileShortcut = 1024
    }
}
