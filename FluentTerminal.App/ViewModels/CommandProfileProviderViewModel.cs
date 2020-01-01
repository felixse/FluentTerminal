﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation.Metadata;
using FluentTerminal.App.Services;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Profiles;
using FluentTerminal.Models;

namespace FluentTerminal.App.ViewModels
{
    public class CommandProfileProviderViewModel : ProfileProviderViewModelBase
    {
        #region Static

        private static readonly Regex WhiteSpaceRx = new Regex(@"\s", RegexOptions.Compiled);

        private static string ExtractCommandAndArgs(string commandAndArgs, out string args)
        {
            args = null;

            commandAndArgs = commandAndArgs?.Trim();

            if (string.IsNullOrEmpty(commandAndArgs)) return commandAndArgs;

            switch (commandAndArgs[0])
            {
                case '\'':
                case '"':

                    for (var i = 1; i < commandAndArgs.Length; ++i)
                    {
                        if (commandAndArgs[0] == commandAndArgs[i])
                        {
                            args = commandAndArgs.Length > i + 1
                                ? commandAndArgs.Substring(i + 1).Trim()
                                : string.Empty;

                            return commandAndArgs.Substring(1, i - 1).Trim();
                        }
                    }

                    return commandAndArgs;

                default:

                    var match = WhiteSpaceRx.Match(commandAndArgs);

                    if (match.Success)
                    {
                        args = commandAndArgs.Substring(match.Index).Trim();

                        return commandAndArgs.Substring(0, match.Index).Trim();
                    }

                    return commandAndArgs;
            }
        }

        #endregion Static

        #region Fields

        private readonly ITrayProcessCommunicationService _trayProcessCommunicationService;
        private readonly ICommandHistoryService _historyService;

        private readonly List<CommandItemViewModel> _allCommands;
        private readonly Lazy<List<ShellProfile>> _allProfiles;

        private string _processedFilter;

        private string _scheduledFilter;

        private readonly object _filteringLock = new object();

