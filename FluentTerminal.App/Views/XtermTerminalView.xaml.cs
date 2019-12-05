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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class XtermTerminalView : UserControl, IxtermEventListener, ITerminalView
    {
        private readonly MenuFlyoutItem _copyMenuItem;
        private readonly MenuFlyoutItem _pasteMenuItem;
        private WebView _webView;
        private readonly DelayedAction<TerminalOptions> _optionsChanged;
        private TerminalBridge _terminalBridge;

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
                    await Task.Delay(delay);
                }
                else
                {
                    await ViewModel.Terminal.SetSize(_requestedSize);
                }
            }
        }

        #endregion Resize handling

        public event EventHandler<object> OnOutput;

        public XtermTerminalView()
        {
            InitializeComponent();

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
            }, 100);

            _webView.Navigate(new Uri("ms-appx-web:///Client/index.html"));
        }

        public TerminalViewModel ViewModel { get; private set; }

        public Task ChangeKeyBindings()
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

        public Task ChangeOptions(TerminalOptions options)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return _optionsChanged.InvokeAsync(options);
        }

        public Task ChangeTheme(TerminalTheme theme)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            var serialized = JsonConvert.SerializeObject(theme.Colors);
            return ExecuteScriptAsync($"changeTheme('{serialized}')");
        }

        public Task<string> SerializeXtermState()
        {
            if (_terminalClosed)
            {
                return Task.FromResult(string.Empty);
            }

            return ExecuteScriptAsync(@"serializeTerminal()");
        }

        public Task FindNext(string searchText)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return ExecuteScriptAsync($"findNext('{searchText}')");
        }

        public Task FindPrevious(string searchText)
        {
            if (_terminalClosed)
            {
                return Task.CompletedTask;
            }

            return ExecuteScriptAsync($"findPrevious('{searchText}')");
        }

        public Task FocusTerminal()
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

            // Waiting for WebView.NavigationCompleted event to happen
            await _tcsNavigationCompleted.Task.ConfigureAwait(true);

            var size = await CreateXtermView(options, theme.Colors,
                FlattenKeyBindings(keyBindings, profiles, sshprofiles)).ConfigureAwait(true);

            // Waiting for IxtermEventListener.OnInitialized() call to happen
            await _tcsConnected.Task;

            var sessionType =
                ViewModel.ShellProfile.UseConPty &&
                ViewModel.ApplicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)
                    ? SessionType.ConPty
                    : SessionType.WinPty;

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

            var response = await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, size, sessionType, ViewModel.XtermBufferState).ConfigureAwait(true);
            if (!response.Success)
            {
                await ViewModel.DialogService.ShowMessageDialogAsnyc("Error", response.Error, DialogButton.OK).ConfigureAwait(true);
                ViewModel.Terminal.ReportLauchFailed();
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

        void IxtermEventListener.OnMouseClick(MouseButton mouseButton, int x, int y, bool hasSelection)
        {
            if (_terminalClosed)
            {
                return;
            }

            if (mouseButton == MouseButton.Middle)
            {
                if (ViewModel.ApplicationSettings.MouseMiddleClickAction == MouseAction.ContextMenu)
                {
                    Dispatcher.ExecuteAsync(() => ShowContextMenu(x, y, hasSelection), enforceNewSchedule: true);
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
                    Dispatcher.ExecuteAsync(() => ShowContextMenu(x, y, hasSelection), enforceNewSchedule: true);
                }
                else if (ViewModel.ApplicationSettings.MouseRightClickAction == MouseAction.Paste)
                {
                    ((IxtermEventListener)this).OnKeyboardCommand(nameof(Command.Paste));
                }
            }
        }

        void IxtermEventListener.OnSelectionChanged(string selection)
        {
            if (_terminalClosed)
            {
                return;
            }

            if (!string.IsNullOrEmpty(selection) && ViewModel.ApplicationSettings.CopyOnSelect && !ViewModel.ShowSearchPanel)
            {
                ViewModel.CopyText(selection);
                // ReSharper disable once AssignmentIsFullyDiscarded
                _ = ExecuteScriptAsync("term.clearSelection()");
            }
        }

        void IxtermEventListener.OnTerminalResized(int columns, int rows)
        {
            if (_terminalClosed)
            {
                return;
            }

            var size = new TerminalSize {Columns = columns, Rows = rows};

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

        private void _webView_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Logger.Instance.Debug("WebView navigation completed. Target: {uri}", args.Uri);
            _tcsNavigationCompleted.TrySetResult(null);
        }

        private void _webView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            _terminalBridge = new TerminalBridge(this);
            _webView.AddWebAllowedObject("terminalBridge", _terminalBridge);
        }

        private void _webView_NewWindowRequested(WebView sender, WebViewNewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Launcher.LaunchUriAsync(args.Uri);
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
            if (_terminalClosed)
            {
                return string.Empty;
            }

            try
            {
                var scriptTask =
                    await Dispatcher.ExecuteAsync(() => _webView.InvokeScriptAsync("eval", new[] {script}));

                return await scriptTask;
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

        private void Terminal_Closed(object sender, EventArgs e)
        {
            _terminalClosed = true;

            ViewModel.Terminal.Closed -= Terminal_Closed;
            ViewModel.Terminal.OutputReceived -= Terminal_OutputReceived;
            ViewModel.Terminal.RegisterSelectedTextCallback(null);

            ViewModel.ApplicationView.ExecuteOnUiThreadAsync(() =>
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

        public void OnInput(string text)
        {
            if (_terminalClosed)
            {
                return;
            }

            ViewModel.Terminal.Write(Encoding.UTF8.GetBytes(text));
        }

        public void Dispose()
        {
            _outputBlockedBuffer?.Dispose();
        }

        public Task PasteAsync(string text) => ExecuteScriptAsync(
                $"window.term.paste('{text.Replace(@"\", @"\\").Replace("'", @"\'").Replace("\n", @"\n").Replace("\r", @"\r")}')");
    }
}