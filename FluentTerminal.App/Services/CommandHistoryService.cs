using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentTerminal.App.Services.Implementation;
using FluentTerminal.Models;
using FluentTerminal.Models.Messages;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;

namespace FluentTerminal.App.Services
{
    public class CommandHistoryService : ICommandHistoryService
    {
        #region Constants

        // Maximal number of commands to save to history
        private const int MaxHistory = 1_000;

        private static readonly DateTime NeverUsedTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #endregion Constants

        #region Static

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

        #endregion Static

        #region Fields

        private readonly IApplicationDataContainer _historyContainer;
        private readonly ISettingsService _settingsService;

        private List<ExecutedCommand> _history;

        #endregion Fields

        #region Constructor

        public CommandHistoryService(ApplicationDataContainers containers, ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _historyContainer = containers.HistoryContainer;
        }

        #endregion Constructor

        #region Methods

        private List<ShellProfile> GetAllProfiles() =>
            _settingsService.GetSshProfiles().Union(_settingsService.GetShellProfiles()).ToList();

        private List<ExecutedCommand> GetRawHistory(Func<List<ShellProfile>> profilesProvider = null)
        {
            if (_history == null)
            {
                _history = LoadHistory();

                CleanupHistory(profilesProvider);
            }

            return _history;
        }

        private List<ExecutedCommand> LoadHistory()
        {
            List<ExecutedCommand> history = null;

            try
            {
                history = _historyContainer.GetAll()
                    .Select(c => JsonConvert.DeserializeObject<ExecutedCommand>((string) c)).ToList();
            }
            catch
            {
                // ignored
            }

            if (history == null)
            {
                // If there's an issue with getting history, we simply clear history as if the app haven't been used at all.
                // Not too useful fix, but it shouldn't happen ever anyway.
                _historyContainer.Clear();

                return new List<ExecutedCommand>();
            }

            FixMoshBackwardCompatibility(history);

            return history;
        }

        private void FixMoshBackwardCompatibility(List<ExecutedCommand> commands)
        {
            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];

                if (command.ShellProfile == null) continue;

                var newProfile = MoshBackwardCompatibility.FixProfile(command.ShellProfile);

                if (ReferenceEquals(command.ShellProfile, newProfile)) continue;

                if (!command.ProfileId.HasValue)
                {
                    newProfile.Name = $"{newProfile.Location} {newProfile.Arguments}".Trim();
                }

                var newCommand = new ExecutedCommand
                {
                    Value = newProfile.Name,
                    ProfileId = command.ProfileId,
                    LastExecution = command.LastExecution,
                    ExecutionCount = command.ExecutionCount,
                    ShellProfile = newProfile
                };

                commands.Insert(i, newCommand);

                commands.RemoveAt(i + 1);

                Delete(command);

