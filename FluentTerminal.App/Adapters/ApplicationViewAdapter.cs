using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;

namespace FluentTerminal.App.Adapters
{
    public class ApplicationViewAdapter : IApplicationView
    {
        private readonly ApplicationView _applicationView;
        private readonly CoreDispatcher _dispatcher;

        public event CloseRequestedHandler CloseRequested;

        public ApplicationViewAdapter()
        {
            _applicationView = ApplicationView.GetForCurrentView();
            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequest;
        }

        public string Title
        {
            get => _applicationView.Title;
            set => _applicationView.Title = value;
        }

        public Task RunOnDispatcherThread(Action action)
        {
            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).AsTask();
        }

        public Task<bool> TryClose()
        {
            return _applicationView.TryConsolidateAsync().AsTask();
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
            await CloseRequested?.Invoke(this, args);

            e.Handled = args.Cancelled;

            deferral.Complete();
        }
    }
}