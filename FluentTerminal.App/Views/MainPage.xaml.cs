using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.EventArgs;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Menu;

namespace FluentTerminal.App.Views
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private AppMenuViewModel _lastMenuViewModel;

        private CoreApplicationViewTitleBar _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public event PropertyChangedEventHandler PropertyChanged;

        public double CoreTitleBarHeight => _coreTitleBar.Height;

        public TimeSpan NoDuration => TimeSpan.Zero;

        public Thickness CoreTitleBarPadding
        {
            get
            {
                if (FlowDirection == FlowDirection.LeftToRight)
                {
                    return new Thickness { Left = _coreTitleBar.SystemOverlayLeftInset, Right = _coreTitleBar.SystemOverlayRightInset };
                }
                else
                {
                    return new Thickness { Left = _coreTitleBar.SystemOverlayRightInset, Right = _coreTitleBar.SystemOverlayLeftInset };
                }
            }
        }

        private MainViewModel _viewModel;

        public MainViewModel ViewModel
        {
            get => _viewModel;
            private set
            {
                var old = _viewModel;

                if (ReferenceEquals(old, value))
                {
                    return;
                }

                if (old != null)
                {
                    old.PropertyChanged -= ViewModelPropertyChanged;
                }

                if (value != null)
                {
                    value.PropertyChanged += ViewModelPropertyChanged;
                }

                _viewModel = value;

                if (value?.MenuViewModel != null)
                {
                    if (_lastMenuViewModel == null)
                    {
                        // The app menu isn't created yet, so we'll go with smaller delay:
                        var unused = CreateAppMenu(200);
                    }
                    else
                    {
                        // The app menu is already created, and now needs to be updated, so we'll go with the default (longer) delay:
                        var unused = CreateAppMenu();
                    }
                }
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (nameof(MainViewModel.MenuViewModel).Equals(e.PropertyName, StringComparison.Ordinal) &&
                _viewModel?.MenuViewModel != null)
            {
                var unused = CreateAppMenu();
            }
        }

        private async Task CreateAppMenu(int delayMilliseconds = 1000)
        {
            if (delayMilliseconds > 0)
            {
                await Task.Delay(delayMilliseconds);
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, CreateAppMenuDispatched);
        }

        // I've had to create such weird manual menu binding because only this way the menu actually updates.
        private void CreateAppMenuDispatched()
        {
            if (!(_viewModel?.MenuViewModel is AppMenuViewModel appMenuViewModel) ||
                ReferenceEquals(appMenuViewModel, _lastMenuViewModel))
            {
                return;
            }

            if ((Hamburger.Content as Button)?.Flyout?.IsOpen ?? false)
            {
                // We don't want to change menu while it's open. Otherwise it would close annoyingly for the user.
                var unused = CreateAppMenu();

                return;
            }

            if (Resources.TryGetValue("AppMenuTemplate", out var value) && value is DataTemplate dataTemplate)
            {
                var appMenu = (Button) dataTemplate.LoadContent();
                appMenu.DataContext = appMenuViewModel;

                Hamburger.Content = appMenu;

                _lastMenuViewModel = appMenuViewModel;
            }
        }

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
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                ViewModel = viewModel;
                ViewModel.Closed += ViewModel_Closed;
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

            _coreTitleBar.LayoutMetricsChanged -= OnLayoutMetricsChanged;

            ViewModel.Closed -= ViewModel_Closed;
            ViewModel = null;
            Root.DataContext = null;
            Window.Current.SetTitleBar(null);

            _coreTitleBar = null;
            Bindings.StopTracking();
            TerminalContainer.Content = null;
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
            _coreTitleBar.LayoutMetricsChanged += OnLayoutMetricsChanged;
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
            if (e.DragUIOverride is DragUIOverride dragUiOverride)
            {
                dragUiOverride.IsGlyphVisible = false;
                dragUiOverride.Caption = I18N.Translate("DropTabHere");
            }
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

        private void MainPage_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var control = e.Key == VirtualKey.Control || Window.Current.CoreWindow.GetKeyState(VirtualKey.Control)
                               .HasFlag(CoreVirtualKeyStates.Down);
            var alt = e.Key == VirtualKey.Menu || Window.Current.CoreWindow.GetKeyState(VirtualKey.Menu)
                          .HasFlag(CoreVirtualKeyStates.Down);
            var shift = e.Key == VirtualKey.Menu || Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift)
                            .HasFlag(CoreVirtualKeyStates.Down);
            var meta = e.Key == VirtualKey.LeftWindows || e.Key == VirtualKey.RightWindows
                                                       || Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftWindows)
                                                           .HasFlag(CoreVirtualKeyStates.Down)
                                                       || Window.Current.CoreWindow.GetKeyState(VirtualKey.RightWindows)
                                                           .HasFlag(CoreVirtualKeyStates.Down);

            ViewModel?.OnWindowKeyDown((int) e.Key, control, alt, shift, meta);
        }
    }
}