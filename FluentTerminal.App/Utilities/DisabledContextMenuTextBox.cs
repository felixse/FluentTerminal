using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Utilities
{
    public sealed class DisabledContextMenuTextBox : TextBox
    {
        public DisabledContextMenuTextBox() : base()
        {
            ContextMenuOpening += OnContextMenuOpening;
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }
    }
}