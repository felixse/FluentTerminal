using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class ThemeSettings : Page
    {
        public ThemesPageViewModel ViewModel { get; private set; }

        public ThemeSettings()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ThemesPageViewModel viewModel)
            {
                ViewModel = viewModel;
                viewModel.SelectedThemeBackgroundColorChanged += OnSelectedThemeBackgroundColorChanged;
                var theme = ContrastHelper.GetIdealThemeForBackgroundColor(ViewModel.SelectedTheme.Background);
                SetTheme(theme);
            }
        }

        private void OnSelectedThemeBackgroundColorChanged(object sender, string e)
        {
            var theme = ContrastHelper.GetIdealThemeForBackgroundColor(e);
            SetTheme(theme);
        }

        private void SetTheme(ElementTheme theme)
        {
            SetActiveButton.RequestedTheme = theme;
            EditButton.RequestedTheme = theme;
            DeleteButton.RequestedTheme = theme;
            SaveButton.RequestedTheme = theme;
            CancelButton.RequestedTheme = theme;
            ExportButton.RequestedTheme = theme;
            CloneButton.RequestedTheme = theme;

            ContrastHelper.SetTitleBarButtonsForTheme(theme);
        }
    }
}