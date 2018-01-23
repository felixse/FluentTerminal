using System;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Owin.Hosting;
using Windows.ApplicationModel;

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

                string baseAddress = "http://localhost:9000/";

                using (WebApp.Start<Startup>(url: baseAddress))
                {
                    Application.Run(new SystemTrayApplicationContext());
                }
                mutex.Close();
            }
        }
    }
}
