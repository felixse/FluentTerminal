using FluentTerminal.App.Services.Dialogs;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml.Input;
using Windows.System;
using FluentTerminal.App.Services.Utilities;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class InputDialog : ContentDialog, IInputDialog
    {

        private bool EnterPressed = false;
        public string Input { get; private set; }

        public InputDialog()
        {
            this.InitializeComponent();
            this.PrimaryButtonText = I18N.Translate("OK");
            this.CloseButtonText = I18N.Translate("Cancel");
        }

        public async Task<string> GetInput()
        {
            var result = await ShowAsync();
            if (result == ContentDialogResult.Primary || EnterPressed)
            {
                return Input;
            }
            return null;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }

        void Dialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
           if (e.Key == VirtualKey.Enter)
           {
                EnterPressed = true;
                Hide();
           }
        }
    }
}
