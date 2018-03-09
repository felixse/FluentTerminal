using Autofac;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IContainer = Autofac.IContainer;

namespace FluentTerminal.App
{
    sealed partial class App : Application
    {
        public static App Instance;
        private bool _alreadyLaunched;
        private IContainer _container;
        private int? _settingsWindowId;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private ApplicationSettings _applicationSettings;

        private List<MainViewModel> _mainViewModels;

        public App()
        {
            _mainViewModels = new List<MainViewModel>();

            InitializeComponent();
            Instance = this;

            var builder = new ContainerBuilder();
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>().SingleInstance();
            builder.RegisterType<TrayProcessCommunicationService>().As<ITrayProcessCommunicationService>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<KeyboardCommandService>().As<IKeyboardCommandService>().InstancePerDependency();
            builder.RegisterType<MainViewModel>().InstancePerDependency();
            builder.RegisterType<SettingsViewModel>().InstancePerDependency();

            _container = builder.Build();

            _settingsService = _container.Resolve<ISettingsService>();
            _settingsService.ApplicationSettingsChanged += OnApplicationSettingsChanged;

            _trayProcessCommunicationService = _container.Resolve<ITrayProcessCommunicationService>();

            _applicationSettings = _settingsService.GetApplicationSettings();
        }

        private void OnApplicationSettingsChanged(object sender, EventArgs e)
        {
            _applicationSettings = _settingsService.GetApplicationSettings();
        }

        public async Task CreateNewTerminalWindow(string startupDirectory)
        {
            var id = await CreateSecondaryView<MainViewModel>(typeof(MainPage), true, startupDirectory);
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
                _settingsWindowId = await CreateSecondaryView<SettingsViewModel>(typeof(SettingsPage), true, null);
            }
            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_settingsWindowId.Value);
        }

        public void TerminalWindowClosed(MainViewModel viewModel)
        {
            _mainViewModels.Remove(viewModel);
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

                if (command == "close")
                {
                    App.Current.Exit();
                }

                if (_alreadyLaunched)
                {
                    if (command == "settings")
                    {
                        await ShowSettings();
                    }
                    else if (command == "new")
                    {
                        if (_applicationSettings.NewTerminalLocation == NewTerminalLocation.Tab && _mainViewModels.Any())
                        {
                            await _mainViewModels.Last().AddTerminal(parameter);
                        }
                        else
                        {
                            await CreateNewTerminalWindow(parameter);
                        }

                    }
                }
                else
                {
                    if (command == "settings")
                    {
                        var viewModel = _container.Resolve<SettingsViewModel>();
                        CreateMainView(typeof(SettingsPage), viewModel, true);
                    }
                    else if (command == "new")
                    {
                        if (_applicationSettings.NewTerminalLocation == NewTerminalLocation.Tab && _mainViewModels.Any())
                        {
                            await _mainViewModels.Last().AddTerminal(parameter);
                        }
                        else
                        {
                            var viewModel = _container.Resolve<MainViewModel>();
                            await viewModel.AddTerminal(parameter);
                            CreateMainView(typeof(MainPage), viewModel, true);
                        }
                    }
                }
            }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!_alreadyLaunched)
            {
                var launch = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync().AsTask();
                var clearCache = WebView.ClearTemporaryWebDataAsync().AsTask();
                await Task.WhenAll(launch, clearCache);

                var trayProcessStatusFile = await ApplicationData.Current.LocalCacheFolder.GetFileAsync($"{nameof(TrayProcessStatus)}.json");
                var trayProcessStatus = JsonConvert.DeserializeObject<TrayProcessStatus>(await FileIO.ReadTextAsync(trayProcessStatusFile));

                await _trayProcessCommunicationService.Initialize(trayProcessStatus);


                var viewModel = _container.Resolve<MainViewModel>();
                await viewModel.AddTerminal(null);
                CreateMainView(typeof(MainPage), viewModel, true);
                Window.Current.Activate();
            }
            else if (_mainViewModels.Any() == false)
            {
                await CreateSecondaryView<MainViewModel>(typeof(MainPage), true, string.Empty);
            }
        }

        private void CreateMainView(Type pageType, INotifyPropertyChanged viewModel, bool extendViewIntoTitleBar)
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

                if (viewModel is MainViewModel mainViewModel)
                {
                    _mainViewModels.Add(mainViewModel);
                }

                rootFrame.Navigate(pageType, viewModel);
            }
            _alreadyLaunched = true;
            Window.Current.Activate();
        }

        private async Task<int> CreateSecondaryView<TViewModel>(Type pageType, bool ExtendViewIntoTitleBar, object parameter)
        {
            int windowId = 0;
            TViewModel viewModel = default;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                viewModel = _container.Resolve<TViewModel>();
                
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = ExtendViewIntoTitleBar;
                var frame = new Frame();
                frame.Navigate(pageType, viewModel);
                Window.Current.Content = frame;
                Window.Current.Activate();

                windowId = ApplicationView.GetForCurrentView().Id;
            });

            if (viewModel is MainViewModel mainViewModel && parameter is string directory)
            {
                _mainViewModels.Add(mainViewModel);
                await mainViewModel.AddTerminal(directory);
            }

            return windowId;
        }
    }
}