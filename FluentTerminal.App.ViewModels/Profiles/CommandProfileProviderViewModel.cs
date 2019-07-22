using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels.Profiles
{
    public class CommandProfileProviderViewModel : ProfileProviderViewModelBase
    {
        #region Static

        private static readonly Regex CommandValidationRx = new Regex(@"^(?<cmd>[^\s\.]+)(\.exe)?(\s+(?<args>\S.*))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion Static

        #region Fields

        private readonly IApplicationDataContainer _historyContainer;

        #endregion Fields

        #region Properties

        private string _command;

        public string Command
        {
            get => _command;
            set => Set(ref _command, value);
        }

        public ObservableCollection<string> CommandHistory { get; private set; }

        #endregion Properties

        #region Constructor

        public CommandProfileProviderViewModel(ISettingsService settingsService, IApplicationView applicationView,
            IApplicationDataContainer historyContainer, ShellProfile original = null) : base(settingsService,
            applicationView, original)
        {
            _historyContainer = historyContainer;

            FillCommandHistory();

            Initialize(Model);
        }

        #endregion Constructor

        #region Methods

        // Fills the view model properties from the input sshProfile
        private void Initialize(ShellProfile sshProfile)
        {
            Command = string.IsNullOrEmpty(sshProfile.Location)
                ? string.Empty
                : $"{sshProfile.Location} {sshProfile.Arguments}";

        }

        protected override void LoadFromProfile(ShellProfile profile)
        {
            base.LoadFromProfile(profile);

            Initialize(profile);
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

            profile.Location = match.Groups["cmd"].Value;
            profile.Arguments = match.Groups["args"].Success ? match.Groups["args"].Value.Trim() : null;

            profile.WorkingDirectory = null;

        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            var match = CommandValidationRx.Match(_command?.Trim() ?? string.Empty);

            if (!match.Success)
            {
                error = I18N.Translate("InvalidCommand");

                return string.IsNullOrEmpty(error) ? "Invalid command." : error;
            }

            if (!match.Groups["args"].Success)
            {
                error = I18N.Translate("CommandArgumentsMandatory");

                return string.IsNullOrEmpty(error) ? "Command arguments are missing." : error;
            }

            return null;
        }

        public override bool HasChanges()
        {
            return base.HasChanges() || !_command.NullableEqualTo($"{Model.Location} {Model.Arguments}");
        }

        #endregion Methods

        #region Command history

        private const string ExecutedCommandsKey = "ExecutedCommands";
        private const int CommandHistoryLimit = 20;

        private void FillCommandHistory()
        {
            var commands = _historyContainer.ReadValueFromJson<ExecutedCommandHistory>(ExecutedCommandsKey, null)
                ?.ExecutedCommands;

            CommandHistory = commands == null
                ? new ObservableCollection<string>()
                : new ObservableCollection<string>(commands.Select(c => $"{c.Command} {c.Args}"));
        }

        public void SaveCommand(string cmd, string args)
        {
            cmd = cmd?.Trim();

            if (string.IsNullOrEmpty(cmd))
            {
                throw new ArgumentNullException(nameof(cmd));
            }

            var commandHistory =
                _historyContainer.ReadValueFromJson<ExecutedCommandHistory>(ExecutedCommandsKey, null) ??
                new ExecutedCommandHistory { ExecutedCommands = new List<ExecutedCommand>() };

            var command = commandHistory.ExecutedCommands.FirstOrDefault(c =>
                string.Equals(cmd, c.Command, StringComparison.Ordinal) && args.NullableEqualTo(c.Args));

            if (command == null)
            {
                command = new ExecutedCommand
                { Command = cmd, Args = args, ExecutionCount = 1, LastExecution = DateTime.UtcNow };
            }
            else
            {
                command.ExecutionCount++;

                command.LastExecution = DateTime.UtcNow;

                commandHistory.ExecutedCommands.Remove(command);
            }

            commandHistory.ExecutedCommands.Insert(0, command);

            while (commandHistory.ExecutedCommands.Count > CommandHistoryLimit)
            {
                commandHistory.ExecutedCommands.RemoveAt(CommandHistoryLimit);
            }

            _historyContainer.WriteValueAsJson(ExecutedCommandsKey, commandHistory);
        }

        #endregion Command history

        #region Links/shortcuts related

        private const string CustomSshUriScheme = "ftcmd";
        private const string CustomUriHost = "fluent.terminal";
        private const string CustomCommandQueryStringName = "cmd";
        private const string CustomArgumentsQueryStringName = "args";

        public static bool CheckScheme(Uri uri) => CustomSshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        public static CommandProfileProviderViewModel ParseUri(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, IApplicationDataContainer historyContainer)
        {
            var vm = new CommandProfileProviderViewModel(settingsService, applicationView, historyContainer);

            // ReSharper disable once ConstantConditionalAccessQualifier
            string queryString = uri.Query?.Trim();

            if (string.IsNullOrEmpty(queryString))
            {
                return vm;
            }

            if (queryString.StartsWith("?", StringComparison.Ordinal))
            {
                queryString = queryString.Substring(1);
            }

            var queryStringParams = ParseParams(queryString, '&').ToList();

            var cmdParam = queryStringParams.FirstOrDefault(t =>
                CustomCommandQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            var argsParam = queryStringParams.FirstOrDefault(t =>
                CustomArgumentsQueryStringName.Equals(t.Item1, StringComparison.OrdinalIgnoreCase));

            vm._command =
                $"{(string.IsNullOrEmpty(cmdParam?.Item2) ? Constants.SshCommandName : cmdParam.Item2)} {argsParam?.Item2}"
                    .Trim();

            vm.LoadBaseFromQueryString(queryStringParams);

            return vm;
        }

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
                $"{CustomSshUriScheme}://{CustomUriHost}/?{CustomCommandQueryStringName}={Constants.SshCommandName}&{CustomArgumentsQueryStringName}={HttpUtility.UrlEncode(match.Groups["args"].Value.Trim())}&{GetBaseQueryString()}");
        }

        #endregion Links/shortcuts related
    }
}