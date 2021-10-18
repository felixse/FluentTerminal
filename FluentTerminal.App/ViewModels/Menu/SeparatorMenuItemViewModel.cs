namespace FluentTerminal.App.ViewModels.Menu
{
    public class SeparatorMenuItemViewModel : MenuItemViewModelBase
    {
        public SeparatorMenuItemViewModel()
            : base(string.Empty, string.Empty, null)
        {
        }

        public override bool EquivalentTo(MenuItemViewModelBase other)
        {
            return other is SeparatorMenuItemViewModel;
        }
    }
}
