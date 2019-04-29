﻿using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using FluentTerminal.SystemTray.Services.ConPty.Processes;
using static FluentTerminal.SystemTray.Native.WindowApi;
using static FluentTerminal.SystemTray.Services.ConPty.Native.ConsoleApi;

namespace FluentTerminal.SystemTray.Services.ConPty
{
    /// <summary>
    /// Class for managing communication with the underlying console, and communicating with its pseudoconsole.
    /// </summary>
    public sealed class Terminal : IDisposable
    {
        private SafeFileHandle _consoleInputPipeWriteHandle;
        private FileStream _consoleInputWriter;
        private PseudoConsolePipe _inputPipe;
        private PseudoConsolePipe _outputPipe;
        private PseudoConsole _pseudoConsole;

        /// <summary>
        /// A stream of VT-100-enabled output from the console.
        /// </summary>
        public FileStream ConsoleOutStream { get; private set; }

        /// <summary>
        /// Fired once the console has been hooked up and is ready to receive input.
        /// </summary>
        public event EventHandler OutputReady;
        public event EventHandler Exited;

        /// <summary>
        /// The exit code of the terminal's process. -1 if the process hasn't exited yet.
        /// </summary>
        public int ExitCode { get; private set; } = -1;

        public Terminal()
        {
            // By default, UI applications don't have a console associated with them.
            // So first, we check to see if this process has a console.
            if (GetConsoleWindow() == IntPtr.Zero)
            {
                // If it doesn't ask Windows to allocate one to it for us.
                bool createConsoleSuccess = AllocConsole();
                if (!createConsoleSuccess)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not allocate console for this process.");
                }
            }

            var windowHandle = GetConsoleWindow();
            ShowWindow(windowHandle, SW_HIDE);

            // And enable VT processing for our process's console.
            EnableVirtualTerminalSequenceProcessing();
        }

        private void EnableVirtualTerminalSequenceProcessing()
        {
            SafeFileHandle screenBuffer = GetConsoleScreenBuffer();
            if (!GetConsoleMode(screenBuffer, out uint outConsoleMode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not get console mode.");
            }
            outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;

            if (!SetConsoleMode(screenBuffer, outConsoleMode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not enable virtual terminal processing.");
            }
        }

        /// <summary>
        /// Start the psuedoconsole and run the process as shown in 
        /// https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#creating-the-pseudoconsole
        /// </summary>
        /// <param name="command">the command to run, e.g. cmd.exe</param>
        /// <param name="consoleHeight">The height (in characters) to start the pseudoconsole with. Defaults to 80.</param>
        /// <param name="consoleWidth">The width (in characters) to start the pseudoconsole with. Defaults to 30.</param>
        public void Start(string command, string directory, string environment, int consoleWidth = 80, int consoleHeight = 30)
        {
            _inputPipe = new PseudoConsolePipe();
            _outputPipe = new PseudoConsolePipe();
            _pseudoConsole = PseudoConsole.Create(_inputPipe.ReadSide, _outputPipe.WriteSide, consoleWidth, consoleHeight);

            using (var process = ProcessFactory.Start(command, directory, environment, PseudoConsole.PseudoConsoleThreadAttribute, _pseudoConsole.Handle))
            {
                // copy all pseudoconsole output to a FileStream and expose it to the rest of the app
                ConsoleOutStream = new FileStream(_outputPipe.ReadSide, FileAccess.Read);
                OutputReady.Invoke(this, EventArgs.Empty);

                // Store input pipe handle, and a writer for later reuse
                _consoleInputPipeWriteHandle = _inputPipe.WriteSide;
                _consoleInputWriter = new FileStream(_consoleInputPipeWriteHandle, FileAccess.Write);

                // free resources in case the console is ungracefully closed (e.g. by the 'x' in the window titlebar)
                OnClose(() => DisposeResources(process, _pseudoConsole, _outputPipe, _inputPipe, _consoleInputWriter));

                WaitForExit(process).WaitOne(Timeout.Infinite);
                this.ExitCode = (int)process.GetExitCode();
            }
            Exited?.Invoke(this, EventArgs.Empty);
        }

        public  void Resize(int width, int height)
        {
            _pseudoConsole?.Resize(width, height);
        }

        /// <summary>
        /// Sends the given string to the anonymous pipe that writes to the active pseudoconsole.
        /// </summary>
        public void WriteToPseudoConsole(byte[] data)
        {
            _consoleInputWriter.Write(data, 0, data.Length);
            _consoleInputWriter.Flush();
        }

        /// <summary>
        /// Get an AutoResetEvent that signals when the process exits
        /// </summary>
        private static AutoResetEvent WaitForExit(Process process) =>
            new AutoResetEvent(false)
            {
                SafeWaitHandle = new SafeWaitHandle(process.ProcessInfo.hProcess, ownsHandle: false)
            };

        /// <summary>
        /// Set a callback for when the terminal is closed (e.g. via the "X" window decoration button).
        /// Intended for resource cleanup logic.
        /// </summary>
        private static void OnClose(Action handler)
        {
            SetConsoleCtrlHandler(eventType =>
            {
                if (eventType == CtrlTypes.CTRL_CLOSE_EVENT)
                {
                    handler();
                }
                return false;
            }, true);
        }

        private void DisposeResources(params IDisposable[] disposables)
        {
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        /// <summary>
        /// A helper method that opens a handle on the console's screen buffer, which will allow us to get its output,
        /// even if STDOUT has been redirected (which Visual Studio does by default).
        /// </summary>
        /// <returns>A file handle to the console's screen buffer.</returns>
        /// <remarks>This is described in more detail here: https://docs.microsoft.com/en-us/windows/console/console-handles </remarks>
        private SafeFileHandle GetConsoleScreenBuffer()
        {
            IntPtr file = CreateFileW(
                ConsoleOutPseudoFilename,
                GENERIC_WRITE | GENERIC_READ,
                FILE_SHARE_WRITE,
                IntPtr.Zero,
                OPEN_EXISTING,
                FILE_ATTRIBUTE_NORMAL,
                IntPtr.Zero);

            if (file == new IntPtr(-1))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not get console screen buffer.");
            }

            return new SafeFileHandle(file, true);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _inputPipe?.Dispose();
                    _outputPipe?.Dispose();
                    _pseudoConsole?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
