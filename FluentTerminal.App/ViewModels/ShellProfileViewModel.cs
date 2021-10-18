using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.ViewModels.Profiles;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace FluentTerminal.App.ViewModels
{
    /// <summary>
    /// Extends <see cref="ProfileViewModelBase{T}"/>, and implements only "Set as default" logic.
    /// </summary>
    public class ShellProfileViewModel : ProfileViewModelBase<CommonProfileProviderViewModel>
    {
        #region Fields

        private readonly IDefaultValueProvider _defaultValueProvider;

        #endregion Fields

        #region Properties

        private bool _isDefault;

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        #endregion Properties

        #region Events

        public event EventHandler SetAsDefault;

        #endregion Events

        #region Commands

        public ICommand SetDefaultCommand { get; }

        public ICommand RestoreDefaultsCommand { get; }

        #endregion Commands

        #region Constrcutor

        public ShellProfileViewModel(ShellProfile shellProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            IDefaultValueProvider defaultValueProvider, bool isNew) : base(shellProfile, settingsService, dialogService,
            isNew)
        {
            _defaultValueProvider = defaultValueProvider;

            ProfileVm = new CommonProfileProviderViewModel(settingsService, applicationView, fileSystemService,
                shellProfile);

            SetDefaultCommand = new RelayCommand(SetDefault);
            RestoreDefaultsCommand = new AsyncRelayCommand(RestoreDefaultsAsync);
        }

        #endregion Constrcutor

        #region Methods

        // Requires UI thread
        private async Task RestoreDefaultsAsync()
        {
            if (InEditMode || !ProfileVm.PreInstalled)
            {
                throw new InvalidOperationException();
            }

            // ConfigureAwait(true) because we need to execute Initialize method in the calling (UI) thread.
            var result = await DialogService.ShowMessageDialogAsync(I18N.Translate("PleaseConfirm"),
                I18N.Translate("ConfirmRestoreProfile"), DialogButton.OK, DialogButton.Cancel).ConfigureAwait(true);

            if (result == DialogButton.OK)
            {
                var profile = _defaultValueProvider.GetPreinstalledShellProfiles().FirstOrDefault(x => x.Id.Equals(Id));

                if (profile == null)
                {
                    // Should not happen ever, but...
                    return;
                }

                ProfileVm.Model = profile;
                Initialize(profile);

                SettingsService.SaveShellProfile(profile);
            }
        }

        protected override bool CanDelete()
        {
            return !ProfileVm.PreInstalled;
        }

        private void SetDefault()
        {
            SetAsDefault?.Invoke(this, EventArgs.Empty);
        }

        #endregion Methods
    }
}