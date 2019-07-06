using System;
using System.Collections.Generic;

namespace FluentTerminal.App.ViewModels.Profiles
{
    public class ExecutedCommand
    {
        public string Command { get; set; }

        public string Args { get; set; }

        public DateTime LastExecution { get; set; }

        public int ExecutionCount { get; set; }
    }

    public class ExecutedCommandHistory
    {
        public List<ExecutedCommand> ExecutedCommands { get; set; }
    }
}