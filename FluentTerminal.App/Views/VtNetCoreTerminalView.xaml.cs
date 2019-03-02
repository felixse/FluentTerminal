using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VtNetCore.VirtualTerminal;
using VtNetCore.VirtualTerminal.Layout;
using VtNetCore.XTermParser;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Views
{
    public sealed partial class VtNetCoreTerminalView : UserControl, ITerminalView
    {
        private readonly int _blinkHideMs = 300;
        private readonly int _blinkShowMs = 600;
        private readonly DataConsumer _consumer;
        private readonly VirtualTerminalController _terminal;
        private readonly DispatcherTimer blinkDispatcher;
        private double _characterHeight = -1;
        private double _characterWidth = -1;
        private int _columns = -1;
        private TextPosition _mouseOver = new TextPosition();
        private int _rows = -1;
        private bool _scrolling;
        private bool _selecting;

        private TextRange _textSelection;

        public VtNetCoreTerminalView()
        {
            this.InitializeComponent();

            _terminal = new VirtualTerminalController();
            _consumer = new DataConsumer(_terminal);
            ViewTop = _terminal.ViewPort.TopRow;
            _terminal.SendData += OnSendData;

            blinkDispatcher = new DispatcherTimer();
            blinkDispatcher.Tick += BlinkTimerHandler;
            blinkDispatcher.Interval = TimeSpan.FromMilliseconds(Math.Min(150, GCD(_blinkShowMs, _blinkHideMs)));
            blinkDispatcher.Start();

            Canvas.Draw += OnDraw;

            KeyDown += TerminalKeyDown;
            Tapped += OnTapped;
            GotFocus += TerminalGotFocus;
            LostFocus += TerminalLostFocus;
            PointerWheelChanged += OnWheelChanged;
            PointerMoved += TerminalPointerMoved;
            PointerExited += TerminalPointerExited;
            PointerPressed += TerminalPointerPressed;
            PointerReleased += TerminalPointerReleased;
        }

        public int MaxScrollValue => _terminal.ViewPort.TopRow;

        public TextPosition MousePressedAt { get; set; }

        public int ScrollValue
        {
            get { return ViewTop; }
            set
            {
                _scrolling = true;
                ViewTop = value;
                Canvas.Invalidate();
            }
        }

        public TerminalViewModel ViewModel { get; private set; }
        public int ViewTop { get; set; }

        public Task ChangeKeyBindings()
        {
            return Task.CompletedTask;
        }

        public Task ChangeOptions(TerminalOptions options)
        {
            return Task.CompletedTask;
        }

        public Task ChangeTheme(TerminalTheme theme)
        {
            return Task.CompletedTask;
        }

        public Task FindNext(string searchText)
        {
            return Task.CompletedTask;
        }

        public Task FindPrevious(string searchText)
        {
            return Task.CompletedTask;
        }

        public Task FocusTerminal()
        {
            return Task.CompletedTask;
        }

        public Color GetSolidColorBrush(string hex)
        {
            byte a = 255; // (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(1, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(3, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(5, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }

        public async Task Initialize(TerminalViewModel viewModel)
        {
            ViewModel = viewModel;

            ViewModel.Terminal.OutputReceived += Terminal_OutputReceived;
            ViewModel.Terminal.Closed += Terminal_Closed;
            ViewModel.Terminal.RegisterSelectedTextCallback(() => Task.FromResult(_terminal.GetText(_textSelection.Start.Column, _textSelection.Start.Row, _textSelection.End.Column, _textSelection.End.Row)));

            _terminal.WindowTitleChanged += OnWindowTitleChanged;
            _terminal.SizeChanged += OnSizeChanged;

            var settings = ViewModel.SettingsService.GetApplicationSettings();
            var options = ViewModel.SettingsService.GetTerminalOptions();

            Canvas.FontFamily = new FontFamily(options.FontFamily);
            Canvas.FontSize = options.FontSize;
            Canvas.Margin = new Thickness(options.Padding, options.Padding, options.Padding, options.Padding);

            var sessionType = SessionType.Unknown;
            if (settings.AlwaysUseWinPty || !ViewModel.ApplicationView.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7))
            {
                sessionType = SessionType.WinPty;
            }
            else
            {
                sessionType = SessionType.ConPty;
            }

            await ViewModel.Terminal.StartShellProcess(ViewModel.ShellProfile, new TerminalSize { Columns = 80, Rows = 30 }, sessionType).ConfigureAwait(true);
        }

        private void BlinkTimerHandler(object sender, object e)
        {
            Canvas.Invalidate();
        }

        private bool BlinkVisible()
        {
            var blinkCycle = _blinkShowMs + _blinkHideMs;

            return (DateTime.Now.Subtract(DateTime.MinValue).Milliseconds % blinkCycle) < _blinkHideMs;
        }

        private bool ControlPressed()
        {
            return (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control) & CoreVirtualKeyStates.Down) != 0;
        }

        private void CoreWindow_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            var ch = char.ConvertFromUtf32((int)args.KeyCode);

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = _terminal.GetKeySequence(ch, false, false);
            if (code == null)
            {
                args.Handled = _terminal.KeyPressed(ch, false, false);
            }
        }

        private long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            // Pull out remainders.
            for (; ; )
            {
                long remainder = a % b;
                if (remainder == 0) return b;
                a = b;
                b = remainder;
            };
        }

        private void OnDraw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            var drawingSession = args.DrawingSession;

            var textFormat =
                new CanvasTextFormat
                {
                    FontSize = Convert.ToSingle(Canvas.FontSize),
                    FontFamily = Canvas.FontFamily.Source,
                    FontWeight = Canvas.FontWeight,
                    WordWrapping = CanvasWordWrapping.NoWrap
                };

            ProcessTextFormat(drawingSession, textFormat);

            var showBlink = BlinkVisible();
            var spans = default(List<LayoutRow>);
            var cursorPosition = default(TextPosition);
            var showCursor = false;
            var cursorColor = Colors.Green;
            var TerminalTop = -1;

            lock (_terminal)
            {
                TerminalTop = _terminal.ViewPort.TopRow;

                if (_terminal.ViewPort.CursorPosition.Row > _rows)
                {
                    throw new Exception("We should never be here");
                }

                spans = _terminal.ViewPort.GetPageSpans(ViewTop, _rows, _columns, _textSelection);
                showCursor = _terminal.CursorState.ShowCursor;
                cursorPosition = _terminal.ViewPort.CursorPosition.Clone();
                cursorColor = GetSolidColorBrush(_terminal.CursorState.Attributes.WebColor);
            }

            if (!_scrolling && ViewTop != TerminalTop)
            {
                ViewTop = TerminalTop;
            }

            var defaultTransform = drawingSession.Transform;

            PaintBackgroundLayer(drawingSession, spans);

            PaintTextLayer(drawingSession, spans, textFormat, showBlink);

            if (showCursor)
            {
                PaintCursor(drawingSession, spans, textFormat, cursorPosition.OffsetBy(0, TerminalTop - ViewTop), cursorColor);
            }

            drawingSession.Transform = defaultTransform;
        }

        private void OnSendData(object sender, SendDataEventArgs e)
        {
            ViewModel.Terminal.Write(Encoding.UTF8.GetString(e.Data));
        }

        private async void OnSizeChanged(object sender, SizeEventArgs args)
        {
            await ViewModel.Terminal.SetSize(new TerminalSize { Rows = args.Height, Columns = args.Width }).ConfigureAwait(true);
        }

        private void OnWindowTitleChanged(object sender, TextEventArgs args)
        {
            ViewModel.Terminal.SetTitle(args.Text);
        }

        private void PaintBackgroundLayer(CanvasDrawingSession drawingSession, List<LayoutRow> spans)
        {
            double lineY = 0;
            foreach (var textRow in spans)
            {
                drawingSession.Transform =
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),  // Scale double width
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0) // Scale double high
                    );

                var drawY =
                    (lineY - (textRow.DoubleHeightBottom ? _characterHeight : 0)) *      // Offset position upwards for bottom of double high char
                    ((textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0); // Scale position for double height

                double drawX = 0;
                foreach (var textSpan in textRow.Spans)
                {
                    var bounds =
                        new Rect(
                            drawX,
                            drawY,
                            _characterWidth * (textSpan.Text.Length) + 0.9,
                            _characterHeight + 0.9
                        );

                    if (textSpan.BackgroundColor != "#000000") // todo set theme
                    {
                        drawingSession.FillRectangle(bounds, GetSolidColorBrush(textSpan.BackgroundColor));
                    }


                    drawX += _characterWidth * (textSpan.Text.Length);
                }

                lineY += _characterHeight;
            }
        }

        private void PaintCursor(CanvasDrawingSession drawingSession, List<LayoutRow> spans, CanvasTextFormat textFormat, TextPosition cursorPosition, Color cursorColor)
        {
            var cursorY = cursorPosition.Row;
            if (cursorY < 0 || cursorY >= _rows)
                return;

            var drawX = cursorPosition.Column * _characterWidth;
            var drawY = (cursorY * _characterHeight);

            if (cursorY < spans.Count)
            {
                var textRow = spans[cursorY];

                drawingSession.Transform =
                    Matrix3x2.CreateTranslation(
                        1.0f,
                        (float)(textRow.DoubleHeightBottom ? -_characterHeight : 0)
                    ) *
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0)
                    );

                drawY *= (textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0;
            }

            var cursorRect = new Rect(
                drawX,
                drawY,
                _characterWidth,
                _characterHeight + 0.9
            );

            drawingSession.DrawRectangle(cursorRect, cursorColor);
        }

        private void PaintTextLayer(CanvasDrawingSession drawingSession, List<LayoutRow> spans, CanvasTextFormat textFormat, bool showBlink)
        {
            var dipToDpiRatio = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96;

            double lineY = 0;
            foreach (var textRow in spans)
            {
                drawingSession.Transform =
                    Matrix3x2.CreateScale(
                        (float)(textRow.DoubleWidth ? 2.0 : 1.0),  // Scale double width
                        (float)(textRow.DoubleHeightBottom | textRow.DoubleHeightTop ? 2.0 : 1.0) // Scale double high
                    );

                var drawY =
                    (lineY - (textRow.DoubleHeightBottom ? _characterHeight : 0)) *      // Offset position upwards for bottom of double high char
                    ((textRow.DoubleHeightBottom | textRow.DoubleHeightTop) ? 0.5 : 1.0); // Scale position for double height

                double drawX = 0;
                foreach (var textSpan in textRow.Spans)
                {
                    var runWidth = _characterWidth * (textSpan.Text.Length);

                    if (textSpan.Hidden || (textSpan.Blink && !showBlink))
                    {
                        drawX += runWidth;
                        continue;
                    }

                    var color = GetSolidColorBrush(textSpan.ForgroundColor);
                    textFormat.FontWeight = textSpan.Bold ? FontWeights.Bold : FontWeights.Light;

                    var textLayout = new CanvasTextLayout(drawingSession, textSpan.Text, textFormat, 0.0f, 0.0f);
                    drawingSession.DrawTextLayout(textLayout, (float)drawX, (float)drawY, color);

                    if (textSpan.Underline)
                    {
                        // TODO : Come up with a better means of identifying line weight and offset
                        var underlineOffset = textLayout.LineMetrics[0].Baseline * dipToDpiRatio * 1.07;

                        drawingSession.DrawLine(new Vector2((float)drawX, (float)(drawY + underlineOffset)), new Vector2((float)(drawX + runWidth), (float)(drawY + underlineOffset)), color);
                    }

                    drawX += _characterWidth * (textSpan.Text.Length);
                }

                lineY += _characterHeight;
            }
        }

        private void ProcessTextFormat(CanvasDrawingSession drawingSession, CanvasTextFormat format)
        {
            CanvasTextLayout textLayout = new CanvasTextLayout(drawingSession, "\u2560", format, 0.0f, 0.0f);
            if (_characterWidth != textLayout.DrawBounds.Width || _characterHeight != textLayout.DrawBounds.Height)
            {
                _characterWidth = textLayout.DrawBounds.Width;
                _characterHeight = textLayout.DrawBounds.Height;
            }

            int columns = Convert.ToInt32(Math.Floor(Canvas.RenderSize.Width / _characterWidth));
            int rows = Convert.ToInt32(Math.Floor(Canvas.RenderSize.Height / _characterHeight));
            if (_columns != columns || _rows != rows)
            {
                _columns = columns;
                _rows = rows;
                ViewModel.Terminal.SetSize(new TerminalSize { Columns = columns, Rows = rows });
            }
        }

        private bool ShiftPressed()
        {
            return (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Shift) & CoreVirtualKeyStates.Down) != 0;
        }

        private void Terminal_Closed(object sender, EventArgs e)
        {
        }

        private void Terminal_OutputReceived(object sender, string e)
        {
            var oldTopRow = _terminal.ViewPort.TopRow;

            _consumer.Push(Encoding.UTF8.GetBytes(e));

            if (_terminal.Changed)
            {
                _terminal.ClearChanges();

                if (oldTopRow != _terminal.ViewPort.TopRow && oldTopRow >= ViewTop)
                {
                    ViewTop = _terminal.ViewPort.TopRow;
                }

                Canvas.Invalidate();
            }
        }

        private void TerminalGotFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived += CoreWindow_CharacterReceived;

            _terminal?.FocusIn();
        }

        private void TerminalKeyDown(object sender, KeyRoutedEventArgs e)
        {
            _scrolling = false;

            // Since I get the same key twice in TerminalKeyDown and in CoreWindow_CharacterReceived
            // I lookup whether KeyPressed should handle the key here or there.
            var code = _terminal.GetKeySequence(e.Key.ToString(), ControlPressed(), ShiftPressed());
            if (code != null)
                e.Handled = _terminal.KeyPressed(e.Key.ToString(), ControlPressed(), ShiftPressed());

            if (ViewTop != _terminal.ViewPort.TopRow)
            {
                ViewTop = _terminal.ViewPort.TopRow;
                Canvas.Invalidate();
            }
        }

        private void TerminalLostFocus(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.CharacterReceived -= CoreWindow_CharacterReceived;
            _terminal?.FocusOut();
        }

        private void TerminalPointerExited(object sender, PointerRoutedEventArgs e)
        {
            _mouseOver = null;
            Canvas.Invalidate();
        }

        private void TerminalPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            var position = PointToTextPosition(pointer.Position);

            var textPosition = position.OffsetBy(0, ViewTop);

            if ((_terminal.UseAllMouseTracking || _terminal.CellMotionMouseTracking) && position.Column >= 0 && position.Row >= 0 && position.Column < _columns && position.Row < _rows)
            {
                var button =
                    pointer.Properties.IsLeftButtonPressed ? 0 :
                        pointer.Properties.IsRightButtonPressed ? 1 :
                            pointer.Properties.IsMiddleButtonPressed ? 2 :
                            3;  // No button

                _terminal.MouseMove(position.Column, position.Row, button, ControlPressed(), ShiftPressed());

                if (button == 3 && !_terminal.UseAllMouseTracking)
                {
                    return;
                }
            }

            if (_mouseOver != null && _mouseOver == position)
            {
                return;
            }

            _mouseOver = position;

            if (pointer.Properties.IsLeftButtonPressed && MousePressedAt != null)
            {
                TextRange newSelection;

                if (MousePressedAt != textPosition)
                {
                    if (MousePressedAt <= textPosition)
                    {
                        newSelection = new TextRange
                        {
                            Start = MousePressedAt,
                            End = textPosition.OffsetBy(-1, 0)
                        };
                    }
                    else
                    {
                        newSelection = new TextRange
                        {
                            Start = textPosition,
                            End = MousePressedAt
                        };
                    }

                    _selecting = true;

                    if (_textSelection != newSelection)
                    {
                        _textSelection = newSelection;
                        Canvas.Invalidate();
                    }
                }
            }
        }

        private void TerminalPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            var position = PointToTextPosition(pointer.Position);

            var textPosition = position.OffsetBy(0, ViewTop);

            if (!_terminal.MouseTrackingEnabled)
            {
                if (pointer.Properties.IsLeftButtonPressed)
                    MousePressedAt = textPosition;
                //else if (pointer.Properties.IsRightButtonPressed)
                //PasteClipboard();
            }

            if (position.Column >= 0 && position.Row >= 0 && position.Column < _columns && position.Row < _rows)
            {
                var button =
                    pointer.Properties.IsLeftButtonPressed ? 0 :
                        pointer.Properties.IsRightButtonPressed ? 1 :
                            pointer.Properties.IsMiddleButtonPressed ? 2 :
                                3;  // No button

                _terminal.MousePress(position.Column, position.Row, button, ControlPressed(), ShiftPressed());
            }
        }

        private void TerminalPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);
            var position = PointToTextPosition(pointer.Position);
            var textPosition = position.OffsetBy(0, ViewTop);

            if (!pointer.Properties.IsLeftButtonPressed)
            {
                if (_selecting)
                {
                    MousePressedAt = null;
                    _selecting = false;

                    var captured = _terminal.GetText(_textSelection.Start.Column, _textSelection.Start.Row, _textSelection.End.Column, _textSelection.End.Row);

                    var dataPackage = new DataPackage();
                    dataPackage.SetText(captured);
                    dataPackage.Properties.EnterpriseId = "FluentTerminal";
                    try
                    {
                        Clipboard.SetContent(dataPackage);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine("Failed to post to clipboard: " + exception.Message);
                    }
                }
                else
                {
                    _textSelection = null;
                    Canvas.Invalidate();
                }
            }

            if (position.Column >= 0 && position.Row >= 0 && position.Column < _columns && position.Row < _rows)
            {
                _terminal.MouseRelease(position.Column, position.Row, ControlPressed(), ShiftPressed());
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Focus(FocusState.Pointer);
        }

        private void OnWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(Canvas);

            int oldViewTop = ViewTop;

            ViewTop -= pointer.Properties.MouseWheelDelta / 40;
            _scrolling = true;

            if (ViewTop < 0)
            {
                ViewTop = 0;
            }
            else if (ViewTop > _terminal.ViewPort.TopRow)
            {
                ViewTop = _terminal.ViewPort.TopRow;
            }

            if (oldViewTop != ViewTop)
            {
                Canvas.Invalidate();
            }
        }

        private TextPosition PointToTextPosition(Point point)
        {
            var overColumn = (int)Math.Floor(point.X / _characterWidth);
            if (overColumn >= _columns)
            {
                overColumn = _columns - 1;
            }

            var overRow = (int)Math.Floor(point.Y / _characterHeight);
            if (overRow >= _rows)
            {
                overRow = _rows - 1;
            }

            return new TextPosition { Column = overColumn, Row = overRow };
        }
    }
}