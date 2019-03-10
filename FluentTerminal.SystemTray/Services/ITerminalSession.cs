using System;
using FluentTerminal.Models;
using FluentTerminal.Models.Requests;

namespace FluentTerminal.SystemTray.Services
{
    public interface ITerminalSession : IDisposable
    {
        int Id { get; }
        string ShellExecutableName { get; }

        event EventHandler ConnectionClosed;

        void Close();
        void Resize(TerminalSize size);
        void Write(byte[] data);
        void Start(CreateTerminalRequest request, TerminalsManager terminalsManager);
    }
}