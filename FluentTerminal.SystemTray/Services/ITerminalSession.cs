using System;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;

namespace FluentTerminal.SystemTray.Services
{
    public interface ITerminalSession : IDisposable
    {
        byte Id { get; }
        string ShellExecutableName { get; }

        event EventHandler<int> ConnectionClosed;

        void Close();
        void Resize(TerminalSize size);
        void Write(byte[] data);
        void Start(CreateTerminalRequest request, TerminalsManager terminalsManager);
        void Pause(bool value);
    }
}