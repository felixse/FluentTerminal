﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Newtonsoft.Json;

namespace FluentTerminal.Models
{
    public class ShellProfile
    {
        /// <summary>
        /// Replace all instances of anything resembling a newline, treating pairs of \r\n in either order as a single linebreak.
        /// </summary>
        public static readonly Regex NewlinePattern = new Regex(@"\n\r|\r\n|\r|\n", RegexOptions.Compiled);
        public const int CurrentMigrationVersion = 1;

        public ShellProfile()
        {

        }

        protected ShellProfile(ShellProfile other)
        {
            Id = other.Id;
            PreInstalled = other.PreInstalled;
            Name = other.Name;
            Arguments = other.Arguments;
            Location = other.Location;
            WorkingDirectory = other.WorkingDirectory;
            TabThemeId = other.TabThemeId;
            TerminalThemeId = other.TerminalThemeId;
            UseConPty = other.UseConPty;
            UseBuffer = other.UseBuffer;
            KeyBindings = other.KeyBindings.Select(x => new KeyBinding(x)).ToList();
        }

        public Guid Id { get; set; }
        public bool PreInstalled { get; set; }
        public string Name { get; set; }
        public string Arguments { get; set; }
        public string Location { get; set; }
        public string WorkingDirectory { get; set; }
        public int TabThemeId { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; set; } = new Dictionary<string, string>();
        public bool UseConPty { get; set; }
        public bool UseBuffer { get; set; } = true;

        public int MigrationVersion { get; set; } = CurrentMigrationVersion;

        /// <summary>
        /// For attaching a data to the profile. This property doesn't get serialized nor cloned.
        /// </summary>
        [JsonIgnore]
        public object Tag { get; set; }

        public Guid TerminalThemeId { get; set; }
        public ICollection<KeyBinding> KeyBindings { get; set; } = new List<KeyBinding>();

        public virtual bool EqualTo(ShellProfile other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Id.Equals(Id)
                   && other.PreInstalled.Equals(PreInstalled)
                   && other.Name.NullableEqualTo(Name)
                   && other.Arguments.NullableEqualTo(Arguments)
                   && other.Location.NullableEqualTo(Location)
                   && other.WorkingDirectory.NullableEqualTo(WorkingDirectory)
                   && other.TabThemeId.Equals(TabThemeId)
                   && other.TerminalThemeId.Equals(TerminalThemeId)
                   && other.UseConPty == UseConPty
                   && other.UseBuffer == UseBuffer
                   && other.KeyBindings.SequenceEqual(KeyBindings);
        }

        public virtual ShellProfile Clone() => new ShellProfile(this);
    }
}