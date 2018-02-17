using FluentTerminal.Models;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.Views
{
    public interface ITerminalView
    {
        event EventHandler<TerminalSize> TerminalSizeChanged;
        event EventHandler<string> TerminalTitleChanged;

        Task ChangeTheme(TerminalColors theme);
        Task ChangeOptions(TerminalOptions options);
        Task<TerminalSize> CreateTerminal(TerminalOptions options, TerminalColors theme);
        Task ConnectToSocket(string url);
        void Close();
        Task FocusTerminal();
    }
}