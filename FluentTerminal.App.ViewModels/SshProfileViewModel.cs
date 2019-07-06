using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels
{
    /// <summary>
    /// Extends <see cref="ShellProfileViewModelBase{T}"/>, and doesn't implement any additional logic because in
    /// case of SSH profiles no additional logic is needed.
    /// </summary>
    public class SshProfileViewModel : ShellProfileViewModelBase<SshConnectViewModel>
    {
        #region Constructor

        public SshProfileViewModel(SshProfile sshProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService,
            IApplicationDataContainer historyContainer, bool isNew) : base(sshProfile, settingsService, dialogService,
            isNew)
        {
            ProfileVm = new SshConnectViewModel(settingsService, applicationView, trayProcessCommunicationService,
                fileSystemService, historyContainer, sshProfile);
        }

        #endregion Constructor

        #region Methods

        protected override bool CanDelete()
        {
            return true;
        }

        #endregion Methods
    }
}