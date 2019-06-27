using System;
using System.Collections.ObjectModel;
using System.Linq;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    /// <summary>
    /// View model used in few forms for selecting Terminal themes / behavior.
    /// </summary>
    public class TerminalInfoViewModel : ViewModelBase, IDisposable
    {
        #region Static

        private static readonly LineEndingStyle[] LineEndingStylesArray =
            Enum.GetValues(typeof(LineEndingStyle)).Cast<LineEndingStyle>().ToArray();

        #endregion Static

        #region Fields

        private readonly ISettingsService _settingsService;

        private readonly IApplicationView _applicationView;

        #endregion Fields

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
                TabTheme theme = value;

                if (theme == null)
                {
                    // How to handle attempt to setting null theme?
                    // 1) Throw an exception:
                    //throw new ArgumentNullException();
                    // 2) Ignore the attempt:
                    //return;
                    // 3) Use default theme (probably the best):
                    theme = TabThemes.First();
                }
                else if (!TabThemes.Contains(theme))
                {
                    // Ensure that it's from the list
                    theme = TabThemes.FirstOrDefault(t => t.Id == theme.Id) ?? TabThemes.First();
                }

                if (Set(ref _selectedTabTheme, theme))
                {
                    _tabThemeId = theme.Id;
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
                TerminalTheme theme = value;

                if (theme == null)
                {
                    // How to handle attempt to setting null theme?
                    // 1) Throw an exception:
                    //throw new ArgumentNullException();
                    // 2) Ignore the attempt:
                    //return;
                    // 3) Use default theme (probably the best):
                    theme = TerminalThemes.First();
                }
                else if (!TerminalThemes.Contains(theme))
                {
                    // Ensure that it's from the list
                    theme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(theme.Id)) ?? TerminalThemes.First();
                }

                if (Set(ref _selectedTerminalTheme, theme))
                {
                    _terminalThemeId = theme.Id;
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

        public TerminalInfoViewModel(ISettingsService settingsService, IApplicationView applicationView)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;
            
            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            SelectedTabTheme = TabThemes.First();

            TerminalThemes = new ObservableCollection<TerminalTheme>(settingsService.GetThemes());

            TerminalThemes.Insert(0, new TerminalTheme {Id = Guid.Empty, Name = "Default"});

            SelectedTerminalTheme = TerminalThemes.First();

            settingsService.ThemeAdded += OnThemeAdded;
            settingsService.ThemeDeleted += OnThemeDeleted;
        }

        #endregion Constructor

        #region Methods

        private void OnThemeDeleted(object sender, Guid e)
        {
            _applicationView.RunOnDispatcherThread(() =>
            {
                var theme = TerminalThemes.FirstOrDefault(t => t.Id.Equals(e));

                if (theme == null)
                {
                    return;
                }

                TerminalThemes.Remove(theme);

                if (_terminalThemeId.Equals(e))
                {
                    SelectedTerminalTheme = TerminalThemes.First();
                }
            });
        }

        private void OnThemeAdded(object sender, TerminalTheme e)
        {
            _applicationView.RunOnDispatcherThread(() => TerminalThemes.Add(e));
        }

        public void LoadFromProfile(ShellProfile profile)
        {
            LineEndingTranslation = profile.LineEndingTranslation;
            UseConPty = profile.UseConPty;
            TerminalThemeId = profile.TerminalThemeId;
            TabThemeId = profile.TabThemeId;
        }

        public void CopyToProfile(ShellProfile profile)
        {
            profile.LineEndingTranslation = _lineEndingTranslation;
            profile.UseConPty = _useConPty;
            profile.TerminalThemeId = _terminalThemeId;
            profile.TabThemeId = _tabThemeId;
        }

        public void Dispose()
        {
            _settingsService.ThemeAdded -= OnThemeAdded;
            _settingsService.ThemeDeleted -= OnThemeDeleted;
        }

        #endregion Methods
    }
}