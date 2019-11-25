using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class AppMenuViewModel : ViewModelBase
    {
        public ObservableCollection<MenuItemViewModelBase> Items { get; }

        public AppMenuViewModel(IEnumerable<MenuItemViewModelBase> items = null)
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

        public bool EquivalentTo(AppMenuViewModel other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            if (Items.Count != other.Items.Count) return false;

            return !Items.Where((t, i) => !t.EquivalentTo(other.Items[i])).Any();
        }
    }
}