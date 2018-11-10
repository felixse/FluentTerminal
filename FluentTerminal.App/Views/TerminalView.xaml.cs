using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels;
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
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TerminalView : UserControl, ITerminalView, ITerminalEventListener
    {
        private MenuFlyoutItem _copyMenuItem;
        private BlockingCollection<Action> _dispatcherJobs;
        private readonly SemaphoreSlim _loaded;
        private MenuFlyoutItem _pasteMenuItem;
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

        public event EventHandler<string> KeyboardCommandReceived;

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

        public Task ChangeKeyBindings(IEnumerable<KeyBinding> keyBindings)
        {
            var serialized = JsonConvert.SerializeObject(keyBindings);
            return ExecuteScriptAsync($"changeKeyBindings('{serialized}')");
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

        public async Task<TerminalSize> CreateTerminal(TerminalOptions options, TerminalColors theme, IEnumerable<KeyBinding> keyBindings)
        {
            var serializedOptions = JsonConvert.SerializeObject(options);
            var serializedTheme = JsonConvert.SerializeObject(theme);
            var serializedKeyBindings = JsonConvert.SerializeObject(keyBindings);
            var size = await ExecuteScriptAsync($"createTerminal('{serializedOptions}', '{serializedTheme}', '{serializedKeyBindings}')").ConfigureAwait(true);
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
                await ExecuteScriptAsync("document.focus();").ConfigureAwait(true);
            }
        }

        public async Task FocusWebView()
        {
            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                await ExecuteScriptAsync("document.focus();").ConfigureAwait(true);
            }
        }

        public Task<string> GetSelection() => ExecuteScriptAsync("term.getSelection()");

        public void OnKeyboardCommand(string command)
        {
            if (Enum.TryParse(command, true, out Command commandValue))
            {
                _dispatcherJobs.Add(() => KeyboardCommandReceived?.Invoke(this, commandValue.ToString()));
            }
            else if (Guid.TryParse(command, out Guid shellProfileId))
            {
                _dispatcherJobs.Add(() => KeyboardCommandReceived?.Invoke(this, shellProfileId.ToString()));
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

        public Task FindNext(string searchText)
        {
            return ExecuteScriptAsync($"findNext('{searchText}')");
        }

        public Task FindPrevious(string searchText)
        {
            return ExecuteScriptAsync($"findPrevious('{searchText}')");
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
            OnKeyboardCommand(nameof(Command.Copy));
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

            _copyMenuItem = new MenuFlyoutItem { Text = "Copy" };
            _copyMenuItem.Click += Copy_Click;

            _pasteMenuItem = new MenuFlyoutItem { Text = "Paste" };
            _pasteMenuItem.Click += Paste_Click;

            _webView.ContextFlyout = new MenuFlyout
            {
                Items = { _copyMenuItem, _pasteMenuItem }
            };

            _webView.Navigate(new Uri("ms-appx-web:///Client/index.html"));

            await _loaded.WaitAsync().ConfigureAwait(true);

            _webView.Focus(FocusState.Programmatic);

            await ViewModel.OnViewIsReady(this).ConfigureAwait(true);
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            OnKeyboardCommand(nameof(Command.Paste));
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

        private void SearchTextBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                ViewModel.CloseSearchPanelCommand.Execute(null);
            }
            else if (e.Key == VirtualKey.Enter)
            {
                ViewModel.FindNextCommand.Execute(null);
            }
        }

        public void FocusSearchTextBox()
        {
            SearchTextBox.Focus(FocusState.Programmatic);
        }

        public void OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection)
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
                        OnKeyboardCommand(nameof(Command.Paste));
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
                        OnKeyboardCommand(nameof(Command.Paste));
                    }
                }
            });
        }

        public async void OnSelectionChanged(string selection)
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

        private void ShowContextMenu(int x, int y, bool terminalHasSelection)
        {
            var flyout = (MenuFlyout)_webView.ContextFlyout;
            _copyMenuItem.IsEnabled = terminalHasSelection;
            flyout.ShowAt(_webView, new Point(x, y));
        }
    }
}