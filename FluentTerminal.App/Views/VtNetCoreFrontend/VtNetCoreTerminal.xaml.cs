using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using VtNetCore.VirtualTerminal;
using VtNetCore.XTermParser;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FluentTerminal.App.Views.VtNetCoreFrontend
{
    public sealed partial class VtNetCoreTerminal : UserControl, ITerminalView
    {
        private double _characterHeight;
        private double _characterWidth;
        private int _cols;
        private CanvasTextFormat _format;
        private int _rows;

        public VtNetCoreTerminal()
        {
            this.InitializeComponent();

            canvas.Draw += _canvas_Draw;
            canvas.KeyDown += Canvas_KeyDown;
            canvas.GotFocus += Canvas_GotFocus;
            canvas.LostFocus += Canvas_LostFocus;
            canvas.Tapped += Canvas_Tapped;
        }

        private void Canvas_LostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
        }

        private void Canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            canvas.Focus(FocusState.Pointer);
        }

        private void Canvas_GotFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;
            Debug.WriteLine("focus");
        }

        private void Canvas_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
            var shiftPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down));

            //Scrolling = false;

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = _terminal.GetKeySequence(e.Key.ToString(), controlPressed, shiftPressed);
            if (code != null)
                e.Handled = _terminal.KeyPressed(e.Key.ToString(), controlPressed, shiftPressed);

            canvas.Invalidate();

            //if (ViewTop != _terminal.ViewPort.TopRow)
            //{
            //    ViewTop = _terminal.ViewPort.TopRow;
            //    canvas.Invalidate();
            //}
        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            //if (!Connected)
            //    return;

            var ch = char.ConvertFromUtf32((int)args.KeyCode);
            switch (ch)
            {
                case "\b":
                case "\t":
                case "\n":
                    return;

                case "\r":
                    ////LogText = "";
                    //lock(_rawText)
                    //{
                    //    _rawTextLength = 0;
                    //    _rawTextChanged = true;
                    //}
                    return;

                default:
                    break;

            }

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = _terminal.GetKeySequence(ch, false, false);
            if (code == null)
                args.Handled = _terminal.KeyPressed(ch, false, false);
        }

        private void _canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var spans = _terminal.ViewPort.GetPageSpans(0, _rows, _cols);

            for (var i = 0; i < spans.Count; i++)
            {
                var span = spans[i];
                var x = 0.0;
                foreach (var item in span.Spans)
                {
                    
                    

                    if (item.BackgroundColor != "#000000")
                    {
                        var background = item.BackgroundColor.FromString();
                        args.DrawingSession.FillRectangle(new Rect { Height = _characterHeight, Width = _characterWidth * item.Text.Length, X = x, Y = i * 16 }, background);
                    }

                    var foreground = item.ForgroundColor.FromString();
                    args.DrawingSession.DrawText(item.Text, new System.Numerics.Vector2 { X = (float)x, Y = i * 16 }, foreground, _format);

                        
                    x += item.Text.Length * _characterWidth;
                }
            }
        }

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
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task FindPrevious(string searchText)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task FocusTerminal()
        {
            canvas.Focus(FocusState.Programmatic);
            return Task.CompletedTask;
        }

        public TerminalViewModel ViewModel { get; private set; }

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

            _terminal = new VirtualTerminalController();
            _terminal.SendData = new EventHandler<SendDataEventArgs>((s, e) => { });
            _dataConsumer = new DataConsumer(_terminal);

            var response = await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, size, sessionType).ConfigureAwait(true);
            if (!response.Success)
            {
                await ViewModel.DialogService.ShowMessageDialogAsnyc("Error", response.Error, DialogButton.OK).ConfigureAwait(true);
                ViewModel.Terminal.ReportLauchFailed();
                return;
            }

            canvas.Focus(FocusState.Keyboard);
        }

        private DataConsumer _dataConsumer;
        private VirtualTerminalController _terminal;

        private void Terminal_OutputReceived(object sender, byte[] e)
        {
            _dataConsumer.Push(e);

            if (_terminal.Changed)
            {
                _terminal.ClearChanges();
                canvas.Invalidate();
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
                        FontSize = 13,
                        FontFamily = "Consolas NF"
                    };

                    var textLayout = new CanvasTextLayout(ds, "\u2560", _format, 0.0f, 0.0f);
                    _characterWidth = textLayout.DrawBounds.Right;
                    _characterHeight = textLayout.DrawBounds.Bottom;

                    _cols = (int)Math.Floor(Window.Current.CoreWindow.Bounds.Width / _characterWidth);
                    _rows = (int)Math.Floor((Window.Current.CoreWindow.Bounds.Height - 32) / _characterHeight);
                }
            }
        }
    }
}
