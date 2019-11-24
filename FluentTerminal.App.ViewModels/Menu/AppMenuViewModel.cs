using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Menu
{
    public class AppMenuViewModel : ViewModelBase
    {
        #region Properties

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

        #endregion Properties

        #region Constructor

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

        #endregion Constructor

        #region Methods

        public bool EquivalentTo(AppMenuViewModel other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (other == null) return false;

            if (!TabMenuItem.EquivalentTo(other.TabMenuItem) ||
                !RemoteTabMenuItem.EquivalentTo(other.RemoteTabMenuItem) ||
                !QuickTabMenuItem.EquivalentTo(other.QuickTabMenuItem) ||
                !SettingsMenuItem.EquivalentTo(other.SettingsMenuItem) ||
                !AboutMenuItem.EquivalentTo(other.AboutMenuItem) ||
                RecentMenuItems.Count != other.RecentMenuItems.Count)
            {
                return false;
            }

            return !RecentMenuItems.Where((t, i) => !t.EquivalentTo(other.RecentMenuItems[i])).Any();
        }

        #endregion Methods
    }
}