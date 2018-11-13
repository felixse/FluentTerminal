using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using static FluentTerminal.SystemTray.Services.ConPty.Native.PseudoConsoleApi;

namespace FluentTerminal.SystemTray.Services.ConPty
{
    /// <summary>
    /// Utility functions around the new Pseudo Console APIs.
    /// </summary>
    internal sealed class PseudoConsole : IDisposable
    {
        public static readonly IntPtr PseudoConsoleThreadAttribute = (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE;

        public IntPtr Handle { get; }

        private PseudoConsole(IntPtr handle)
        {
            Handle = handle;
        }

        internal static PseudoConsole Create(SafeFileHandle inputReadSide, SafeFileHandle outputWriteSide, int width, int height)
        {
            var createResult = CreatePseudoConsole(
                new COORD { X = (short)width, Y = (short)height },
                inputReadSide, outputWriteSide,
                0, out IntPtr hPC);
            if (createResult != 0)
            {
                throw new Win32Exception(createResult, "Could not create psuedo console.");
            }
            return new PseudoConsole(hPC);
        }

        internal void Resize(int width, int height)
        {
            ResizePseudoConsole(Handle, new COORD { X = (short)width, Y = (short)height });
        }

        public void Dispose()
        {
            ClosePseudoConsole(Handle);
        }
    }
}
