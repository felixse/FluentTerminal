using Autofac;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;
using System;
using System.ComponentModel;
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
using IContainer = Autofac.IContainer;

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
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>().SingleInstance();
            builder.RegisterType<TerminalService>().As<ITerminalService>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<MainViewModel>().InstancePerDependency();
            builder.RegisterType<SettingsViewModel>().InstancePerDependency();

            _container = builder.Build();
        }

        public async Task CreateNewTerminalWindow(string startupDirectory)
        {
            var viewModel = _container.Resolve<MainViewModel>();
            viewModel.AddTerminal(startupDirectory);
            var id = await CreateSecondaryView(typeof(MainPage), viewModel, true);
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
                var viewModel = _container.Resolve<SettingsViewModel>();
                _settingsWindowId = await CreateSecondaryView(typeof(SettingsPage), viewModel, true);
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
                if (string.IsNullOrWhiteSpace(commandLineActivated.Operation.Arguments))
                {
                    return;
                }

                var arguments = commandLineActivated.Operation.Arguments.Split(' ', 2);
                var command = arguments[0];
                var parameter = arguments.Length > 1 ? arguments[1] : string.Empty;

                if (_alreadyLaunched)
                {
                    if (command == "settings")
                    {
                        await ShowSettings();
                    }
                    else if (command == "new")
                    {
                        await CreateNewTerminalWindow(parameter);
                    }
                }
                else
                {
                    if (command == "settings")
                    {
                        var viewModel = _container.Resolve<SettingsViewModel>();
                        await CreateMainView(typeof(SettingsPage), viewModel, true);
                    }
                    else if (command == "new")
                    {
                        var viewModel = _container.Resolve<MainViewModel>();
                        viewModel.AddTerminal(parameter);
                        await CreateMainView(typeof(MainPage), viewModel, true);
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
            if (!_alreadyLaunched)
            {
                var viewModel = _container.Resolve<MainViewModel>();
                viewModel.AddTerminal(null);
                await CreateMainView(typeof(MainPage), viewModel, true);
                Window.Current.Activate();
            }
            else if (_openTerminalWindows == 0)
            {
                var viewModel = _container.Resolve<MainViewModel>();
                viewModel.AddTerminal(null);
                await CreateSecondaryView(typeof(MainPage), viewModel, true);
            }
        }

        private async Task CreateMainView(Type pageType, INotifyPropertyChanged viewModel, bool extendViewIntoTitleBar)
        {
            var launch = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync().AsTask();
            var clearCache = WebView.ClearTemporaryWebDataAsync().AsTask();
            await Task.WhenAll(launch, clearCache);

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extendViewIntoTitleBar;

                if (pageType == typeof(MainPage))
                {
                    _openTerminalWindows++;
                }

                rootFrame.Navigate(pageType, viewModel);
            }
            _alreadyLaunched = true;
            Window.Current.Activate();
        }

        private async Task<int> CreateSecondaryView(Type pageType, INotifyPropertyChanged viewModel, bool ExtendViewIntoTitleBar)
        {
            int windowId = 0;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = ExtendViewIntoTitleBar;
                var frame = new Frame();
                frame.Navigate(pageType, viewModel);
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