using FluentTerminal.App.Services;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static FluentTerminal.SystemTray.Services.ConPty.Native.ProcessApi;

namespace FluentTerminal.SystemTray.Services.ConPty.Processes
{
    /// <summary>
    /// Support for starting and configuring processes.
    /// </summary>
    /// <remarks>
    /// Possible to replace with managed code? The key is being able to provide the PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE attribute
    /// </remarks>
    internal static class ProcessFactory
    {
        /// <summary>
        /// Start and configure a process. The return value represents the process and should be disposed.
        /// </summary>
        internal static Process Start(string command, string directory, string environment, IntPtr attributes, IntPtr hPC)
        {
            var startupInfo = ConfigureProcessThread(hPC, attributes);
            var processInfo = RunProcess(ref startupInfo, command, directory, environment);
            return new Process(startupInfo, processInfo);
        }

        private static STARTUPINFOEX ConfigureProcessThread(IntPtr hPC, IntPtr attributes)
        {
            // this method implements the behavior described in https://docs.microsoft.com/en-us/windows/console/creating-a-pseudoconsole-session#preparing-for-creation-of-the-child-process

            var lpSize = IntPtr.Zero;
            var success = InitializeProcThreadAttributeList(
                lpAttributeList: IntPtr.Zero,
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: ref lpSize
            );
            if (success || lpSize == IntPtr.Zero) // we're not expecting `success` here, we just want to get the calculated lpSize
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not calculate the number of bytes for the attribute list.");
            }

            var startupInfo = new STARTUPINFOEX();
            startupInfo.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();
            startupInfo.lpAttributeList = Marshal.AllocHGlobal(lpSize);

            success = InitializeProcThreadAttributeList(
                lpAttributeList: startupInfo.lpAttributeList,
                dwAttributeCount: 1,
                dwFlags: 0,
                lpSize: ref lpSize
            );
            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set up attribute list.");
            }

            success = UpdateProcThreadAttribute(
                lpAttributeList: startupInfo.lpAttributeList,
                dwFlags: 0,
                attribute: attributes,
                lpValue: hPC,
                cbSize: (IntPtr)IntPtr.Size,
                lpPreviousValue: IntPtr.Zero,
                lpReturnSize: IntPtr.Zero
            );
            if (!success)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not set pseudoconsole thread attribute.");
            }

            return startupInfo;
        }

        private static PROCESS_INFORMATION RunProcess(ref STARTUPINFOEX sInfoEx, string commandLine, string directory, string environment)
        {
            var lpEnvironment = Marshal.StringToHGlobalAnsi(environment);
            int securityAttributeSize = Marshal.SizeOf<SECURITY_ATTRIBUTES>();
            var pSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
            var tSec = new SECURITY_ATTRIBUTES { nLength = securityAttributeSize };
            var success = CreateProcess(
                lpApplicationName: null,
                lpCommandLine: commandLine,
                lpProcessAttributes: ref pSec,
                lpThreadAttributes: ref tSec,
                bInheritHandles: false,
                dwCreationFlags: EXTENDED_STARTUPINFO_PRESENT,
                lpEnvironment: lpEnvironment,
                lpCurrentDirectory: directory,
                lpStartupInfo: ref sInfoEx,
                lpProcessInformation: out PROCESS_INFORMATION pInfo
            );
            if (!success)
            {
                var errorCode = Marshal.GetLastWin32Error();
                Logger.Instance.Error("RunProcess: Could not create Process. Win32 Error: {@errorCode}", errorCode);
                Marshal.FreeHGlobal(lpEnvironment);
                throw new Win32Exception(errorCode, "Could not create process.");
            }

            Marshal.FreeHGlobal(lpEnvironment);
            return pInfo;
        }
    }
}
