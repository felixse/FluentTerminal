using FluentTerminal.App.Models;
using FluentTerminal.App.ViewModels;
using FluentTerminal.RuntimeComponent.Interfaces;
using FluentTerminal.RuntimeComponent.WebAllowedObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalView : UserControl, ITerminalView, ITerminalEventListener
    {
        private WebView _webView;

        private SemaphoreSlim _loaded;

        private BlockingCollection<Action> _dispatcherJobs;

        public event EventHandler<TerminalSize> TerminalSizeChanged;
        public event EventHandler<string> TerminalTitleChanged;

        public TerminalViewModel ViewModel { get; }

        public TerminalView(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested; //todo move to mainpage
            _loaded = new SemaphoreSlim(0, 1);

            StartMediatorTask();
        }

        public async Task FocusWebView()
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();");
            }
        }

        private void OnCloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            _webView.Navigate(new Uri("about:blank"));
            _webView = null;
        }

        public Task<string> ExecuteScriptAsync(string script)
        {
            try
            {
                return _webView.InvokeScriptAsync("eval", new[] { script }).AsTask();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception while running:\n \"{script}\"\n\n {e}");
            }
            return Task.FromResult(string.Empty);
        }

        private void StartMediatorTask()
        {
            _dispatcherJobs = new BlockingCollection<Action>();
            var dispatcher = CoreApplication.GetCurrentView().CoreWindow.Dispatcher;
            Task.Run(async () =>
            {
                foreach (var job in _dispatcherJobs.GetConsumingEnumerable())
                {
                    try
                    {
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => job.Invoke());
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }
                }
            });
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _webView = new WebView(WebViewExecutionMode.SeparateThread)
            {
                DefaultBackgroundColor = Colors.Transparent
            };
            WebViewContainer.Children.Add(_webView);

            _webView.NavigationCompleted += _webView_NavigationCompleted;
            _webView.NavigationStarting += _webView_NavigationStarting;
            _webView.Navigate(new Uri("http://localhost:9000/Client/index.html"));

            await _loaded.WaitAsync();

            _webView.Focus(FocusState.Programmatic);

            await ViewModel.OnViewIsReady(this);
        }

        private void _webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            _loaded.Release();
        }

        private void _webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            var bridge = new TerminalBridge(this);
            _webView.AddWebAllowedObject("terminalBridge", bridge);
        }

        public async Task<TerminalSize> CreateTerminal(TerminalConfiguration configuration)
        {
            var size = await ExecuteScriptAsync($"createTerminal()");
            return JsonConvert.DeserializeObject<TerminalSize>(size);
        }

        public Task ConnectToSocket(string url)
        {
            return ExecuteScriptAsync($"connectToWebSocket('{url}');");
        }

        public void OnTerminalResized(int columns, int rows)
        {
            _dispatcherJobs.Add(() => TerminalSizeChanged?.Invoke(this, new TerminalSize(columns, rows)));
        }

        public void OnTitleChanged(string title)
        {
            _dispatcherJobs.Add(() => TerminalTitleChanged?.Invoke(this, title));
        }
    }
}
