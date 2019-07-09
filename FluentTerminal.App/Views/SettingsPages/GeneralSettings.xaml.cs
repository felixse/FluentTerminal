using System;
using FluentTerminal.App.ViewModels.Settings;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class GeneralSettings : Page
    {
        public GeneralPageViewModel ViewModel { get; private set; }

        public GeneralSettings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GeneralPageViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.OnNavigatedTo();
            }
        }

        private async void BrowseButtonOnClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new FolderPicker() { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            picker.FileTypeFilter.Add(".whatever"); // else a ComException is thrown
            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                ViewModel.LogDirectoryPath = folder?.Path;
            }
        }
    }
}