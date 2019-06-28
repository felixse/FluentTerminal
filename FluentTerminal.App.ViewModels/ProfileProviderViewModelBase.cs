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

        /// <summary>
        /// Makes changes only if the data is valid (<see cref="ValidateAsync"/> returned <c>null</c> or empty string).
        /// </summary>
        /// <returns><c>null</c> or empty string if the operation was successful (the data is valid), or an error
        /// message returned by <see cref="ValidateAsync"/> method.</returns>
        public async Task<string> AcceptChangesAsync()
        {
            var error = await ValidateAsync();

            if (string.IsNullOrEmpty(error))
            {
                CopyToProfile(OriginalProfile);
            }

            return error;
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