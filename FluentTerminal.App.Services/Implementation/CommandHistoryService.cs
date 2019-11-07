using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using FluentTerminal.Models;
using Newtonsoft.Json;

namespace FluentTerminal.App.Services.Implementation
{
    public class CommandHistoryService : ICommandHistoryService
    {
        private readonly IApplicationDataContainer _historyContainer;

        public CommandHistoryService(ApplicationDataContainers containers)
        {
            _historyContainer = containers.HistoryContainer;
        }

        public List<ExecutedCommand> GetAll()
        {
            var commands = GetAllPrivate();

            for (var i = 0; i < commands.Count; i++)
            {
                var command = commands[i];

                if (command.ShellProfile == null) continue;

                var newProfile = MoshBackwardCompatibility.FixProfile(command.ShellProfile);

                if (ReferenceEquals(command.ShellProfile, newProfile)) continue;

                if (!command.IsProfile)
                {
                    newProfile.Name = $"{newProfile.Location} {newProfile.Arguments}".Trim();
                }

                var newCommand = new ExecutedCommand
                {
                    Value = newProfile.Name, IsProfile = command.IsProfile, LastExecution = command.LastExecution,
                    ExecutionCount = command.ExecutionCount, ShellProfile = newProfile
                };

                commands.Insert(i, newCommand);

                commands.RemoveAt(i + 1);

                Delete(command);

                Save(newCommand);
            }

            return commands;
        }

        private List<ExecutedCommand> GetAllPrivate()
        {
            try
            {
                return _historyContainer.GetAll()
                    .Select(c => JsonConvert.DeserializeObject<ExecutedCommand>((string) c)).ToList();
            }
            catch
            {
                // ignored
            }

            _historyContainer.Clear();

            return new List<ExecutedCommand>();
        }

        public void Clear() => _historyContainer.Clear();

        public bool TryGetCommand(string value, out ExecutedCommand executedCommand)
        {
            var found = TryGetCommandPrivate(value, out executedCommand);

            if (!found || executedCommand?.ShellProfile == null) return found;

            var newProfile = MoshBackwardCompatibility.FixProfile(executedCommand.ShellProfile);

            if (ReferenceEquals(executedCommand.ShellProfile, newProfile)) return true;

            if (!executedCommand.IsProfile)
            {
                newProfile.Name = $"{newProfile.Location} {newProfile.Arguments}".Trim();
            }

            var newCommand = new ExecutedCommand
            {
                Value = newProfile.Name,
                IsProfile = executedCommand.IsProfile,
                LastExecution = executedCommand.LastExecution,
                ExecutionCount = executedCommand.ExecutionCount,
                ShellProfile = newProfile
            };

            Delete(executedCommand);

            Save(newCommand);

            executedCommand = newCommand;

            return true;
        }

        private bool TryGetCommandPrivate(string value, out ExecutedCommand executedCommand)
        {
            var key = GetHash(value);

            executedCommand = null;

            if (!_historyContainer.TryGetValue(key, out var cmd)) return false;

            if (cmd is string cmdStr)
            {
                try
                {
                    executedCommand = JsonConvert.DeserializeObject<ExecutedCommand>(cmdStr);

                    return true;
                }
                catch
                {
                    // ignored
                }
            }
            else if (cmd is ExecutedCommand execCmd)
            {
                executedCommand = execCmd;

                return true;
            }

            // Not a valid command, so delete it:
            _historyContainer.Delete(key);

            return false;

        }

        public void Delete(ExecutedCommand executedCommand) => _historyContainer.Delete(GetHash(executedCommand.Value));

        public void Save(ExecutedCommand executedCommand) =>
            _historyContainer.WriteValueAsJson(GetHash(executedCommand.Value), executedCommand);

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
    }
}