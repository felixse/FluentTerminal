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
        private SshProfileViewModel _selectedShellProfile;
        private readonly IApplicationView _applicationView;
        private readonly IDefaultValueProvider _defaultValueProvider;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public SshProfilesPageViewModel(ISettingsService settingsService, IDialogService dialogService,
            IFileSystemService fileSystemService, IApplicationView applicationView,
            IDefaultValueProvider defaultValueProvider,
            ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;
            _applicationView = applicationView;
            _defaultValueProvider = defaultValueProvider;
            _trayProcessCommunicationService = trayProcessCommunicationService;

            CreateSshProfileCommand = new RelayCommand(CreateSshProfile);
            CloneCommand = new RelayCommand<SshProfileViewModel>(Clone);

            var defaultSshProfileId = _settingsService.GetDefaultSshProfileId();
            foreach (var sshProfile in _settingsService.GetSshProfiles())
            {
                var viewModel = new SshProfileViewModel(sshProfile, settingsService, dialogService,
                    fileSystemService, applicationView, defaultValueProvider, _trayProcessCommunicationService, false);
                viewModel.Deleted += OnSshProfileDeleted;
                viewModel.SetAsDefault += OnSshProfileSetAsDefault;

                if (sshProfile.Id == defaultSshProfileId)
                {
                    viewModel.IsDefault = true;
                }

                SshProfiles.Add(viewModel);
            }

            if (SshProfiles.Count == 0)
                CreateSshProfile();

            SelectedSshProfile = SshProfiles.FirstOrDefault(p => p.IsDefault) ?? SshProfiles.First();
        }

        private void OnSshProfileSetAsDefault(object sender, EventArgs e)
        {
            if (sender is SshProfileViewModel defaultSshProfile)
            {
                _settingsService.SaveDefaultSshProfileId(defaultSshProfile.Id);

                foreach (var shellProfile in SshProfiles)
                {
                    shellProfile.IsDefault = shellProfile.Id == defaultSshProfile.Id;
                }
            }
        }

        private void OnSshProfileDeleted(object sender, EventArgs e)
        {
            if (sender is SshProfileViewModel shellProfile)
            {
                if (SelectedSshProfile == shellProfile)
                {
                    SelectedSshProfile = SshProfiles.First();
                }
                SshProfiles.Remove(shellProfile);

                if (shellProfile.IsDefault)
                {
                    SshProfiles.First().IsDefault = true;
                    _settingsService.SaveDefaultSshProfileId(SshProfiles.First().Id);
                }
                _settingsService.DeleteSshProfile(shellProfile.Id);
            }
        }

        public RelayCommand CreateSshProfileCommand { get; }

        public RelayCommand<SshProfileViewModel> CloneCommand { get; }

        public void CreateSshProfile()
        {
            var shellProfile = new SshProfile
            {
                Id = Guid.NewGuid(),
                PreInstalled = false,
                Name = "New SSH profile",
                KeyBindings = new List<KeyBinding>()
            };

            AddSshProfile(shellProfile);
        }

        private void Clone(SshProfileViewModel shellProfile)
        {
            var cloned = (SshProfile) shellProfile.Model.Clone();

            cloned.Id = Guid.NewGuid();
            cloned.PreInstalled = false;
            cloned.Name = $"Copy of {shellProfile.Name}";

            foreach (KeyBinding keyBinding in cloned.KeyBindings)
            {
                keyBinding.Command = cloned.Id.ToString();
            }
            AddSshProfile(cloned);
        }

        private void AddSshProfile(SshProfile sshProfile)
        {
            var viewModel = new SshProfileViewModel(sshProfile, _settingsService, _dialogService, _fileSystemService,
                _applicationView, _defaultValueProvider, _trayProcessCommunicationService, true);

            viewModel.EditCommand.Execute(null);
            viewModel.SetAsDefault += OnSshProfileSetAsDefault;
            viewModel.Deleted += OnSshProfileDeleted;
            SshProfiles.Add(viewModel);
            SelectedSshProfile = viewModel;
        }

        public SshProfileViewModel SelectedSshProfile
        {
            get => _selectedShellProfile;
            set => Set(ref _selectedShellProfile, value);
        }

        public ObservableCollection<SshProfileViewModel> SshProfiles { get; } = new ObservableCollection<SshProfileViewModel>();
    }
}
