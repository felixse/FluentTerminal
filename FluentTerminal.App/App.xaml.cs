using Autofac;
using CommandLine;
using FluentTerminal.App.Adapters;
using FluentTerminal.App.CommandLineArguments;
using FluentTerminal.App.Dialogs;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Adapters;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Utilities;
using IContainer = Autofac.IContainer;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Messaging;

namespace FluentTerminal.App
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class App : Application
    {
        private readonly TaskCompletionSource<int> _trayReady = new TaskCompletionSource<int>();
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IDialogService _dialogService;
        private bool _alreadyLaunched;
        private bool _isLaunching;
        private ApplicationSettings _applicationSettings;
        private readonly IContainer _container;
        private readonly List<MainViewModel> _mainViewModels;
        private SettingsViewModel _settingsViewModel;
        private int? _settingsWindowId;
        private IAppServiceConnection _appServiceConnection;
        private BackgroundTaskDeferral _appServiceDeferral;
        private Parser _commandLineParser;
        private int? _activeWindowId;

        public App()
        {
            _mainViewModels = new List<MainViewModel>();

            InitializeComponent();

            UnhandledException += OnUnhandledException;

            TaskScheduler.UnobservedTaskException += (sender, e) =>
                Logger.Instance.Error(e.Exception, "Unobserved Task Exception");

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

            var builder = new ContainerBuilder();
            builder.RegisterInstance(applicationDataContainers);
            builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();
            builder.RegisterType<CommandHistoryService>().As<ICommandHistoryService>().SingleInstance();
            builder.RegisterType<DefaultValueProvider>().As<IDefaultValueProvider>().SingleInstance();
            builder.RegisterType<TrayProcessCommunicationService>().As<ITrayProcessCommunicationService>().SingleInstance();
            builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
            builder.RegisterType<KeyboardCommandService>().As<IKeyboardCommandService>().InstancePerDependency();
            builder.RegisterType<NotificationService>().As<INotificationService>().InstancePerDependency();
            builder.RegisterType<UpdateService>().As<IUpdateService>().InstancePerDependency();
            builder.RegisterType<MainViewModel>().InstancePerDependency();
            builder.RegisterType<SettingsViewModel>().InstancePerDependency();
            builder.RegisterType<ThemeParserFactory>().As<IThemeParserFactory>().SingleInstance();
            builder.RegisterType<ITermThemeParser>().As<IThemeParser>().SingleInstance();
            builder.RegisterType<FluentTerminalThemeParser>().As<IThemeParser>().SingleInstance();
            builder.RegisterType<ClipboardService>().As<IClipboardService>().SingleInstance();
            builder.RegisterType<FileSystemService>().As<IFileSystemService>().SingleInstance();
            builder.RegisterType<ImageFileSystemService>().As<IImageFileSystemService>().SingleInstance();
            builder.RegisterType<SystemFontService>().As<ISystemFontService>().SingleInstance();
            builder.RegisterType<ShellProfileSelectionDialog>().As<IShellProfileSelectionDialog>().InstancePerDependency();
            builder.RegisterType<CreateKeyBindingDialog>().As<ICreateKeyBindingDialog>().InstancePerDependency();
            builder.RegisterType<InputDialog>().As<IInputDialog>().InstancePerDependency();
            builder.RegisterType<AboutDialog>().As<IAboutDialog>().InstancePerDependency();
            builder.RegisterType<MessageDialogAdapter>().As<IMessageDialog>().InstancePerDependency();
            builder.RegisterType<SshInfoDialog>().As<ISshConnectionInfoDialog>().InstancePerDependency();
            builder.RegisterType<CustomCommandDialog>().As<ICustomCommandDialog>().InstancePerDependency();
            builder.RegisterType<ApplicationViewAdapter>().As<IApplicationView>().InstancePerDependency();
            builder.RegisterType<DispatcherTimerAdapter>().As<IDispatcherTimer>().InstancePerDependency();
            builder.RegisterType<StartupTaskService>().As<IStartupTaskService>().SingleInstance();
            builder.RegisterType<ApplicationLanguageService>().As<IApplicationLanguageService>().SingleInstance();
            builder.RegisterType<ShellProfileMigrationService>().As<IShellProfileMigrationService>().SingleInstance();

            _container = builder.Build();

            Messenger.Default.Register<ApplicationSettingsChangedMessage>(this, OnApplicationSettingsChanged);

            _settingsService = _container.Resolve<ISettingsService>();

            var shellProfileMigrationService = _container.Resolve<IShellProfileMigrationService>();
            foreach (var profile in _settingsService.GetShellProfiles())
            {
                shellProfileMigrationService.Migrate(profile);
                _settingsService.SaveShellProfile(profile);
            }

            foreach (var profile in _settingsService.GetSshProfiles())
            {
                shellProfileMigrationService.Migrate(profile);
                _settingsService.SaveSshProfile(profile);
            }

            _trayProcessCommunicationService = _container.Resolve<ITrayProcessCommunicationService>();

            _dialogService = _container.Resolve<IDialogService>();

            _applicationSettings = _settingsService.GetApplicationSettings();

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                settings.Converters.Add(new StringEnumConverter(typeof(CamelCaseNamingStrategy)));

                return settings;
            };

            _commandLineParser = new Parser(settings =>
            {
                settings.CaseSensitive = false;
                settings.CaseInsensitiveEnumValues = true;
            });
        }

        private void OnUnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Logger.Instance.Error(e.Exception, "Unhandled Exception");
        }

        private static IEnumerable<string> SplitArguments(string arguments)
        {
            var chars = arguments.ToCharArray();
            var inQuote = false;

            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '"')
                {
                    inQuote = !inQuote;
                }

                if (!inQuote && chars[i] == ' ')
                {
                    chars[i] = '\n';
                }
            }

            foreach (var value in new string(chars).Split('\n'))
            {
                yield return value.Trim('"');
            }
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            if (args is ProtocolActivatedEventArgs protocolActivated)
            {
                if (protocolActivated.Uri == new Uri("ftcmd://fluent.terminal?focus"))
                {
                    await ShowOrCreateWindow(protocolActivated.ViewSwitcher);
                    return;
                }

                MainViewModel mainViewModel = null;
                // IApplicationView to use for creating view models
                IApplicationView applicationView;

                if (_alreadyLaunched)
                {
                    applicationView =
                        (_mainViewModels.FirstOrDefault(o => o.ApplicationView.Id == _activeWindowId) ??
                         _mainViewModels.Last()).ApplicationView;
                }
                else
                {
                    // App wasn't launched before double clicking a shortcut, so we have to create a window
                    // in order to be able to communicate with user.
                    mainViewModel = _container.Resolve<MainViewModel>();

                    await CreateMainView(typeof(MainPage), mainViewModel, true);

                    applicationView = mainViewModel.ApplicationView;
                }

                bool isSsh;

                try
                {
                    isSsh = SshConnectViewModel.CheckScheme(protocolActivated.Uri);
                }
                catch (Exception ex)
                {
                    await new MessageDialog(
                            $"{I18N.TranslateWithFallback("InvalidLink", "Invalid link.")} {ex.Message}",
                            "Invalid Link")
                        .ShowAsync();

                    mainViewModel?.ApplicationView.TryClose();

                    return;
                }

                if (isSsh)
                {
                    SshConnectViewModel vm;

                    try
                    {
                        vm = SshConnectViewModel.ParseUri(protocolActivated.Uri, _settingsService, applicationView,
                            _trayProcessCommunicationService, _container.Resolve<IFileSystemService>(),
                            _container.Resolve<ApplicationDataContainers>().HistoryContainer);
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(
                                $"{I18N.TranslateWithFallback("InvalidLink", "Invalid link.")} {ex.Message}",
                                "Invalid Link")
                            .ShowAsync();

                        mainViewModel?.ApplicationView.TryClose();

                        return;
                    }

                    if (_applicationSettings.AutoFallbackToWindowsUsernameInLinks && string.IsNullOrEmpty(vm.Username))
                    {
                        vm.Username = await _trayProcessCommunicationService.GetUserName();
                    }

                    var error = await vm.AcceptChangesAsync(true);

                    SshProfile profile = (SshProfile) vm.Model;

                    if (!string.IsNullOrEmpty(error))
                    {
                        // Link is valid, but incomplete (i.e. username missing), so we need to show dialog.
                        profile = await _dialogService.ShowSshConnectionInfoDialogAsync(profile);

                        if (profile == null)
                        {
                            // User clicked "Cancel" in the dialog.
                            mainViewModel?.ApplicationView.TryClose();

                            return;
                        }
                    }

                    if (mainViewModel == null)
                        await CreateTerminal(profile, _applicationSettings.NewTerminalLocation, protocolActivated.ViewSwitcher);
                    else
                        await mainViewModel.AddTabAsync(profile);

                    return;
                }

                if (CommandProfileProviderViewModel.CheckScheme(protocolActivated.Uri))
                {
                    CommandProfileProviderViewModel vm;

                    try
                    {
                        vm = CommandProfileProviderViewModel.ParseUri(protocolActivated.Uri, _settingsService,
                            applicationView, _trayProcessCommunicationService,
                            _container.Resolve<ICommandHistoryService>());
                    }
                    catch (Exception ex)
                    {
                        await new MessageDialog(
                                $"{I18N.TranslateWithFallback("InvalidLink", "Invalid link.")} {ex.Message}",
                                "Invalid Link")
                            .ShowAsync();

                        mainViewModel?.ApplicationView.TryClose();

                        return;
                    }

                    var error = await vm.AcceptChangesAsync(true);

                    var profile = vm.Model;

                    if (!string.IsNullOrEmpty(error))
                    {
                        // Link is valid, but incomplete, so we need to show dialog.
                        profile = await _dialogService.ShowCustomCommandDialogAsync(profile);

                        if (profile == null)
                        {
                            // User clicked "Cancel" in the dialog.
                            mainViewModel?.ApplicationView.TryClose();

                            return;
                        }
                    }

                    if (mainViewModel == null)
                    {
                        await CreateTerminal(profile, _applicationSettings.NewTerminalLocation, protocolActivated.ViewSwitcher);
                    }
                    else
                    {
                        await mainViewModel.AddTabAsync(profile);
                    }
                    return;
                }

                await new MessageDialog(
                        $"{I18N.TranslateWithFallback("InvalidLink", "Invalid link.")} {protocolActivated.Uri}",
                        "Invalid Link")
                    .ShowAsync();

                mainViewModel?.ApplicationView.TryClose();

                return;
            }

            if (args is CommandLineActivatedEventArgs commandLineActivated)
            {
                var arguments = commandLineActivated.Operation.Arguments;
                if (string.IsNullOrWhiteSpace(arguments))
                {
                    arguments = "new";
                }

                _commandLineParser.ParseArguments(SplitArguments(arguments), typeof(NewVerb), typeof(RunVerb), typeof(SettingsVerb)).WithParsed(async verb =>
                {
                    if (verb is SettingsVerb settingsVerb)
                    {
                        if (!settingsVerb.Import && !settingsVerb.Export)
                        {
                            await ShowSettings().ConfigureAwait(true);
                        }
                        else if (settingsVerb.Export)
                        {
                            var exportFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists);

                            var settings = _settingsService.ExportSettings();
                            await FileIO.WriteTextAsync(exportFile, settings);
                            await new MessageDialog($"{I18N.Translate("SettingsExported")} {exportFile.Path}").ShowAsync();
                        }
                        else if (settingsVerb.Import)
                        {
                            var file = await ApplicationData.Current.LocalFolder.GetFileAsync("config.json");
                            var content = await FileIO.ReadTextAsync(file);
                            _settingsService.ImportSettings(content);
                            await new MessageDialog($"{I18N.Translate("SettingsImported")} {file.Path}").ShowAsync();
                        }
                    }
                    else if (verb is NewVerb newVerb)
                    {
                        var profile = default(ShellProfile);
                        if (!string.IsNullOrWhiteSpace(newVerb.Profile))
                        {
                            profile = _settingsService.GetShellProfiles().FirstOrDefault(x => x.Name.Equals(newVerb.Profile, StringComparison.CurrentCultureIgnoreCase));
                        }

                        if (profile == null)
                        {
                            profile = _settingsService.GetDefaultShellProfile();
                        }

                        if (!string.IsNullOrWhiteSpace(newVerb.Directory) && newVerb.Directory != ".")
                        {
                            profile.WorkingDirectory = newVerb.Directory;
                        }
                        else
                        {
                            profile.WorkingDirectory = commandLineActivated.Operation.CurrentDirectoryPath;
                        }

                        var location = newVerb.Target == Target.Default ? _applicationSettings.NewTerminalLocation
                            : newVerb.Target == Target.Tab ? NewTerminalLocation.Tab
                            : NewTerminalLocation.Window;

                        await CreateTerminal(profile, location).ConfigureAwait(true);
                    }
                    else if (verb is RunVerb runVerb)
                    {
                        var profile = new ShellProfile
                        {
                            Id = Guid.Empty,
                            Location = null,
                            Arguments = runVerb.Command,
                            WorkingDirectory = runVerb.Directory
                        };

                        if (!string.IsNullOrWhiteSpace(runVerb.Theme))
                        {
                            var theme = _settingsService.GetThemes().FirstOrDefault(x => x.Name.Equals(runVerb.Theme, StringComparison.CurrentCultureIgnoreCase));
                            if (theme != null)
                            {
                                profile.TerminalThemeId = theme.Id;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(profile.WorkingDirectory))
                        {
                            profile.WorkingDirectory = commandLineActivated.Operation.CurrentDirectoryPath;
                        }

                        var location = runVerb.Target == Target.Default ? _applicationSettings.NewTerminalLocation
                            : runVerb.Target == Target.Tab ? NewTerminalLocation.Tab
                            : NewTerminalLocation.Window;

                        await CreateTerminal(profile, location).ConfigureAwait(true);
                    }
                });
            }
        }

        private async Task ShowOrCreateWindow(ActivationViewSwitcher viewSwitcher)
        {
            if (_mainViewModels.Count == 0)
            {
                await CreateTerminal(_settingsService.GetDefaultShellProfile(), NewTerminalLocation.Tab, viewSwitcher);
            }
            else
            {
                var viewModel = _mainViewModels.Find(o => o.ApplicationView.Id == _activeWindowId) ?? _mainViewModels.Last();
                await ShowAsStandaloneAsync(viewModel, viewSwitcher);
            }
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (_isLaunching)
            {
                return;
            }

            _isLaunching = true;
            if (!_alreadyLaunched)
            {
                await InitializeLogger();

                // TODO: Check the reason for such strange using of tasks!
                Task.Run(async () => await JumpListHelper.Update(_settingsService.GetShellProfiles()));

                var viewModel = _container.Resolve<MainViewModel>();
                if (args.Arguments.StartsWith(JumpListHelper.ShellProfileFlag))
                {
                    await viewModel.AddProfileByGuidAsync(Guid.Parse(args.Arguments.Replace(JumpListHelper.ShellProfileFlag, string.Empty)));
                }
                else
                {
                    await viewModel.AddDefaultProfileAsync(NewTerminalLocation.Tab);
                }
                await CreateMainView(typeof(MainPage), viewModel, true).ConfigureAwait(true);
                Window.Current.Activate();
            }
            else if (args.Arguments.StartsWith(JumpListHelper.ShellProfileFlag))
            {
                var location = _applicationSettings.NewTerminalLocation;
                var profile = _settingsService.GetShellProfile(Guid.Parse(args.Arguments.Replace(JumpListHelper.ShellProfileFlag, string.Empty)));
                await CreateTerminal(profile, location, args.ViewSwitcher).ConfigureAwait(true);
            }
            else
            {
                var viewModel = await CreateNewTerminalWindow().ConfigureAwait(true);
                await viewModel.AddDefaultProfileAsync(NewTerminalLocation.Tab);
                await ShowAsStandaloneAsync(viewModel, args.ViewSwitcher);
            }

            _isLaunching = false;
        }

        private static async Task InitializeLogger()
        {
            var logDirectory = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Logs", CreationCollisionOption.OpenIfExists);
            var logFile = Path.Combine(logDirectory.Path, "fluentterminal.app.log");
            var configFile = await logDirectory.CreateFileAsync("config.json", CreationCollisionOption.OpenIfExists);
            var configContent = await FileIO.ReadTextAsync(configFile);


            if (string.IsNullOrWhiteSpace(configContent))
            {
                configContent = JsonConvert.SerializeObject(new Logger.Configuration());
                await FileIO.WriteTextAsync(configFile, configContent);
            }

            var config = JsonConvert.DeserializeObject<Logger.Configuration>(configContent) ?? new Logger.Configuration();

            Logger.Instance.Initialize(logFile, config);
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);

            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                if (details.CallerPackageFamilyName == Package.Current.Id.FamilyName)
                {
                    _appServiceDeferral = args.TaskInstance.GetDeferral();
                    args.TaskInstance.Canceled += OnTaskCanceled;

                    _appServiceConnection = new AppServiceConnectionAdapter(details.AppServiceConnection);

                    _trayReady.SetResult(0);
                }
            }
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _appServiceDeferral?.Complete();
            _appServiceDeferral = null;
            _appServiceConnection = null;

            // ReSharper disable once ArrangeStaticMemberQualifier
            Application.Current.Exit();
        }

        private async Task CreateMainView(Type pageType, INotifyPropertyChanged viewModel, bool extendViewIntoTitleBar)
        {
            ApplicationViewSwitcher.DisableSystemViewActivationPolicy();

            await StartSystemTray().ConfigureAwait(true);

            if (!(Window.Current.Content is Frame rootFrame))
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extendViewIntoTitleBar;

                if (viewModel is MainViewModel mainViewModel)
                {
                    mainViewModel.Closed += OnMainViewModelClosed;
                    mainViewModel.NewWindowRequested += OnNewWindowRequested;
                    mainViewModel.ShowSettingsRequested += OnShowSettingsRequested;
                    mainViewModel.ActivatedMv += OnMainViewActivated;
                    mainViewModel.TabTearedOff += OnTabTearOff;
                    _mainViewModels.Add(mainViewModel);
                }

                rootFrame.Navigate(pageType, viewModel);
            }
            _alreadyLaunched = true;
            Window.Current.Activate();
        }

        private async Task<MainViewModel> CreateNewTerminalWindow()
        {
            var viewModel = await CreateSecondaryView<MainViewModel>(typeof(MainPage), true).ConfigureAwait(true);
            viewModel.Closed += OnMainViewModelClosed;
            viewModel.NewWindowRequested += OnNewWindowRequested;
            viewModel.ShowSettingsRequested += OnShowSettingsRequested;
            viewModel.ActivatedMv += OnMainViewActivated;
            viewModel.TabTearedOff += OnTabTearOff;
            _mainViewModels.Add(viewModel);

            return viewModel;
        }

        private async Task<TViewModel> CreateSecondaryView<TViewModel>(Type pageType, bool extendViewIntoTitleBar)
        {
            int windowId = 0;
            TViewModel viewModel = default;
            await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                viewModel = _container.Resolve<TViewModel>();

                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = extendViewIntoTitleBar;
                var frame = new Frame();
                frame.Navigate(pageType, viewModel);
                Window.Current.Content = frame;
                Window.Current.Activate();

                windowId = ApplicationView.GetForCurrentView().Id;
            });

            if (viewModel is SettingsViewModel settingsViewModel)
            {
                _settingsViewModel = settingsViewModel;
                _settingsViewModel.Closed += OnSettingsClosed;
                _settingsWindowId = windowId;
            }

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowId);

            return viewModel;
        }

        private void OnApplicationSettingsChanged(ApplicationSettingsChangedMessage message)
        {
            _applicationSettings = message.ApplicationSettings;
            _trayProcessCommunicationService.UpdateSettings(message.ApplicationSettings);
        }

        private void OnMainViewModelClosed(object sender, EventArgs e)
        {
            if (sender is MainViewModel viewModel)
            {
                Logger.Instance.Debug("MainViewModel with ApplicationView Id: {@id} closed.", viewModel.ApplicationView.Id);

                viewModel.Closed -= OnMainViewModelClosed;
                viewModel.NewWindowRequested -= OnNewWindowRequested;
                viewModel.ShowSettingsRequested -= OnShowSettingsRequested;
                viewModel.ActivatedMv -= OnMainViewActivated;
                viewModel.TabTearedOff -= OnTabTearOff;
                if (_activeWindowId == viewModel.ApplicationView.Id)
                {
                    _activeWindowId = 0;
                }

                _mainViewModels.Remove(viewModel);

                try
                {
                    // try/catch because the method cannot be called on the last window
                    Window.Current.Close();
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void OnMainViewActivated(object sender, EventArgs e)
        {
            if (sender is MainViewModel viewModel)
            {
                Logger.Instance.Debug("MainViewModel with ApplicationView Id: {@id} activated.", viewModel.ApplicationView.Id);
                _activeWindowId = viewModel.ApplicationView.Id;
            }
        }

        private async void OnTabTearOff(object sender, TerminalViewModel model)
        {
            Logger.Instance.Debug("App.xaml.cs on tab tear off");

            var newViewModel = await CreateNewTerminalWindow().ConfigureAwait(true);
            await newViewModel.AddTabAsync(await model.Serialize(), 0);
        }

        private async void OnNewWindowRequested(object sender, NewWindowRequestedEventArgs e)
        {
            var viewModel = await CreateNewTerminalWindow().ConfigureAwait(true);

            await viewModel.AddTabAsync(e.Profile);
        }

        private void OnSettingsClosed(object sender, EventArgs e)
        {
            Task.Run(async () => await JumpListHelper.Update(_settingsService.GetShellProfiles()));
            _settingsViewModel.Closed -= OnSettingsClosed;
            _settingsViewModel = null;
            _settingsWindowId = null;
        }

        private async void OnShowSettingsRequested(object sender, EventArgs e)
        {
            await ShowSettings().ConfigureAwait(true);
        }

        private async Task ShowAsStandaloneAsync(MainViewModel viewModel, ActivationViewSwitcher viewSwitcher = null)
        {
            int viewId = viewModel.ApplicationView.Id;
            if (viewSwitcher != null)
            {
                await viewModel.ApplicationView.RunOnDispatcherThread(async () => await viewSwitcher.ShowAsStandaloneAsync(viewId));
            }
            else
            {
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(viewId);
            }
        }

        private async Task CreateTerminal(ShellProfile profile, NewTerminalLocation location, ActivationViewSwitcher viewSwitcher = null)
        {
            

            if (!_alreadyLaunched)
            {
                var viewModel = _container.Resolve<MainViewModel>();
                await viewModel.AddTabAsync(profile);
                await CreateMainView(typeof(MainPage), viewModel, true).ConfigureAwait(true);
            }
            else if (location == NewTerminalLocation.Tab && _mainViewModels.Count > 0)
            {

                MainViewModel item = _mainViewModels.FirstOrDefault(o => o.ApplicationView.Id == _activeWindowId);
                if (item == null)
                {
                    item = _mainViewModels.Last();
                }

                await item.AddTabAsync(profile);
                await ShowAsStandaloneAsync(item, viewSwitcher);
            }
            else
            {
                var viewModel = await CreateNewTerminalWindow().ConfigureAwait(true);
                await viewModel.AddTabAsync(profile);
                await ShowAsStandaloneAsync(viewModel, viewSwitcher);
            }
        }

        private async Task ShowSettings()
        {
            if (!_alreadyLaunched)
            {
                var viewModel = _container.Resolve<SettingsViewModel>();
                await CreateMainView(typeof(SettingsPage), viewModel, true).ConfigureAwait(true);
            }
            else if (_settingsViewModel == null)
            {
                await CreateSecondaryView<SettingsViewModel>(typeof(SettingsPage), true).ConfigureAwait(true);
            }
            else
            {
                await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_settingsWindowId.Value);
            }
        }

        private async Task StartSystemTray()
        {
            var launch = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync("AppLaunchedParameterGroup").AsTask();
            await Task.WhenAll(launch, _trayReady.Task).ConfigureAwait(true);
            _trayProcessCommunicationService.Initialize(_appServiceConnection);
        }
    }
}
