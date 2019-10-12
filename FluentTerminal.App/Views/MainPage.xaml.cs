using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.Models;
using System;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.Services.Utilities;

namespace FluentTerminal.App.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

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

        private long _propertyChangedCallbackToken;

        public MainPage()
        {
            InitializeComponent();
            Root.DataContext = this;
            Window.Current.SetTitleBar(TitleBar);
            Loaded += OnLoaded;
            DraggingHappensChanged += MainPage_DraggingHappensChanged;
            Window.Current.Activated += OnWindowActivated;
            _propertyChangedCallbackToken = RegisterPropertyChangedCallback(RequestedThemeProperty, OnRequestedThemeProperty);

            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.Auto;
        }

        private void OnRequestedThemeProperty(DependencyObject sender, DependencyProperty dp)
        {
            ContrastHelper.SetTitleBarButtonsForTheme(RequestedTheme);
        }

        private async void MainPage_DraggingHappensChanged(object sender, bool e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (sender != TopTabBar && sender != BottomTabBar)
                {
                    DraggingHappensFromAnotherWindow = e;
                }
                DraggingHappens = e;
            });
         }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MainViewModel viewModel)
            {
                if (ViewModel != null)
                {
                    ViewModel.SelectedTerminal.ThemeChanged -= OnTerminalThemeChanged;
                }

                ViewModel = viewModel;
                ViewModel.Closed += ViewModel_Closed;

                SetGridBackgroundTheme(ViewModel.SelectedTerminal.TerminalTheme);

                ViewModel.SelectedTerminal.ThemeChanged += OnTerminalThemeChanged;
            }
            base.OnNavigatedTo(e);
        }

        private void ViewModel_Closed(object sender, EventArgs e)
        {
            TopTabBar.TabDraggedOutside -= TabBar_TabDraggedOutside;
            TopTabBar.TabWindowChanged -= TabView_Drop;
            TopTabBar.TabDraggingCompleted -= TabBar_TabDraggingCompleted;
            TopTabBar.TabDraggingChanged -= TabBar_TabDraggingChanged;

            BottomTabBar.TabDraggedOutside -= TabBar_TabDraggedOutside;
            BottomTabBar.TabWindowChanged -= TabView_Drop;
            BottomTabBar.TabDraggingCompleted -= TabBar_TabDraggingCompleted;
            BottomTabBar.TabDraggingChanged -= TabBar_TabDraggingChanged;

            TopTabBar.DisposalPrepare();
            BottomTabBar.DisposalPrepare();

            UnregisterPropertyChangedCallback(RequestedThemeProperty, _propertyChangedCallbackToken);

            Loaded -= OnLoaded;
            DraggingHappensChanged -= MainPage_DraggingHappensChanged;
            Window.Current.Activated -= OnWindowActivated;

            coreTitleBar.LayoutMetricsChanged -= OnLayoutMetricsChanged;

            ViewModel.Closed -= ViewModel_Closed;
            ViewModel = null;
            Root.DataContext = null;
            Window.Current.SetTitleBar(null);

            coreTitleBar = null;
            Bindings.StopTracking();
            TerminalContainer.Content = null;
        }

        private void OnTerminalThemeChanged(object sender, TerminalTheme e)
        {
            SetGridBackgroundTheme(e);
        }

        private void SetGridBackgroundTheme(TerminalTheme terminalTheme)
        {
            var color = terminalTheme.Colors.Background;
            var imageFile = terminalTheme.BackgroundImage;

            Brush backgroundBrush;

            if (imageFile != null)
            {
                backgroundBrush = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(
                        imageFile.Path,
                        UriKind.Absolute)),
                };
            }
            else
            {
                backgroundBrush = new AcrylicBrush
                {
                    BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                    FallbackColor = color.FromString(),
                    TintColor = color.FromString(),
                    TintOpacity = ViewModel.BackgroundOpacity
                };
            }

            Root.Background = backgroundBrush;
        }

        private void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated && TerminalContainer.Content is TerminalView terminal)
            {
                terminal.ViewModel?.FocusTerminal();
                ViewModel.FocusWindow();
            }
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

        public static readonly DependencyProperty DraggingHappensFromAnotherWindowProperty =
            DependencyProperty.Register(nameof(DraggingHappensFromAnotherWindow), typeof(bool), typeof(MainPage), new PropertyMetadata(null));

        public bool DraggingHappensFromAnotherWindow
        {
            get { return (bool)GetValue(DraggingHappensFromAnotherWindowProperty); }
            set { SetValue(DraggingHappensFromAnotherWindowProperty, value); }
        }

        public static readonly DependencyProperty DraggingHappensProperty =
            DependencyProperty.Register(nameof(DraggingHappens), typeof(bool), typeof(MainPage), new PropertyMetadata(null));

        public bool DraggingHappens
        {
            get { return (bool)GetValue(DraggingHappensProperty); }
            set { SetValue(DraggingHappensProperty, value); }
        }

        static private event EventHandler<bool> DraggingHappensChanged;

        private void TabDropArea_DragEnter(object sender, DragEventArgs e)
        {
            Logger.Instance.Debug("TabDropArea_DragEnter.");
            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.Caption = I18N.Translate("DropTabHere");
        }

        private async void TabDropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.TryGetValue(Constants.TerminalViewModelStateId, out object stateObj) && stateObj is string terminalViewModelState)
            {
                TabBar.ItemWasDropped = true;
                await ViewModel.AddTerminalAsync(terminalViewModelState, ViewModel.Terminals.Count);
            }
        }

        private void TabBar_TabDraggingChanged(object sender, bool e)
        {
            DraggingHappensChanged?.Invoke(sender, e);
        }
    }
}