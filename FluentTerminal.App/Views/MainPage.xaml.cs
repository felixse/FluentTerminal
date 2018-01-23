using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private readonly CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

        public event PropertyChangedEventHandler PropertyChanged;

        public double CoreTitleBarHeight => coreTitleBar.Height;

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

        public MainPage()
        {
            InitializeComponent();
            Root.DataContext = this;
            SetTitleBarColors();
            Window.Current.SetTitleBar(TitleBar);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Window.Current.Activated += OnWindowActivated;
        }

        private async void OnWindowActivated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated && RootFrame.Content is TerminalPage terminal)
            {
                await terminal.FocusWebView();
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
            RootFrame.Navigate(typeof(TerminalPage));
        }

        private void OnLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object e)
        {
            UpdateLayoutMetrics();
        }

        private void SetTitleBarColors()
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveForegroundColor = Colors.White;
            titleBar.ButtonPressedForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Color.FromArgb(24, 255, 255, 255);
            titleBar.ButtonPressedBackgroundColor = Color.FromArgb(48, 255, 255, 255);
        }

        private void UpdateLayoutMetrics()
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CoreTitleBarHeight)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(CoreTitleBarPadding)));
            }
        }

        private async void OnSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            await App.Instance.ShowSettings();
        }
    }
}
