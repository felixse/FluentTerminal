using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    public class TerminalInfoViewModel : ViewModelBase
    {
        #region Static

        private static readonly LineEndingStyle[] LineEndingStylesArray =
            Enum.GetValues(typeof(LineEndingStyle)).Cast<LineEndingStyle>().ToArray();

        #endregion Static

        #region Properties

        public ObservableCollection<TabTheme> TabThemes { get; }

        public ObservableCollection<TerminalTheme> TerminalThemes { get; }

        public ObservableCollection<LineEndingStyle> LineEndingStyles { get; } =
            new ObservableCollection<LineEndingStyle>(LineEndingStylesArray);

        private LineEndingStyle _lineEndingTranslation;

        public LineEndingStyle LineEndingTranslation
        {
            get => _lineEndingTranslation;
            set => Set(ref _lineEndingTranslation, value);
        }

        private TabTheme _selectedTabTheme;

        public TabTheme SelectedTabTheme
        {
            get => _selectedTabTheme ?? (_selectedTabTheme = TabThemes.FirstOrDefault(t => t.Id.Equals(_tabThemeId)));
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (Set(ref _selectedTabTheme, value))
                {
                    _tabThemeId = value.Id;
                }
            }
        }

        private int _tabThemeId;

        public int TabThemeId
        {
            get => _tabThemeId;
            set
            {
                if (Set(ref _tabThemeId, value))
                {
                    SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id.Equals(value));
                }
            }
        }

        private TerminalTheme _selectedTerminalTheme;

        public TerminalTheme SelectedTerminalTheme
        {
            get => _selectedTerminalTheme ??
                   (_selectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(_terminalThemeId)));
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (Set(ref _selectedTerminalTheme, value))
                {
                    _terminalThemeId = value.Id;
                }
            }
        }

        private Guid _terminalThemeId;

        public Guid TerminalThemeId
        {
            get => _terminalThemeId;
            set
            {
                if (Set(ref _terminalThemeId, value))
                {
                    SelectedTerminalTheme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(value));
                }
            }
        }

        private bool _useConPty;

        public bool UseConPty
        {
            get => _useConPty;
            set => Set(ref _useConPty, value);
        }

        #endregion Properties

        #region Constructor

        public TerminalInfoViewModel(ISettingsService settingsService)
        {
            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            SelectedTabTheme = TabThemes.First();

            TerminalThemes = new ObservableCollection<TerminalTheme>(settingsService.GetThemes());

            TerminalThemes.Insert(0, new TerminalTheme {Id = Guid.Empty, Name = "Default"});

            SelectedTerminalTheme = TerminalThemes.First();
        }

        #endregion Constructor

        #region Methods

        public void AddTheme(TerminalTheme theme) => TerminalThemes.Add(theme);

        public void RemoveTheme(Guid themeId)
        {
            var theme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(themeId));

            if (theme == null)
            {
                return;
            }

            TerminalThemes.Remove(theme);

            if (_terminalThemeId.Equals(themeId))
            {
                SelectedTerminalTheme = TerminalThemes.First();
            }
        }

        #endregion Methods
    }
}