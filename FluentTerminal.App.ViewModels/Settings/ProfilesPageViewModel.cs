using FluentTerminal.App.Services;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class ProfilesPageViewModel : ViewModelBase
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSystemService _fileSystemService;
        private ShellProfileViewModel _selectedShellProfile;
        private SettingsViewModel _settingsParent;

        public ProfilesPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, IFileSystemService fileSystemService, SettingsViewModel settingsParent)
        {
            _settingsParent = settingsParent;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _fileSystemService = fileSystemService;

            CreateShellProfileCommand = new RelayCommand(CreateShellProfile);

            var defaultShellProfileId = _settingsService.GetDefaultShellProfileId();
            foreach (var shellProfile in _settingsService.GetShellProfiles())
            {
                var viewModel = new ShellProfileViewModel(shellProfile, settingsService, dialogService, fileSystemService, _settingsParent);
                viewModel.Deleted += OnShellProfileDeleted;
                viewModel.SetAsDefault += OnShellProfileSetAsDefault;

                if (shellProfile.Id == defaultShellProfileId)
                {
                    viewModel.IsDefault = true;
                }
                ShellProfiles.Add(viewModel);
            }

            SelectedShellProfile = ShellProfiles.First(p => p.IsDefault);
        }

        private void OnShellProfileSetAsDefault(object sender, EventArgs e)
        {
            if (sender is ShellProfileViewModel defaultShellProfile)
            {
                _settingsService.SaveDefaultShellProfileId(defaultShellProfile.Id);

                foreach (var shellProfile in ShellProfiles)
                {
                    shellProfile.IsDefault = shellProfile.Id == defaultShellProfile.Id;
                }
            }
        }

        private void OnShellProfileDeleted(object sender, EventArgs e)
        {
            if (sender is ShellProfileViewModel shellProfile)
            {
                if (SelectedShellProfile == shellProfile)
                {
                    SelectedShellProfile = ShellProfiles.First();
                }
                ShellProfiles.Remove(shellProfile);

                if (shellProfile.IsDefault)
                {
                    ShellProfiles.First().IsDefault = true;
                    _settingsService.SaveDefaultShellProfileId(ShellProfiles.First().Id);
                }
                _settingsService.DeleteShellProfile(shellProfile.Id);
                _settingsParent.KeyBindings.UpdateKeyBindings();
            }
        }

        public RelayCommand CreateShellProfileCommand { get; }

        private void CreateShellProfile()
        {
            // Find the maximum shell profile command ID in use, and pick the next largest one.
            Command maxShellCommand = Command.ShellProfileShortcut;
            foreach (ShellProfile _shellProfile in _settingsService.GetShellProfiles())
            {
                if (_shellProfile.KeyBindingCommand > maxShellCommand)
                {
                    maxShellCommand = _shellProfile.KeyBindingCommand;
                }
            }
            maxShellCommand += 1;

            var shellProfile = new ShellProfile
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New profile",
                KeyBindingCommand = maxShellCommand,
                KeyBinding = new List<KeyBinding> { }
            };

            _settingsService.SaveShellProfile(shellProfile);

            var viewModel = new ShellProfileViewModel(shellProfile, _settingsService, _dialogService, _fileSystemService, _settingsParent);
            viewModel.EditCommand.Execute(null);
            viewModel.SetAsDefault += OnShellProfileSetAsDefault;
            viewModel.Deleted += OnShellProfileDeleted;
            ShellProfiles.Add(viewModel);
            SelectedShellProfile = viewModel;
            _settingsParent.KeyBindings.UpdateKeyBindings();
        }

        public ShellProfileViewModel SelectedShellProfile
        {
            get => _selectedShellProfile;
            set => Set(ref _selectedShellProfile, value);
        }

        public ObservableCollection<ShellProfileViewModel> ShellProfiles { get; } = new ObservableCollection<ShellProfileViewModel>();
    }
}