using System.Collections.Generic;
using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class AppMenuViewModel : ViewModelBase
    {
        private ObservableCollection<MenuItemViewModelBase> _items;

        public ObservableCollection<MenuItemViewModelBase> MenuItems
        {
            get => _items;
            set => Set(ref _items, value);
        }

        public AppMenuViewModel(IEnumerable<MenuItemViewModelBase> items = null)
        {
            if (items is ObservableCollection<MenuItemViewModelBase> observableItems)
            {
                MenuItems = observableItems;
            }
            else if (items != null)
            {
                MenuItems = new ObservableCollection<MenuItemViewModelBase>(items);
            }
        }
    }
}