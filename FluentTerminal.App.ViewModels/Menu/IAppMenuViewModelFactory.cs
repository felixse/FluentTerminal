using System.Collections.ObjectModel;

namespace FluentTerminal.App.ViewModels.Menu
{
    public interface IAppMenuViewModelFactory
    {
        AppMenuViewModel CreateAppMenuViewModel(MainViewModel mainViewModel, out ExpandableMenuItemViewModel recentItem);

        ObservableCollection<MenuItemViewModelBase> GetRecentMenuSubItems(MainViewModel mainViewModel);
    }
}