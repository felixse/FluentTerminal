using FluentTerminal.App.Services.Dialogs;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Input;
using Windows.System;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.Services;
using FluentTerminal.App.Utilities;

namespace FluentTerminal.App.Dialogs
{
    // ReSharper disable once RedundantExtendsListEntry
    public sealed partial class InputDialog : ContentDialog, IInputDialog
    {

        // ReSharper disable once RedundantDefaultMemberInitializer
        private bool _enterPressed = false;
        public string Input { get; private set; }

        public InputDialog(ISettingsService settingsService)
        {
            this.InitializeComponent();
            this.PrimaryButtonText = I18N.Translate("OK");
            this.CloseButtonText = I18N.Translate("Cancel");
            var currentTheme = settingsService.GetCurrentTheme();
            RequestedTheme = ContrastHelper.GetIdealThemeForBackgroundColor(currentTheme.Colors.Background);
        }

        public Task<string> GetInput()
        {
            return ShowAsync().AsTask()
                .ContinueWith(t => t.Result == ContentDialogResult.Primary || _enterPressed ? Input : null,
                    TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        void Dialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
           if (e.Key == VirtualKey.Enter)
           {
                _enterPressed = true;
                Hide();
           }
        }
    }
}
