using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;

namespace FluentTerminal.App.Adapters
{
    public class ApplicationViewAdapter : IApplicationView
    {
        private readonly ApplicationView _applicationView;
        private readonly CoreDispatcher _dispatcher;
        private bool _closed;

        public event CloseRequestedHandler CloseRequested;

        public ApplicationViewAdapter()
        {
            _applicationView = ApplicationView.GetForCurrentView();
            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;

            Logger.Instance.Debug("Created ApplicationViewAdapter for ApplicationView with Id: {Id}", _applicationView.Id);
        }

        public string Title
        {
            get => _applicationView.Title;
            set => _applicationView.Title = value;
        }

        public bool IsApiContractPresent(string api, ushort version)
        {
            return ApiInformation.IsApiContractPresent(api, version);
        }

        public Task RunOnDispatcherThread(Action action)
        {
            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
        }

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

            deferral.Complete();
        }
    }
}