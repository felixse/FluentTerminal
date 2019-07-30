using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;
using Newtonsoft.Json;

namespace FluentTerminal.App.ViewModels
{
    public class CommandProfileProviderViewModel : ProfileProviderViewModelBase
    {
        #region Static

        private static readonly Regex CommandValidationRx = new Regex(@"^(?<cmd>[^\s\.]+)(\.exe)?(\s+(?<args>\S.*))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        #endregion Static

        #region Fields

        private readonly IApplicationDataContainer _historyContainer;

        private List<CommandItemViewModel> _allCommands;

        private string _oldFilter;

        #endregion Fields

        #region Properties

        private ProfileType _profileType = ProfileType.History;

        public ProfileType ProfileType
        {
            get => _profileType;
            private set => Set(ref _profileType, value);
        }

        private string _command;

        public string Command
        {
            get => _command;
            set => Set(ref _command, value);
        }

        private ObservableCollection<CommandItemViewModel> _commands;

        public ObservableCollection<CommandItemViewModel> Commands
        {
            get => _commands;
            private set => Set(ref _commands, value);
        }

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
            Command = sshProfile.Name;
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

            if (ProfileType != ProfileType.Shell && ProfileType != ProfileType.Ssh)
            {
                profile.Name = $"{profile.Location} {profile.Arguments}".Trim();
            }
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

            //if (!match.Groups["args"].Success)
            //{
            //    error = I18N.Translate("CommandArgumentsMandatory");

            //    return string.IsNullOrEmpty(error) ? "Command arguments are missing." : error;
            //}

            return null;
        }

        public override bool HasChanges()
        {
            return base.HasChanges() || !_command.NullableEqualTo($"{Model.Location} {Model.Arguments}");
        }

        public void SetProfile(ProfileType profileType, ShellProfile profile)
        {
            Model = profile;

            ProfileType = profileType;

            LoadFromProfile(Model);
        }

        #endregion Methods

        #region Command history

        private const string HistoryKeyFormat = "hist_{0:##########}_{1:##########}";
        private const int CommandHistoryLimit = 50;

        private static readonly DateTime NeverUsedTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static string GetHash(string value)
        {
            value = value.ToLower();

            return string.Format(HistoryKeyFormat, value.GetHashCode(), value.Length);
        }

        private void FillCommandHistory()
        {
            var savedCommands = _historyContainer.GetAll()
                .Select(c => JsonConvert.DeserializeObject<ExecutedCommand>((string)c)).ToList();

            var sshProfiles = SettingsService.GetSshProfiles().ToList();

            var shellProfiles = SettingsService.GetShellProfiles().ToList();

            var counter = 0;

            while (counter < savedCommands.Count)
            {
                var command = savedCommands[counter];

                switch (command.ProfileType)
                {
                    case ProfileType.Ssh:

                        var sshProfile = sshProfiles.FirstOrDefault(p =>
                            string.Equals(p.Name, command.Value, StringComparison.OrdinalIgnoreCase));

                        if (sshProfile == null)
                        {
                            _historyContainer.Delete(GetHash(command.Value));

                            savedCommands.RemoveAt(counter);

                            counter--;
                        }
                        else
                        {
                            command.ShellProfile = sshProfile;

                            sshProfiles.Remove(sshProfile);
                        }

                        break;

                    case ProfileType.Shell:

                        var shellProfile = shellProfiles.FirstOrDefault(p =>
                            string.Equals(p.Name, command.Value, StringComparison.OrdinalIgnoreCase));

                        if (shellProfile == null)
                        {
                            _historyContainer.Delete(GetHash(command.Value));

                            savedCommands.RemoveAt(counter);

                            counter--;
                        }
                        else
                        {
                            command.ShellProfile = shellProfile;

                            shellProfiles.Remove(shellProfile);
                        }

                        break;
                }

                counter++;
            }

            if (savedCommands.Count > CommandHistoryLimit)
            {
                var toRemove = savedCommands.OrderBy(c => c.LastExecution)
                    .Take(savedCommands.Count - CommandHistoryLimit).ToList();

                foreach (var remove in toRemove)
                {
                    savedCommands.Remove(remove);

                    _historyContainer.Delete(GetHash(remove.Value));
                }
            }

            IEnumerable<ExecutedCommand> commands = savedCommands
                .Union(sshProfiles.Select(p => new ExecutedCommand
                {
                    Value = p.Name,
                    ExecutionCount = 0,
                    LastExecution = NeverUsedTime,
                    ProfileType = ProfileType.Ssh,
                    ShellProfile = p
                })).Union(shellProfiles.Select(p => new ExecutedCommand
                {
                    Value = p.Name,
                    ExecutionCount = 0,
                    LastExecution = NeverUsedTime,
                    ProfileType = ProfileType.Shell,
                    ShellProfile = p
                }));

            _allCommands = commands.OrderByDescending(c => c.ExecutionCount).ThenByDescending(c => c.LastExecution)
                .Select(c => new CommandItemViewModel(c)).ToList();

            Commands = new ObservableCollection<CommandItemViewModel>(_allCommands);
        }

        public void SetFilter(string filter)
        {
            if (filter.NullableEqualTo(_oldFilter, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var toCheck = !string.IsNullOrEmpty(_oldFilter) && filter.Contains(_oldFilter, StringComparison.OrdinalIgnoreCase)
                ? (IEnumerable<CommandItemViewModel>) Commands
                : _allCommands;

            _oldFilter = filter;

            var words = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var commandItem in toCheck)
            {
                commandItem.SetFilter(filter, words);
            }

            //if (string.IsNullOrEmpty(filter))
            //{
            //    Commands = new ObservableCollection<CommandItemViewModel>(_allCommands);

            //    return;
            //}

            var index = 0;

            foreach (var commandItem in _allCommands)
            {
                if (commandItem.IsMatch)
                {
                    if (Commands.Count <= index)
                    {
                        Commands.Add(commandItem);
                    }
                    else if (!ReferenceEquals(commandItem, Commands[index]))
                    {
                        Commands.Insert(index, commandItem);
                    }

                    index++;
                }
                else if (Commands.Count > index && ReferenceEquals(commandItem, Commands[index]))
                {
                    Commands.RemoveAt(index);
                }
            }
        }

        public void SaveCommand(string profileOrCommand, ShellProfile profile)
        {
            profileOrCommand = profileOrCommand?.Trim();

            if (string.IsNullOrEmpty(profileOrCommand))
            {
                throw new ArgumentNullException(nameof(profileOrCommand));
            }

            var key = GetHash(profileOrCommand);

            var command = _historyContainer.TryGetValue(key, out var cmd)
                ? (ExecutedCommand) cmd
                : new ExecutedCommand {ExecutionCount = 0};

            command.Value = profileOrCommand;
            command.ExecutionCount++;
            command.LastExecution = DateTime.UtcNow;
            command.ShellProfile = profile;
            command.ProfileType = ProfileType;

            _historyContainer.WriteValueAsJson(key, command);
        }

        public void RemoveCommand(ExecutedCommand command)
        {
            _historyContainer.Delete(GetHash(command.Value));

            CommandItemViewModel toRemove = Commands.FirstOrDefault(c =>
                string.Equals(c.ExecutedCommand.Value, command.Value, StringComparison.Ordinal));

            if (toRemove != null)
            {
                Commands.Remove(toRemove);
            }
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