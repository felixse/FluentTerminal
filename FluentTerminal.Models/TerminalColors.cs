using System;

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

        public override bool Equals(object obj)
        {
            if (obj is TerminalColors other)
            {
                return other.Foreground.Equals(Foreground)
                    && other.Background.Equals(Background)
                    && other.Cursor.Equals(Cursor)
                    && other.CursorAccent.Equals(CursorAccent)
                    && other.Selection.Equals(Selection)
                    && other.Black.Equals(Black)
                    && other.Red.Equals(Red)
                    && other.Green.Equals(Green)
                    && other.Yellow.Equals(Yellow)
                    && other.Blue.Equals(Blue)
                    && other.Magenta.Equals(Magenta)
                    && other.Cyan.Equals(Cyan)
                    && other.White.Equals(White)
                    && other.BrightBlack.Equals(BrightBlack)
                    && other.BrightRed.Equals(BrightRed)
                    && other.BrightGreen.Equals(BrightGreen)
                    && other.BrightYellow.Equals(BrightYellow)
                    && other.BrightBlue.Equals(BrightBlue)
                    && other.BrightMagenta.Equals(BrightMagenta)
                    && other.BrightCyan.Equals(BrightCyan)
                    && other.BrightWhite.Equals(BrightWhite);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                HashCode.Combine(Foreground, Background, Cursor, CursorAccent, Selection),
                HashCode.Combine(Black, Red, Green, Yellow, Blue, Magenta, Cyan, White),
                HashCode.Combine(BrightBlack, BrightRed, BrightGreen, BrightYellow, BrightBlue, BrightMagenta, BrightCyan, BrightWhite)
            );
        }
    }
}