        private bool _filterLoopRunning;

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
            ITrayProcessCommunicationService trayProcessCommunicationService, ICommandHistoryService historyService,
            ShellProfile original = null) : base(settingsService, applicationView, false,
            original ?? new ShellProfile
            {
                UseConPty = settingsService.GetApplicationSettings().UseConPty &&
                            ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 7)
            })
        {
            _trayProcessCommunicationService = trayProcessCommunicationService;
            _historyService = historyService;

            _allProfiles = new Lazy<List<ShellProfile>>(() =>
                SettingsService.GetSshProfiles().Union(SettingsService.GetShellProfiles()).ToList());

            _allCommands = _historyService
                .GetHistoryMostUsedFirst(true, profilesProvider: () => _allProfiles.Value.ToList())
                .Select(c => new CommandItemViewModel(c)).ToList();

            Commands = new ObservableCollection<CommandItemViewModel>(_allCommands);

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

        protected override async Task CopyToProfileAsync(ShellProfile profile)
        {
            await base.CopyToProfileAsync(profile).ConfigureAwait(false);

            var command = _command?.Trim();

            if (string.IsNullOrEmpty(command))
            {
                profile.Location = null;
                profile.Arguments = null;

                profile.Name = null;

                return;
            }

            var existingProfile =
                _allProfiles.Value.FirstOrDefault(p =>
                    string.Equals(p.Name, command, StringComparison.OrdinalIgnoreCase));

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

            var cmd = ExtractCommandAndArgs(command, out var args);

            if (cmd.Equals(Constants.MoshCommandName, StringComparison.OrdinalIgnoreCase) ||
                cmd.Equals($"{Constants.MoshCommandName}.exe", StringComparison.OrdinalIgnoreCase) ||
                cmd.Contains(Path.DirectorySeparatorChar))
            {
                profile.Location = cmd;
            }
            else
            {
                profile.Location =
                    await _trayProcessCommunicationService.GetCommandPathAsync(cmd).ConfigureAwait(false);
            }

            profile.Arguments = args;

            profile.Name = command;
        }

        public override async Task<string> ValidateAsync()
        {
            var error = await base.ValidateAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }

            var command = _command?.Trim() ?? string.Empty;

            if (!string.IsNullOrEmpty(command) &&
                _allProfiles.Value.Any(p => string.Equals(p.Name, command, StringComparison.OrdinalIgnoreCase)))
            {
                // It's a saved profile
                return null;
            }

            command = ExtractCommandAndArgs(command, out var args);

            if (command.Equals(Constants.MoshCommandName, StringComparison.OrdinalIgnoreCase) ||
                command.Equals($"{Constants.MoshCommandName}.exe", StringComparison.OrdinalIgnoreCase) ||
                command.Equals(Constants.SshCommandName, StringComparison.OrdinalIgnoreCase) ||
                command.Equals($"{Constants.SshCommandName}.exe", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrEmpty(args)
                    ? I18N.TranslateWithFallback("CommandArgumentsMandatory", "Command arguments are missing.")
                    : null;
            }

            if (command.Contains(Path.DirectorySeparatorChar))
            {
                if (await _trayProcessCommunicationService.CheckFileExistsAsync(command).ConfigureAwait(false))
                {
                    return null;
                }

                return $"{I18N.TranslateWithFallback("FileNotFound", "File not found:")} '{command}'";
            }

            try
            {
                await _trayProcessCommunicationService.GetCommandPathAsync(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                return
                    $"{I18N.TranslateWithFallback("UnsupportedCommand", "Unsupported command:")} '{command}'. {e.Message}";
            }

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

        public void SetFilter(string filter)
        {
            filter = filter?.Trim().ToLowerInvariant() ?? string.Empty;

            lock (_filteringLock)
            {
                _scheduledFilter = filter;

                if (!_filterLoopRunning)
                {
                    _filterLoopRunning = true;

                    // ReSharper disable once AssignmentIsFullyDiscarded
                    _ = FilteringLoop();
                }
            }
        }

        private async Task FilteringLoop()
        {
            while (true)
            {
                string filter;
                bool containsPrevious;

                lock (_filteringLock)
                {
                    filter = _scheduledFilter;

                    _scheduledFilter = null;

                    if (filter == null || filter.NullableEqualTo(_processedFilter))
                    {
                        _filterLoopRunning = false;

                        return;
                    }

                    containsPrevious = !string.IsNullOrEmpty(_processedFilter) &&
                                       filter.Contains(_processedFilter, StringComparison.Ordinal);

                    _processedFilter = filter;
                }

                try
                {
                    await SetFilterAsync(filter, containsPrevious).ConfigureAwait(false);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private Task SetFilterAsync(string filter, bool containsPrevious)
        {
            var toCheck = containsPrevious ? (ICollection<CommandItemViewModel>)Commands : _allCommands;

            var words = filter.SplitWords().ToArray();

            if (toCheck.Count > 1 && toCheck.Count * words.Length > 3)
            {
                Parallel.ForEach(toCheck, cmd => cmd.CalculateMatch(filter, words));
            }
            else
            {
                foreach (var command in toCheck)
                {
                    command.CalculateMatch(filter, words);
                }
            }

            return ApplicationView.ExecuteOnUiThreadAsync(() => {

                foreach (var command in toCheck)
                {
                    command.ShowMatch(filter, null);
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
            });
        }

        public void RemoveCommand(ExecutedCommand command)
        {
            _historyService.Delete(command);

            var toRemove = _allCommands.FirstOrDefault(c =>
                string.Equals(c.ExecutedCommand.Value, command.Value, StringComparison.Ordinal));

            if (toRemove != null)
            {
                _allCommands.Remove(toRemove);
                Commands.Remove(toRemove);
            }
        }

        public bool IsProfileCommand(ExecutedCommand command) => _allProfiles.Value.Any(p =>
            command.Value.NullableEqualTo(p.Name, StringComparison.OrdinalIgnoreCase));

        #endregion Methods

        #region Links/shortcuts related

        private const string CustomSshUriScheme = "ftcmd";
        private const string CustomUriHost = "fluent.terminal";
        private const string CustomCommandQueryStringName = "cmd";

        public static bool CheckScheme(Uri uri) => CustomSshUriScheme.Equals(uri?.Scheme, StringComparison.OrdinalIgnoreCase);

        public static CommandProfileProviderViewModel ParseUri(Uri uri, ISettingsService settingsService,
            IApplicationView applicationView, ITrayProcessCommunicationService trayProcessCommunicationService,
            ICommandHistoryService historyService)
        {
            var vm = new CommandProfileProviderViewModel(settingsService, applicationView,
                trayProcessCommunicationService, historyService);

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

            if (cmdParam == null)
            {
                throw new Exception(I18N.TranslateWithFallback("InvalidLink", "Invalid link."));
            }

            vm._command = cmdParam.Item2;

            vm.LoadBaseFromQueryString(queryStringParams);

            return vm;
        }

        public override async Task<Tuple<bool, string>> GetUrlAsync()
        {
            var error = await ValidateAsync().ConfigureAwait(false);

            if (!string.IsNullOrEmpty(error))
            {
                return Tuple.Create(false, error);
            }

            return Tuple.Create(true,
                $"{CustomSshUriScheme}://{CustomUriHost}/?{CustomCommandQueryStringName}={HttpUtility.UrlEncode(_command)}&{GetBaseQueryString()}");
        }

        #endregion Links/shortcuts related
    }
}
