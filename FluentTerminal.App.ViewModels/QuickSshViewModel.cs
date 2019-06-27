using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentTerminal.App.Services;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels
{
    public class QuickSshViewModel : ProfileProviderViewModelBase
    {
        #region Static

        private static readonly Regex CommandValidationRx = new Regex(@"^ssh(\.exe)?\s+(?<args>\S.+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion Static

        #region Properties

        private string _command;

        public string Command
        {
            get => _command;
            set => Set(ref _command, value);
        }

        #endregion Properties

        #region Constructor

        public QuickSshViewModel(ISettingsService settingsService, IApplicationView applicationView,
            SshProfile original = null) : base(settingsService, applicationView, original ?? new SshProfile())
        {
            if (string.IsNullOrEmpty(OriginalProfile.Location))
            {
                if (!string.IsNullOrEmpty(OriginalProfile.Arguments))
                {
                    // TODO: We have arguments without command. Strange, but as long as we support only SSH, we can easily fix:
                    OriginalProfile.Location = Constants.SshCommandName;
                }
            }
            else if (!OriginalProfile.Location.Equals(Constants.SshCommandName))
            {
                throw new ArgumentException($"At the moment {nameof(QuickSshViewModel)} supports only SSH commands.");
            }

            CommandFromProfile((SshProfile) OriginalProfile);
        }

        #endregion Constructor

        #region Methods

        private void CommandFromProfile(SshProfile profile)
        {
            Command = string.IsNullOrEmpty(profile.Location) ? string.Empty : $"{profile.Location} {profile.Arguments}";
        }

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            CommandFromProfile((SshProfile) profile);
        }

        protected override void CopyToProfile(ShellProfile profile)
        {
            base.CopyToProfile(profile);

            var command = _command?.Trim();

            if (string.IsNullOrEmpty(command))
            {
                profile.Location = null;
                profile.Arguments = null;

                return;
            }

            var match = CommandValidationRx.Match(command);

            if (!match.Success)
            {
                // Should not happen ever because this method gets called only if validation succeeds.
                throw new Exception("Invalid command.");
            }

            profile.Location = Constants.SshCommandName;
            profile.Arguments = match.Groups["args"].Value.Trim();
        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            return CommandValidationRx.IsMatch(_command?.Trim() ?? string.Empty) ? null : "Invalid command.";
        }

        #endregion Methods
    }
}