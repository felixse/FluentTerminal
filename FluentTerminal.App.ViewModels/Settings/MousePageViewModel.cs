using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class MousePageViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ApplicationSettings _applicationSettings;
        private bool _editingMouseRightClickAction;
        private bool _editingMouseMiddleClickAction;

        public MousePageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _applicationSettings = _settingsService.GetApplicationSettings();

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));
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
                if (_applicationSettings.MouseRightClickAction != value && !_editingMouseRightClickAction)
                {
                    _editingMouseRightClickAction = true;
                    _applicationSettings.MouseRightClickAction = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MouseRightClickNoneIsSelected));
                    RaisePropertyChanged(nameof(MouseRightClickContextMenuIsSelected));
                    RaisePropertyChanged(nameof(MouseRightClickPasteIsSelected));
                    _editingMouseRightClickAction = false;
                }
            }
        }

        public bool MouseRightClickNoneIsSelected
        {
            get => MouseRightClickAction == MouseAction.None;
            set => MouseRightClickAction = MouseAction.None;
        }

        public bool MouseRightClickContextMenuIsSelected
        {
            get => MouseRightClickAction == MouseAction.ContextMenu;
            set => MouseRightClickAction = MouseAction.ContextMenu;
        }

        public bool MouseRightClickPasteIsSelected
        {
            get => MouseRightClickAction == MouseAction.Paste;
            set => MouseRightClickAction = MouseAction.Paste;
        }

        public MouseAction MouseMiddleClickAction
        {
            get => _applicationSettings.MouseMiddleClickAction;
            set
            {
                if (_applicationSettings.MouseMiddleClickAction != value && !_editingMouseMiddleClickAction)
                {
                    _editingMouseMiddleClickAction = true;
                    _applicationSettings.MouseMiddleClickAction = value;
                    _settingsService.SaveApplicationSettings(_applicationSettings);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(MouseMiddleClickNoneIsSelected));
                    RaisePropertyChanged(nameof(MouseMiddleClickContextMenuIsSelected));
                    RaisePropertyChanged(nameof(MouseMiddleClickPasteIsSelected));
                    _editingMouseMiddleClickAction = false;
                }
            }
        }

        public bool MouseMiddleClickNoneIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.None;
            set => MouseMiddleClickAction = MouseAction.None;
        }

        public bool MouseMiddleClickContextMenuIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.ContextMenu;
            set => MouseMiddleClickAction = MouseAction.ContextMenu;
        }

        public bool MouseMiddleClickPasteIsSelected
        {
            get => MouseMiddleClickAction == MouseAction.Paste;
            set => MouseMiddleClickAction = MouseAction.Paste;
        }

        public RelayCommand RestoreDefaultsCommand { get; }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc(StringsHelper.GetString("PleaseConfirm"), StringsHelper.GetString("ConfirmRestoreMouseSettings"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

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
