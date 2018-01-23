using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalPage : Page
    {
        private WebView _webView;

        public TerminalPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += OnCloseRequested;
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
                return _webView.InvokeScriptAsync("eval", new[]
                {
                    "try {" + script + "} catch(error) {}"
                }).AsTask();
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

            _webView.Navigate(new Uri("http://localhost:9000/Client/index.html"));
            _webView.Focus(FocusState.Programmatic);
        }
    }
}
