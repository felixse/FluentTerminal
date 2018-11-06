﻿using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FluentTerminal.App.ViewModels
{
    public class ShellProfileViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IFileSystemService _fileSystemService;
        private readonly ISettingsService _settingsService;
        private readonly ShellProfile _shellProfile;
        private string _arguments;
        private string _fallbackArguments;
        private string _fallbackLocation;
        private string _fallbackName;
        private int _fallbackTabThemeId;
        private string _fallbackWorkingDirectory;
        private bool _inEditMode;
        private bool _isDefault;
        private string _location;
        private string _name;
        private TabTheme _selectedTabTheme;
        private string _workingDirectory;
        private ICollection<KeyBinding> _keyBindings;
        KeyBindingsViewModel keyBindingsViewModel;
        public ObservableCollection<KeyBindingsViewModel> KeyBindings { get; } = new ObservableCollection<KeyBindingsViewModel>();

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, IFileSystemService fileSystemService)
        {
            _shellProfile = shellProfile;
            _settingsService = settingsService;
            _dialogService = dialogService;
            _fileSystemService = fileSystemService;

            TabThemes = new ObservableCollection<TabTheme>(settingsService.GetTabThemes());

            Id = shellProfile.Id;
            Name = shellProfile.Name;
            Arguments = shellProfile.Arguments;
            Location = shellProfile.Location;
            WorkingDirectory = shellProfile.WorkingDirectory;
            SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == shellProfile.TabThemeId);
            PreInstalled = shellProfile.PreInstalled;
            _keyBindings = shellProfile.KeyBinding;

            keyBindingsViewModel = new KeyBindingsViewModel(shellProfile, shellProfile.KeyBinding, _dialogService, "New Tab With Profile");
            KeyBindings.Add(keyBindingsViewModel);

            SetDefaultCommand = new RelayCommand(SetDefault);
            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new RelayCommand(SaveChanges);
            AddKeyboardShortcutCommand = new RelayCommand(AddKeyboardShortcut);
            BrowseForCustomShellCommand = new AsyncCommand(BrowseForCustomShell);
            BrowseForWorkingDirectoryCommand = new AsyncCommand(BrowseForWorkingDirectory);
        }

        public void AddKeyboardShortcut()
        {
            keyBindingsViewModel.Add();
        }

        public event EventHandler Deleted;
        public event EventHandler SetAsDefault;

        public IAsyncCommand BrowseForCustomShellCommand { get; }
        public IAsyncCommand BrowseForWorkingDirectoryCommand { get; }
        public IAsyncCommand CancelEditCommand { get; }
        public IAsyncCommand DeleteCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand SaveChangesCommand { get; }
        public RelayCommand SetDefaultCommand { get; }
        public RelayCommand AddKeyboardShortcutCommand { get; }
        public ObservableCollection<TabTheme> TabThemes { get; }

        public bool PreInstalled { get; }

        public string Arguments
        {
            get => _arguments;
            set => Set(ref _arguments, value);
        }

        public Guid Id { get; }

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => Set(ref _isDefault, value);
        }

        public string Location
        {
            get => _location;
            set => Set(ref _location, value);
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public TabTheme SelectedTabTheme
        {
            get => _selectedTabTheme;
            set
            {
                if (value != null)
                {
                    Set(ref _selectedTabTheme, value);
                }
            }
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
            set => Set(ref _workingDirectory, value);
        }

        public void SaveChanges()
        {
            _shellProfile.Arguments = Arguments;
            _shellProfile.Location = Location;
            _shellProfile.Name = Name;
            _shellProfile.WorkingDirectory = WorkingDirectory;
            _shellProfile.TabThemeId = SelectedTabTheme.Id;
            // The keybindings were edited in-place by reference, so we don't need to update them here.

            _settingsService.SaveShellProfile(_shellProfile);

            InEditMode = false;
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

        private async Task CancelEdit()
        {
            var result = await _dialogService.ShowMessageDialogAsnyc("Please confirm", "Are you sure you want to discard all changes?", DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Arguments = _fallbackArguments;
                Location = _fallbackLocation;
                Name = _fallbackName;
                WorkingDirectory = _fallbackWorkingDirectory;
                SelectedTabTheme = TabThemes.FirstOrDefault(t => t.Id == _fallbackTabThemeId);

                InEditMode = false;
            }
        }

        private bool CanDelete()
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

        private void Edit()
        {
            _fallbackArguments = _shellProfile.Arguments;
            _fallbackLocation = _shellProfile.Location;
            _fallbackName = _shellProfile.Name;
            _fallbackWorkingDirectory = _shellProfile.WorkingDirectory;
            _fallbackTabThemeId = _shellProfile.TabThemeId;
            InEditMode = true;
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }
    }
}