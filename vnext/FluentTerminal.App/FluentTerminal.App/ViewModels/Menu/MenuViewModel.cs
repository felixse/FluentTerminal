using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class MenuViewModel : ObservableObject
    {
        public ObservableCollection<MenuItemViewModelBase> Items { get; }

        public MenuViewModel(IEnumerable<MenuItemViewModelBase> items = null)
        {
            if (items == null)
            {
                Items = new ObservableCollection<MenuItemViewModelBase>();
            }
            else if (items is ObservableCollection<MenuItemViewModelBase> observableItems)
            {
                Items = observableItems;
            }
            else
            {
                Items = new ObservableCollection<MenuItemViewModelBase>(items);
            }
        }

        public bool EquivalentTo(MenuViewModel other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            if (Items.Count != other.Items.Count) return false;

            return !Items.Where((t, i) => !t.EquivalentTo(other.Items[i])).Any();
        }
    }
}