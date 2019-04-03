using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.ViewModels.Settings;
using FluentTerminal.App.Views;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public class TerminalFactoryService : ITerminalFactory
    {
        private readonly IApplicationView _applicationView;
        private readonly IClipboardService _clipboardService;
        private readonly IDialogService _dialogService;
        private readonly IDispatcherTimer _dispatcherTimer;
        private readonly IKeyboardCommandService _keyboardCommandService;
        private readonly ISettingsService _settingsService;
        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;

        public TerminalFactoryService(ISettingsService settingsService, IApplicationView applicationView,
            IClipboardService clipboardService, IDialogService dialogService, IDispatcherTimer dispatcherTimer,
            IKeyboardCommandService keyboardCommandService,
            ITrayProcessCommunicationService trayProcessCommunicationService)
        {
            _settingsService = settingsService;
            _applicationView = applicationView;
            _clipboardService = clipboardService;
            _dialogService = dialogService;
            _dispatcherTimer = dispatcherTimer;
            _keyboardCommandService = keyboardCommandService;
            _trayProcessCommunicationService = trayProcessCommunicationService;
        }

        public async Task<TerminalViewModel> InitializeTerminalAsync(ShellProfile profile)
        {
            if (profile.Id.Equals(DefaultValueProvider.SshShellProfileId))
            {
                SshInfoDialog dialog = new SshInfoDialog();

                ContentDialogResult result = await dialog.ShowAsync();

                if (result != ContentDialogResult.Primary)
                    return null;

                SshConnectionInfoViewModel sshInfo = (SshConnectionInfoViewModel) dialog.DataContext;

                if (sshInfo == null)
                    return null;

                profile.Arguments = $"-p {sshInfo.Port:#####} {sshInfo.Username}@{sshInfo.Host}";
            }

            return new TerminalViewModel(_settingsService, _trayProcessCommunicationService, _dialogService, _keyboardCommandService,
                _settingsService.GetApplicationSettings(), profile, _applicationView, _dispatcherTimer, _clipboardService);
        }
    }
}