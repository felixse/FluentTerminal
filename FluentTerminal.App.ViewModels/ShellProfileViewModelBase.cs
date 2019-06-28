using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace FluentTerminal.App.ViewModels
{
    public abstract class ShellProfileViewModelBase<T> : ViewModelBase, IDisposable where T : ProfileProviderViewModelBase
    {
        #region Fields

        protected readonly IDialogService DialogService;
        protected readonly ISettingsService SettingsService;

        private List<KeyBinding> _bindingsBeforeEdit;
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

        public T ProfileVm { get; protected set; }

        #endregion Properties

        #region Events

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

        protected ShellProfileViewModelBase(ShellProfile shellProfile, ISettingsService settingsService, IDialogService dialogService, bool isNew)
        {
            SettingsService = settingsService;
            DialogService = dialogService;
            IsNew = isNew;

            KeyBindings = new KeyBindingsViewModel(shellProfile.Id.ToString(), dialogService, string.Empty, false);

            Initialize(shellProfile);

            DeleteCommand = new AsyncCommand(Delete, CanDelete);
            EditCommand = new RelayCommand(Edit);
            CancelEditCommand = new AsyncCommand(CancelEdit);
            SaveChangesCommand = new AsyncCommand(SaveChangesAsync);
            AddKeyboardShortcutCommand = new AsyncCommand(AddKeyboardShortcut);
        }

        #endregion Constructor

        #region Methods

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
            return ProfileVm.HasChanges() || !_nameBeforeEdit.NullableEqualTo(_name) ||
                   (_bindingsBeforeEdit?.SequenceEqual(KeyBindings.KeyBindings.Select(x => x.Model).ToList()) ?? true);
        }

        private async Task Delete()
        {
            var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"),
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
            _nameBeforeEdit = _name;

            KeyBindings.Editable = true;
            InEditMode = true;
        }

        private async Task CancelEdit()
        {
            if (IsNew)
            {
                await Delete();
            }
            else
            {
                if (HasChanges())
                {
                    var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"),
                            I18N.Translate("ConfirmDiscardChanges"), DialogButton.OK, DialogButton.Cancel)
                        .ConfigureAwait(true);

                    if (result == DialogButton.OK)
                    {
                        // Cancelled, so rollback
                        ProfileVm.RejectChanges();

                        KeyBindings.Editable = false;
                        InEditMode = false;
                    }
                }
                else
                {
                    KeyBindings.Editable = false;
                    InEditMode = false;
                }
            }
        }

        public virtual async Task SaveChangesAsync()
        {
            var error = await ProfileVm.AcceptChangesAsync();

            if (!string.IsNullOrEmpty(error))
            {
                await DialogService.ShowMessageDialogAsnyc(I18N.Translate("InvalidInput"), error, DialogButton.OK);

                return;
            }

            var profile = ProfileVm.Model;

            profile.Id = Id;
            profile.Name = _name;
            profile.KeyBindings = KeyBindings.KeyBindings.Select(x => x.Model).ToList();

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

        public Task AddKeyboardShortcut()
        {
            return KeyBindings.ShowAddKeyBindingDialog();
        }

        public void Dispose()
        {
            ProfileVm.Dispose();
        }

        #endregion Methods
    }
}