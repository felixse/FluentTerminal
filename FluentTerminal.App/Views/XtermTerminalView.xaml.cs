using FluentTerminal.App.Converters;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using FluentTerminal.RuntimeComponent.Enums;
using FluentTerminal.RuntimeComponent.Interfaces;
using FluentTerminal.RuntimeComponent.WebAllowedObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ISessionSuccessTracker = FluentTerminal.App.ViewModels.ISessionSuccessTracker;

namespace FluentTerminal.App.Views
{
    public sealed partial class XtermTerminalView : UserControl, IxtermEventListener, ITerminalView
    {
        private readonly ManualResetEventSlim _connectedEvent;
        private readonly SemaphoreSlim _navigationCompleted;
        private readonly MenuFlyoutItem _copyMenuItem;
        private BlockingCollection<Action> _dispatcherJobs;
        private readonly MenuFlyoutItem _pasteMenuItem;
        private WebView _webView;
        private readonly DelayedAction<TerminalOptions> _optionsChanged;
        private CancellationTokenSource _mediatorTaskCTSource;

        // Members related to resize handling
        private readonly DelayedAction<TerminalSize> _sizeChanged;
        private ManualResetEventSlim _outputBlocked;
        private MemoryStream _outputBlockedBuffer;
        private readonly DelayedAction<bool> _unblockOutput;
        private TerminalBridge _terminalBridge;

        public event EventHandler<object> OnOutput;

        public XtermTerminalView()
        {
            InitializeComponent();
            StartMediatorTask();

            _webView = new WebView(WebViewExecutionMode.SeparateThread)
            {
                DefaultBackgroundColor = Colors.Transparent
            };
            Root.Children.Add(_webView);

            _webView.NavigationCompleted += _webView_NavigationCompleted;
            _webView.NavigationStarting += _webView_NavigationStarting;
            _webView.NewWindowRequested += _webView_NewWindowRequested;

            _copyMenuItem = new MenuFlyoutItem { Text = I18N.Translate("Command.Copy") };
            _copyMenuItem.Click += Copy_Click;

            _pasteMenuItem = new MenuFlyoutItem { Text = I18N.Translate("Command.Paste") };
            _pasteMenuItem.Click += Paste_Click;

            _webView.ContextFlyout = new MenuFlyout
            {
                Items = { _copyMenuItem, _pasteMenuItem }
            };

            _optionsChanged = new DelayedAction<TerminalOptions>(async options =>
            {
                var serialized = JsonConvert.SerializeObject(options);
                await ExecuteScriptAsync($"changeOptions('{serialized}')");
            }, 800, Dispatcher);

            // _sizedChanged is used to debounce terminal resize events to
            // avoid spamming the terminal with them (this can result in
            // buffer corruption).
            _sizeChanged = new DelayedAction<TerminalSize>(async size => {
                await ViewModel.Terminal.SetSize(size).ConfigureAwait(true);

                // Allow output to the terminal soon (hopefully, once the resize event has been processed).
                await _unblockOutput.InvokeAsync(true);
            }, 1000, Dispatcher);

            _outputBlockedBuffer = new MemoryStream();
            _outputBlocked = new ManualResetEventSlim();

            // _unblockOutput allows output to the terminal again, 500ms after it invoked.
            _unblockOutput = new DelayedAction<bool>(x => {
                _outputBlocked.Reset();
            }, 500, Dispatcher);

            _navigationCompleted = new SemaphoreSlim(0, 1);
            _connectedEvent = new ManualResetEventSlim(false);

            _webView.Navigate(new Uri("ms-appx-web:///Client/index.html"));
        }

        public TerminalViewModel ViewModel { get; private set; }

        public Task ChangeKeyBindings()
        {
            var keyBindings = ViewModel.SettingsService.GetCommandKeyBindings();
            var profiles = ViewModel.SettingsService.GetShellProfiles();
            var sshprofiles = ViewModel.SettingsService.GetSshProfiles();
            var serialized = JsonConvert.SerializeObject(FlattenKeyBindings(keyBindings, profiles, sshprofiles));
            return ExecuteScriptAsync($"changeKeyBindings('{serialized}')");
        }