                Save(newCommand);
            }
        }

        private void Save(ExecutedCommand executedCommand)
        {
            _historyContainer.WriteValueAsJson(GetHash(executedCommand.Value), executedCommand);

            Messenger.Default.Send(new CommandHistoryChangedMessage());
        }

        /// <summary>
        /// Removes all profiles from history if they are deleted in the settings page meanwhile.
        /// </summary>
        private void CleanupHistory(Func<List<ShellProfile>> profilesProvider = null)
        {
            if (_history == null)
            {
                // Not loaded yet, so we won't clear. This won't happen though.

                return;
            }

            if (!_history.Any())
            {
                return;
            }

            var profiles = profilesProvider?.Invoke() ?? GetAllProfiles();

            for (var i = 0; i < _history.Count; i++)
            {
                var command = _history[i];

                if (command.ProfileId.HasValue && !profiles.Any(p => p.Id.Equals(command.ProfileId.Value)))
                {
                    _history.Remove(command);

                    Delete(command);

                    i--;

                    continue;
                }

                if (command.ShellProfile == null)
                {
                    var profile = profiles.FirstOrDefault(p =>
                        string.Equals(p.Name, command.Value, StringComparison.OrdinalIgnoreCase));

                    if (profile == null)
                    {
                        _history.Remove(command);

                        Delete(command);

                        i--;
                    }
                    else
                    {
                        command.ProfileId = profile.Id;

                        Save(command);
                    }
                }
            }
        }

        private IEnumerable<ExecutedCommand> GetHistory(bool byNumberOfUsages, bool includeProfiles,
            Func<List<ShellProfile>> profilesProvider = null)
        {
            var allProfiles = new Lazy<List<ShellProfile>>(profilesProvider ?? GetAllProfiles);

            var history = GetRawHistory(() => allProfiles.Value);

            if (!history.Any())
            {
                yield break;
            }

            var ordered = byNumberOfUsages
                ? history.OrderByDescending(c => c.ExecutionCount).ThenByDescending(c => c.LastExecution).ToList()
                : history.OrderByDescending(c => c.LastExecution).ThenByDescending(c => c.ExecutionCount).ToList();

            foreach (var command in ordered)
            {
                if (command.ProfileId.HasValue)
                {
                    var profile = allProfiles.Value.FirstOrDefault(p => p.Id.Equals(command.ProfileId.Value));

                    if (profile == null)
                    {
                        history.Remove(command);

                        Delete(command);
                    }
                    else
                    {
                        allProfiles.Value.Remove(profile);

                        var newCommand = command.Clone();

                        newCommand.ShellProfile = profile;

                        yield return newCommand;
                    }
                }
                else
                {
                    yield return command.Clone();
                }
            }

            if (!includeProfiles)
            {
                yield break;
            }

            foreach (var unusedProfile in allProfiles.Value)
            {
                yield return new ExecutedCommand
                {
                    Value = unusedProfile.Name,
                    ProfileId = unusedProfile.Id,
                    ExecutionCount = 0,
                    LastExecution = NeverUsedTime,
                    ShellProfile = unusedProfile
                };
            }
        }

        /// <inheritdoc cref="ICommandHistoryService.GetHistoryRecentFirst"/>
        public List<ExecutedCommand> GetHistoryRecentFirst(bool includeProfiles = false, int top = int.MaxValue,
            Func<List<ShellProfile>> profileProvider = null) =>
            GetHistory(false, includeProfiles, profileProvider).Take(top).ToList();

        /// <inheritdoc cref="ICommandHistoryService.GetHistoryMostUsedFirst"/>
        public List<ExecutedCommand> GetHistoryMostUsedFirst(bool includeProfiles = false, int top = int.MaxValue,
            Func<List<ShellProfile>> profileProvider = null) =>
            GetHistory(true, includeProfiles, profileProvider).Take(top).ToList();

        public void MarkUsed(ShellProfile profile)
        {
            var history = GetRawHistory();

            var existing = history.FirstOrDefault(c =>
                c.ProfileId?.Equals(profile.Id) ??
                string.Equals(profile.Name, c.Value, StringComparison.OrdinalIgnoreCase));

            if (existing == null)
            {
                if (string.IsNullOrEmpty(profile.Name))
                {
                    profile.Name = $"{profile.Location} {profile.Arguments}".Trim();
                }

                existing = new ExecutedCommand {Value = profile.Name};

                if (GetAllProfiles().Any(p => p.Id.Equals(profile.Id)))
                {
                    existing.ProfileId = profile.Id;
                }
                else
                {
                    existing.ShellProfile = profile;
                }

                var overflow = history.Count - MaxHistory + 1;

                if (overflow > 0)
                {
                    // We already have max number of commands in history, so we need to delete some
                    // Let's first try with cleanup
                    CleanupHistory();

                    overflow = history.Count - MaxHistory + 1;

                    if (overflow > 0)
                    {
                        // We will remove the oldest commands
                        var toRemove = history.OrderBy(c => c.LastExecution).ThenBy(c => c.ExecutionCount)
                            .Take(overflow).ToList();

                        foreach (var command in toRemove)
                        {
                            history.Remove(command);

                            Delete(command);
                        }
                    }
                }

                history.Add(existing);
            }

            existing.LastExecution = DateTime.UtcNow;
            existing.ExecutionCount++;

            Save(existing);
        }

        public void Clear()
        {
            _history?.Clear();

            _historyContainer.Clear();

            Messenger.Default.Send(new CommandHistoryChangedMessage());
        }

        public void Delete(ExecutedCommand executedCommand)
        {
            _history?.RemoveAll(c => string.Equals(c.Value, executedCommand.Value, StringComparison.OrdinalIgnoreCase));

            _historyContainer.Delete(GetHash(executedCommand.Value));

            Messenger.Default.Send(new CommandHistoryChangedMessage());
        }

        #endregion Methods
    }
}