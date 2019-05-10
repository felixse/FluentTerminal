using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentTerminal.Models.Enums;
using System.Linq;

namespace FluentTerminal.Models
{
    public class ShellProfile
    {
        /// <summary>
        /// Replace all instances of anything resembling a newline, treating pairs of \r\n in either order as a single linebreak.
        /// </summary>
        public static Regex NewlinePattern = new Regex(@"\n\r|\r\n|\r|\n", RegexOptions.Compiled);

        public ShellProfile()
        {

        }

        public ShellProfile(ShellProfile other)
        {
            Id = other.Id;
            PreInstalled = other.PreInstalled;
            Name = other.Name;
            Arguments = other.Arguments;
            Location = other.Location;
            WorkingDirectory = other.WorkingDirectory;
            TabThemeId = other.TabThemeId;
            TerminalThemeId = other.TerminalThemeId;
            LineEndingTranslation = other.LineEndingTranslation;
            KeyBindings = other.KeyBindings.Select(x => new KeyBinding(x)).ToList();
        }

        public Guid Id { get; set; }
        public bool PreInstalled { get; set; }
        public string Name { get; set; }
        public virtual string Arguments { get; set; }
        public virtual string Location { get; set; }
        public string WorkingDirectory { get; set; }
        public int TabThemeId { get; set; }
        public LineEndingStyle LineEndingTranslation { get; set; }
        public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();

        public string TranslateLineEndings(string content)
        {
            switch (LineEndingTranslation)
            {
                case LineEndingStyle.ToCR:
                    return NewlinePattern.Replace(content, "\r");
                case LineEndingStyle.ToCRLF:
                    return NewlinePattern.Replace(content, "\r\n");
                case LineEndingStyle.ToLF:
                    return NewlinePattern.Replace(content, "\n");
                case LineEndingStyle.DoNotModify:
                default:
                    return content;
            }
        }

        public Guid TerminalThemeId { get; set; }
        public ICollection<KeyBinding> KeyBindings { get; set; } = new List<KeyBinding>();

        public override bool Equals(object obj)
        {
            if (obj is ShellProfile other)
            {
                return other.Id.Equals(Id)
                    && other.PreInstalled.Equals(PreInstalled)
                    && other.Name.Equals(Name)
                    && other.Arguments.Equals(Arguments)
                    && other.Location.Equals(Location)
                    && other.WorkingDirectory.Equals(WorkingDirectory)
                    && other.TabThemeId.Equals(TabThemeId)
                    && other.TerminalThemeId.Equals(TerminalThemeId)
                    && other.LineEndingTranslation == LineEndingTranslation
                    && other.KeyBindings.SequenceEqual(KeyBindings);
            }
            return false;
        }

        /// <summary>
        /// Checks the profile for errors and returns string describing the found errors, if any.
        /// Returns null if no errors found.
        /// </summary>
        public virtual string ValidateAndGetErrors()
        {
            return null;
        }
    }
}