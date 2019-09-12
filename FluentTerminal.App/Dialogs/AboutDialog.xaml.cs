using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Dialogs;
using FluentTerminal.App.Utilities;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class AboutDialog : ContentDialog, IAboutDialog
    {
        public AboutDialog(ISettingsService settingsService, IUpdateService updateService)
        {
            this.InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);

            CreatedBy.Inlines.Add(new Run { Text = "Created by " });
            CreatedBy.Inlines.Add(new Italic { Inlines = { new Run { Text = "felixse " } } });
            CreatedBy.Inlines.Add(new Run { Text = "and " });
            CreatedBy.Inlines.Add(new Hyperlink { Inlines = { new Run { Text = "contributors" } }, NavigateUri = new Uri("https://github.com/felixse/FluentTerminal/graphs/contributors") });

            CurrentVersion.Text = updateService.GetCurrentVersion().ToString();
            ReleaseNotesHyperlink.NavigateUri = new Uri("https://github.com/felixse/FluentTerminal/releases/tag/" + CurrentVersion.Text);
        }

        public new Task ShowAsync()
        {
            return base.ShowAsync().AsTask();
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
