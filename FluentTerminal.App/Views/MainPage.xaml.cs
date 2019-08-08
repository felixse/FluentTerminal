using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;

namespace FluentTerminal.App.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public event PropertyChangedEventHandler PropertyChanged;

        public double CoreTitleBarHeight => coreTitleBar.Height;

        public TimeSpan NoDuration => TimeSpan.Zero;

        public Thickness CoreTitleBarPadding
        {
            get
            {
                if (FlowDirection == FlowDirection.LeftToRight)
                {
                    return new Thickness { Left = coreTitleBar.SystemOverlayLeftInset, Right = coreTitleBar.SystemOverlayRightInset };
                }
                else
                {
                    return new Thickness { Left = coreTitleBar.SystemOverlayRightInset, Right = coreTitleBar.SystemOverlayLeftInset };
                }
            }
        }

        public MainViewModel ViewModel { get; private set; }

        public MainPage()
        {
            InitializeComponent();
            Root.DataContext = this;
            Window.Current.SetTitleBar(TitleBar);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Window.Current.Activated += OnWindowActivated;
            RegisterPropertyChangedCallback(RequestedThemeProperty, (s, e) =>
            {
                ContrastHelper.SetTitleBarButtonsForTheme(RequestedTheme);
            });

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated && TerminalContainer.Content is TerminalView terminal)
            {
                terminal.ViewModel.FocusTerminal();
                ViewModel.FocusWindow();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            coreTitleBar.LayoutMetricsChanged -= OnLayoutMetricsChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            coreTitleBar.LayoutMetricsChanged += OnLayoutMetricsChanged;
            UpdateLayoutMetrics();
        }

        private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object e)
        {
            UpdateLayoutMetrics();
        }

        private void UpdateLayoutMetrics()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CoreTitleBarHeight)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CoreTitleBarPadding)));
            }
        }

        private async void TabView_Drop(object sender, NewTabRequestedEventArgs e)
        {
            if (e.DragEventArgs.DataView.Properties.TryGetValue(Constants.TerminalViewModelStateId, out object stateObj) && stateObj is string terminalViewModelState)
            {
                await ViewModel.AddTerminalAsync(terminalViewModelState, e.Position);
            }
        }

        private void TabBar_TabDraggedOutside(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var item = args.Items.FirstOrDefault();
            if (item is TerminalViewModel model)
            {
                ViewModel.TearOffTab(model);
            }
        }

        private void TabBar_TabDraggingCompleted(object sender, TerminalViewModel model)
        {
            int position = ViewModel.Terminals.IndexOf(model);
            ViewModel.Terminals.Remove(model);
            if (ViewModel.Terminals.Count > 0)
            {
                ViewModel.SelectedTerminal = ViewModel.Terminals[Math.Max(0, position - 1)];
            }
            else
            {
                ViewModel.ApplicationView.TryClose();
            }
        }
    }
}