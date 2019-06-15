using FluentTerminal.App.ViewModels.Settings;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;

namespace FluentTerminal.App.Views.SettingsPages
{
    public sealed partial class About : Page
    {
        public AboutPageViewModel ViewModel { get; private set; }

        public About()
        {
            InitializeComponent();
            CreatedBy.Inlines.Add(new Run { Text = "Created by " });
            CreatedBy.Inlines.Add(new Italic { Inlines = { new Run { Text = "felixse " } } });
            CreatedBy.Inlines.Add(new Run { Text = "and " });
            CreatedBy.Inlines.Add(new Hyperlink { Inlines = { new Run { Text = "contributors" } }, NavigateUri = new Uri("https://github.com/felixse/FluentTerminal/graphs/contributors") });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is AboutPageViewModel viewModel)
            {
                ViewModel = viewModel;
            }
            ViewModel.OnNavigatedTo();
        }
    }
}