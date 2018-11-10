using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentTerminal.App.Services
{
    public interface ITerminalView
    {
        event EventHandler<TerminalSize> TerminalSizeChanged;

        event EventHandler<string> TerminalTitleChanged;

        event EventHandler<string> KeyboardCommandReceived;

        Task ChangeTheme(TerminalColors theme);

        Task ChangeOptions(TerminalOptions options);

        Task ChangeKeyBindings(IEnumerable<KeyBinding> keyBindings);

        Task<TerminalSize> CreateTerminal(TerminalOptions options, TerminalColors theme, IEnumerable<KeyBinding> keyBindings);

        Task ConnectToSocket(string url);

        void Close();

        Task FocusTerminal();

        Task<string> GetSelection();

        Task FindNext(string searchText);

        Task FindPrevious(string searchText);

        void FocusSearchTextBox();
    }
}