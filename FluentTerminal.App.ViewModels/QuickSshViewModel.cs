using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
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
            SshProfile original = null) : base(settingsService, applicationView,
            original ?? new SshProfile {UseMosh = settingsService.GetApplicationSettings().UseMoshByDefault})
        {
            if (string.IsNullOrEmpty(Model.Location))
            {
                if (!string.IsNullOrEmpty(Model.Arguments))
                {
                    // TODO: We have arguments without command. Strange, but as long as we support only SSH, we can easily fix:
                    Model.Location = Constants.SshCommandName;
                }
            }
            else if (!Model.Location.Equals(Constants.SshCommandName))
            {
                throw new ArgumentException($"At the moment {nameof(QuickSshViewModel)} supports only SSH commands.");
            }

            Initialize((SshProfile) Model);
        }

        #endregion Constructor

        #region Methods

        private void Initialize(SshProfile profile)
        {
            Command = string.IsNullOrEmpty(profile.Location) ? string.Empty : $"{profile.Location} {profile.Arguments}";
        }

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            Initialize((SshProfile) profile);
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

            if (CommandValidationRx.IsMatch(_command?.Trim() ?? string.Empty))
            {
                return null;
            }

            error = I18N.Translate("InvalidCommand");

            return string.IsNullOrEmpty(error) ? "Invalid command." : error;
        }

        public override bool HasChanges()
        {
            return base.HasChanges() ||
                   !_command.NullableEqualTo($"{Model.Location} {Model.Arguments}");
        }

        #endregion Methods

        #region Links/shortcuts related

        private const string UriScheme = "sshft";
        private const string UriHost = "fluent.terminal";
        private const string CommandQueryStringName = "cmd";
        private const string ArgumentsQueryStringName = "args";

        public override async Task<Tuple<bool, string>> GetUrlAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return Tuple.Create(false, error);
            }

            var match = CommandValidationRx.Match(_command);

            if (!match.Success)
            {
                error = I18N.Translate("InvalidCommand");

                return Tuple.Create(false, string.IsNullOrEmpty(error) ? "Invalid command." : error);
            }

            return Tuple.Create(true,
                $"{UriScheme}://{UriHost}/?{CommandQueryStringName}={Constants.SshCommandName}&{ArgumentsQueryStringName}={HttpUtility.UrlEncode(match.Groups["args"].Value.Trim())}&{GetBaseQueryString()}");
        }

        #endregion Links/shortcuts related
    }
}