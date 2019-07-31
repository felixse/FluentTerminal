using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;
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

        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly IApplicationDataContainer _historyContainer;

        private List<CommandItemViewModel> _allCommands;

        private string _oldFilter;

        #endregion Fields

        #region Properties

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
            ITrayProcessCommunicationService trayProcessCommunicationService,
            IApplicationDataContainer historyContainer, ShellProfile original = null) : base(settingsService,
            applicationView, original)
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _historyContainer = historyContainer;

            FillCommandHistory();

            Initialize(Model);
        }

        #endregion Constructor

        #region Methods

        // Fills the view model properties from the input sshProfile
        private void Initialize(ShellProfile profile)
        {
            Command = profile.Name;
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

                profile.Name = null;

                return;
            }

            var existingProfile =
                _allProfiles.FirstOrDefault(p => string.Equals(p.Name, command, StringComparison.OrdinalIgnoreCase));

            if (existingProfile != null)
            {
                profile.Name = existingProfile.Name;
                profile.Id = existingProfile.Id;
                profile.PreInstalled = existingProfile.PreInstalled;
                profile.Location = existingProfile.Location;
                profile.Arguments = existingProfile.Arguments;
                profile.WorkingDirectory = existingProfile.WorkingDirectory;

                profile.EnvironmentVariables.Clear();

                foreach (var kvp in existingProfile.EnvironmentVariables)
                {
                    profile.EnvironmentVariables.Add(kvp.Key, kvp.Value);
                }

                Command = existingProfile.Name;

                return;
            }

            var match = CommandValidationRx.Match(command);

            if (!match.Success)
            {
                // Should not happen ever because this method gets called only if validation succeeds.
                throw new Exception("Invalid command.");
            }

            profile.Location = _trayProcessCommunicationService.GetCommandPathAsync(match.Groups["cmd"].Value).Result;
            profile.Arguments = match.Groups["args"].Success ? match.Groups["args"].Value.Trim() : null;

            profile.Name = command;
        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync();

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            var command = _command?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(command) &&
                _allProfiles.Any(p => string.Equals(p.Name, command, StringComparison.OrdinalIgnoreCase)))
            {
                // It's a saved profile
                return null;
            }

            var match = CommandValidationRx.Match(_command?.Trim() ?? string.Empty);

            if (!match.Success)
            {
                error = I18N.Translate("InvalidCommand");

                return string.IsNullOrEmpty(error) ? "Invalid command." : error;
            }

            try
            {
                var unused = await _trayProcessCommunicationService.GetCommandPathAsync(match.Groups["cmd"].Value);
            }
            catch (Exception e)
            {
                return $"{I18N.Translate("UnsupportedCommand")} '{match.Groups["cmd"].Value}'. {e.Message}";
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
            return base.HasChanges() || !_command.NullableEqualTo(Model.Name);
        }

        public void SetProfile(ShellProfile profile)
        {
            Model = profile;

            LoadFromProfile(Model);
        }

        #endregion Methods

        #region Command history

        private const int CommandHistoryLimit = 50;

        private static readonly DateTime NeverUsedTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private List<ShellProfile> _allProfiles;

        private static string GetHash(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            value = value.ToLowerInvariant();

            byte[] hashed;

            using (var md5 = MD5.Create())
            {
                hashed = md5.ComputeHash(Encoding.UTF32.GetBytes(value));
            }

            var builder = new StringBuilder();

            foreach (var b in hashed)
            {
                builder.Append(b.ToString("X2"));
            }

            return $"hist_{builder}_{value.Length:##########}";
        }

        private void FillCommandHistory()
        {
            var savedCommands = _historyContainer.GetAll()
                .Select(c => JsonConvert.DeserializeObject<ExecutedCommand>((string)c)).ToList();

            _allProfiles = SettingsService.GetShellProfiles().Union(SettingsService.GetSshProfiles()).ToList();

            var counter = 0;

            while (counter < savedCommands.Count)
            {
                var command = savedCommands[counter];

                if (command.IsProfile)
                {
                    var profile = _allProfiles.FirstOrDefault(p =>
                        string.Equals(p.Name, command.Value, StringComparison.OrdinalIgnoreCase));

                    if (profile == null)
                    {
                        _historyContainer.Delete(GetHash(command.Value));

                        savedCommands.RemoveAt(counter);

                        counter--;
                    }
                    else
                    {
                        command.ShellProfile = profile;
                    }
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

            var unusedProfiles = _allProfiles.Where(p =>
                !savedCommands.Any(c => string.Equals(p.Name, c.Value, StringComparison.OrdinalIgnoreCase))).ToList();

            IEnumerable<ExecutedCommand> commands = savedCommands
                .Union(unusedProfiles.Select(p => new ExecutedCommand
                {
                    Value = p.Name,
                    ExecutionCount = 0,
                    LastExecution = NeverUsedTime,
                    IsProfile = true,
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

        public void SaveToHistory()
        {
            var key = GetHash(Model.Name);

            var isProfile =
                _allProfiles.Any(p => string.Equals(p.Name, Model.Name, StringComparison.OrdinalIgnoreCase));

            ExecutedCommand command;

            if (_historyContainer.TryGetValue(key, out var cmd))
            {
                if (cmd is string cmdStr)
                {
                    command = JsonConvert.DeserializeObject<ExecutedCommand>(cmdStr);
                }
                else if (cmd is ExecutedCommand executedCommand)
                {
                    command = executedCommand;
                }
                else
                {
                    throw new Exception("Unexpected history type: " + cmd.GetType());
                }
            }
            else
            {
                command = new ExecutedCommand {ExecutionCount = 0};
            }

            command.Value = Model.Name;
            command.ExecutionCount++;
            command.LastExecution = DateTime.UtcNow;
            command.ShellProfile = isProfile ? null : Model;
            command.IsProfile = isProfile;

            _historyContainer.WriteValueAsJson(key, command);
        }

        public void RemoveCommand(ExecutedCommand command)
        {
            _historyContainer.Delete(GetHash(command.Value));

            CommandItemViewModel toRemove = _allCommands.FirstOrDefault(c =>
                string.Equals(c.ExecutedCommand.Value, command.Value, StringComparison.Ordinal));

            if (toRemove != null)
            {
                _allCommands.Remove(toRemove);
                Commands.Remove(toRemove);

                _historyContainer.Delete(GetHash(toRemove.ExecutedCommand.Value));
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
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            IApplicationDataContainer historyContainer)
        {
            var vm = new CommandProfileProviderViewModel(settingsService, applicationView,
                trayProcessCommunicationService, historyContainer);

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