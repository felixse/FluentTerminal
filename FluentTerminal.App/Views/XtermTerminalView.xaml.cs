using Fleck;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.ViewModels.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.RuntimeComponent.Enums;
using FluentTerminal.RuntimeComponent.Interfaces;
using FluentTerminal.RuntimeComponent.WebAllowedObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class XtermTerminalView : UserControl, IxtermEventListener, ITerminalView
    {
        private readonly ManualResetEventSlim _connectedEvent;
        private readonly SemaphoreSlim _loaded;
        private bool _alreadyLoaded;
        private InputBuffer _buffer;
        private MenuFlyoutItem _copyMenuItem;
        private BlockingCollection<Action> _dispatcherJobs;
        private MenuFlyoutItem _pasteMenuItem;
        private IWebSocketConnection _webSocket;
        private WebView _webView;

        public XtermTerminalView()
        {
            _loaded = new SemaphoreSlim(0, 1);
            _connectedEvent = new ManualResetEventSlim(false);

            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };
                settings.Converters.Add(new StringEnumConverter(true));

                return settings;
            };

            InitializeComponent();

            StartMediatorTask();

            Loaded += XtermTerminalView_Loaded;
            GotFocus += XtermTerminalView_GotFocus;
        }

        public TerminalViewModel ViewModel { get; private set; }

        public Task ChangeKeyBindings()
        {
            var keyBindings = ViewModel.SettingsService.GetCommandKeyBindings();
            var profiles = ViewModel.SettingsService.GetShellProfiles();
            var serialized = JsonConvert.SerializeObject(FlattenKeyBindings(keyBindings, profiles));
            return ExecuteScriptAsync($"changeKeyBindings('{serialized}')");
        }

        public Task ChangeOptions(TerminalOptions options)
        {
            var serialized = JsonConvert.SerializeObject(options);
            return ExecuteScriptAsync($"changeOptions('{serialized}')");
        }

        public Task ChangeTheme(TerminalTheme theme)
        {
            var serialized = JsonConvert.SerializeObject(theme.Colors);
            return ExecuteScriptAsync($"changeTheme('{serialized}')");
        }

        public Task FindNext(string searchText)
        {
            return ExecuteScriptAsync($"findNext('{searchText}')");
        }

        public Task FindPrevious(string searchText)
        {
            return ExecuteScriptAsync($"findPrevious('{searchText}')");
        }

        public async Task FocusTerminal()
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();").ConfigureAwait(true);
            }
        }

        public async Task Initialize(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.Terminal.OutputReceived += Terminal_OutputReceived;
            ViewModel.Terminal.RegisterSelectedTextCallback(() => ExecuteScriptAsync("term.getSelection()"));
            ViewModel.Terminal.Closed += Terminal_Closed;

            var options = ViewModel.SettingsService.GetTerminalOptions();
            var keyBindings = ViewModel.SettingsService.GetCommandKeyBindings();
            var profiles = ViewModel.SettingsService.GetShellProfiles();
            var settings = ViewModel.SettingsService.GetApplicationSettings();
            var theme = ViewModel.TerminalTheme;
            var sessionType = SessionType.Unknown;
            if (settings.AlwaysUseWinPty || !ViewModel.ApplicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                sessionType = SessionType.WinPty;
            }
            else
            {
                sessionType = SessionType.ConPty;
            }
            await _loaded.WaitAsync().ConfigureAwait(true);
            var size = await CreateXtermView(options, theme.Colors, FlattenKeyBindings(keyBindings, profiles), sessionType).ConfigureAwait(true);
            var port = await ViewModel.TrayProcessCommunicationService.GetAvailablePort().ConfigureAwait(true);
            await CreateWebSocketServer(port.Port).ConfigureAwait(true);
            _connectedEvent.Wait();
            await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, size, sessionType).ConfigureAwait(true);
            _webView.Focus(FocusState.Programmatic);
        }

        void IxtermEventListener.OnKeyboardCommand(string command)
        {
            if (Enum.TryParse(command, true, out Command commandValue))
            {
                _dispatcherJobs.Add(() => ViewModel.Terminal.ProcessKeyboardCommand(commandValue.ToString()));
            }
            else if (Guid.TryParse(command, out Guid shellProfileId))
            {
                _dispatcherJobs.Add(() => ViewModel.Terminal.ProcessKeyboardCommand(shellProfileId.ToString()));
            }
        }

        void IxtermEventListener.OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection)
        {
            _dispatcherJobs.Add(() =>
            {
                if (mouseButton == MouseButton.Middle)
                {
                    if (ViewModel.ApplicationSettings.MouseMiddleClickAction == MouseAction.ContextMenu)
                    {
                        ShowContextMenu(x, y, hasSelection);
                    }
                    else if (ViewModel.ApplicationSettings.MouseMiddleClickAction == MouseAction.Paste)
                    {
                        ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
                    }
                }
                else if (mouseButton == MouseButton.Right)
                {
                    if (ViewModel.ApplicationSettings.MouseRightClickAction == MouseAction.ContextMenu)
                    {
                        ShowContextMenu(x, y, hasSelection);
                    }
                    else if (ViewModel.ApplicationSettings.MouseRightClickAction == MouseAction.Paste)
                    {
                        ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
                    }
                }
            });
        }

        void IxtermEventListener.OnSelectionChanged(string selection)
        {
            if (ViewModel.ApplicationSettings.CopyOnSelect && !ViewModel.ShowSearchPanel)
            {
                _dispatcherJobs.Add(async () =>
                {
                    ViewModel.CopyText(selection);
                    await ExecuteScriptAsync("term.clearSelection()");
                });
            }
        }

        void IxtermEventListener.OnTerminalResized(int columns, int rows)
        {
            if (_connectedEvent.IsSet) // only propagate after xterm.js is finished with fitting
            {
                _dispatcherJobs.Add(async () => await ViewModel.Terminal.SetSize(new TerminalSize { Columns = columns, Rows = rows }).ConfigureAwait(true));
            }
        }

        void IxtermEventListener.OnTitleChanged(string title)
        {
            _dispatcherJobs.Add(() => ViewModel.Terminal.SetTitle(title));
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

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Copy));
        }

        private async Task CreateWebSocketServer(int port)
        {
            var webSocketUrl = "ws://127.0.0.1:" + port;
            var webSocketServer = new WebSocketServer(webSocketUrl);
            webSocketServer.Start(socket =>
            {
                _webSocket = socket;
                _buffer = new InputBuffer(_webSocket.Send);
                socket.OnOpen = () => _connectedEvent.Set();
                socket.OnMessage = message => ViewModel.Terminal.Write(message);
            });

            await ExecuteScriptAsync($"connectToWebSocket('{webSocketUrl}');").ConfigureAwait(true);
        }

        private async Task<TerminalSize> CreateXtermView(TerminalOptions options, TerminalColors theme, IEnumerable<KeyBinding> keyBindings, SessionType sessionType)
        {
            var serializedOptions = JsonConvert.SerializeObject(options);
            var serializedTheme = JsonConvert.SerializeObject(theme);
            var serializedKeyBindings = JsonConvert.SerializeObject(keyBindings);
            var size = await ExecuteScriptAsync($"createTerminal('{serializedOptions}', '{serializedTheme}', '{serializedKeyBindings}', '{sessionType}')").ConfigureAwait(true);
            return JsonConvert.DeserializeObject<TerminalSize>(size);
        }

        private Task<string> ExecuteScriptAsync(string script)
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

        private IEnumerable<KeyBinding> FlattenKeyBindings(IDictionary<string, ICollection<KeyBinding>> commandKeyBindings, IEnumerable<ShellProfile> profiles)
        {
            return commandKeyBindings.Values.SelectMany(k => k).Concat(profiles.SelectMany(x => x.KeyBindings));
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
        }

        private void ShowContextMenu(int x, int y, bool terminalHasSelection)
        {
            var flyout = (MenuFlyout)_webView.ContextFlyout;
            _copyMenuItem.IsEnabled = terminalHasSelection;
            flyout.ShowAt(_webView, new Point(x, y));
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

        private async void Terminal_Closed(object sender, EventArgs e)
        {
            ViewModel.Terminal.OutputReceived -= Terminal_OutputReceived;
            ViewModel.Terminal.Closed -= Terminal_Closed;
            _webView?.Navigate(new Uri("about:blank"));
            _webView = null;
            await ViewModel.Terminal.Close().ConfigureAwait(true);
        }

        private void Terminal_OutputReceived(object sender, string e)
        {
            _webSocket.Send(e);
        }

        private async void XtermTerminalView_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();").ConfigureAwait(true);
            }
        }

        private void XtermTerminalView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_alreadyLoaded)
            {
                return;
            }

            if (Root.Children.Any())
            {
                return;
            }

            _webView = new WebView(WebViewExecutionMode.SeparateThread)
            {
                DefaultBackgroundColor = Colors.Transparent
            };
            Root.Children.Add(_webView);

            _webView.NavigationCompleted += _webView_NavigationCompleted;
            _webView.NavigationStarting += _webView_NavigationStarting;

            _copyMenuItem = new MenuFlyoutItem { Text = "Copy" };
            _copyMenuItem.Click += Copy_Click;

            _pasteMenuItem = new MenuFlyoutItem { Text = "Paste" };
            _pasteMenuItem.Click += Paste_Click;

            _webView.ContextFlyout = new MenuFlyout
            {
                Items = { _copyMenuItem, _pasteMenuItem }
            };

            _webView.Navigate(new Uri("ms-appx-web:///Client/index.html"));
            _alreadyLoaded = true;
        }
    }
}