using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.RuntimeComponent.Interfaces
{
    public interface ITerminalEventListener
    {
        void OnTerminalResized(int columns, int rows);

        void OnTitleChanged(string title);
    }
}
