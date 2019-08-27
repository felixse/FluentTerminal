using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;

using FluentTerminal.App.Services;

namespace FluentTerminal.SystemTray
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
                Process.GetProcessById(pid).Kill();
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
}
