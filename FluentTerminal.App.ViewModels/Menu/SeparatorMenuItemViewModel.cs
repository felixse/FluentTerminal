using System;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class SeparatorMenuItemViewModel : MenuItemViewModelBase, IEquatable<SeparatorMenuItemViewModel>
    {
        public SeparatorMenuItemViewModel()
            : base(string.Empty, string.Empty, null)
        {
        }

        public bool Equals(SeparatorMenuItemViewModel other)
        {
            return true;
        }
    }
}
