using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class ExpandableMenuItemViewModel : MenuItemViewModelBase
    {
        public ObservableCollection<MenuItemViewModelBase> SubItems { get; }

        public ExpandableMenuItemViewModel(string text, IEnumerable<MenuItemViewModelBase> subItems = null,
            string description = null, Mdl2Icon icon = null) : base(text, description, icon)
        {
            if (subItems == null)
            {
                SubItems = new ObservableCollection<MenuItemViewModelBase>();
            }
            else if (subItems is ObservableCollection<MenuItemViewModelBase> observableSubItems)
            {
                SubItems = observableSubItems;
            }
            else
            {
                SubItems = new ObservableCollection<MenuItemViewModelBase>(subItems);
            }
        }

        public override bool EquivalentTo(MenuItemViewModelBase other)
        {
            if (!base.EquivalentTo(other)) return false;

            if (!(other is ExpandableMenuItemViewModel otherExpandable) ||
                SubItems.Count != otherExpandable.SubItems.Count)
            {
                return false;
            }

            return !SubItems.Where((t, i) => !t.EquivalentTo(otherExpandable.SubItems[i])).Any();
        }
    }
}