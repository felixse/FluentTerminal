using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.RuntimeComponent.Interfaces;
using FluentTerminal.RuntimeComponent.WebAllowedObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalView : UserControl, ITerminalView, ITerminalEventListener
    {
        private BlockingCollection<Action> _dispatcherJobs;
        private SemaphoreSlim _loaded;
        private WebView _webView;

        public TerminalView(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
            Loaded += OnLoaded;
            _loaded = new SemaphoreSlim(0, 1);

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                settings.Converters.Add(new StringEnumConverter(true));

                return settings;
            };

            StartMediatorTask();
        }

        public event EventHandler<TerminalSize> TerminalSizeChanged;
        public event EventHandler<string> TerminalTitleChanged;

        public TerminalViewModel ViewModel { get; }

        public Task ChangeOptions(TerminalOptions options)
        {
            var serialized = JsonConvert.SerializeObject(options);
            return ExecuteScriptAsync($"changeOptions('{serialized}')");
        }

        public Task ChangeTheme(TerminalColors theme)
        {
            var serialized = JsonConvert.SerializeObject(theme);
            return ExecuteScriptAsync($"changeTheme('{serialized}')");
        }

        public void Close()
        {
            _webView?.Navigate(new Uri("about:blank"));
            _webView = null;
        }

        public Task ConnectToSocket(string url)
        {
            return ExecuteScriptAsync($"connectToWebSocket('{url}');");
        }

        public async Task<TerminalSize> CreateTerminal(TerminalOptions options, TerminalColors theme)
        {
            var serializedOptions = JsonConvert.SerializeObject(options);
            var serializedTheme = JsonConvert.SerializeObject(theme);
            var size = await ExecuteScriptAsync($"createTerminal('{serializedOptions}', '{serializedTheme}')");
            return JsonConvert.DeserializeObject<TerminalSize>(size);
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

        public async Task FocusTerminal()
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();");
            }
        }

        public async Task FocusWebView()
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();");
            }
        }

        public void OnTerminalResized(int columns, int rows)
        {
            _dispatcherJobs.Add(() => TerminalSizeChanged?.Invoke(this, new TerminalSize { Columns = columns, Rows = rows }));
        }

        public void OnTitleChanged(string title)
        {
            _dispatcherJobs.Add(() => TerminalTitleChanged?.Invoke(this, title));
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

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (WebViewContainer.Children.Any())
            {
                return;
            }

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
    }
}