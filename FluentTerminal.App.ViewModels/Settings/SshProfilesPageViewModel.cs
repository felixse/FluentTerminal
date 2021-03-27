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
    public class SshProfilesPageViewModel: ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly IFileSystemService _fileSystemService;
        private SshProfileViewModel _selectedShellProfile;
        private readonly IApplicationView _applicationView;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IApplicationDataContainer _historyContainer;

        public SshProfilesPageViewModel(ISettingsService settingsService, IDialogService dialogService,
            IFileSystemService fileSystemService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService,
            IApplicationDataContainer historyContainer)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _historyContainer = historyContainer;

            CreateSshProfileCommand = new RelayCommand(CreateSshProfile);
            CloneCommand = new RelayCommand<SshProfileViewModel>(Clone);

            foreach (var sshProfile in _settingsService.GetSshProfiles())
            {
                var viewModel = new SshProfileViewModel(sshProfile, settingsService, dialogService, fileSystemService,
                    applicationView, _trayProcessCommunicationService, historyContainer, false);
                viewModel.Deleted += OnSshProfileDeleted;
                SshProfiles.Add(viewModel);
            }

            if (SshProfiles.Count == 0)
            {
                CreateSshProfile();
            }

            SelectedSshProfile = SshProfiles.First();
        }

        private void OnSshProfileDeleted(object sender, EventArgs e)
        {
            if (sender is SshProfileViewModel sshProfile)
            {
                if (SelectedSshProfile == sshProfile)
                {
                    SelectedSshProfile = SshProfiles.First();
                }
                SshProfiles.Remove(sshProfile);
                _settingsService.DeleteSshProfile(sshProfile.Id);
            }
        }

        public ICommand CreateSshProfileCommand { get; }

        public ICommand CloneCommand { get; }

        public void CreateSshProfile()
        {
            var shellProfile = new SshProfile
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New SSH profile",
                KeyBindings = new List<KeyBinding>(),
                UseMosh = _settingsService.GetApplicationSettings().UseMoshByDefault,
                RequestConPty = _settingsService.GetApplicationSettings().UseConPty
            };

            AddSshProfile(shellProfile);
        }

        private void Clone(SshProfileViewModel shellProfile)
        {
            var cloned = (SshProfile) shellProfile.ProfileVm.Model.Clone();

            cloned.Id = Guid.NewGuid();
            cloned.PreInstalled = false;
            cloned.Name = $"Copy of {shellProfile.Name}";
            cloned.KeyBindings = new List<KeyBinding>();

            foreach (KeyBinding keyBinding in cloned.KeyBindings)
            {
                keyBinding.Command = cloned.Id.ToString();
            }
            AddSshProfile(cloned);
        }

        private void AddSshProfile(SshProfile sshProfile)
        {
            var viewModel = new SshProfileViewModel(sshProfile, _settingsService, _dialogService, _fileSystemService,
                _applicationView, _trayProcessCommunicationService, _historyContainer, true);

            viewModel.EditCommand.Execute(null);
            viewModel.Deleted += OnSshProfileDeleted;
            SshProfiles.Add(viewModel);
            SelectedSshProfile = viewModel;
        }

        public SshProfileViewModel SelectedSshProfile
        {
            get => _selectedShellProfile;
            set => SetProperty(ref _selectedShellProfile, value);
        }

        public ObservableCollection<SshProfileViewModel> SshProfiles { get; } = new ObservableCollection<SshProfileViewModel>();
    }
}