        public Task ChangeOptions(TerminalOptions options)
        {
            return _optionsChanged.InvokeAsync(options);
        }

        public Task ChangeTheme(TerminalTheme theme)
        {
            var serialized = JsonConvert.SerializeObject(theme.Colors);
            return ExecuteScriptAsync($"changeTheme('{serialized}')");
        }

        public async Task<string> SerializeXtermState()
        {
            return await ExecuteScriptAsync(@"serializeTerminal()");
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
            var sshprofiles = ViewModel.SettingsService.GetSshProfiles();
            var theme = ViewModel.TerminalTheme;
            SessionType sessionType;
            if (ViewModel.ShellProfile.UseConPty && ViewModel.ApplicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                sessionType = SessionType.ConPty;
            }
            else
            {
                sessionType = SessionType.WinPty;
            }
            await _navigationCompleted.WaitAsync().ConfigureAwait(true);
            var size = await CreateXtermView(options, theme.Colors, FlattenKeyBindings(keyBindings, profiles, sshprofiles)).ConfigureAwait(true);
            _connectedEvent.Wait();

            var response = await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, size, sessionType, ViewModel.XtermBufferState).ConfigureAwait(true);
            if (!response.Success)
            {
                await ViewModel.DialogService.ShowMessageDialogAsnyc("Error", response.Error, DialogButton.OK).ConfigureAwait(true);
                ViewModel.Terminal.ReportLauchFailed();
                return;
            }

            if (ViewModel.ShellProfile?.Tag is ISessionSuccessTracker tracker)
            {
                tracker.SetSuccessfulSessionStart();
            }

            _webView.Focus(FocusState.Programmatic);
        }

        public void DisposalPrepare()
        {
            if (_terminalBridge != null)
            {
                _terminalBridge.DisposalPrepare();
                _terminalBridge = null;
            }
            _optionsChanged.Dispose();
            _sizeChanged.Dispose();
            _unblockOutput.Dispose();
            ViewModel = null;
        }

