﻿using System;
using FluentTerminal.Models;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.App.ViewModels.Profiles
{
    public class ExecutedCommand
    {
        public string Value { get; set; }

        public DateTime LastExecution { get; set; }

        public int ExecutionCount { get; set; }

        public ProfileType ProfileType { get; set; }

        public ShellProfile ShellProfile { get; set; }
    }
}