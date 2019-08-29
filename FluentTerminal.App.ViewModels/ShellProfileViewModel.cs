using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Infrastructure;
using FluentTerminal.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentTerminal.App.ViewModels.Profiles;

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
            set => Set(ref _isDefault, value);
        }

        #endregion Properties

        #region Events

        public event EventHandler SetAsDefault;

        #endregion Events

        #region Commands

        public RelayCommand SetDefaultCommand { get; }

        public IAsyncCommand RestoreDefaultsCommand { get; }

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
            RestoreDefaultsCommand = new AsyncCommand(RestoreDefaults);
        }

        #endregion Constrcutor

        #region Methods

        private async Task RestoreDefaults()
        {
            if (InEditMode || !ProfileVm.PreInstalled)
            {
                throw new InvalidOperationException();
            }

            var result = await DialogService.ShowMessageDialogAsnyc(I18N.Translate("PleaseConfirm"),
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