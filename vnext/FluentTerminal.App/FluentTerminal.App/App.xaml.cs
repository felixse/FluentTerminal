using FluentTerminal.App.Adapters;
using FluentTerminal.App.Dialogs;
using FluentTerminal.App.Pages;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Adapters;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FluentTerminal.App
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly List<MainViewModel> _mainViewModels;

        public IServiceProvider Services { get; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            _mainViewModels = new List<MainViewModel>();

            this.InitializeComponent();

            UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += (sender, e) => Logger.Instance.Error(e.Exception, "Unobserved Task Exception");

            Services = ConfigureServices();

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                settings.Converters.Add(new StringEnumConverter(typeof(CamelCaseNamingStrategy)));

                return settings;
            };
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var applicationDataContainers = new ApplicationDataContainers
            {
                LocalSettings = new ApplicationDataContainerAdapter(ApplicationData.Current.LocalSettings),
                RoamingSettings = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings),
                KeyBindings = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.KeyBindingsContainerName, ApplicationDataCreateDisposition.Always)),
                ShellProfiles = new ApplicationDataContainerAdapter(ApplicationData.Current.LocalSettings.CreateContainer(Constants.ShellProfilesContainerName, ApplicationDataCreateDisposition.Always)),
                Themes = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.ThemesContainerName, ApplicationDataCreateDisposition.Always)),
                SshProfiles = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.SshProfilesContainerName, ApplicationDataCreateDisposition.Always)),
                HistoryContainer = new ApplicationDataContainerAdapter(ApplicationData.Current.RoamingSettings.CreateContainer(Constants.ExecutedCommandsContainerName, ApplicationDataCreateDisposition.Always))
            };

            services.AddSingleton(applicationDataContainers);
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<ICommandHistoryService, CommandHistoryService>();
            services.AddSingleton<IDefaultValueProvider, DefaultValueProvider>();
            services.AddSingleton<ITrayProcessCommunicationService, TrayProcessCommunicationService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IKeyboardCommandService, KeyboardCommandService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<IUpdateService, UpdateService>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddSingleton<IThemeParserFactory, ThemeParserFactory>();
            services.AddSingleton<IThemeParser, FluentTerminalThemeParser>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<IFileSystemService, FileSystemService>();
            services.AddSingleton<IImageFileSystemService, ImageFileSystemService>();
            services.AddSingleton<ISystemFontService, SystemFontService>();

            services.AddTransient<ICreateKeyBindingDialog, CreateKeyBindingDialog>();
            services.AddTransient<IMessageDialog, MessageDialogAdapter>();
            services.AddTransient<IInputDialog, InputDialog>();
            services.AddTransient<ISshConnectionInfoDialog, SshInfoDialog>();
            services.AddTransient<ICustomCommandDialog, CustomCommandDialog>();
            services.AddTransient<IAboutDialog, AboutDialog>();

            services.AddSingleton(provider => new Func<IMessageDialog>(() => provider.GetService<IMessageDialog>()));
            services.AddSingleton(provider => new Func<ICreateKeyBindingDialog>(() => provider.GetService<ICreateKeyBindingDialog>()));
            services.AddSingleton(provider => new Func<IInputDialog>(() => provider.GetService<IInputDialog>()));
            services.AddSingleton(provider => new Func<ISshConnectionInfoDialog>(() => provider.GetService<ISshConnectionInfoDialog>()));
            services.AddSingleton(provider => new Func<ICustomCommandDialog>(() => provider.GetService<ICustomCommandDialog>()));
            services.AddSingleton(provider => new Func<IAboutDialog>(() => provider.GetService<IAboutDialog>()));

            services.AddSingleton<IApplicationLanguageService, ApplicationLanguageService>();
            services.AddSingleton<IShellProfileMigrationService, ShellProfileMigrationService>();

            return services.BuildServiceProvider();
        }

        private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error(e.Exception, "Unhandled Exception");
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();

            var viewModel = Services.GetService<MainViewModel>();

            await viewModel.AddDefaultProfileAsync(NewTerminalLocation.Tab);

            if (_window.Content is Frame rootFrame)
            {
                rootFrame.Navigate(typeof(MainPage), viewModel);
            }
        }

        private Window _window;
    }
}
