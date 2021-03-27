using System.Threading.Tasks;
using System.Windows.Input;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentTerminal.App.ViewModels.Profiles
{
    /// <summary>
    /// Used for common profiles (PowerShell, CMD, WSL).
    /// </summary>
    public class CommonProfileProviderViewModel : ProfileProviderViewModelBase
    {
        #region Fields

        private readonly IFileSystemService _fileSystemService;

        #endregion Fields

        #region Properties

        public bool PreInstalled { get; private set; }

        private string _location;

        public string Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private string _workingDirectory;

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => SetProperty(ref _workingDirectory, value);
        }

        private string _arguments;

        public string Arguments
        {
            get => _arguments;
            set => SetProperty(ref _arguments, value);
        }

        #endregion Properties

        #region Commands

        public ICommand BrowseForCustomShellCommand { get; }

        public ICommand BrowseForWorkingDirectoryCommand { get; }

        #endregion Commands

        #region Constructor

        public CommonProfileProviderViewModel(ISettingsService settingsService, IApplicationView applicationView,
            IFileSystemService fileSystemService, ShellProfile original = null) : base(settingsService, applicationView,
            true, original)
        {
            _fileSystemService = fileSystemService;

            BrowseForCustomShellCommand = new AsyncRelayCommand(BrowseForCustomShell);
            BrowseForWorkingDirectoryCommand = new AsyncRelayCommand(BrowseForWorkingDirectory);

            Initialize(Model);
        }

        #endregion Constructor

        #region Methods

        private void Initialize(ShellProfile profile)
        {
            PreInstalled = profile.PreInstalled;
            Location = profile.Location;
            Arguments = profile.Arguments;
            WorkingDirectory = profile.WorkingDirectory;
        }

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            Initialize(profile);
        }

        protected override async Task CopyToProfileAsync(ShellProfile profile)
        {
            await base.CopyToProfileAsync(profile).ConfigureAwait(false);

            profile.Location = _location;
            profile.Arguments = _arguments;
            profile.WorkingDirectory = _workingDirectory;
        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            if (!string.IsNullOrEmpty(_location))
            {
                return null;
            }

            error = I18N.Translate("LocationCantBeEmpty");

            return string.IsNullOrEmpty(error) ? "Location cannot be empty." : error;
        }

        public override bool HasChanges()
        {
            return base.HasChanges() || !Model.Location.NullableEqualTo(_location) ||
                   !Model.WorkingDirectory.NullableEqualTo(_workingDirectory) ||
                   !Model.Arguments.NullableEqualTo(_arguments);
        }

        // Requires UI thread
        private async Task BrowseForCustomShell()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var file = await _fileSystemService.OpenFileAsync(new[] { ".exe" }).ConfigureAwait(true);
            if (file != null)
            {
                Location = file.Path;
            }
        }

        // Requires UI thread
        private async Task BrowseForWorkingDirectory()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var directory = await _fileSystemService.BrowseForDirectoryAsync().ConfigureAwait(true);
            if (directory != null)
            {
                WorkingDirectory = directory;
            }
        }

        #endregion Methods
    }
}