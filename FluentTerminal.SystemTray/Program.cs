using Autofac;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Adapters;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.SystemTray.Services;
using GlobalHotKey;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

                JsonConvert.DefaultSettings = () =>
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    };
                    settings.Converters.Add(new StringEnumConverter(typeof(CamelCaseNamingStrategy)));

                    return settings;
                };

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
                containerBuilder.RegisterType<NotificationService>().As<INotificationService>().InstancePerDependency();
                containerBuilder.RegisterType<TerminalsManager>().SingleInstance();
                containerBuilder.RegisterType<ToggleWindowService>().SingleInstance();
                containerBuilder.RegisterInstance(new HotKeyManager()).SingleInstance();
                containerBuilder.RegisterType<SystemTrayApplicationContext>().SingleInstance();
                containerBuilder.RegisterType<AppCommunicationService>().SingleInstance();
                containerBuilder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>();
                containerBuilder.RegisterType<SettingsService>().As<ISettingsService>();
                containerBuilder.RegisterType<UpdateService>().As<IUpdateService>();
                containerBuilder.RegisterInstance(Dispatcher.CurrentDispatcher).SingleInstance();

                var container = containerBuilder.Build();

                Task.Run(async () =>
                {

                    var logDirectory = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(true);
                    var logFile = Path.Combine(logDirectory.Path, "fluentterminal.systemtray.log");
                    var configFile = await logDirectory.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists).AsTask().ConfigureAwait(true);
                    var configContent = await FileIO.ReadTextAsync(configFile).AsTask().ConfigureAwait(true);
                    var config = JsonConvert.DeserializeObject<Logger.Configuration>(configContent) ?? new Logger.Configuration();
                    Logger.Instance.Initialize(logFile, config);

                    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
                });

                var appCommunicationService = container.Resolve<AppCommunicationService>();

                if (args.Length > 0 && args[2] == "appLaunched")
                {
                    appCommunicationService.StartAppServiceConnection();
                }

                Task.Run(() => container.Resolve<IUpdateService>().CheckForUpdate());

                var settingsService = container.Resolve<ISettingsService>();
                if (settingsService.GetApplicationSettings().EnableTrayIcon)
                {
                    Application.Run(container.Resolve<SystemTrayApplicationContext>());
                }
                else
                {
                    Application.Run();
                }

                mutex.Close();
            }
            else
            {
                var eventWaitHandle = EventWaitHandle.OpenExisting(AppCommunicationService.EventWaitHandleName, System.Security.AccessControl.EventWaitHandleRights.Modify);
                eventWaitHandle.Set();
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error((Exception)e.ExceptionObject, "Unhandled Exception");
        }
    }
}