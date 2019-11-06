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

        public CommandHistoryService(ApplicationDataContainers containers) =>
            _historyContainer = containers.HistoryContainer;

        public List<ExecutedCommand> GetAll()
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