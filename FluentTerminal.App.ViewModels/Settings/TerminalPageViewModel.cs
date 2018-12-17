using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class TerminalPageViewModel : ViewModelBase
    {
        private readonly TerminalOptions _terminalOptions;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private bool _isEditingCursorStyle;
        private bool _isEditingScrollBarStyle;

        public bool BarIsSelected
        {
            get => CursorStyle == CursorStyle.Bar;
            set => CursorStyle = CursorStyle.Bar;
        }

        public bool BlockIsSelected
        {
            get => CursorStyle == CursorStyle.Block;
            set => CursorStyle = CursorStyle.Block;
        }

        public bool UnderlineIsSelected
        {
            get => CursorStyle == CursorStyle.Underline;
            set => CursorStyle = CursorStyle.Underline;
        }

        public bool HiddenIsSelected
        {
            get => ScrollBarStyle == ScrollBarStyle.Hidden;
            set => ScrollBarStyle = ScrollBarStyle.Hidden;
        }

        public bool AutoHidingIsSelected
        {
            get => ScrollBarStyle == ScrollBarStyle.AutoHiding;
            set => ScrollBarStyle = ScrollBarStyle.AutoHiding;
        }

        public bool VisibleIsSelected
        {
            get => ScrollBarStyle == ScrollBarStyle.Visible;
            set => ScrollBarStyle = ScrollBarStyle.Visible;
        }

        public bool CursorBlink
        {
            get => _terminalOptions.CursorBlink;
            set
            {
                if (_terminalOptions.CursorBlink != value)
                {
                    _terminalOptions.CursorBlink = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public string FontFamily
        {
            get => _terminalOptions.FontFamily;
            set
            {
                if (_terminalOptions.FontFamily != value)
                {
                    _terminalOptions.FontFamily = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public double BackgroundOpacity
        {
            get => _terminalOptions.BackgroundOpacity;
            set
            {
                if (_terminalOptions.BackgroundOpacity != value)
                {
                    _terminalOptions.BackgroundOpacity = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public int Padding
        {
            get => _terminalOptions.Padding;
            set
            {
                if (_terminalOptions.Padding != value)
                {
                    _terminalOptions.Padding = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public string ScrollBackLimit
        {
            get => _terminalOptions.ScrollBackLimit.ToString();
            set
            {
                var oldValue = _terminalOptions.ScrollBackLimit;
                if (uint.TryParse(value, out uint intValue))
                {
                    if (intValue == uint.MinValue)
                    {
                        intValue = uint.MaxValue;
                    }
                    if (_terminalOptions.ScrollBackLimit != intValue)
                    {
                        _terminalOptions.ScrollBackLimit = intValue;
                        _settingsService.SaveTerminalOptions(_terminalOptions);
                        RaisePropertyChanged();
                    }
                }
                else
                {
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<string> Fonts { get; }

        public int FontSize
        {
            get => _terminalOptions.FontSize;
            set
            {
                if (_terminalOptions.FontSize != value)
                {
                    _terminalOptions.FontSize = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                }
            }
        }

        public IEnumerable<int> Sizes { get; }

        public RelayCommand RestoreDefaultsCommand { get; }

        private CursorStyle CursorStyle
        {
            get => _terminalOptions.CursorStyle;
            set
            {
                if (_terminalOptions.CursorStyle != value && !_isEditingCursorStyle)
                {
                    _isEditingCursorStyle = true;
                    _terminalOptions.CursorStyle = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(BlockIsSelected));
                    RaisePropertyChanged(nameof(BarIsSelected));
                    RaisePropertyChanged(nameof(UnderlineIsSelected));
                    _isEditingCursorStyle = false;
                }
            }
        }

        private ScrollBarStyle ScrollBarStyle
        {
            get => _terminalOptions.ScrollBarStyle;
            set
            {
                if (_terminalOptions.ScrollBarStyle != value && !_isEditingScrollBarStyle)
                {
                    _isEditingScrollBarStyle = true;
                    _terminalOptions.ScrollBarStyle = value;
                    _settingsService.SaveTerminalOptions(_terminalOptions);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(HiddenIsSelected));
                    RaisePropertyChanged(nameof(AutoHidingIsSelected));
                    RaisePropertyChanged(nameof(VisibleIsSelected));
                    _isEditingScrollBarStyle = false;
                }
            }
        }

        private async Task RestoreDefaults()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to restore the default terminal options?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                var defaults = _defaultValueProvider.GetDefaultTerminalOptions();
                CursorBlink = defaults.CursorBlink;
                CursorStyle = defaults.CursorStyle;
                ScrollBarStyle = defaults.ScrollBarStyle;
                FontFamily = defaults.FontFamily;
                FontSize = defaults.FontSize;
                BackgroundOpacity = defaults.BackgroundOpacity;
                ScrollBackLimit = defaults.ScrollBackLimit.ToString();
            }
        }

        public TerminalPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, ISystemFontService systemFontService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;

            RestoreDefaultsCommand = new RelayCommand(async () => await RestoreDefaults().ConfigureAwait(false));

            Fonts = systemFontService.GetSystemFontFamilies().OrderBy(s => s);
            Sizes = Enumerable.Range(1, 72);

            _terminalOptions = _settingsService.GetTerminalOptions();
        }
    }
}