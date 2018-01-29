using FluentTerminal.App.Models;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views
{
    public interface ITerminalView
    {
        event EventHandler<TerminalSize> TerminalSizeChanged;
        event EventHandler<string> TerminalTitleChanged;

        Task<TerminalSize> CreateTerminal(TerminalConfiguration configuration);
        Task ConnectToSocket(string url);
    }
}