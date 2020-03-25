using System;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class RadioMenuItemViewModel : MenuItemViewModelBase
    {
        public object BindingSource { get; }
        public string BindingPath { get; }
        public string GroupName { get; }

        public RadioMenuItemViewModel(string text, string groupName, object bindingSource, string bindingPath = null, string description = null, Mdl2Icon icon = null)
            : base(text, description, icon)
        {
            GroupName = groupName ?? throw new ArgumentNullException(nameof(groupName));
            BindingSource = bindingSource ?? throw new ArgumentNullException(nameof(bindingSource));
            BindingPath = bindingPath;
        }

        public override bool EquivalentTo(MenuItemViewModelBase other)
        {
            if (!base.EquivalentTo(other)) return false;

            if (!(other is RadioMenuItemViewModel radioMenuItem)) return false;

            return GroupName == radioMenuItem.GroupName &&  BindingSource == radioMenuItem.BindingSource && BindingPath == radioMenuItem.BindingPath;
        }
    }
}
