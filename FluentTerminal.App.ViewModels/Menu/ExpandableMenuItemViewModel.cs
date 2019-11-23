using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class ExpandableMenuItemViewModel : MenuItemViewModelBase
    {
        private ObservableCollection<MenuItemViewModelBase> _subItems;

        public ObservableCollection<MenuItemViewModelBase> SubItems
        {
            get => _subItems;
            set => Set(ref _subItems, value);
        }

        public ExpandableMenuItemViewModel(string text, string description = null, object icon = null,
            IEnumerable<MenuItemViewModelBase> subItems = null) : base(text, description, icon)
        {
            if (subItems is ObservableCollection<MenuItemViewModelBase> observableSubItems)
            {
                _subItems = observableSubItems;
            }
            else if (subItems != null)
            {
                _subItems = new ObservableCollection<MenuItemViewModelBase>(subItems);
            }
        }
    }
}