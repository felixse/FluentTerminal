using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class ShellProfileViewModel : ViewModelBase
    {
        private readonly ShellProfile _shellProfile;
        private string _name;
        private string _arguments;
        private string _location;
        private string _workingDirectory;
        private string _fallbackName;
        private string _fallbackArguments;
        private string _fallbackLocation;
        private string _fallbackWorkingDirectory;
        private bool _isDefault;
        private bool _inEditMode;
        private readonly ISettingsService _settingsService;
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, IFileSystemService fileSystemService)
        {
            _shellProfile = shellProfile;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;

            Id = shellProfile.Id;
            Name = shellProfile.Name;
            Arguments = shellProfile.Arguments;
            Location = shellProfile.Location;
            WorkingDirectory = shellProfile.WorkingDirectory;

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new RelayCommand(async () => await Delete().ConfigureAwait(false), CanDelete);
            EditCommand = new RelayCommand(Edit, CanEdit);
            CancelEditCommand = new RelayCommand(async () => await CancelEdit().ConfigureAwait(false));
            SaveChangesCommand = new RelayCommand(SaveChanges);
            BrowseForCustomShellCommand = new RelayCommand(async () => await BrowseForCustomShell().ConfigureAwait(false));
            BrowseForWorkingDirectoryCommand = new RelayCommand(async () => await BrowseForWorkingDirectory().ConfigureAwait(false));
        }

        public event EventHandler Deleted;
        public event EventHandler SetAsDefault;

        public RelayCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand CancelEditCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand BrowseForCustomShellCommand { get; }
        public RelayCommand BrowseForWorkingDirectoryCommand { get; }

        public Guid Id { get; }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string Arguments
        {
            get => _arguments;
            set => Set(ref _arguments, value);
        }

        public string Location
        {
            get => _location;
            set => Set(ref _location, value);
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => Set(ref _workingDirectory, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => Set(ref _isDefault, value);
        }

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        public void SaveChanges()
        {
            _shellProfile.Arguments = Arguments;
            _shellProfile.Location = Location;
            _shellProfile.Name = Name;
            _shellProfile.WorkingDirectory = WorkingDirectory;

            _settingsService.SaveShellProfile(_shellProfile);

            InEditMode = false;
        }

        private async Task CancelEdit()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Arguments = _fallbackArguments;
                Location = _fallbackLocation;
                Name = _fallbackName;
                WorkingDirectory = _fallbackWorkingDirectory;

                InEditMode = false;
            }
        }

        private void Edit()
        {
            _fallbackArguments = Arguments;
            _fallbackLocation = Location;
            _fallbackName = Name;
            _fallbackWorkingDirectory = WorkingDirectory;
            InEditMode = true;
        }

        private bool CanDelete()
        {
            return !_shellProfile.PreInstalled;
        }

        private bool CanEdit()
        {
            return !_shellProfile.PreInstalled;
        }

        private async Task Delete()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to delete this theme?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }

        private async Task BrowseForCustomShell()
        {
            var file = await _fileSystemService.OpenFile(new[] { ".exe" }).ConfigureAwait(true);
            if (file != null)
            {
                Location = file.Path;
            }
        }

        private async Task BrowseForWorkingDirectory()
        {
            var directory = await _fileSystemService.BrowseForDirectory().ConfigureAwait(true);
            if (directory != null)
            {
                WorkingDirectory = directory;
            }
        }
    }
}
