using FluentTerminal.App.Services;
using FluentTerminal.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Windows.Input;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class ProfilesPageViewModel : ObservableObject
    {
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSystemService _fileSystemService;
        private ShellProfileViewModel _selectedShellProfile;

        public ProfilesPageViewModel(ISettingsService settingsService, IDialogService dialogService, IDefaultValueProvider defaultValueProvider, IFileSystemService fileSystemService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _defaultValueProvider = defaultValueProvider;
            _fileSystemService = fileSystemService;

            CreateShellProfileCommand = new RelayCommand(CreateShellProfile);
            CloneCommand = new RelayCommand<ShellProfileViewModel>(Clone);

            var defaultShellProfileId = _settingsService.GetDefaultShellProfileId();
            foreach (var shellProfile in _settingsService.GetShellProfiles())
            {
                var viewModel = new ShellProfileViewModel(shellProfile, settingsService, dialogService, fileSystemService, defaultValueProvider, false);
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
            }
        }

        public ICommand CreateShellProfileCommand { get; }

        public ICommand CloneCommand { get; }

        private void CreateShellProfile()
        {
            var shellProfile = new ShellProfile
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New profile",
                KeyBindings = new List<KeyBinding>(),
                UseConPty = _settingsService.GetApplicationSettings().UseConPty
            };

            AddShellProfile(shellProfile);
        }

        private void Clone(ShellProfileViewModel shellProfile)
        {
            var cloned = shellProfile.ProfileVm.Model.Clone();

            cloned.Id = Guid.NewGuid();
            cloned.PreInstalled = false;
            cloned.Name = $"Copy of {shellProfile.Name}";
            cloned.KeyBindings = new List<KeyBinding>();

            AddShellProfile(cloned);
        }

        private void AddShellProfile(ShellProfile shellProfile)
        {
            var viewModel = new ShellProfileViewModel(shellProfile, _settingsService, _dialogService, _fileSystemService, _defaultValueProvider, true);
            viewModel.EditCommand.Execute(null);
            viewModel.SetAsDefault += OnShellProfileSetAsDefault;
            viewModel.Deleted += OnShellProfileDeleted;
            ShellProfiles.Add(viewModel);
            SelectedShellProfile = viewModel;
        }

        public ShellProfileViewModel SelectedShellProfile
        {
            get => _selectedShellProfile;
            set => SetProperty(ref _selectedShellProfile, value);
        }

        public ObservableCollection<ShellProfileViewModel> ShellProfiles { get; } = new ObservableCollection<ShellProfileViewModel>();
    }
}