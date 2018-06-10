using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views.SettingsPages;
using System.Linq;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using FluentTerminal.App.Utilities;

namespace FluentTerminal.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly UISettings _uiSettings;
        private readonly ApplicationViewTitleBar _titleBar;
        private readonly CoreDispatcher _dispatcher;
        private bool _onThemesPage;

        public SettingsPage()
        {
            InitializeComponent();
            Root.DataContext = this;

            ApplicationView.GetForCurrentView().Title = "Settings";

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            CoreTitleBarHeight = coreTitleBar.Height;

            _titleBar = ApplicationView.GetForCurrentView().TitleBar;
            _dispatcher = Window.Current.Dispatcher;

            SetTitleBarColors();

            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += SettingsPage_CloseRequested;
        }

        private void OnColorValuesChanged(UISettings sender, object args)
        {
            if (_onThemesPage == false)
            {
                SetTitleBarColors();
            }
        }

        private IAsyncAction SetTitleBarColors()
        {
            return _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var theme = Application.Current.RequestedTheme == ApplicationTheme.Light ? ElementTheme.Light : ElementTheme.Dark;
                ContrastHelper.SetTitleBarButtonsForTheme(theme);
            });
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
            _onThemesPage = false;

            if (args.SelectedItem is NavigationViewItem item)
            {
                switch (item.Tag)
                {
                    case "general":
                        ContentFrame.Navigate(typeof(GeneralSettings), ViewModel.General);
                        break;

                    case "profiles":
                        ContentFrame.Navigate(typeof(ShellProfileSettings), ViewModel.Shell);
                        break;

                    case "themes":
                        ContentFrame.Navigate(typeof(ThemeSettings), ViewModel.Themes);
                        _onThemesPage = true;
                        break;

                    case "terminal":
                        ContentFrame.Navigate(typeof(TerminalSettings), ViewModel.Terminal);
                        break;

                    case "keyBindings":
                        ContentFrame.Navigate(typeof(KeyBindingSettings), ViewModel.KeyBindings);
                        break;
                }
            }

            if (!_onThemesPage)
            {
                SetTitleBarColors();
            }
        }

        private void SettingsPage_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            ViewModel.Close();
        }

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitle.Margin = new Thickness(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset + 12, 8, 0, 0);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryResizeView(new Size { Width = 800, Height = 600 });
        }
    }
}