        void IxtermEventListener.OnKeyboardCommand(string command)
        {
            Logger.Instance.Debug("Received keyboard command: '{command}'", command);

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
            if (!string.IsNullOrEmpty(selection) && ViewModel.ApplicationSettings.CopyOnSelect && !ViewModel.ShowSearchPanel)
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
                // Prevent output from being sent during the terminal while
                // resize events are being processed (to avoid buffer corruption).
                _outputBlocked.Set();
                _dispatcherJobs.Add(() => _sizeChanged.InvokeAsync(new TerminalSize { Columns = columns, Rows = rows }));
            }
        }

        void IxtermEventListener.OnInitialized()
        {
            _connectedEvent.Set();
        }

        void IxtermEventListener.OnTitleChanged(string title)
        {
            _dispatcherJobs.Add(() => ViewModel.Terminal.SetTitle(title));
        }

        private void _webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Logger.Instance.Debug("WebView navigation completed. Target: {uri}", args.Uri);
            _navigationCompleted.Release();
        }

        private void _webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            _terminalBridge = new TerminalBridge(this);
            _webView.AddWebAllowedObject("terminalBridge", _terminalBridge);
        }

        private void _webView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            Launcher.LaunchUriAsync(args.Uri);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Copy));
        }

        private async Task<TerminalSize> CreateXtermView(TerminalOptions options, TerminalColors theme, IEnumerable<KeyBinding> keyBindings)
        {
            var serializedOptions = JsonConvert.SerializeObject(options);
            var serializedTheme = JsonConvert.SerializeObject(theme);
            var serializedKeyBindings = JsonConvert.SerializeObject(keyBindings);
            var size = await ExecuteScriptAsync($"createTerminal('{serializedOptions}', '{serializedTheme}', '{serializedKeyBindings}')").ConfigureAwait(true);
            return JsonConvert.DeserializeObject<TerminalSize>(size);
        }

        private async Task<string> ExecuteScriptAsync(string script)
        {
            try
            {
                return await _webView.InvokeScriptAsync("eval", new[] { script }).AsTask();
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Exception while running:\n \"{script}\"\n\n {e}");
            }
            return await Task.FromResult(string.Empty);
        }

        private IEnumerable<KeyBinding> FlattenKeyBindings(IDictionary<string, ICollection<KeyBinding>> commandKeyBindings, IEnumerable<ShellProfile> profiles, IEnumerable<SshProfile> sshprofiles)
        {
            return commandKeyBindings.Values.SelectMany(k => k).Concat(profiles.SelectMany(x => x.KeyBindings).Concat(sshprofiles.SelectMany(x => x.KeyBindings)));
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
            _mediatorTaskCTSource = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    _mediatorTaskCTSource.Token.ThrowIfCancellationRequested();
                    foreach (var job in _dispatcherJobs.GetConsumingEnumerable(_mediatorTaskCTSource.Token))
                    {
                        _mediatorTaskCTSource.Token.ThrowIfCancellationRequested();
                        try
                        {
                            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => job.Invoke());
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }
                catch (OperationCanceledException) { }
            }, TaskCreationOptions.LongRunning);
        }

        private async void Terminal_Closed(object sender, EventArgs e)
        {
            ViewModel.Terminal.Closed -= Terminal_Closed;
            ViewModel.Terminal.OutputReceived -= Terminal_OutputReceived;
            ViewModel.Terminal.RegisterSelectedTextCallback(null);
            _mediatorTaskCTSource.Cancel();
            await ViewModel.ApplicationView.DispatchAsync(() =>
            {
                _webView.NavigationCompleted -= _webView_NavigationCompleted;
                _webView.NavigationStarting -= _webView_NavigationStarting;
                _webView.NewWindowRequested -= _webView_NewWindowRequested;

                _webView?.Navigate(new Uri("about:blank"));
                Root.Children.Remove(_webView);
                _webView = null;

                _copyMenuItem.Click -= Copy_Click;
                _pasteMenuItem.Click -= Paste_Click;
                
                if (Window.Current.Content is Frame frame && frame.Content is Page mainPage)
                {
                    if (mainPage.Resources["TerminalViewModelToViewConverter"] is TerminalViewModelToViewConverter converter)
                    {
                        converter.RemoveTerminal(ViewModel);
                    }
                }
            });
        }

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
            if (_outputBlocked.IsSet)
            {
                // Output to the terminal is currently blocked. Hold on to the output.
                _outputBlockedBuffer.Write(e, 0, e.Length);
            }
            else
            {
                // Output to the terminal is not blocked. Send any previously
                // buffered output first, and then the output for the current
                // event.
                if (_outputBlockedBuffer.Length > 0) {
                    OnOutput?.Invoke(this, _outputBlockedBuffer.ToArray());
                    _outputBlockedBuffer.SetLength(0);
                }
                OnOutput?.Invoke(this, e);
            }
        }

        void IxtermEventListener.OnError(string error)
        {
            Logger.Instance.Error(error);
        }

        public void OnInput(string text)
        {
            ViewModel.Terminal.Write(Encoding.UTF8.GetBytes(text));
        }

        public void Dispose()
        {
            _connectedEvent?.Dispose();
            _navigationCompleted?.Dispose();
            _dispatcherJobs?.Dispose();
            _mediatorTaskCTSource?.Dispose();
            _outputBlocked?.Dispose();
            _outputBlockedBuffer?.Dispose();
        }

        public Task PasteAsync(string text) => ExecuteScriptAsync(
                $"window.term.paste('{text.Replace(@"\", @"\\").Replace("'", @"\'").Replace("\n", @"\n").Replace("\r", @"\r")}')");
    }
}