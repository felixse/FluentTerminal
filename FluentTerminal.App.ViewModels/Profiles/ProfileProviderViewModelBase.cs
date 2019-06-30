using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels.Profiles
{
    /// <summary>
    /// Base class for all profile view model classes. It contains properties shared by all other view models
    /// (theme-related properties, line-ending translation, and WinPTY/ConPTY selection).
    /// </summary>
    public abstract class ProfileProviderViewModelBase : ViewModelBase
    {
        #region Static

        private static readonly LineEndingStyle[] LineEndingStylesArray =
            Enum.GetValues(typeof(LineEndingStyle)).Cast<LineEndingStyle>().ToArray();

        #endregion Static

        #region Fields

        protected readonly ISettingsService SettingsService;
        protected readonly IApplicationView ApplicationView;

        #endregion Fields

        #region Properties

        private ShellProfile _model;

        public ShellProfile Model
        {
            get => _model;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                if (ReferenceEquals(_model, value))
                {
                    return;
                }

                if (_model != null && _model.GetType() != value.GetType())
                {
                    throw new ArgumentException($"The value has to be of type {_model.GetType().Name}.");
                }

                _model = value;

                LoadFromProfile(value);
            }
        }
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

        protected ProfileProviderViewModelBase(ISettingsService settingsService, IApplicationView applicationView,
            ShellProfile original = null)
        {
            SettingsService = settingsService;
            ApplicationView = applicationView;

            _model = original ?? new ShellProfile();

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            SelectedTabTheme = TabThemes.First();

            TerminalThemes = new ObservableCollection<TerminalTheme>(settingsService.GetThemes());

            TerminalThemes.Insert(0, new TerminalTheme { Id = Guid.Empty, Name = "Default" });

            SelectedTerminalTheme = TerminalThemes.First();

            settingsService.ThemeAdded += OnThemeAdded;
            settingsService.ThemeDeleted += OnThemeDeleted;

            Initialize(Model);
        }

        #endregion Constructor

        #region Methods

        private void OnThemeDeleted(object sender, Guid e)
        {
            ApplicationView.RunOnDispatcherThread(() =>
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
            }, false);
        }

        private void OnThemeAdded(object sender, TerminalTheme e)
        {
            ApplicationView.RunOnDispatcherThread(() => TerminalThemes.Add(e), false);
        }

        private void Initialize(ShellProfile profile)
        {
            LineEndingTranslation = profile.LineEndingTranslation;
            UseConPty = profile.UseConPty;
            TerminalThemeId = profile.TerminalThemeId;
            TabThemeId = profile.TabThemeId;
        }

        protected virtual void LoadFromProfile(ShellProfile profile)
        {
            Initialize(profile);
        }

        protected virtual void CopyToProfile(ShellProfile profile)
        {
            profile.LineEndingTranslation = _lineEndingTranslation;
            profile.UseConPty = _useConPty;
            profile.TerminalThemeId = _terminalThemeId;
            profile.TabThemeId = _tabThemeId;
        }

        public virtual Task<string> ValidateAsync()
        {
            return Task.FromResult<string>(null);
        }

        /// <summary>
        /// Returns <c>true</c> if view model values aren't equal to the corresponding values of the
        /// underlying <see cref="ShellProfile"/>.
        /// </summary>
        public virtual bool HasChanges()
        {
            return Model.LineEndingTranslation != _lineEndingTranslation ||
                   Model.UseConPty != _useConPty ||
                   !Model.TerminalThemeId.Equals(_terminalThemeId) ||
                   Model.TabThemeId != _tabThemeId;
        }

        /// <summary>
        /// Copies values from the view model to the underlying <see cref="ShellProfile"/>.
        /// </summary>
        /// <param name="acceptIfInvalid">Defines what will happen if the view model data is invalid (if
        /// <see cref="ValidateAsync"/> returns non-empty string). If this parameter is set to <c>true</c>, the
        /// data will be copied to the underlying <see cref="ShellProfile"/> anyway. If it's set to <c>false</c>,
        /// (default) the underlying <see cref="ShellProfile"/> won't be changed at all.</param>
        /// <returns><c>null</c> or empty string if the operation was successful (the data is valid), or an error
        /// message returned by <see cref="ValidateAsync"/> method.</returns>
        public async Task<string> AcceptChangesAsync(bool acceptIfInvalid = false)
        {
            var error = await ValidateAsync();

            if (acceptIfInvalid || string.IsNullOrEmpty(error))
            {
                CopyToProfile(Model);
            }

            return error;
        }

        public void RejectChanges()
        {
            ApplicationView.RunOnDispatcherThread(() => LoadFromProfile(Model), false);
        }

        #endregion Methods

        #region Links/shortcuts related

        private const string UseConPtyQueryStringName = "conpty";
        private const string LineEndingQueryStringName = "lineending";
        private const string TerminalThemeIdQueryStringName = "theme";
        private const string TabThemeIdQueryStringName = "tab";

        private const string ShortcutFileFormat = @"[{{000214A0-0000-0000-C000-000000000046}}]
