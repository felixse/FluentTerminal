using System;
using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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
                viewModel.SelectedThemeBackgroundImageChanged += OnSelectedThemeBackgroundImageChanged;

                var theme = ContrastHelper.GetIdealThemeForBackgroundColor(ViewModel.SelectedTheme.Background);

                SetTheme(theme);
                SetGridBackground(ViewModel.SelectedTheme.Background, ViewModel.SelectedTheme.BackgroundThemeFile);
            }
        }

        private void OnSelectedThemeBackgroundImageChanged(object sender, ImageFile e)
        {
            SetGridBackground(ViewModel.SelectedTheme.Background, e);
        }

        private void OnSelectedThemeBackgroundColorChanged(object sender, string e)
        {
            var theme = ContrastHelper.GetIdealThemeForBackgroundColor(e);
            SetTheme(theme);

            SetGridBackground(e, ViewModel.SelectedTheme.BackgroundThemeFile);
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

        private void SetGridBackground(string color, ImageFile imageFile)
        {
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
    }
}