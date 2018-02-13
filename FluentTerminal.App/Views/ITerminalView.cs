using FluentTerminal.Models;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views
{
    public interface ITerminalView
    {
        event EventHandler<TerminalSize> TerminalSizeChanged;
        event EventHandler<string> TerminalTitleChanged;

        Task<TerminalSize> CreateTerminal(TerminalColors theme);
        Task ConnectToSocket(string url);
    }
}