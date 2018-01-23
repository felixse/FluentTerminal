using FluentTerminal.App.Views;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App
{
    sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Instance = this;
        }

        public static BackgroundTaskDeferral AppServiceDeferral = null;
        public static AppServiceConnection Connection = null;
        public static App Instance;
        int? _settingsWindowId;
        int idCreate = 0;
        List<int> idSaved = new List<int>();

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var launch = FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync().AsTask();
            var clearCache = WebView.ClearTemporaryWebDataAsync().AsTask();

            await Task.WhenAll(launch, clearCache);

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
                idSaved.Add(ApplicationView.GetForCurrentView().Id);
            }
            else if (false) //todo: add option to create new windows on launch
            {
                await CreateNewWindow(typeof(MainPage), true);
            }
            Window.Current.Activate();
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            // connection established from the fulltrust process
            base.OnBackgroundActivated(args);
            if (args.TaskInstance.TriggerDetails is AppServiceTriggerDetails details)
            {
                AppServiceDeferral = args.TaskInstance.GetDeferral();
                args.TaskInstance.Canceled += OnTaskCanceled;
                Connection = details.AppServiceConnection;
                Connection.RequestReceived += OnRequestReceived;
            }
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

        public async Task<int> CreateNewWindow(Type pageType, bool ExtendViewIntoTitleBar)
        {
            var create = CoreApplication.CreateNewView();
            await create.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = ExtendViewIntoTitleBar;
                var frame = new Frame();
                frame.Navigate(pageType, null);
                Window.Current.Content = frame;
                Window.Current.Activate();

                idCreate = ApplicationView.GetForCurrentView().Id;
            });

            for (int i = idSaved.Count - 1; i >= 0; i--)
                if (await ApplicationViewSwitcher.TryShowAsStandaloneAsync(
                        idCreate, ViewSizePreference.Default,
                        idSaved[i], ViewSizePreference.Default)
                   ) break;

            idSaved.Add(idCreate);
            return idCreate;
        }

        public async Task ShowNew()
        {
            var id = await CreateNewWindow(typeof(MainPage), true);
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(id);
        }

        public async Task ShowSettings()
        {
            if (_settingsWindowId == null)
            {
                _settingsWindowId = await CreateNewWindow(typeof(Views.SettingsPage), false);
            }
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_settingsWindowId.Value);
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
    }
}
