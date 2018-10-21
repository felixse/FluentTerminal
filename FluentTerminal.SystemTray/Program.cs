using Autofac;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Adapters;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.SystemTray.Services;
using GlobalHotKey;
using System;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using Windows.Storage;

namespace FluentTerminal.SystemTray
{
    public static class Program
    {
        private const string MutexName = "FluentTerminalMutex";

        [STAThread]
        public static void Main(string[] args)
        {
            if (!Mutex.TryOpenExisting(MutexName, out Mutex mutex))
            {
                mutex = new Mutex(false, MutexName);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var applicationDataContainers = new ApplicationDataContainers
                {
                    LocalSettings = new ApplicationDataContainerAdapter(ApplicationData.Current.LocalSettings),
                    RoamingSettings = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings),
                    KeyBindings = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.KeyBindingsContainerName, ApplicationDataCreateDisposition.Always)),
                    ShellProfiles = new ApplicationDataContainerAdapter(ApplicationData.Current.LocalSettings.CreateContainer(Constants.ShellProfilesContainerName, ApplicationDataCreateDisposition.Always)),
                    Themes = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.ThemesContainerName, ApplicationDataCreateDisposition.Always))
                };

                var containerBuilder = new ContainerBuilder();

                containerBuilder.RegisterInstance(applicationDataContainers);
                containerBuilder.RegisterType<NotificationService>().SingleInstance();
                containerBuilder.RegisterType<TerminalsManager>().SingleInstance();
                containerBuilder.RegisterType<ToggleWindowService>().SingleInstance();
                containerBuilder.RegisterType<HotKeyManager>().SingleInstance();
                containerBuilder.RegisterType<SystemTrayApplicationContext>().SingleInstance();
                containerBuilder.RegisterType<AppCommunicationService>().SingleInstance();
                containerBuilder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>();
                containerBuilder.RegisterType<SettingsService>().As<ISettingsService>();
                containerBuilder.RegisterInstance(Dispatcher.CurrentDispatcher).SingleInstance();

                var container = containerBuilder.Build();

                var appCommunicationService = container.Resolve<AppCommunicationService>();

                if (args.Length > 0 && args[2] == "appLaunched")
                {
                    appCommunicationService.StartAppServiceConnection();
                }
                
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