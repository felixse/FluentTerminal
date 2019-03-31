using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Ansi
{
    internal struct TerminalCode
    {
        public TerminalCodeType Type { get; }
        public int Line { get; }
        public int Column { get; }
        public string Text { get; }
        public CharAttributes CharAttributes { get; }

        public TerminalCode(TerminalCodeType type) : this()
        {
            Type = type;
        }

        public TerminalCode(TerminalCodeType type, string text) : this(type)
        {
            Text = text;
        }

        public TerminalCode(TerminalCodeType type, int line, int column) : this(type)
        {
            Line = line;
            Column = column;
        }

        public TerminalCode(TerminalCodeType type, CharAttributes charAttributes) : this(type)
        {
            CharAttributes = charAttributes;
        }

        public override string ToString()
        {
            if (Type == TerminalCodeType.Text)
            {
                return Text;
            }
            else
            {
                return Type.ToString();
            }
        }
    }
}
