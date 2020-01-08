﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels
{
    /// <summary>
    /// Base class for all profile view models. Implements logic for saving, editing, resetting, etc.
    /// </summary>
    public abstract class ProfileViewModelBase<T> : ViewModelBase where T : ProfileProviderViewModelBase
    {
        #region Fields

        protected readonly IDialogService DialogService;
        protected readonly ISettingsService SettingsService;

        private List<KeyBinding> _bindingsBeforeEdit;
        private Dictionary<string, string> _environmentVariablesBeforeEdit;
        private string _nameBeforeEdit;

        #endregion Fields

        #region Properties

        protected bool IsNew { get; private set; }

        public Guid Id { get; private set; }

        private bool _inEditMode;

        public bool InEditMode
        {
            get => _inEditMode;
            set => Set(ref _inEditMode, value);
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public KeyBindingsViewModel KeyBindings { get; }

        public ObservableCollection<EnvironmentVariableViewModel> EnvironmentVariables { get; } = new ObservableCollection<EnvironmentVariableViewModel>();

        public T ProfileVm { get; protected set; }

        #endregion Properties

        #region Events

        // Needs to be triggered from UI thread
        public event EventHandler Deleted;

        #endregion Events

        #region Commands

        public IAsyncCommand CancelEditCommand { get; }

        public IAsyncCommand DeleteCommand { get; }

        public RelayCommand EditCommand { get; }

        public IAsyncCommand SaveChangesCommand { get; }

        public IAsyncCommand AddKeyboardShortcutCommand { get; }

        #endregion Commands

        #region Constructor

        protected ProfileViewModelBase(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, bool isNew)
        {
            SettingsService = settingsService;
            DialogService = dialogService;
            IsNew = isNew;

            KeyBindings = new KeyBindingsViewModel(shellProfile.Id.ToString(), dialogService, string.Empty, false);

            foreach (var environmentVariable in shellProfile.EnvironmentVariables)
            {
                EnvironmentVariables.Add(new EnvironmentVariableViewModel
                {
                    Name = environmentVariable.Key,
                    Value = environmentVariable.Value
                });
            }

            Initialize(shellProfile);

            DeleteCommand = new AsyncCommand(DeleteAsync, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEditAsync);
            SaveChangesCommand = new AsyncCommand(SaveChangesAsync);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
        }

        #endregion Constructor

        #region Methods

        // Requires UI thread
        // Loads view model properties from the input shellProfile
        protected void Initialize(ShellProfile shellProfile)
        {
            Id = shellProfile.Id;
            Name = shellProfile.Name;

            KeyBindings.Clear();
            foreach (var keyBinding in shellProfile.KeyBindings.Select(x => new KeyBinding(x)).ToList())
            {
                KeyBindings.Add(keyBinding);
            }
        }

        protected virtual bool HasChanges()
        {
            return ProfileVm.HasChanges()
                || !_nameBeforeEdit.NullableEqualTo(_name)
                || !(_bindingsBeforeEdit?.SequenceEqual(KeyBindings.KeyBindings.Select(x => x.Model).ToList()) ?? false)
                || !(_environmentVariablesBeforeEdit?.SequenceEqual(EnvironmentVariables.ToDictionary(x => x.Name, x => x.Value)) ?? false);
        }

        // Requires UI thread
        private async Task DeleteAsync()
        {
            // ConfigureAwait(true) because we need to trigger Deleted event from the calling (UI) thread.
            var result = await DialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmDeleteProfile"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                Deleted?.Invoke(this, EventArgs.Empty);
            }
        }

        protected abstract bool CanDelete();

        private void Edit()
        {
            if (InEditMode)
            {
                return;
            }

            _bindingsBeforeEdit = KeyBindings.KeyBindings.Select(x => x.Model).ToList();
            _environmentVariablesBeforeEdit = EnvironmentVariables.ToDictionary(x => x.Name, x => x.Value);
            _nameBeforeEdit = _name;

            KeyBindings.Editable = true;
            InEditMode = true;
        }

        // Requires UI thread
        private async Task CancelEditAsync()
        {
            if (IsNew)
            {
                await DeleteAsync().ConfigureAwait(false);

                return;
            }

            if (!HasChanges())
            {
                KeyBindings.Editable = false;
                InEditMode = false;

                return;
            }

            // ConfigureAwait(true) because we're setting some view-model properties afterwards
            if (await DialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                        I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel)
                    .ConfigureAwait(true) == DialogButton.OK)
            {
                // Cancelled, so rollback
                // ConfigureAwait(true) because we're setting some view-model properties afterwards
                await ProfileVm.RejectChangesAsync().ConfigureAwait(true);

                KeyBindings.Editable = false;
                InEditMode = false;
            }
        }

        // Requires UI thread
        private async Task SaveChangesAsync()
        {
            // ConfigureAwait(true) because we're setting some view-model properties afterwards.
            var error = await ProfileVm.AcceptChangesAsync().ConfigureAwait(true);

            if (!string.IsNullOrEmpty(error))
            {
                await DialogService.ShowMessageDialogAsync(I18N.Translate("InvalidInput"), error, DialogButton.OK)
                    .ConfigureAwait(false);

                return;
            }

            if (EnvironmentVariables.Select(x => x.Name).Any(string.IsNullOrWhiteSpace))
            {
                await DialogService.ShowMessageDialogAsync(I18N.Translate("InvalidInput"),
                    I18N.Translate("EmptyEnvironmentVariableName"), DialogButton.OK).ConfigureAwait(false);

                return;
            }

            if (EnvironmentVariables.Select(x => x.Name).Distinct().Count() != EnvironmentVariables.Count)
            {
                await DialogService.ShowMessageDialogAsync(I18N.Translate("InvalidInput"),
                    I18N.Translate("DuplicateEnvironmentVariable"), DialogButton.OK).ConfigureAwait(false);

                return;
            }

            var profile = ProfileVm.Model;

            profile.Id = Id;
            profile.Name = _name;
            profile.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();
            profile.EnvironmentVariables = EnvironmentVariables.ToDictionary(x => x.Name, x => x.Value);

            if (profile is SshProfile sshProfile)
            {
                SettingsService.SaveSshProfile(sshProfile, IsNew);
            }
            else
            {
                SettingsService.SaveShellProfile(profile, IsNew);
            }

            KeyBindings.Editable = false;
            InEditMode = false;
            IsNew = false;
        }

        private Task AddKeyboardShortcut()
        {
            return KeyBindings.ShowAddKeyBindingDialogAsync();
        }

        #endregion Methods
    }
}