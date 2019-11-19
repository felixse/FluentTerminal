using System;

namespace FluentTerminal.Models
{
    public class ExecutedCommand
    {
        public string Value { get; set; }

        public bool IsProfile { get; set; }

        public DateTime LastExecution { get; set; }

        public int ExecutionCount { get; set; }

        public ShellProfile ShellProfile { get; set; }

        public ExecutedCommand Clone() => new ExecutedCommand
        {
            Value = Value, IsProfile = IsProfile, LastExecution = LastExecution, ExecutionCount = ExecutionCount,
            ShellProfile = ShellProfile?.Clone()
        };
    }
}