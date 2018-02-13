using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views.SettingsPages;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public double CoreTitleBarHeight { get; }

        public SettingsViewModel ViewModel { get; private set; }

        public SettingsPage()
        {
            InitializeComponent();
            Root.DataContext = this;

            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titleBar.ButtonForegroundColor = (Color)this.Resources["SystemBaseHighColor"];

            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1024, 768);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            ApplicationView.GetForCurrentView().Title = "Settings";
            

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            CoreTitleBarHeight = coreTitleBar.Height;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += SettingsPage_CloseRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SettingsViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }

        private void SettingsPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            App.Instance.SettingsWindowClosed();
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitle.Margin = new Thickness(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset + 12, 8, 0, 0);
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView.SelectedItem = NavigationView.MenuItems.Cast<NavigationViewItemBase>().FirstOrDefault(m => m.Tag.ToString() == "shell");
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "general":
                        ContentFrame.Navigate(typeof(GeneralSettings), ViewModel);
                        break;
                    case "shell":
                        ContentFrame.Navigate(typeof(ShellSettings), ViewModel);
                        break;
                    case "themes":
                        ContentFrame.Navigate(typeof(ThemeSettings), ViewModel);
                        break;
                    case "font":
                        ContentFrame.Navigate(typeof(FontSettings), ViewModel);
                        break;
                    case "keyBindings":
                        ContentFrame.Navigate(typeof(KeyBindingSettings), ViewModel);
                        break;
                }
            }            
        }
    }
}
