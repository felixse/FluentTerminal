using System;
using System.Runtime.InteropServices;

namespace FluentTerminal.SystemTray.Native
{
    public static class ProcessApi
    {
        [DllImport("kernel32.dll")]
        public static extern int GetProcessId(IntPtr handle);
    }
}
