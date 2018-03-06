using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentTerminal.Models;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
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

                var trayProcessStatus = new TrayProcessStatus
                {
                    Port = Utilities.GetAvailablePort().Value
                };

                string baseAddress = $"http://localhost:{trayProcessStatus.Port}/";

                Task.Run(async () => await WriteStatus(trayProcessStatus)).Wait();

                using (WebApp.Start<Startup>(url: baseAddress))
                {
                    Application.Run(new SystemTrayApplicationContext());
                }
                mutex.Close();
            }
        }

        private static async Task WriteStatus(TrayProcessStatus trayProcessStatus)
        {
            var file = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync($"{nameof(TrayProcessStatus)}.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(trayProcessStatus));
        }
    }
}
