using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views.NativeFrontend.Ansi;
using FluentTerminal.App.Views.NativeFrontend.Terminal;
using FluentTerminal.App.Views.NativeFrontend.Utility;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace FluentTerminal.App.Views.NativeFrontend
{
    public sealed partial class TerminalControl : UserControl, ITerminalView
    {
        private TerminalBuffer _buffer;
        private double _characterHeight;
        private double _characterWidth;
        private TerminalColorHelper _colorHelper = new TerminalColorHelper();
        private readonly TerminalOptions _terminalOptions;
        private int _cols;
        private CanvasTextFormat _format;
        private Ansi.AnsiParser _parser;
        private int _rows;
        private int _gridSize = 150;
        private CoreDispatcher _dispatcher;

        public TerminalControl(ISettingsService settingsService)
        {
            InitializeComponent();
            canvas.KeyUp += _canvas_KeyUp;
            canvas.GotFocus += _canvas_GotFocus;
            canvas.CharacterReceived += Canvas_CharacterReceived;

            _terminalOptions = settingsService.GetTerminalOptions();
            _dispatcher = Window.Current.CoreWindow.Dispatcher;
        }

        int regionsInvalidatedEventCount = 0;
        int regionsInvalidatedCount = 0;

        private void OnRegionsInvalidated(CanvasVirtualControl sender, CanvasRegionsInvalidatedEventArgs args)
        {
            ++regionsInvalidatedEventCount;

            var invalidatedRegions = args.InvalidatedRegions;
            regionsInvalidatedCount += invalidatedRegions.Count();

            foreach (var region in invalidatedRegions)
            {
                DrawRegion(sender, region);
            }
        }

        private void DrawRegion(CanvasVirtualControl sender, Rect region)
        {
            var top = (int)(region.Top / _characterHeight);
            var bottom = top + (int)(region.Height / _characterHeight);

            using (var ds = sender.CreateDrawingSession(region))
            {
                var y = 0.0;
                for (int i = top; i < bottom; i++)
                {
                    var x = 0.0;
                    var line = _buffer.GetFormattedLine(i);
                    foreach (var item in line)
                    {
                        if (item.Attributes.BackgroundColor != 0)
                        {
                            ds.FillRectangle(new Windows.Foundation.Rect { Height = _characterHeight, Width = _characterWidth * item.Text.Length, X = x, Y = y }, _colorHelper.GetColour(item.Attributes.BackgroundColor));
                        }

                        var foreground = Colors.White;
                        if (item.Attributes.ForegroundColor != 0)
                        {
                            foreground = _colorHelper.GetColour(item.Attributes.ForegroundColor);
                        }
                        var pos = new System.Numerics.Vector2 { X = (float)x, Y = (float)y };
                
                        ds.DrawText(item.Text, pos, foreground, _format);
                        x += item.Text.Length * _characterWidth;
                    }
                    y += _characterHeight;
                }
            }
        }

        public void Measure()
        {
            var device = CanvasDevice.GetSharedDevice();
            using (CanvasRenderTarget offscreen = new CanvasRenderTarget(device, (float)256, (float)256, canvas.Dpi))
            {
                using (CanvasDrawingSession ds = offscreen.CreateDrawingSession())
                {
                    _format = new CanvasTextFormat
                    {
                        FontSize = _terminalOptions.FontSize,
                        FontFamily = _terminalOptions.FontFamily,
                        FontWeight = _terminalOptions.FontWeight
                    };

                    var textLayout = new CanvasTextLayout(ds, "\u2560", _format, 0.0f, 0.0f);
                    _characterWidth = textLayout.DrawBounds.Right;
                    _characterHeight = textLayout.DrawBounds.Bottom;

                    _cols = (int)Math.Floor(Window.Current.CoreWindow.Bounds.Width / _characterWidth);
                    _rows = (int)Math.Floor((Window.Current.CoreWindow.Bounds.Height - 32) / _characterHeight);
                }
            }
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs args)
        {
            // Explicitly remove references to allow the Win2D controls to get garbage collected
            canvas.RemoveFromVisualTree();
            canvas = null;
        }

        public TerminalViewModel ViewModel { get; private set; }

        public Task ChangeKeyBindings()
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task ChangeOptions(TerminalOptions options)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task ChangeTheme(TerminalTheme theme)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task FindNext(string searchText)
        {
            throw new NotImplementedException();
        }

        public Task FindPrevious(string searchText)
        {
            throw new NotImplementedException();
        }

        public Task FocusTerminal()
        {
            canvas.Focus(FocusState.Programmatic);
            return Task.CompletedTask;
        }

        public async Task Initialize(TerminalViewModel viewModel)
        {
            Measure();
            ViewModel = viewModel;
            ViewModel.Terminal.OutputReceived += Terminal_OutputReceived;
            //ViewModel.Terminal.RegisterSelectedTextCallback(() => ExecuteScriptAsync("term.getSelection()"));
            //ViewModel.Terminal.Closed += Terminal_Closed;

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
            var size = new Models.TerminalSize { Rows = _rows, Columns = _cols };
            _buffer = new TerminalBuffer(new Terminal.TerminalSize(size.Columns, size.Rows));
            _parser = new Ansi.AnsiParser();
            var response = await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, size, sessionType).ConfigureAwait(true);
            if (!response.Success)
            {
                await ViewModel.DialogService.ShowMessageDialogAsnyc("Error", response.Error, DialogButton.OK).ConfigureAwait(true);
                ViewModel.Terminal.ReportLauchFailed();
                return;
            }

            canvas.Focus(FocusState.Keyboard);
        }

        private void _canvas_GotFocus(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("focus");
        }

        private void _canvas_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
            var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));
            var windowsPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.LeftWindows).HasFlag(CoreVirtualKeyStates.Down));
            var altPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Menu).HasFlag(CoreVirtualKeyStates.Down));

            int modCode = 0;
            if (shiftPressed) modCode |= 1;
            if (altPressed) modCode |= 2;
            if (controlPressed) modCode |= 4;
            if (windowsPressed) modCode |= 8;

            //if (IsSelecting && !modifiers.HasFlag(ModifierKeys.Alt))
            //{
            //    ClearSelection();
            //    if (e.Key == Key.Escape)
            //    {
            //        return;
            //    }
            //}

            string text = string.Empty;
            switch (e.Key)
            {
                case VirtualKey.Escape:
                    text = $"{C0.ESC}{C0.ESC}{C0.ESC}";
                    break;

                case VirtualKey.Back:
                    text = shiftPressed ?
                        C0.BS.ToString() :
                        C0.DEL.ToString();
                    break;

                case VirtualKey.Delete:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[3~" :
                        $"{C0.ESC}[3;{modCode + 1}~";
                    break;

                case VirtualKey.Tab:
                    text = shiftPressed ?
                        $"{C0.ESC}[Z" :
                        C0.HT.ToString();
                    break;

                case VirtualKey.Up:
                    text = Construct(1, 'A');
                    break;

                case VirtualKey.Down:
                    text = Construct(1, 'B');
                    break;

                case VirtualKey.Right:
                    text = Construct(1, 'C');
                    break;

                case VirtualKey.Left:
                    text = Construct(1, 'D');
                    break;

                case VirtualKey.Home:
                    text = Construct(1, 'H');
                    break;

                case VirtualKey.End:
                    text = Construct(1, 'F');
                    break;

                case VirtualKey.PageUp:
                    text = $"{C0.ESC}[5~";
                    break;

                case VirtualKey.PageDown:
                    text = $"{C0.ESC}[6~";
                    break;

                case VirtualKey.Enter:
                    text = C0.CR.ToString();
                    break;

                case VirtualKey.Space:
                    text = " ";
                    break;

                case VirtualKey.F1:
                    text = Construct(1, 'P');
                    break;

                case VirtualKey.F2:
                    text = Construct(1, 'Q');
                    break;

                case VirtualKey.F3:
                    text = Construct(1, 'R');
                    break;

                case VirtualKey.F4:
                    text = Construct(1, 'S');
                    break;

                case VirtualKey.F5:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[15~" :
                        $"{C0.ESC}[15;{modCode + 1}~";
                    break;

                case VirtualKey.F6:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[17~" :
                        $"{C0.ESC}[17;{modCode + 1}~";
                    break;

                case VirtualKey.F7:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[18~" :
                        $"{C0.ESC}[18;{modCode + 1}~";
                    break;

                case VirtualKey.F8:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[19~" :
                        $"{C0.ESC}[19;{modCode + 1}~";
                    break;

                case VirtualKey.F9:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[20~" :
                        $"{C0.ESC}[20;{modCode + 1}~";
                    break;

                case VirtualKey.F10:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[21~" :
                        $"{C0.ESC}[21;{modCode + 1}~";
                    break;

                case VirtualKey.F11:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[23~" :
                        $"{C0.ESC}[23;{modCode + 1}~";
                    break;

                case VirtualKey.F12:
                    text = (modCode == 0) ?
                        $"{C0.ESC}[24~" :
                        $"{C0.ESC}[24;{modCode + 1}~";
                    break;
            }
            if (text != string.Empty)
            {
                ViewModel.Terminal.Write(Encoding.UTF8.GetBytes(text));
                e.Handled = true;
            }

            string Construct(int a, char c)
            {
                return (modCode == 0) ?
                    $"{C0.ESC}O{c}" :
                    $"{C0.ESC}[{a};{modCode + 1}{c}";
            }
        }

        private void Canvas_CharacterReceived(UIElement sender, CharacterReceivedRoutedEventArgs args)
        {
            var text = Encoding.UTF8.GetBytes(new[] { args.Character });
            ViewModel.Terminal.Write(text);
        }

        private void ProcessTerminalCode(TerminalCode code)
        {
            switch (code.Type)
            {
                case TerminalCodeType.ResetMode:
                    _buffer.ShowCursor = false;
                    break;

                case TerminalCodeType.SetMode:
                    _buffer.ShowCursor = true;

                    // HACK We want clear to reset the window position but not general typing.
                    //      We therefore reset the window only if the cursor is moved to the top.
                    if (_buffer.CursorY == 0)
                    {
                        _buffer.WindowTop = 0;
                    }
                    break;

                case TerminalCodeType.Text:
                    _buffer.Type(code.Text);
                    break;

                case TerminalCodeType.LineFeed:
                    if (_buffer.CursorY == _buffer.Size.Rows - 1)
                    {
                        _buffer.ShiftUp();
                    }
                    else
                    {
                        _buffer.CursorY++;
                    }
                    break;

                case TerminalCodeType.CarriageReturn:
                    _buffer.CursorX = 0;
                    break;

                case TerminalCodeType.CharAttributes:
                    _buffer.CurrentCharAttributes = code.CharAttributes;
                    break;

                case TerminalCodeType.CursorPosition:
                    _buffer.CursorX = code.Column;
                    _buffer.CursorY = code.Line;
                    break;

                case TerminalCodeType.CursorUp:
                    _buffer.CursorY -= code.Line;
                    break;

                case TerminalCodeType.CursorCharAbsolute:
                    _buffer.CursorX = code.Column;
                    break;

                case TerminalCodeType.EraseInLine:
                    if (code.Line == 0)
                    {
                        _buffer.ClearBlock(_buffer.CursorX, _buffer.CursorY, _buffer.Size.Columns - 1, _buffer.CursorY);
                    }
                    break;

                case TerminalCodeType.EraseInDisplay:
                    _buffer.Clear();
                    _buffer.CursorX = 0;
                    _buffer.CursorY = 0;
                    break;

                case TerminalCodeType.SetTitle:
                    //ViewModel.Terminal.SetTitle(code.Text);
                    break;
            }
        }

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
  
            var text = Encoding.UTF8.GetString(e, 0, e.Length);
            var reader = new ArrayReader<char>(text.ToCharArray());
            var codes = _parser.Parse(reader);
            foreach (var code in codes)
            {
                ProcessTerminalCode(code);
            }

            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                canvas.Height = Math.Max(_buffer.CurrentHistoryLength * _characterHeight, _buffer.Size.Rows * _characterHeight);
                canvas.Invalidate();
            });
        }

    }
}