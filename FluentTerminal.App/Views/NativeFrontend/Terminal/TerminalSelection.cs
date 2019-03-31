using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views.NativeFrontend.Terminal
{
    internal class TerminalSelection
    {
        public SelectionMode Mode { get; }
        public TerminalPoint Start { get; }
        public TerminalPoint End { get; }
        public bool IsReversed { get; }

        public TerminalSelection(SelectionMode mode, TerminalPoint start, TerminalPoint end)
        {
            Mode = mode;
            Start = start;
            End = end;
            IsReversed = start > end;
        }

        public (TerminalPoint, TerminalPoint) GetMinMax()
        {
            return IsReversed ? (End, Start) : (Start, End);
        }
    }
}
