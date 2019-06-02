using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views.SettingsPages;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views
{
    public sealed partial class SettingsPage : Page
    {
        private readonly UISettings _uiSettings;
        private readonly ApplicationViewTitleBar _titleBar;
        private readonly CoreDispatcher _dispatcher;
        private readonly NavigationViewItem hiddenNavigationItem;
        private bool _onThemesPage;

        public SettingsPage()
        {
            InitializeComponent();
            Root.DataContext = this;
            var asd= I18N.Translate("Setting");
            ApplicationView.GetForCurrentView().Title = I18N.Translate("Setting");

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
            CoreTitleBarHeight = coreTitleBar.Height;

            _titleBar = ApplicationView.GetForCurrentView().TitleBar;
            _dispatcher = Window.Current.Dispatcher;

            SetTitleBarColors();

            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += OnColorValuesChanged;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += SettingsPage_CloseRequested;
            
            hiddenNavigationItem = new NavigationViewItem();
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
                ViewModel.AboutPageRequested += OnAboutPageRequested;
            }
        }

        private void OnAboutPageRequested(object sender, System.EventArgs e)
        {
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ContentFrame.Navigate(typeof(About), ViewModel.About);

                // Deselect item in NavigationView (https://stackoverflow.com/a/49082640/4132379)
                NavigationView.MenuItems.Add(hiddenNavigationItem);
                NavigationView.SelectedItem = hiddenNavigationItem;
                NavigationView.SelectedItem = null;
                NavigationView.MenuItems.Remove(hiddenNavigationItem);
            });
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

                    case "mouse":
                        ContentFrame.Navigate(typeof(MouseSettings), ViewModel.Mouse);
                        break;

                    case "sshprofiles":
                        ContentFrame.Navigate(typeof(SshProfileSettings), ViewModel.SshShell);
                        break;
                }
            }

            if (!_onThemesPage)
            {
                SetTitleBarColors();
            }
        }

        private void AboutTapped(object sender, TappedRoutedEventArgs e)
        {
            OnAboutPageRequested(sender, EventArgs.Empty);
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