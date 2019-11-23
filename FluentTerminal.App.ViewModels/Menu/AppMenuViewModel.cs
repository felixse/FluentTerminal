using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class AppMenuViewModel : ViewModelBase
    {
        public MenuItemViewModel TabMenuItem { get; }

        public MenuItemViewModel RemoteTabMenuItem { get; }

        public MenuItemViewModel QuickTabMenuItem { get; }

        public MenuItemViewModel SettingsMenuItem { get; }

        public MenuItemViewModel AboutMenuItem { get; }

        private ObservableCollection<MenuItemViewModel> _recentMenuItems;

        public ObservableCollection<MenuItemViewModel> RecentMenuItems
        {
            get => _recentMenuItems;
            set => Set(ref _recentMenuItems, value);
        }

        public AppMenuViewModel(MenuItemViewModel tabMenuItem, MenuItemViewModel remoteTabMenuItem,
            MenuItemViewModel quickTabMenuItem, MenuItemViewModel settingsMenuItem, MenuItemViewModel aboutMenuItem,
            ObservableCollection<MenuItemViewModel> recentMenuItems)
        {
            TabMenuItem = tabMenuItem;
            RemoteTabMenuItem = remoteTabMenuItem;
            QuickTabMenuItem = quickTabMenuItem;
            SettingsMenuItem = settingsMenuItem;
            AboutMenuItem = aboutMenuItem;
            _recentMenuItems = recentMenuItems;
        }
    }
}