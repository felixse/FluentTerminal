using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.Models;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels
{
    public class AppMenuViewModelFactory : IAppMenuViewModelFactory
    {
        #region Constants

        private const int RecentItemsMaxCount = 10;

        private static readonly FontFamily SegoeMdl2AssetsFontFamily = new FontFamily("Segoe MDL2 Assets");

        #endregion Constants

        private readonly ICommandHistoryService _commandHistoryService;
        private readonly IDialogService _dialogService;

        public AppMenuViewModelFactory(ICommandHistoryService commandHistoryService, IDialogService dialogService)
        {
            _commandHistoryService = commandHistoryService;
            _dialogService = dialogService;
        }

        public AppMenuViewModel CreateAppMenuViewModel(MainViewModel mainViewModel, out ExpandableMenuItemViewModel recentItem)
        {
            recentItem = new ExpandableMenuItemViewModel(I18N.TranslateWithFallback("Recent.Text", "Recent"),
                I18N.TranslateWithFallback("Recent.Description", "Recently executed commands."),
                new FontIcon {FontFamily = SegoeMdl2AssetsFontFamily, Glyph = "&#xF738;"},
                GetRecentMenuSubItems(mainViewModel));

            return new AppMenuViewModel(new MenuItemViewModelBase[]
            {
                // "New tab" menu item
                new MenuItemViewModel(new RelayCommand(async () => await mainViewModel.AddLocalTabAsync(), keepTargetAlive: true),
                    I18N.TranslateWithFallback("NewTab.Text", "New tab"),
                    I18N.TranslateWithFallback("NewTab.Description", "Opens default profile in a new tab."),
                    new SymbolIcon(Symbol.Add)),

                // "New remote tab" menu item
                new MenuItemViewModel(new RelayCommand(async () => await mainViewModel.AddSshTabAsync(), keepTargetAlive: true),
                    I18N.TranslateWithFallback("NewSshTab.Text", "New remote tab"),
                    I18N.TranslateWithFallback("NewSshTab.Description", "Opens a new SSH or Mosh session in a new tab."),
                    new SymbolIcon(Symbol.Add)),
                
                // "New quick tab" menu item
                new MenuItemViewModel(new RelayCommand(async () => await mainViewModel.AddCustomCommandTabAsync(), keepTargetAlive: true),
                    I18N.TranslateWithFallback("NewQuickTab.Text", "New quick tab"),
                    I18N.TranslateWithFallback("NewQuickTab.Description", "Opens \"Quick Launch\" dialog and starts session in a new tab."),
                    new SymbolIcon(Symbol.Add)),

                // "Settings" menu item
                new MenuItemViewModel(new RelayCommand(mainViewModel.ShowSettings, keepTargetAlive: true),
                    I18N.TranslateWithFallback("Settings.Text", "Settings"),
                    I18N.TranslateWithFallback("Settings.Description", "Opens settings window."),
                    new SymbolIcon(Symbol.Setting)),

                // "Recent >" menu item
                recentItem,

                // "About" menu item
                new MenuItemViewModel(new RelayCommand(async () => await _dialogService.ShowAboutDialogAsync(), keepTargetAlive: true),
                    I18N.TranslateWithFallback("AboutDialog.Title", "About"), null,
                    new FontIcon {FontFamily = SegoeMdl2AssetsFontFamily, Glyph = "&#xE946;"})
            });
        }

        public ObservableCollection<MenuItemViewModelBase> GetRecentMenuSubItems(MainViewModel mainViewModel)
        {
            var recentItems = _commandHistoryService.GetHistoryRecentFirst(top: RecentItemsMaxCount).Select(c =>
            {
                var command = new RelayCommand(async () => await mainViewModel.AddTerminalAsync(c.ShellProfile), keepTargetAlive: true);

                if (c.ShellProfile.KeyBindings?.FirstOrDefault() is KeyBinding keyBinding)
                {
                    return new MenuItemViewModel(new MenuItemKeyBindingViewModel(command, keyBinding), c.Value);
                }

                return new MenuItemViewModel(command, c.Value);
            });

            return new ObservableCollection<MenuItemViewModelBase>(recentItems);
        }
    }
}