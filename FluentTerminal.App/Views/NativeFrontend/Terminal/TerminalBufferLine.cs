using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Terminal
{
    /// <summary>
    /// Represents a single immutable line from a terminal buffer.
    /// </summary>
    internal class TerminalBufferLine
    {
        public int Columns { get; }
        public ImmutableArray<TerminalBufferChar> Buffer { get; }

        public TerminalBufferLine(TerminalBufferChar[] buffer, int index, int length)
        {
            Buffer = buffer.Skip(index).Take(length).ToImmutableArray();
            Columns = length;
        }

        public override string ToString()
        {
            return new string(Buffer.Select(x => x.Char).ToArray());
        }
    }
}
