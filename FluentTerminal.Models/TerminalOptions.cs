using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public class TerminalOptions
    {
        public string FontFamily { get; set; }

        public int FontSize { get; set; }

        public CursorStyle CursorStyle { get; set; }

        public bool CursorBlink { get; set; }

        public BellStyle BellStyle { get; set; }

        public ScrollBarStyle ScrollBarStyle { get; set; }
    }
}
