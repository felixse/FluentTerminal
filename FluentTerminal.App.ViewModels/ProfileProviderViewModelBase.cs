using System;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.Models;
using GalaSoft.MvvmLight;

namespace FluentTerminal.App.ViewModels
{
    /// <summary>
    /// Base class for all profile view model classes.
    /// </summary>
    public abstract class ProfileProviderViewModelBase : ViewModelBase, IDisposable
    {
        #region Fields

        protected readonly ShellProfile OriginalProfile;
        protected readonly ISettingsService SettingsService;
        protected readonly IApplicationView ApplicationView;

        #endregion Fields

        #region Properties

        public TerminalInfoViewModel TerminalInfoViewModel { get; }

        #endregion Properties

        #region Constructor

        protected ProfileProviderViewModelBase(ISettingsService settingsService, IApplicationView applicationView,
            ShellProfile original = null)
        {
            SettingsService = settingsService;
            ApplicationView = applicationView;

            TerminalInfoViewModel = new TerminalInfoViewModel(settingsService, applicationView);

            OriginalProfile = original ?? new ShellProfile();

            TerminalInfoViewModel.LoadFromProfile(OriginalProfile);
        }

        #endregion Constructor

        #region Methods

        protected virtual void LoadFromProfile(ShellProfile profile)
        {
            TerminalInfoViewModel.LoadFromProfile(profile);
        }

        protected virtual void CopyToProfile(ShellProfile profile)
        {
            TerminalInfoViewModel.CopyToProfile(profile);
        }

        public virtual Task<string> ValidateAsync()
        {
            return Task.FromResult<string>(null);
        }

        public async Task AcceptChangesAsync()
        {
            var error = await ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            CopyToProfile(OriginalProfile);
        }

        public void RejectChanges()
        {
            ApplicationView.RunOnDispatcherThread(() => LoadFromProfile(OriginalProfile), false);
        }

        public ShellProfile GetProfile()
        {
            return OriginalProfile;
        }

        public virtual void Dispose()
        {
            TerminalInfoViewModel.Dispose();
        }

        #endregion Methods
    }
}