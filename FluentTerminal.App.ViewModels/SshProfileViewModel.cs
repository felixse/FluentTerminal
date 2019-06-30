using FluentTerminal.App.Services;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels
{
    public class SshProfileViewModel : ShellProfileViewModelBase<FullSshViewModel>
    {
        #region Constructor

        public SshProfileViewModel(SshProfile sshProfile, ISettingsService settingsService,
            IDialogService dialogService, IFileSystemService fileSystemService, IApplicationView applicationView,
            ITrayProcessCommunicationService trayProcessCommunicationService, bool isNew) : base(sshProfile,
            settingsService, dialogService, isNew)
        {
            ProfileVm = new FullSshViewModel(settingsService, applicationView, trayProcessCommunicationService,
                fileSystemService, sshProfile);
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