Prop3=19,0
[InternetShortcut]
IDList=
URL={0}
";

        public static string GetShortcutFileContent(string url) => string.Format(ShortcutFileFormat, url);

        public static IEnumerable<Tuple<string, string>> ParseParams(string uriOpts, char separator) =>
            uriOpts.Split(separator).Select(ParseSshOptionFromUri).Where(p => p != null);

        private static Tuple<string, string> ParseSshOptionFromUri(string option)
        {
            string[] nv = option.Split('=');

            if (nv.Length != 2 || string.IsNullOrEmpty(nv[0]))
            {
                // For now simply ignore invalid options
                return null;
                //throw new FormatException($"Invalid SSH option '{option}'.");
            }

            return Tuple.Create(HttpUtility.UrlDecode(nv[0]), HttpUtility.UrlDecode(nv[1]));
        }

        /// <summary>
        /// Generates URL.
        /// </summary>
        /// <returns><see cref="Tuple"/> whose first item represents success. If the first item is <c>true</c>,
        /// the second contains the URL. If the first item is <c>false</c>, the second contains error message.</returns>
        public virtual Task<Tuple<bool, string>> GetUrlAsync() =>
            Task.FromResult(Tuple.Create(false, "Not supported."));

        public string GetBaseQueryString()
        {
            var queryString =
                $"{UseConPtyQueryStringName}={_useConPty}&{LineEndingQueryStringName}={_lineEndingTranslation}";

            if (_tabThemeId != TabThemes.First().Id)
            {
                queryString += $"&{TabThemeIdQueryStringName}={_tabThemeId:##########}";
            }

            if (!_terminalThemeId.Equals(TerminalThemes.First().Id))
            {
                queryString += $"&{TerminalThemeIdQueryStringName}={_terminalThemeId}";
            }

            return queryString;
        }

        public void LoadBaseFromQueryString(IList<Tuple<string, string>> queryStringParams)
        {
            if (queryStringParams == null)
            {
                return;
            }

            var keyValue = queryStringParams.FirstOrDefault(t =>
                UseConPtyQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(keyValue?.Item2) && bool.TryParse(keyValue.Item2?.ToLower(), out bool useConPty))
            {
                UseConPty = useConPty;
            }

            keyValue = queryStringParams.FirstOrDefault(t =>
                LineEndingQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(keyValue?.Item2) && Enum.TryParse(keyValue.Item2, true, out LineEndingStyle lineEndingTranslation))
            {
                LineEndingTranslation = lineEndingTranslation;
            }

            keyValue = queryStringParams.FirstOrDefault(t =>
                TabThemeIdQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(keyValue?.Item2) && int.TryParse(keyValue.Item2, out int tabThemeId))
            {
                TabThemeId = tabThemeId;
            }

            keyValue = queryStringParams.FirstOrDefault(t =>
                TerminalThemeIdQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(keyValue?.Item2) && Guid.TryParse(keyValue.Item2, out Guid terminalThemeId))
            {
                TerminalThemeId = terminalThemeId;
            }
        }

        #endregion Links/shortcuts related
    }
}