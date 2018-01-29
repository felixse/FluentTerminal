using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Models
{
    public class TerminalConfiguration
    {
        public bool CursorBlinking { get; set; }
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
    }
}
