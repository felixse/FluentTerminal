using System;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class ToggleMenuItemViewModel : MenuItemViewModelBase
    {
        public object BindingSource { get; }
        public string BindingPath { get; }

        public ToggleMenuItemViewModel(string text, object bindingSource, string bindingPath = null, string description = null, Mdl2Icon icon = null)
            : base(text, description, icon)
        {
            BindingSource = bindingSource ?? throw new ArgumentNullException(nameof(bindingSource));
            BindingPath = bindingPath;
        }

        public override bool EquivalentTo(MenuItemViewModelBase other)
        {
            if (!base.EquivalentTo(other)) return false;

            if (!(other is ToggleMenuItemViewModel toggleMenuItem)) return false;

            return BindingSource == toggleMenuItem.BindingSource && BindingPath == toggleMenuItem.BindingPath;
        }
    }
}
