using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class MousePageViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ApplicationSettings _applicationSettings;

        public MousePageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaultsAsync().ConfigureAwait(false));
        }

        public bool CopyOnSelect
        {
            get => _applicationSettings.CopyOnSelect;
            set
            {
                if (_applicationSettings.CopyOnSelect != value)
                {
                    _applicationSettings.CopyOnSelect = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public MouseAction MouseRightClickAction
        {
            get => _applicationSettings.MouseRightClickAction;
            set
            {
                if (_applicationSettings.MouseRightClickAction != value)
                {
                    _applicationSettings.MouseRightClickAction = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool MouseRightClickNoneIsSelected
        {
            get => MouseRightClickAction == MouseAction.None;
            set { if (value) MouseRightClickAction = MouseAction.None; }
        }

        public bool MouseRightClickContextMenuIsSelected
        {
            get => MouseRightClickAction == MouseAction.ContextMenu;
            set { if (value) MouseRightClickAction = MouseAction.ContextMenu; }
        }

        public bool MouseRightClickPasteIsSelected
        {
            get => MouseRightClickAction == MouseAction.Paste;
            set { if (value) MouseRightClickAction = MouseAction.Paste; }
        }

        public MouseAction MouseMiddleClickAction
        {
            get => _applicationSettings.MouseMiddleClickAction;
            set
            {
                if (_applicationSettings.MouseMiddleClickAction != value)
                {
                    _applicationSettings.MouseMiddleClickAction = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                }
            }
        }

        public bool MouseMiddleClickNoneIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.None;
            set { if (value) MouseMiddleClickAction = MouseAction.None; }
        }

        public bool MouseMiddleClickContextMenuIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.ContextMenu;
            set { if (value) MouseMiddleClickAction = MouseAction.ContextMenu; }
        }

        public bool MouseMiddleClickPasteIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.Paste;
            set { if (value) MouseMiddleClickAction = MouseAction.Paste; }
        }

        public RelayCommand RestoreDefaultsCommand { get; }

        private async Task RestoreDefaultsAsync()
        {
            var result = await _dialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmRestoreMouseSettings"), DialogButton.OK, DialogButton.Cancel);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultApplicationSettings();
                CopyOnSelect = defaults.CopyOnSelect;
                MouseMiddleClickAction = defaults.MouseMiddleClickAction;
                MouseRightClickAction = defaults.MouseRightClickAction;
            }
        }
    }
}
