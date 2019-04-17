using System;
using System.IO;

namespace FluentTerminal.App.ViewModels.Utilities
{
    internal static class Helpers
    {
        private static readonly Lazy<string> SshExeLocationLazy = new Lazy<string>(() =>
        {
            //
            // See https://stackoverflow.com/a/25919981
            //

            string system32Folder;

            if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
            {
                system32Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"Sysnative");
            }
            else
            {
                system32Folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            }

            return Path.Combine(system32Folder, @"OpenSSH\ssh.exe");
        });

        internal static string SshExeLocation => SshExeLocationLazy.Value;
    }
}