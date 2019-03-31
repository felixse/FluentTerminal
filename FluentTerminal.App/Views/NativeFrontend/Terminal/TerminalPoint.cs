using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Terminal
{
    [DebuggerDisplay("{Column}, {Row}")]
    public struct TerminalPoint : IEquatable<TerminalPoint>, IComparable<TerminalPoint>
    {
        public int Column { get; set; }
        public int Row { get; set; }

        public TerminalPoint(int col, int row)
        {
            Column = col;
            Row = row;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(TerminalPoint other)
        {
            return Column == other.Column &&
                   Row == other.Row;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(TerminalPoint other)
        {
            if (Row < other.Row) return -1;
            if (Row > other.Row) return 1;
            if (Column < other.Column) return -1;
            if (Column > other.Column) return 1;
            return 0;
        }

        public static bool operator ==(TerminalPoint a, TerminalPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TerminalPoint a, TerminalPoint b)
        {
            return !a.Equals(b);
        }

        public static bool operator >(TerminalPoint a, TerminalPoint b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator <(TerminalPoint a, TerminalPoint b)
        {
            return a.CompareTo(b) == -1;
        }

        public static bool operator >=(TerminalPoint a, TerminalPoint b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator <=(TerminalPoint a, TerminalPoint b)
        {
            return a.CompareTo(b) <= 0;
        }
    }
}
