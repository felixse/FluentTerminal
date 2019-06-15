using FluentTerminal.App.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class TabBar : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<TerminalViewModel>), typeof(TabBar), new PropertyMetadata(null));

        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register(nameof(AddCommand), typeof(RelayCommand), typeof(TabBar), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TabBar), new PropertyMetadata(null));

        public TabBar()
        {
            InitializeComponent();
            ScrollViewer.RegisterPropertyChangedCallback(ScrollViewer.ScrollableWidthProperty, OnScrollableWidthChanged);
            ListView.SelectionChanged += OnListViewSelectionChanged;
            ScrollLeftButton.Tapped += OnScrollLeftButtonTapped;
            ScrollRightButton.Tapped += OnScrollRightButtonTapped;
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

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ListView.SelectedItem;
            if (item != null)
            {
                var container = ListView.ContainerFromItem(item);

                if (container != null)
                {
                    ((UIElement)container).StartBringIntoView();
                    SetScrollButtonsEnabledState();
                }
                else
                {
                    Task.Run(async () =>
                    {
                        do
                        {
                            await Task.Delay(50);
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => container = ListView.ContainerFromItem(item));
                        }
                        while (container == null);

                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            ((UIElement)container).StartBringIntoView();
                            SetScrollButtonsEnabledState();
                        });
                    });
                }
            }
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
    }
}