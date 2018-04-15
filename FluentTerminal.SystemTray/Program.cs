using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Owin.Hosting;
using Windows.ApplicationModel;
using Windows.Storage;

namespace FluentTerminal.SystemTray
{
    public static class Program
    {
        private const string MutexName = "FluentTerminalMutex";

        [STAThread]
        public static void Main()
        {
            if (!Mutex.TryOpenExisting(MutexName, out Mutex mutex))
            {
                mutex = new Mutex(false, MutexName);

                var loopbackEnabler = new LoopbackEnabler();
                loopbackEnabler.EnableForApp(Package.Current.DisplayName);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var port = Utilities.GetAvailablePort();
                ApplicationData.Current.LocalSettings.Values["Port"] = port.Value;
                ApplicationData.Current.LocalSettings.Values["SystemTrayReady"] = true;

                string baseAddress = $"http://localhost:{port.Value}/";

                using (WebApp.Start<Startup>(url: baseAddress))
                {
                    Application.Run(new SystemTrayApplicationContext());
                }
                mutex.Close();
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values["SystemTrayReady"] = true;
            }
        }
    }
}
