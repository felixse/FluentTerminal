using FluentTerminal.App.Views.NativeFrontend.Ansi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Terminal
{
    public struct TerminalTag : IEquatable<TerminalTag>
    {
        public string Text { get; }
        public CharAttributes Attributes { get; }

        public TerminalTag(string text, CharAttributes attributes)
        {
            Text = text;
            Attributes = attributes;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals((TerminalTag)obj);
        }

        public bool Equals(TerminalTag other)
        {
            return Text == other.Text &&
                   Attributes == other.Attributes;
        }

        public TerminalTag Substring(int index)
        {
            return new TerminalTag(Text.Substring(index), Attributes);
        }

        public TerminalTag Substring(int index, int length)
        {
            return new TerminalTag(Text.Substring(index, length), Attributes);
        }

        public override string ToString()
        {
            return Text;
        }

        public static bool operator ==(TerminalTag a, TerminalTag b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TerminalTag a, TerminalTag b)
        {
            return !a.Equals(b);
        }
    }
}
