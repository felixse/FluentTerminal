using FluentTerminal.App.Converters;
using FluentTerminal.App.Services;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using FluentTerminal.Model;
using FluentTerminal.Model.Enums;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class XtermTerminalView : UserControl, IxtermEventListener, ITerminalView
    {
        private WebView2 _webView;
        private readonly DelayedAction<TerminalOptions> _optionsChanged;
        //private TerminalBridge _terminalBridge;

        private volatile bool _terminalClosed;

        // Members related to initialization
        private readonly TaskCompletionSource<object> _tcsConnected = new TaskCompletionSource<object>();
        private readonly TaskCompletionSource<object> _tcsNavigationCompleted = new TaskCompletionSource<object>();

        #region Resize handling

        // Members related to resize handling
        private static readonly TimeSpan ResizeDelay = TimeSpan.FromMilliseconds(60);
        private readonly object _resizeLock = new object();
        private TerminalSize _requestedSize;
        private TerminalSize _setSize;
        private DateTime _resizeScheduleTime;
        private Task _resizeTask;
        private MemoryStream _outputBlockedBuffer;

        // Must be called from a code locked with _resizeLock
        private void ScheduleResize(TerminalSize size, bool scheduleIfEqual)
        {
            if (!scheduleIfEqual && size.EquivalentTo(_requestedSize))
            {
                return;
            }

            _requestedSize = size;
            _resizeScheduleTime = DateTime.UtcNow.Add(ResizeDelay);

            if (_resizeTask == null)
            {
                _resizeTask = ResizeTask();
            }
        }

        private async Task ResizeTask()
        {
            while (true)
            {
                TimeSpan delay;
                TerminalSize size = null;

                lock (_resizeLock)
                {
                    if (_requestedSize?.EquivalentTo(_setSize) ?? true)
                    {
                        // Resize finished. Unblock output and exit.

                        if (_outputBlockedBuffer != null)
                        {
                            OnOutput?.Invoke(this, _outputBlockedBuffer.ToArray());

                            _outputBlockedBuffer.Dispose();
                            _outputBlockedBuffer = null;
                        }

                        _resizeTask = null;

                        break;
                    }

                    delay = _resizeScheduleTime.Subtract(DateTime.UtcNow);

                    // To avoid sleeping for only few milliseconds we're introducing a threshold of 10 milliseconds
                    if (delay.TotalMilliseconds < 10)
                    {
                        _setSize = size = _requestedSize;

                        if (_outputBlockedBuffer == null)
                        {
                            _outputBlockedBuffer = new MemoryStream();
                        }
                    }
                }

                if (size == null)
                {
                    await Task.Delay(delay).ConfigureAwait(false);
                }
                else
                {
                    await ViewModel.Terminal.SetSizeAsync(_requestedSize).ConfigureAwait(false);
                }
            }
        }

        #endregion Resize handling

        public event EventHandler<object> OnOutput;
        public event EventHandler<string> OnPaste;
        public event EventHandler<string> OnSessionRestart;

        public XtermTerminalView()
        {
            InitializeComponent();

            //_webView = new WebView2();
            //_webView.CoreWebView2Initialized += _webView_CoreWebView2Initialized;
            //Root.Children.Add(_webView);

            //_webView.NavigationCompleted += _webView_NavigationCompleted;
            //_webView.NavigationStarting += _webView_NavigationStarting;
            //_webView.NewWindowRequested += _webView_NewWindowRequested;

            _optionsChanged = new DelayedAction<TerminalOptions>(async options =>
            {
                var serialized = JsonConvert.SerializeObject(options);
                await ExecuteScriptAsync($"changeOptions('{serialized}')");
            }, 100);

            
        }

        private void _webView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            _webView.CoreWebView2.OpenDevToolsWindow();
            _webView.CoreWebView2.Navigate("ms-appx-web:///Client/index.html");
        }

        public TerminalViewModel ViewModel { get; private set; }

        public Task ChangeKeyBindingsAsync()
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            var keyBindings = ViewModel.SettingsService.GetCommandKeyBindings();
            var profiles = ViewModel.SettingsService.GetShellProfiles();
            var sshprofiles = ViewModel.SettingsService.GetSshProfiles();
            var serialized = JsonConvert.SerializeObject(FlattenKeyBindings(keyBindings, profiles, sshprofiles));
            return ExecuteScriptAsync($"changeKeyBindings('{serialized}')");
        }

        public Task ChangeOptionsAsync(TerminalOptions options)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return _optionsChanged.InvokeAsync(options);
        }

        public Task ChangeThemeAsync(TerminalTheme theme)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            var serialized = JsonConvert.SerializeObject(theme.Colors);
            return ExecuteScriptAsync($"changeTheme('{serialized}')");
        }

        public Task ChangeFontSize(int fontSize)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return ExecuteScriptAsync($"setFontSize({fontSize})");
        }

        public Task<string> SerializeXtermStateAsync()
        {
            if (_terminalClosed)
            {
                return Task.FromResult(string.Empty);
            }

            return ExecuteScriptAsync(@"serializeTerminal()");
        }

        public Task FindNextAsync(SearchRequest request)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return ExecuteScriptAsync($"findNext('{request.Term}', {request.MatchCase.ToString().ToLower()}, {request.WholeWord.ToString().ToLower()}, {request.Regex.ToString().ToLower()})");
        }

        public Task FindPreviousAsync(SearchRequest request)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return ExecuteScriptAsync($"findPrevious('{request.Term}', {request.MatchCase.ToString().ToLower()}, {request.WholeWord.ToString().ToLower()}, {request.Regex.ToString().ToLower()})");
        }

        public Task FocusTerminalAsync()
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            if (_webView != null)
            {
                _webView.Focus(FocusState.Programmatic);
                return ExecuteScriptAsync("document.focus();");
            }

            return Task.CompletedTask;
        }

        private const string OpenSSH = "OpenSSH";

        public async Task ReconnectAsync()
        {
            if (ViewModel.ShellProfile.Location.Contains(OpenSSH))
            {
                OnSessionRestart?.Invoke(this, OpenSSH);

                await Task.Delay(500).ConfigureAwait(false);

                Terminal_OutputReceived(this, System.Text.Encoding.UTF8.GetBytes("\n\x1b[2EReconnecting current session...\n\x1b[E"));
            }
            else
            {
                // Fill full screen with new output to not allow erasing of available output on start of new shell
                Terminal_OutputReceived(this, System.Text.Encoding.UTF8.GetBytes(new String('\n', _requestedSize.Rows + 1)));
            }

            if (true == await StartShellProcessAsync(_requestedSize).ConfigureAwait(false))
            {
                ViewModel.Terminal.Reconnect();
            }
        }

        public async Task InitializeAsync(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.Terminal.OutputReceived += Terminal_OutputReceived;
            ViewModel.Terminal.RegisterSelectedTextCallback(() => ExecuteScriptAsync("term.getSelection()"));
            ViewModel.Terminal.Closed += Terminal_Closed;

            await WebView.EnsureCoreWebView2Async();
            WebView.CoreWebView2.OpenDevToolsWindow();
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "appassets.example", "Client",
            Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow);

            WebView.Source = new Uri("https://appassets.example/index.html");
                                             //WebView.CoreWebView2.Navigate("ms-appx-web:///Client/index.html");
                //WebView.CoreWebView2.Navigate("https://xtermjs.org/");

            //_webView.SetBinding(ContextFlyoutProperty, new Binding
            //{
            //    Converter = (IValueConverter)Application.Current.Resources["MenuViewModelToFlyoutMenuConverter"],
            //    Mode = BindingMode.OneWay,
            //    Source = ViewModel,
            //    Path = new PropertyPath(nameof(TerminalViewModel.ContextMenu))
            //});

            var options = ViewModel.SettingsService.GetTerminalOptions();
            var keyBindings = ViewModel.SettingsService.GetCommandKeyBindings();
            var profiles = ViewModel.SettingsService.GetShellProfiles();
            var sshprofiles = ViewModel.SettingsService.GetSshProfiles();
            var theme = ViewModel.TerminalTheme;

            // Waiting for WebView.NavigationCompleted event to happen
            await _tcsNavigationCompleted.Task.ConfigureAwait(false);

            var size = await CreateXtermViewAsync(options, theme.Colors,
                FlattenKeyBindings(keyBindings, profiles, sshprofiles)).ConfigureAwait(false);

            // Waiting for IxtermEventListener.OnInitialized() call to happen
            await _tcsConnected.Task;

            lock (_resizeLock)
            {
                // Check to see if some resizing has happened meanwhile
                if (_requestedSize != null)
                {
                    size = _requestedSize;
                }
                else
                {
                    _requestedSize = size;
                }
            }

            if (false == await StartShellProcessAsync(size).ConfigureAwait(false))
            {
                return;
            }

            lock (_resizeLock)
            {
                // Check to see if some resizing has happened meanwhile
                if (!size.EquivalentTo(_requestedSize))
                {
                    ScheduleResize(_requestedSize, true);
                }
                else
                {
                    _setSize = size;
                }
            }

            if (ViewModel.ShellProfile?.Tag is ISessionSuccessTracker tracker)
            {
                tracker.SetSuccessfulSessionStart();
            }

            await Dispatcher.ExecuteAsync(() => _webView.Focus(FocusState.Programmatic)).ConfigureAwait(false);
        }

        public void DisposalPrepare()
        {
            //if (_terminalBridge != null)
            //{
            //    _terminalBridge.DisposalPrepare();
            //    _terminalBridge = null;
            //}
            _optionsChanged.Dispose();
            ViewModel = null;
        }

        void IxtermEventListener.OnKeyboardCommand(string command)
        {
            if (_terminalClosed)
            {
                return;
            }

            Logger.Instance.Debug("Received keyboard command: '{command}'", command);

            if (Enum.TryParse(command, true, out Command commandValue))
            {
                ViewModel.Terminal.ProcessKeyboardCommand(commandValue.ToString());
            }
            else if (Guid.TryParse(command, out Guid shellProfileId))
            {
                ViewModel.Terminal.ProcessKeyboardCommand(shellProfileId.ToString());
            }
        }

        void IxtermEventListener.OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection, string hoveredUri)
        {
            if (_terminalClosed)
            {
                return;
            }

            var action = MouseAction.None;

            switch (mouseButton)
            {
                case MouseButton.Middle:
                    action = ViewModel.ApplicationSettings.MouseMiddleClickAction;
                    break;
                case MouseButton.Right:
                    action = ViewModel.ApplicationSettings.MouseRightClickAction;
                    break;
            }

            if (action == MouseAction.ContextMenu)
            {
                Dispatcher.ExecuteAsync(() => ShowContextMenu(x, y, hasSelection, hoveredUri), enforceNewSchedule: true);
            }
            else if (action == MouseAction.Paste)
            {
                ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
            }
            else if (action == MouseAction.CopySelectionOrPaste)
            {
                if (hasSelection)
                {
                    ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Copy));
                }
                else
                {
                    ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
                }
            }
        }

        async void IxtermEventListener.OnSelectionChanged(string selection)
        {
            if (_terminalClosed)
            {
                return;
            }

            if (!string.IsNullOrEmpty(selection) && ViewModel.ApplicationSettings.CopyOnSelect && !ViewModel.ShowSearchPanel)
            {
                await ViewModel.CopyTextAsync(selection).ConfigureAwait(false);
                await ExecuteScriptAsync("term.clearSelection()").ConfigureAwait(false);
            }
        }

        void IxtermEventListener.OnTerminalResized(int columns, int rows)
        {
            if (_terminalClosed)
            {
                return;
            }

            var size = new TerminalSize { Columns = columns, Rows = rows };

            lock (_resizeLock)
            {
                if (_setSize == null)
                {
                    // Initialization not finished yet
                    _requestedSize = size;
                }
                else
                {
                    ScheduleResize(size, false);
                }
            }
        }

        void IxtermEventListener.OnInitialized()
        {
            _tcsConnected.TrySetResult(null);
        }

        void IxtermEventListener.OnTitleChanged(string title)
        {
            if (_terminalClosed)
            {
                return;
            }

            ViewModel.Terminal.SetTitle(title);
        }

        //private void _webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        //{
        //    Logger.Instance.Debug("WebView navigation completed. Target: {uri}", args.Uri);
        //    _tcsNavigationCompleted.TrySetResult(null);
        //}

        //private void _webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        //{
        //    _terminalBridge = new TerminalBridge(this);
        //    _webView.AddWebAllowedObject("terminalBridge", _terminalBridge);
        //}

        //private void _webView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        //{
        //    args.Handled = true;
        //    // ReSharper disable once AssignmentIsFullyDiscarded
        //    _ = Launcher.LaunchUriAsync(args.Uri);
        //}

        private Task<TerminalSize> CreateXtermViewAsync(TerminalOptions options, TerminalColors theme, IEnumerable<KeyBinding> keyBindings)
        {
            var serializedOptions = JsonConvert.SerializeObject(options);
            var serializedTheme = JsonConvert.SerializeObject(theme);
            var serializedKeyBindings = JsonConvert.SerializeObject(keyBindings);
            return ExecuteScriptAsync(
                    $"createTerminal('{serializedOptions}', '{serializedTheme}', '{serializedKeyBindings}')")
                .ContinueWith(t => JsonConvert.DeserializeObject<TerminalSize>(t.Result));
        }

        private async Task<string> ExecuteScriptAsync(string script)
        {
            if (_terminalClosed)
            {
                return string.Empty;
            }

            try
            {
                //var scriptTask = await Dispatcher.ExecuteAsync(() => _webView.InvokeScriptAsync("eval", new[] { script }))
                //    .ConfigureAwait(false);


                //return await scriptTask;
            }
            catch (Exception e)
            {
                Logger.Instance.Error($"Exception while running:\n \"{script}\"\n\n {e}");
            }

            return string.Empty;
        }

        private IEnumerable<KeyBinding> FlattenKeyBindings(IDictionary<string, ICollection<KeyBinding>> commandKeyBindings, IEnumerable<ShellProfile> profiles, IEnumerable<SshProfile> sshprofiles)
        {
            return commandKeyBindings.Values.SelectMany(k => k).Concat(profiles.SelectMany(x => x.KeyBindings).Concat(sshprofiles.SelectMany(x => x.KeyBindings)));
        }

        private void ShowContextMenu(int x, int y, bool terminalHasSelection, string hoveredUri)
        {
            ViewModel.HoveredUri = hoveredUri;
            ViewModel.HasSelection = terminalHasSelection;
            var flyout = (MenuFlyout)_webView.ContextFlyout;
            flyout.ShowAt(_webView, new Point(x, y));
        }

        private void Terminal_Closed(object sender, EventArgs e)
        {
            _terminalClosed = true;

            ViewModel.Terminal.Closed -= Terminal_Closed;
            ViewModel.Terminal.OutputReceived -= Terminal_OutputReceived;
            ViewModel.Terminal.RegisterSelectedTextCallback(null);

            //ViewModel.ApplicationView.ExecuteOnUiThreadAsync(() =>
            //{
            //    _webView.NavigationCompleted -= _webView_NavigationCompleted;
            //    _webView.NavigationStarting -= _webView_NavigationStarting;
            //    _webView.NewWindowRequested -= _webView_NewWindowRequested;

            //    _webView?.Navigate(new Uri("about:blank"));
            //    Root.Children.Remove(_webView);
            //    _webView = null;

            //    if (Window.Current.Content is Frame frame && frame.Content is Page mainPage)
            //    {
            //        if (mainPage.Resources["TerminalViewModelToViewConverter"] is TerminalViewModelToViewConverter converter)
            //        {
            //            converter.RemoveTerminal(ViewModel);
            //        }
            //    }
            //});
        }

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
            if (_terminalClosed)
            {
                return;
            }

            lock (_resizeLock)
            {
                if (_outputBlockedBuffer != null)
                {
                    _outputBlockedBuffer.Write(e, 0, e.Length);

                    return;
                }
            }

            OnOutput?.Invoke(this, e);
        }

        void IxtermEventListener.OnError(string error)
        {
            Logger.Instance.Error(error);
        }

        public void OnInput(byte[] data)
        {
            if (_terminalClosed)
            {
                return;
            }

            ViewModel.Terminal.Write(data);
        }

        public void Dispose()
        {
            _outputBlockedBuffer?.Dispose();
        }

        public void Paste(string text) => OnPaste?.Invoke(this, text);

        private async Task<bool> StartShellProcessAsync(TerminalSize size)
        {
            var sessionType = ViewModel.ShellProfile.UseConPty &&
                true //ViewModel.ApplicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)
                    ? SessionType.ConPty
                    : SessionType.WinPty;

            var response = await ViewModel.Terminal.StartShellProcessAsync(ViewModel.ShellProfile, size, sessionType, ViewModel.XtermBufferState).ConfigureAwait(false);
            if (!response.Success)
            {
                await Dispatcher.ExecuteAsync(async () =>
                        await ViewModel.DialogService.ShowMessageDialogAsync("Error", response.Error, DialogButton.OK))
                    .ConfigureAwait(false);

                ViewModel.Terminal.ReportLauchFailed();
                return false;
            }
            return true;
        }
    }
}