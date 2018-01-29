using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentTerminal.App.Models;
using FluentTerminal.App.ViewModels;
using FluentTerminal.RuntimeComponent.Interfaces;
using FluentTerminal.RuntimeComponent.WebAllowedObjects;
using Newtonsoft.Json;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalPage : Page, ITerminalView, ITerminalEventListener
    {
        private WebView _webView;
        private TerminalViewModel _viewModel;

        private SemaphoreSlim _loaded;

        public event EventHandler<TerminalSize> TerminalSizeChanged;
        public event EventHandler<string> TerminalTitleChanged;

        public TerminalPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
            _loaded = new SemaphoreSlim(0, 1);
            _viewModel = new TerminalViewModel();
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

            await _viewModel.OnViewIsReady(this);
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
            TerminalSizeChanged?.Invoke(this, new TerminalSize(columns, rows));
        }

        public void OnTitleChanged(string title)
        {
            TerminalTitleChanged?.Invoke(this, title);
        }
    }
}
