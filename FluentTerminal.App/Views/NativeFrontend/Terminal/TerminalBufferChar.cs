using FluentTerminal.App.Views.NativeFrontend.Ansi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Terminal
{
    public struct TerminalBufferChar
    {
        public char Char { get; }
        public CharAttributes Attributes { get; }

        public TerminalBufferChar(char c, CharAttributes attr)
        {
            Char = c;
            Attributes = attr;
        }

        public override string ToString()
        {
            return Char.ToString();
        }
    }
}
