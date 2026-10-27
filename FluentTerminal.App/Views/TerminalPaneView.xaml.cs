using FluentTerminal.App.Converters;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Views
{
    // Renders a tab's pane tree (issue #501) onto a plain Canvas.
    //
    // UWP WebViews are HWND islands with an OS-side focus identity. The former
    // recursive-Grid design detached and re-attached every cached TerminalView
    // on every split, close and tab switch; each re-parenting replants the
    // WebView's HWND, re-asserts focus, and can start a focus war between
    // islands (~40Hz focus-in/focus-out floods, keystrokes garbled across
    // panes). Here every pane keeps its Border (holding the converter-cached
    // TerminalView) for its whole lifetime, and every split keeps its
    // separator + drag grip: splits, closes and tab switches only move, size
    // and collapse things — the visual tree is never rebuilt.
    public sealed partial class TerminalPaneView : UserControl
    {
        private const double SplitterSize = 4;
        private const double SeparatorSize = 1;

        public static readonly DependencyProperty TabProperty =
            DependencyProperty.Register(nameof(Tab), typeof(TerminalViewModel), typeof(TerminalPaneView), new PropertyMetadata(null, OnTabChanged));

        private TerminalViewModelToViewConverter _viewConverter;

        // Pane -> its one-and-only Border wrapping the cached TerminalView.
        private readonly Dictionary<TerminalViewModel, Border> _paneBorders =
            new Dictionary<TerminalViewModel, Border>();

        // Split node -> its separator line and transparent drag grip, reused
        // for as long as the node exists in the tree.
        private readonly Dictionary<PaneSplitNode, SplitZone> _splitZones =
            new Dictionary<PaneSplitNode, SplitZone>();

        // Active splitter drag (the state is captured at PointerPressed).
        private PaneSplitNode _dragSplit;
        private bool _dragVertical;
        private double _dragStartCoord;
        private double _dragStartRatio;
        private double _dragExtent;

        private sealed class SplitZone
        {
            public Border Line { get; set; }
            public Border Grip { get; set; }
            public bool Vertical { get; set; }
            public Rect Rect { get; set; }
        }

        public TerminalPaneView()
        {
            InitializeComponent();
            PaneRoot.SizeChanged += (s, e) => ApplyLayout();
        }

        public TerminalViewModel Tab
        {
            get => (TerminalViewModel)GetValue(TabProperty);
            set => SetValue(TabProperty, value);
        }

        public void FocusSelectedPane()
        {
            Tab?.FocusedPane?.FocusTerminal();
        }

        private static void OnTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (TerminalPaneView)d;
            if (e.OldValue is TerminalViewModel oldTab)
            {
                oldTab.PaneLayoutChanged -= view.OnPaneLayoutChanged;
                oldTab.PaneFocusChanged -= view.OnPaneFocusChanged;
            }
            if (e.NewValue is TerminalViewModel newTab)
            {
                newTab.PaneLayoutChanged += view.OnPaneLayoutChanged;
                newTab.PaneFocusChanged += view.OnPaneFocusChanged;
            }
            view.OnPaneLayoutChanged(null, EventArgs.Empty);
        }

        private void OnPaneLayoutChanged(object sender, EventArgs e)
        {
            // Layout changes can originate off the UI thread (tray callbacks).
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, ApplyLayout);
        }

        private void OnPaneFocusChanged(object sender, EventArgs e)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, RefreshFocusHighlight);
        }

        private void RefreshFocusHighlight()
        {
            var tab = Tab;
            if (tab == null)
            {
                return;
            }
            foreach (var pair in _paneBorders)
            {
                UpdateLeafBorder(tab, pair.Key, pair.Value);
            }
        }

        private TerminalViewModelToViewConverter ViewConverter
        {
            get
            {
                if (_viewConverter == null && Window.Current.Content is Frame frame && frame.Content is Page mainPage)
                {
                    _viewConverter = mainPage.Resources["TerminalViewModelToViewConverter"] as TerminalViewModelToViewConverter;
                }
                return _viewConverter;
            }
        }

        // Re-applies the whole geometry from the VM tree. Creating a Border for
        // a brand-new pane is the only time anything enters the visual tree
        // during a split; everything else is position/size/visibility only.
        private void ApplyLayout()
        {
            var tab = Tab;
            if (tab == null)
            {
                foreach (var pair in _paneBorders)
                {
                    pair.Value.Visibility = Visibility.Collapsed;
                }
                return;
            }

            var width = PaneRoot.ActualWidth;
            var height = PaneRoot.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            var root = tab.TabRoot;
            var livePanes = new HashSet<TerminalViewModel>();
            var liveSplits = new HashSet<PaneSplitNode>();

            if (!root.HasSplitPanes || root.PaneTreeRoot == null)
            {
                // Single state: display the live primary (a promoted survivor
                // if the root session died).
                var only = root.GetAllPanes().FirstOrDefault() ?? root;
                livePanes.Add(only);
                PlaceLeaf(EnsurePaneBorder(tab, only), new Rect(0, 0, width, height));
            }
            else
            {
                LayoutNode(tab, root.PaneTreeRoot, new Rect(0, 0, width, height), livePanes, liveSplits);
            }

            PruneStaleVisuals(livePanes, liveSplits);
        }

        private void LayoutNode(TerminalViewModel tab, PaneNode node, Rect rect, HashSet<TerminalViewModel> livePanes, HashSet<PaneSplitNode> liveSplits)
        {
            if (node is PaneLeafNode leaf)
            {
                var pane = tab.GetPaneByLeafId(leaf.Id);
                if (pane == null)
                {
                    return;
                }
                livePanes.Add(pane);
                PlaceLeaf(EnsurePaneBorder(tab, pane), rect);
                return;
            }

            var split = (PaneSplitNode)node;
            liveSplits.Add(split);
            var vertical = split.Orientation == SplitOrientation.Vertical;

            Rect firstRect;
            Rect secondRect;
            Rect lineRect;
            if (vertical)
            {
                var firstExtent = Math.Round(rect.Width * split.Ratio);
                firstExtent = Math.Min(Math.Max(firstExtent, SeparatorSize), Math.Max(rect.Width - SeparatorSize, SeparatorSize));
                var x = rect.X + firstExtent;
                lineRect = new Rect(x, rect.Y, SeparatorSize, rect.Height);
                firstRect = new Rect(rect.X, rect.Y, firstExtent, rect.Height);
                secondRect = new Rect(x + SeparatorSize, rect.Y, Math.Max(rect.Width - firstExtent - SeparatorSize, 0), rect.Height);
            }
            else
            {
                var firstExtent = Math.Round(rect.Height * split.Ratio);
                firstExtent = Math.Min(Math.Max(firstExtent, SeparatorSize), Math.Max(rect.Height - SeparatorSize, SeparatorSize));
                var y = rect.Y + firstExtent;
                lineRect = new Rect(rect.X, y, rect.Width, SeparatorSize);
                firstRect = new Rect(rect.X, rect.Y, rect.Width, firstExtent);
                secondRect = new Rect(rect.X, y + SeparatorSize, rect.Width, Math.Max(rect.Height - firstExtent - SeparatorSize, 0));
            }

            var zone = EnsureSplitZone(split, vertical);
            zone.Rect = rect;
            PlaceLeaf(zone.Line, lineRect, zIndex: 99);
            PlaceLeaf(zone.Grip, GripRect(rect, lineRect, vertical), zIndex: 100);

            LayoutNode(tab, split.First, firstRect, livePanes, liveSplits);
            LayoutNode(tab, split.Second, secondRect, livePanes, liveSplits);
        }

        // Transparent drag area straddling the visible 1 px separator.
        private static Rect GripRect(Rect parentRect, Rect lineRect, bool vertical)
        {
            const double inset = (SplitterSize - SeparatorSize) / 2;
            return vertical
                ? new Rect(lineRect.X - inset, parentRect.Y, SplitterSize, parentRect.Height)
                : new Rect(parentRect.X, lineRect.Y - inset, parentRect.Width, SplitterSize);
        }

        private static void PlaceLeaf(FrameworkElement element, Rect rect, int zIndex = 10)
        {
            Canvas.SetZIndex(element, zIndex);
            Canvas.SetLeft(element, rect.X);
            Canvas.SetTop(element, rect.Y);
            if (rect.Width >= 0)
            {
                element.Width = Math.Max(rect.Width, 0);
            }
            if (rect.Height >= 0)
            {
                element.Height = Math.Max(rect.Height, 0);
            }
            element.Visibility = Visibility.Visible;
        }

        private Border EnsurePaneBorder(TerminalViewModel tab, TerminalViewModel pane)
        {
            if (_paneBorders.TryGetValue(pane, out var border))
            {
                return border;
            }

            var view = ViewConverter.Convert(pane, null, null, null) as UIElement;
            border = new Border
            {
                BorderThickness = new Thickness(0),
                Child = view
            };
            UpdateLeafBorder(tab, pane, border);
            Canvas.SetZIndex(border, 10);
            PaneRoot.Children.Add(border);
            _paneBorders[pane] = border;

            // UWP has no routed GotFocus for plain elements: use PointerPressed
            // (a real routed event, handled-events-too) so a click on the pane
            // frame outside the WebView island still moves the active-pane
            // highlight. Clicks inside the terminal surface arrive through the
            // JS bridge (window.onmousedown) and never raise this event.
            border.AddHandler(Windows.UI.Xaml.UIElement.PointerPressedEvent,
                new Windows.UI.Xaml.Input.PointerEventHandler((s, e) => Tab?.FocusPane(pane)), true);
            return border;
        }

        private SplitZone EnsureSplitZone(PaneSplitNode split, bool vertical)
        {
            if (_splitZones.TryGetValue(split, out var zone))
            {
                zone.Vertical = vertical;
                return zone;
            }

            var line = new Border
            {
                // 1 px separator; the active pane draws its own accent frame on
                // top of its edges.
                Background = SeparatorBrush(),
                IsHitTestVisible = false
            };

            var grip = new Border
            {
                // Invisible drag strip: wider than the line, transparent, hit
                // testable. UWP WebView is an HWND island that eats XAML
                // pointer events inside its own bounds, so an overlaying
                // element is required to grab the seam.
                Background = new SolidColorBrush(Windows.UI.Colors.Transparent)
            };
            Canvas.SetZIndex(line, 99);
            Canvas.SetZIndex(grip, 100);
            PaneRoot.Children.Add(line);
            PaneRoot.Children.Add(grip);

            zone = new SplitZone { Line = line, Grip = grip, Vertical = vertical };
            _splitZones[split] = zone;

            grip.PointerEntered += (s, e) => SetSplitterCursor(zone.Vertical);
            grip.PointerExited += (s, e) => SetSplitterCursor(null);
            grip.PointerPressed += (s, e) =>
            {
                if (_dragSplit != null)
                {
                    return;
                }
                _dragSplit = split;
                _dragVertical = zone.Vertical;
                var pos = e.GetCurrentPoint(PaneRoot).Position;
                _dragStartCoord = _dragVertical ? pos.X : pos.Y;
                _dragStartRatio = split.Ratio;
                _dragExtent = zone.Rect.Width > 0
                    ? (_dragVertical ? zone.Rect.Width : zone.Rect.Height)
                    : (_dragVertical ? PaneRoot.ActualWidth : PaneRoot.ActualHeight);
                grip.CapturePointer(e.Pointer);
            };
            grip.PointerMoved += (s, e) =>
            {
                if (!ReferenceEquals(_dragSplit, split))
                {
                    return;
                }
                var pos = e.GetCurrentPoint(PaneRoot).Position;
                var coord = _dragVertical ? pos.X : pos.Y;
                var firstExtent = (_dragStartRatio * _dragExtent) + (coord - _dragStartCoord);
                Tab?.UpdateSplitRatio(split, firstExtent / _dragExtent);
                // Immediate feedback: don't wait for the scheduled event.
                ApplyLayout();
            };
            grip.PointerReleased += (s, e) =>
            {
                if (!ReferenceEquals(_dragSplit, split))
                {
                    return;
                }
                _dragSplit = null;
                SetSplitterCursor(null);
                grip.ReleasePointerCapture(e.Pointer);
            };
            grip.PointerCaptureLost += (s, e) =>
            {
                if (ReferenceEquals(_dragSplit, split))
                {
                    _dragSplit = null;
                }
            };
            return zone;
        }

        private static void SetSplitterCursor(bool? vertical)
        {
            var window = Window.Current;
            if (window?.CoreWindow == null)
            {
                return;
            }
            window.CoreWindow.PointerCursor = vertical == null
                ? new CoreCursor(CoreCursorType.Arrow, 1)
                : new CoreCursor(vertical.Value ? CoreCursorType.SizeWestEast : CoreCursorType.SizeNorthSouth, 1);
        }

        // Panes and splits whose node left the tree: dropped when the underlying
        // VM was disposed (tab or pane closed), merely collapsed otherwise so a
        // tab switch can show them again without touching the WebView.
        private void PruneStaleVisuals(HashSet<TerminalViewModel> livePanes, HashSet<PaneSplitNode> liveSplits)
        {
            foreach (var node in _splitZones.Keys.Where(n => !liveSplits.Contains(n)).ToList())
            {
                var zone = _splitZones[node];
                PaneRoot.Children.Remove(zone.Line);
                PaneRoot.Children.Remove(zone.Grip);
                _splitZones.Remove(node);
            }

            foreach (var pane in _paneBorders.Keys.Where(p => !livePanes.Contains(p)).ToList())
            {
                var border = _paneBorders[pane];
                if (pane == null || pane.DisposalRequested)
                {
                    PaneRoot.Children.Remove(border);
                    _paneBorders.Remove(pane);
                }
                else
                {
                    border.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static void UpdateLeafBorder(TerminalViewModel tab, TerminalViewModel pane, Border border)
        {
            // Focus indication: a thin 1px accent frame around the active pane.
            // Unfocused panes carry no border, so the only lines between panes
            // are the 1px separators.
            if (tab.HasSplitPanes && pane.IsPaneFocused)
            {
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush;
            }
            else
            {
                border.BorderThickness = new Thickness(0);
                border.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
        }

        private static Brush SeparatorBrush()
        {
            if (Application.Current.Resources["SystemControlForegroundBaseLowBrush"] is SolidColorBrush brush)
            {
                return brush;
            }
            return new SolidColorBrush(Windows.UI.Color.FromArgb(0x30, 0xFF, 0xFF, 0xFF));
        }
    }
}
