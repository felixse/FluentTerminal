namespace FluentTerminal.App.Views.NativeFrontend.Ansi
{
    public enum TerminalCodeType
    {
        Text,
        Bell,
        LineFeed,
        CarriageReturn,
        Backspace,
        Tab,
        ShiftOut,
        ShiftIn,
        SaveCursor,
        Reset,
        SetTitle,
        RestoreCursor,
        TabSet,

        CursorPosition,
        CursorCharAbsolute,
        CursorUp,
        CursorDown,
        CursorForward,
        CursorBackward,
        RestoreCursorPosition,
        EraseInDisplay,
        EraseInLine,
        SetGraphicsMode,
        SetMode,
        CharAttributes,
        ResetMode,
    }
}
