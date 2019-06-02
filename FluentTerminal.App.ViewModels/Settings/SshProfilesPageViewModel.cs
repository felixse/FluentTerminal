using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

namespace FluentTerminal.App.ViewModels.Settings
{
    public class SshProfilesPageViewModel: ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSystemService _fileSystemService;
        private SshShellProfileViewModel _selectedShellProfile;
        private readonly IApplicationView _applicationView;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public SshProfilesPageViewModel(ISettingsService settingsService, IDialogService dialogService,
            IFileSystemService fileSystemService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _trayProcessCommunicationService = trayProcessCommunicationService;

            CreateSshShellProfileCommand = new RelayCommand(CreateSshShellProfile);
            CloneCommand = new RelayCommand<SshShellProfileViewModel>(Clone);

            var defaultSshShellProfileId = _settingsService.GetDefaultSshShellProfileId();
            foreach (var sshShellProfile in _settingsService.GetSshShellProfiles())
            {
                var viewModel = new SshShellProfileViewModel(sshShellProfile, settingsService, dialogService,
                    fileSystemService, applicationView, _trayProcessCommunicationService, false);
                viewModel.Deleted += OnSshShellProfileDeleted;
                viewModel.SetAsDefault += OnSshShellProfileSetAsDefault;

                if (sshShellProfile.Id == defaultSshShellProfileId)
                {
                    viewModel.IsDefault = true;
                }

                SshShellProfiles.Add(viewModel);
            }

            if (SshShellProfiles.Count == 0)
                CreateSshShellProfile();
            SelectedSshShellProfile = SshShellProfiles.FirstOrDefault(p => p.IsDefault);
            if (SelectedSshShellProfile == null)
                SelectedSshShellProfile = SshShellProfiles.First();
        }

        private void OnSshShellProfileSetAsDefault(object sender, EventArgs e)
        {
            if (sender is SshShellProfileViewModel defaultSshShellProfile)
            {
                _settingsService.SaveDefaultSshShellProfileId(defaultSshShellProfile.Id);

                foreach (var shellProfile in SshShellProfiles)
                {
                    shellProfile.IsDefault = shellProfile.Id == defaultSshShellProfile.Id;
                }
            }
        }

        private void OnSshShellProfileDeleted(object sender, EventArgs e)
        {
            if (sender is SshShellProfileViewModel shellProfile)
            {
                if (SelectedSshShellProfile == shellProfile)
                {
                    SelectedSshShellProfile = SshShellProfiles.First();
                }
                SshShellProfiles.Remove(shellProfile);

                if (shellProfile.IsDefault)
                {
                    SshShellProfiles.First().IsDefault = true;
                    _settingsService.SaveDefaultSshShellProfileId(SshShellProfiles.First().Id);
                }
                _settingsService.DeleteSshShellProfile(shellProfile.Id);
            }
        }

        public RelayCommand CreateSshShellProfileCommand { get; }

        public RelayCommand<SshShellProfileViewModel> CloneCommand { get; }

        public void CreateSshShellProfile()
        {
            var shellProfile = new SshShellProfile
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New SSH profile",
                KeyBindings = new List<KeyBinding>()
            };

            AddSshShellProfile(shellProfile);
        }

        private void Clone(SshShellProfileViewModel shellProfile)
        {
            var cloned = new SshShellProfile(shellProfile.Model)
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = $"Copy of {shellProfile.Name}"
            };
            foreach (KeyBinding keyBinding in cloned.KeyBindings)
            {
                keyBinding.Command = cloned.Id.ToString();
            }
            AddSshShellProfile(cloned);
        }

        private void AddSshShellProfile(SshShellProfile sshShellProfile)
        {
            //_settingsService.SaveSshShellProfile(sshShellProfile, true);

            var viewModel = new SshShellProfileViewModel(sshShellProfile, _settingsService, _dialogService,
                _fileSystemService, _applicationView, _trayProcessCommunicationService, true);
            viewModel.EditCommand.Execute(null);
            viewModel.SetAsDefault += OnSshShellProfileSetAsDefault;
            viewModel.Deleted += OnSshShellProfileDeleted;
            SshShellProfiles.Add(viewModel);
            SelectedSshShellProfile = viewModel;
        }

        public SshShellProfileViewModel SelectedSshShellProfile
        {
            get => _selectedShellProfile;
            set => Set(ref _selectedShellProfile, value);
        }

        public ObservableCollection<SshShellProfileViewModel> SshShellProfiles { get; } = new ObservableCollection<SshShellProfileViewModel>();
    }
}