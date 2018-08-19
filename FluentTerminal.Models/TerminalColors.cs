namespace FluentTerminal.Models
{
    public class TerminalColors
    {
        public string Foreground { get; set; }
        public string Background { get; set; }
        public string Cursor { get; set; }
        public string CursorAccent { get; set; }
        public string Selection { get; set; }

        public string Black { get; set; }
        public string Red { get; set; }
        public string Green { get; set; }
        public string Yellow { get; set; }
        public string Blue { get; set; }
        public string Magenta { get; set; }
        public string Cyan { get; set; }
        public string White { get; set; }

        public string BrightBlack { get; set; }
        public string BrightRed { get; set; }
        public string BrightGreen { get; set; }
        public string BrightYellow { get; set; }
        public string BrightBlue { get; set; }
        public string BrightMagenta { get; set; }
        public string BrightCyan { get; set; }
        public string BrightWhite { get; set; }

        public string TabActiveBackground { get; set; }
        public string TabActiveUnderline { get; set; }
        public string TabActiveForeground { get; set; }
        public string TabInactiveBackground { get; set; }
        public string TabInactiveUnderline { get; set; }
        public string TabInactiveForeground { get; set; }
        public string TabActiveHoverBackground { get; set; }
        public string TabActiveClickBackground { get; set; }
        public string TabInactiveHoverBackground { get; set; }
        public string TabInactiveHoverForeground { get; set; }
        public string TabInactiveClickBackground { get; set; }

        public TerminalColors()
        {
        }

        public TerminalColors(TerminalColors other)
        {
            Foreground = other.Foreground;
            Background = other.Background;
            Cursor = other.Cursor;
            CursorAccent = other.CursorAccent;
            Selection = other.Selection;

            TabActiveBackground = other.TabActiveBackground;
            TabActiveUnderline = other.TabActiveUnderline;
            TabActiveForeground = other.TabActiveForeground;
            TabInactiveBackground = other.TabInactiveBackground;
            TabInactiveUnderline = other.TabInactiveUnderline;
            TabInactiveForeground = other.TabInactiveForeground;
            TabActiveHoverBackground = other.TabActiveHoverBackground;
            TabActiveClickBackground = other.TabActiveClickBackground;
            TabInactiveHoverBackground = other.TabInactiveHoverBackground;
            TabInactiveHoverForeground = other.TabInactiveHoverForeground;
            TabInactiveClickBackground = other.TabInactiveClickBackground;

            Black = other.Black;
            Red = other.Red;
            Green = other.Green;
            Yellow = other.Yellow;
            Blue = other.Blue;
            Magenta = other.Magenta;
            Cyan = other.Cyan;
            White = other.White;

            BrightBlack = other.BrightBlack;
            BrightRed = other.BrightRed;
            BrightGreen = other.BrightGreen;
            BrightYellow = other.BrightYellow;
            BrightBlue = other.BrightBlue;
            BrightMagenta = other.BrightMagenta;
            BrightCyan = other.BrightCyan;
            BrightWhite = other.BrightWhite;
        }
    }
}