using FluentTerminal.App.Converters;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Views
{
    // Renders a tab's pane tree (issue #501) as a recursive Grid.
    // Leaves reuse TerminalView instances through the shared
    // TerminalViewModelToViewConverter so disposal works exactly like tabs.
    public sealed partial class TerminalPaneView : UserControl
    {
        private const double SplitterSize = 4;

        public static readonly DependencyProperty TabProperty =
            DependencyProperty.Register(nameof(Tab), typeof(TerminalViewModel), typeof(TerminalPaneView), new PropertyMetadata(null, OnTabChanged));

        private TerminalViewModelToViewConverter _viewConverter;

        public TerminalPaneView()
        {
            InitializeComponent();
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
            }
            view.Rebuild();
            if (e.NewValue is TerminalViewModel newTab)
            {
                newTab.PaneLayoutChanged += view.OnPaneLayoutChanged;
            }
        }

        private void OnPaneLayoutChanged(object sender, EventArgs e)
        {
            // Layout changes can originate off the UI thread (tray callbacks).
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Rebuild);
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

        public void Rebuild()
        {
            PaneRoot.Children.Clear();
            PaneRoot.RowDefinitions.Clear();
            PaneRoot.ColumnDefinitions.Clear();

            var tab = Tab;
            var converter = ViewConverter;
            if (tab == null || converter == null)
            {
                return;
            }

            var root = tab.TabRoot;
            UIElement content = !root.HasSplitPanes || root.PaneTreeRoot == null
                ? BuildLeaf(root, root.GetAllPanes().FirstOrDefault() ?? root)
                : BuildNode(root, root.PaneTreeRoot);
            PaneRoot.Children.Add(content);

            // Re-parenting cached views can drop input focus; restore it.
            // FocusTerminal only raises Activated (no layout event), so no loop.
            root.FocusedPane?.FocusTerminal();
        }

        private UIElement BuildNode(TerminalViewModel tab, PaneNode node)
        {
            if (node is PaneLeafNode leaf)
            {
                return BuildLeaf(tab, tab.GetPaneByLeafId(leaf.Id) ?? tab);
            }

            var split = (PaneSplitNode)node;
            var grid = new Grid();

            if (split.Orientation == SplitOrientation.Vertical)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(split.Ratio, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - split.Ratio, GridUnitType.Star) });

                var first = BuildNode(tab, split.First);
                Grid.SetColumn(first, 0);
                grid.Children.Add(first);

                var splitter = new GridSplitter { ResizeDirection = GridResizeDirection.Columns, Width = SplitterSize };
                Grid.SetColumn(splitter, 1);
                grid.Children.Add(splitter);

                var second = BuildNode(tab, split.Second);
                Grid.SetColumn(second, 2);
                grid.Children.Add(second);

                grid.SizeChanged += (s, e) => PersistRatio(split, grid, vertical: true);
            }
            else
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(split.Ratio, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1 - split.Ratio, GridUnitType.Star) });

                var first = BuildNode(tab, split.First);
                Grid.SetRow(first, 0);
                grid.Children.Add(first);

                var splitter = new GridSplitter { ResizeDirection = GridResizeDirection.Rows, Height = SplitterSize };
                Grid.SetRow(splitter, 1);
                grid.Children.Add(splitter);

                var second = BuildNode(tab, split.Second);
                Grid.SetRow(second, 2);
                grid.Children.Add(second);

                grid.SizeChanged += (s, e) => PersistRatio(split, grid, vertical: false);
            }

            return grid;
        }

        private UIElement BuildLeaf(TerminalViewModel tab, TerminalViewModel pane)
        {
            var view = ViewConverter.Convert(pane, null, null, null) as UIElement;
            var border = new Border
            {
                BorderThickness = new Thickness(2),
                Child = view
            };
            UpdateLeafBorder(tab, pane, border);

            // WebView hosts its own input surface: listen for focus (even handled)
            // so clicking a pane moves the active-pane highlight.
            border.AddHandler(GotFocusEvent, new RoutedEventHandler((s, e) => tab.FocusPane(pane)), true);
            return border;
        }

        private static void UpdateLeafBorder(TerminalViewModel tab, TerminalViewModel pane, Border border)
        {
            if (tab.HasSplitPanes && pane.IsPaneFocused)
            {
                border.BorderBrush = Application.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush;
            }
            else
            {
                border.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
        }

        private static void PersistRatio(PaneSplitNode split, Grid grid, bool vertical)
        {
            if (vertical)
            {
                if (grid.ColumnDefinitions.Count < 3)
                {
                    return;
                }
                var first = grid.ColumnDefinitions[0].ActualWidth;
                var total = first + grid.ColumnDefinitions[2].ActualWidth;
                if (total > 0)
                {
                    split.Ratio = first / total;
                }
            }
            else
            {
                if (grid.RowDefinitions.Count < 3)
                {
                    return;
                }
                var first = grid.RowDefinitions[0].ActualHeight;
                var total = first + grid.RowDefinitions[2].ActualHeight;
                if (total > 0)
                {
                    split.Ratio = first / total;
                }
            }
        }
    }
}
