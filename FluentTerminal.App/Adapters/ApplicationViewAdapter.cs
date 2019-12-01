using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using FluentTerminal.App.Utilities;

namespace FluentTerminal.App.Adapters
{
    public class ApplicationViewAdapter : IApplicationView
    {
        private readonly ApplicationView _applicationView;
        private readonly CoreDispatcher _dispatcher;
        private bool _closed;

        public event CloseRequestedHandler CloseRequested;
        public event EventHandler Closed;

        public ApplicationViewAdapter()
        {
            _applicationView = ApplicationView.GetForCurrentView();
            _applicationView.Consolidated += _applicationView_Consolidated;
            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;

            Logger.Instance.Debug("Created ApplicationViewAdapter for ApplicationView with Id: {Id}", _applicationView.Id);
        }

        private void _applicationView_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            _applicationView.Consolidated -= _applicationView_Consolidated;
            Closed?.Invoke(this, EventArgs.Empty);
        }

        public int Id => _applicationView.Id;

        public string Title
        {
            get => _applicationView.Title;
            set => _applicationView.Title = value ?? string.Empty;
        }

        public bool IsApiContractPresent(string api, ushort version)
        {
            return ApiInformation.IsApiContractPresent(api, version);
        }

        public Task RunOnDispatcherThread(Action action, bool enforceNewSchedule = true)
        {
            if (!enforceNewSchedule && _dispatcher.HasThreadAccess)
            {
                action();

                return Task.CompletedTask;
            }

            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
        }

        public Task ExecuteOnUiThreadAsync(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal,
            bool enforceNewSchedule = false) => _dispatcher.ExecuteAsync(action, priority, enforceNewSchedule);

        public Task<T> ExecuteOnUiThreadAsync<T>(Func<T> func, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal,
            bool enforceNewSchedule = false) => _dispatcher.ExecuteAsync(func, priority, enforceNewSchedule);

        public async Task<bool> TryClose()
        {
            if (_closed)
            {
                Logger.Instance.Debug("ApplicationViewAdapter.TryClose was called, but was already closed. ApplicationView.Id: {Id}", _applicationView.Id);
                return true;
            }
            Logger.Instance.Debug("TryClose ApplicationView with Id: {Id}", _applicationView.Id);
            _closed = await _applicationView.TryConsolidateAsync().AsTask();
            return _closed;
        }

        public bool ToggleFullScreen()
        {
            if (_applicationView.IsFullScreenMode)
            {
                _applicationView.ExitFullScreenMode();
                return true;
            }
            else
            {
                return _applicationView.TryEnterFullScreenMode();
            }
        }

        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();

            var args = new CancelableEventArgs();

            if (CloseRequested != null)
            {
                await CloseRequested.Invoke(this, args);
            }

            e.Handled = args.Cancelled;

            if (!e.Handled)
            {
                SystemNavigationManagerPreview.GetForCurrentView().CloseRequested -= OnCloseRequest;
            }

            deferral.Complete();
        }
    }
}