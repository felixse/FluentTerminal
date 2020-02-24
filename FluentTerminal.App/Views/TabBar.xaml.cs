using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class TabBar : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<TerminalViewModel>), typeof(TabBar), new PropertyMetadata(null));

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(RelayCommand), typeof(TabBar), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TabBar), new PropertyMetadata(null));

        private long _scrollableWidthChangedToken;

        public TabBar()
        {
            InitializeComponent();
            _scrollableWidthChangedToken = ScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ScrollableWidthProperty, OnScrollableWidthChanged);
        }

        private void OnListViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetScrollButtonsEnabledState();
        }

        public void DisposalPrepare()
        {
            ListView.ItemsSource = null;
            ListView.SelectedItem = null;

            ItemsSource = null;
            AddCommand = null;
            SelectedItem = null;

            Bindings.StopTracking();

            ScrollViewer.UnregisterPropertyChangedCallback(ScrollViewer.ScrollableWidthProperty, _scrollableWidthChangedToken);
        }

        public RelayCommand AddCommand
        {
            get { return (RelayCommand)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }

        public ObservableCollection<TerminalViewModel> ItemsSource
        {
            get { return (ObservableCollection<TerminalViewModel>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        private async void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(ListView.SelectedItem is { } item)) return;

            var container = ListView.ContainerFromItem(item);

            while (container == null)
            {
                // We need to ConfigureAwait(true) because the UI thread is needed below.
                await Task.Delay(30).ConfigureAwait(true);

                container = ListView.ContainerFromItem(item);
            }

            ((UIElement)container).StartBringIntoView();

            SetScrollButtonsEnabledState();
        }

        private void OnScrollableWidthChanged(DependencyObject sender, DependencyProperty property)
        {
            if (ScrollViewer.ScrollableWidth > 0)
            {
                ScrollLeftButton.Visibility = Visibility.Visible;
                ScrollRightButton.Visibility = Visibility.Visible;
            }
            else
            {
                ScrollLeftButton.Visibility = Visibility.Collapsed;
                ScrollRightButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnScrollLeftButtonTapped(object sender, RoutedEventArgs e)
        {
            var offset = ScrollViewer.HorizontalOffset - 10;
            ScrollViewer.ChangeView(offset, null, null);
            SetScrollButtonsEnabledState();
        }

        private void OnScrollRightButtonTapped(object sender, RoutedEventArgs e)
        {
            var offset = ScrollViewer.HorizontalOffset + 10;
            ScrollViewer.ChangeView(offset, null, null);
            SetScrollButtonsEnabledState();
        }

        private void SetScrollButtonsEnabledState()
        {
            ScrollLeftButton.IsEnabled = ScrollViewer.HorizontalOffset > 0;
            ScrollRightButton.IsEnabled = ScrollViewer.HorizontalOffset < ScrollViewer.ScrollableWidth;
        }

        #region Drag and drop support

        public static bool ItemWasDropped { get; set; }

        public event EventHandler<NewTabRequestedEventArgs> TabWindowChanged;
        public event EventHandler<TerminalViewModel> TabDraggingCompleted;
        public event TypedEventHandler<ListViewBase, DragItemsCompletedEventArgs> TabDraggedOutside;
        public event EventHandler<bool> TabDraggingChanged;

        private void ListView_DragEnter(object sender, DragEventArgs e)
        {
            Logger.Instance.Debug($"ListView_DragEnter.");
            e.AcceptedOperation = DataPackageOperation.Move;
            if (e.DragUIOverride is { } dragUiOverride)
            {
                dragUiOverride.IsGlyphVisible = false;
                dragUiOverride.Caption = I18N.Translate("DropTabHere");
            }
        }

        private async void ListView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            TabDraggingChanged?.Invoke(this, true);
            ItemWasDropped = false;

            Logger.Instance.Debug($"ListView_DragItemsStarting. e.Data.RequestedOperation: {e.Data.RequestedOperation}. Items count: {e.Items.Count}.");

            var item = e.Items.FirstOrDefault();
            if (item is TerminalViewModel model)
            {
                await model.TrayProcessCommunicationService.PauseTerminalOutputAsync(model.Terminal.Id, true);
                e.Data.Properties.Add(Constants.TerminalViewModelStateId, await model.SerializeAsync());
            }
        }

        private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            TabDraggingChanged?.Invoke(this, false);
            Logger.Instance.Debug($"ListView_DragItemsCompleted. Drop result: {args.DropResult}. Items count: {args.Items.Count}");

            var item = args.Items.FirstOrDefault();
            if (item is TerminalViewModel model)
            {
                if (ItemsSource.Count > 1 && !ItemWasDropped && args.DropResult == DataPackageOperation.None)
                {
                    TabDraggedOutside?.Invoke(sender, args);
                    ItemWasDropped = true;
                }

                if (ItemWasDropped)
                {
                    TabDraggingCompleted?.Invoke(sender, model);
                }

                model.TrayProcessCommunicationService.PauseTerminalOutputAsync(model.Terminal.Id, false);
            }
        }

        private double GetWidth(ListViewItem item)
        {
            return item.ActualWidth + item.Margin.Left + item.Margin.Right;
        }

        private int CalculateDropPosition(DragEventArgs e)
        {
            int index = 0;
            Point position = e.GetPosition(ListView.ItemsPanelRoot);
            for (int i = 0, posInParent = 0; i < ListView.Items.Count; ++i)
            {
                ListViewItem item = (ListViewItem)ListView.ContainerFromIndex(i);
                int itemWidth = (int)GetWidth(item);
                if (posInParent + itemWidth > position.X)
                {
                    index = i;
                    break;
                }
                else
                {
                    posInParent += itemWidth;
                }
            }

            ListViewItem targetItem = (ListViewItem)ListView.ContainerFromIndex(index);
            Point posInItem = e.GetPosition(targetItem);
            if (posInItem.X > GetWidth(targetItem) / 2)
            {
                index++;
            }
            return index;
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {
            Logger.Instance.Debug($"ListView_Drop. e.AcceptedOperation: {e.AcceptedOperation}.");
            int dropIndex = CalculateDropPosition(e);
            Logger.Instance.Debug($"Tab dropped to index: {dropIndex}.");
            TabWindowChanged?.Invoke(sender, new NewTabRequestedEventArgs { DragEventArgs = e, Position = dropIndex });
            ItemWasDropped = true;
        }

        #endregion Drag and drop support
    }
}