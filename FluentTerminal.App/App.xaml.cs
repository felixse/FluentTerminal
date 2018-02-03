using Autofac;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App
{
    sealed partial class App : Application
    {
        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static App Instance;
        private bool _alreadyLaunched;
        private IContainer _container;
        private int _openTerminalWindows;
        private int? _settingsWindowId;

        public App()
        {
            InitializeComponent();
            Instance = this;

            var builder = new ContainerBuilder();
            builder.RegisterType<SettingsService>().As<ISettingsService>().InstancePerDependency();
            builder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>().SingleInstance();
            builder.RegisterType<TerminalService>().As<ITerminalService>().SingleInstance();
            builder.RegisterType<MainViewModel>().InstancePerDependency();
            builder.RegisterType<SettingsViewModel>().InstancePerDependency();

            _container = builder.Build();
        }

        public async Task CreateNewTerminalWindow()
        {
            var id = await CreateSecondaryView(typeof(MainPage), true);
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(id);
        }

        public void SettingsWindowClosed()
        {
            _settingsWindowId = null;
        }

        public async Task ShowSettings()
        {
            if (_settingsWindowId == null)
            {
                _settingsWindowId = await CreateSecondaryView(typeof(SettingsPage), true);
            }
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_settingsWindowId.Value);
        }

        public void TerminalWindowClosed()
        {
            _openTerminalWindows--;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args is CommandLineActivatedEventArgs commandLineActivated)
            {
                var argument = commandLineActivated.Operation.Arguments.Trim();
                if (_alreadyLaunched)
                {
                    if (argument == "settings")
                    {
                        await ShowSettings();
                    }
                    else if (argument == "new")
                    {
                        await CreateNewTerminalWindow();
                    }
                }
                else
                {
                    if (argument == "settings")
                    {
                        CreateMainView(typeof(SettingsPage), true);
                    }
                    else if (argument == "new")
                    {
                        CreateMainView(typeof(MainPage), true);
                    }
                }
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;
                Connection = details.AppServiceConnection;
                Connection.RequestReceived += OnRequestReceived;
            }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var launch = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync().AsTask();
            var clearCache = WebView.ClearTemporaryWebDataAsync().AsTask();
            await Task.WhenAll(launch, clearCache);

            if (!_alreadyLaunched)
            {
                CreateMainView(typeof(MainPage), true);
                Window.Current.Activate();
            }
            else if (_openTerminalWindows == 0)
            {
                await CreateSecondaryView(typeof(MainPage), true);
            }
        }

        private void CreateMainView(Type pageType, bool extendViewIntoTitleBar)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extendViewIntoTitleBar;
                rootFrame.Navigate(pageType, GetViewModelForPageType(pageType));

                if (pageType == typeof(MainPage))
                {
                    _openTerminalWindows++;
                }
            }
            _alreadyLaunched = true;
            Window.Current.Activate();
        }

        private async Task<int> CreateSecondaryView(Type pageType, bool ExtendViewIntoTitleBar)
        {
            int windowId = 0;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = ExtendViewIntoTitleBar;
                var frame = new Frame();
                frame.Navigate(pageType, GetViewModelForPageType(pageType));
                Window.Current.Content = frame;
                Window.Current.Activate();

                windowId = ApplicationView.GetForCurrentView().Id;
            });

            if (pageType == typeof(MainPage))
            {
                _openTerminalWindows++;
            }

            return windowId;
        }

        private object GetViewModelForPageType(Type pageType)
        {
            if (pageType == typeof(MainPage))
            {
                return _container.Resolve<MainViewModel>();
            }
            else if (pageType == typeof(SettingsPage))
            {
                return _container.Resolve<SettingsViewModel>();
            }
            throw new Exception("No ViewModel defined for Page: " + pageType.Name);
        }

        private void OnRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (args.Request.Message.ContainsKey("exit"))
            {
                App.Current.Exit();
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (AppServiceDeferral != null)
            {
                AppServiceDeferral.Complete();
            }
        }
    }
}