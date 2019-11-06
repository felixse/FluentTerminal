using System.Collections.Generic;
using FluentTerminal.Models;

namespace FluentTerminal.App.Services
{
    public interface ICommandHistoryService
    {
        List<ExecutedCommand> GetAll();

        void Clear();

        bool TryGetCommand(string value, out ExecutedCommand executedCommand);

        void Delete(ExecutedCommand executedCommand);

        void Save(ExecutedCommand executedCommand);
    }
}