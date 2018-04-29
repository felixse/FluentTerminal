using Autofac;
using FluentTerminal.SystemTray.Services;
using GlobalHotKey;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
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

                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterType<NotificationService>().SingleInstance();
                containerBuilder.RegisterType<TerminalsManager>().SingleInstance();
                containerBuilder.RegisterType<ToggleWindowService>().SingleInstance();
                containerBuilder.RegisterType<HotKeyManager>().SingleInstance();
                containerBuilder.RegisterType<SystemTrayApplicationContext>().SingleInstance();
                containerBuilder.RegisterType<AppCommunicationService>().SingleInstance();
                containerBuilder.RegisterInstance(Dispatcher.CurrentDispatcher).SingleInstance();

                var container = containerBuilder.Build();

                var appCommunicationService = container.Resolve<AppCommunicationService>();
                appCommunicationService.StartAppServiceConnection();

                Application.Run(container.Resolve<SystemTrayApplicationContext>());

                mutex.Close();
            }
            else
            {
                var eventWaitHandle = EventWaitHandle.OpenExisting(AppCommunicationService.EventWaitHandleName, System.Security.AccessControl.EventWaitHandleRights.Modify);
                eventWaitHandle.Set();
            }
        }
    }
}
