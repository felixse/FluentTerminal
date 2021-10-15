using System;
using System.ComponentModel;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static FluentTerminal.SystemTray.Services.ConPty.Native.ProcessApi;
using static FluentTerminal.SystemTray.Services.WinPty.ProcessApi;

namespace FluentTerminal.SystemTray.Services.ConPty.Processes
{
    public static class ProcessUtils
    {
        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// </summary>
        /// <param name="pid">Process ID.</param>
        public static void KillTree(int pid)
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid);
            foreach (ManagementObject mo in searcher.Get())
            {
                KillTree(Convert.ToInt32(mo["ProcessID"]));
            }

            try
            {
                 System.Diagnostics.Process.GetProcessById(pid).Kill();
            }
            catch (ArgumentException)
            {
                // Process already exited.
            }
            catch (Win32Exception)
            {
                // Ignore access is denied
            }
        }
    }

    /// <summary>
    /// Represents an instance of a process.
    /// </summary>
    internal sealed class Process : IDisposable
    {
        public Process(STARTUPINFOEX startupInfo, PROCESS_INFORMATION processInfo)
        {
            StartupInfo = startupInfo;
            ProcessInfo = processInfo;
        }

        ~Process()
        {
            Dispose(false);
        }

        public STARTUPINFOEX StartupInfo { get; }
        public PROCESS_INFORMATION ProcessInfo { get; }

        /// <summary>
        /// Returns the exit code for the process.
        /// </summary>
        public uint GetExitCode() {
            if (!GetExitCodeProcess(ProcessInfo.hProcess, out uint exitCode))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "could not retrieve process exit code");
            }
            return exitCode;
        }

        // <summary>
        // Kills the process and its children, and grandchildren etc.
        // </summary>
        public void KillTree() {
            if (ProcessInfo.dwProcessId > 0)
            {
                ProcessUtils.KillTree(ProcessInfo.dwProcessId);
            }
        }

        #region IDisposable Support

        public void Dispose()
        {
            Dispose(true);
        }

        private bool alreadyDisposed = false; // To detect redundant calls

        public void Dispose(bool disposeManaged)
        {
            if (alreadyDisposed)
            {
                return;
            }
            
            // Clean up resources (all unmanaged in this case)

            // Kill the process and any children
            KillTree();

            // Free the attribute list
            if (StartupInfo.lpAttributeList != IntPtr.Zero)
            {
                DeleteProcThreadAttributeList(StartupInfo.lpAttributeList);
                Marshal.FreeHGlobal(StartupInfo.lpAttributeList);
            }

            // Close process and thread handles
            if (ProcessInfo.hProcess != IntPtr.Zero)
            {
                CloseHandle(ProcessInfo.hProcess);
            }
            if (ProcessInfo.hThread != IntPtr.Zero)
            {
                CloseHandle(ProcessInfo.hThread);
            }

            alreadyDisposed = true;
        }

        #endregion
    }
}
