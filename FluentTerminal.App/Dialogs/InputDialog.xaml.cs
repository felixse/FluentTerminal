using FluentTerminal.App.Services.Dialogs;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System;

namespace FluentTerminal.App.Dialogs
{
    public sealed partial class InputDialog : ContentDialog, IInputDialog
    {
        public string Input { get; private set; }

        public InputDialog()
        {
            this.InitializeComponent();
        }

        public async Task<string> GetInput()
        {
            var result = await ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return Input;
            }
            return null;
        }

        public void SetTitle(string title)
        {
            Title = title;
        }
    }
}
