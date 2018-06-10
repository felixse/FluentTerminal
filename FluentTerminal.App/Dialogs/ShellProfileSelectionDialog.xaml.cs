using FluentTerminal.App.Services;
using FluentTerminal.App.Utilities;
using FluentTerminal.Models;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class ShellProfileSelectionDialog : ContentDialog
    {
        public IEnumerable<ShellProfile> Profiles { get; }

        public ShellProfile SelectedProfile { get; private set; }

        public ShellProfileSelectionDialog(ISettingsService settingsService)
        {
            Profiles = settingsService.GetShellProfiles();
            SelectedProfile = Profiles.First();
            this.InitializeComponent();
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }
    }
}
