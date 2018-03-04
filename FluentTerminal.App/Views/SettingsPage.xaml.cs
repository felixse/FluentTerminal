using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views.SettingsPages;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            Root.DataContext = this;

            ApplicationView.PreferredLaunchViewSize = new Windows.Foundation.Size(1024, 768);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;

            ApplicationView.GetForCurrentView().Title = "Settings";

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            CoreTitleBarHeight = coreTitleBar.Height;

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += SettingsPage_CloseRequested;
        }

        public double CoreTitleBarHeight { get; }

        public SettingsViewModel ViewModel { get; private set; }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SettingsViewModel viewModel)
            {
                ViewModel = viewModel;
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationView.SelectedItem = NavigationView.MenuItems.Cast<NavigationViewItemBase>().FirstOrDefault(m => m.Tag.ToString() == "general");
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "general":
                        ContentFrame.Navigate(typeof(GeneralSettings), ViewModel.General);
                        break;

                    case "shell":
                        ContentFrame.Navigate(typeof(ShellSettings), ViewModel.Shell);
                        break;

                    case "themes":
                        ContentFrame.Navigate(typeof(ThemeSettings), ViewModel.Themes);
                        break;

                    case "terminal":
                        ContentFrame.Navigate(typeof(TerminalSettings), ViewModel.Terminal);
                        break;

                    case "keyBindings":
                        ContentFrame.Navigate(typeof(KeyBindingSettings), ViewModel.KeyBindings);
                        break;
                }
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
    }
}