using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentTerminal.Models.Enums;
using System.Linq;
using System.Runtime.Serialization;

namespace FluentTerminal.Models
{
    [DataContract]
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

        [DataMember(Order = 0)]
        public Guid Id { get; set; }

        [DataMember(Order = 1)]
        public bool PreInstalled { get; set; }

        [DataMember(Order = 2)]
        public string Name { get; set; }

        [DataMember(Order = 3)]
        public string Arguments { get; set; }

        [DataMember(Order = 4)]
        public string Location { get; set; }

        [DataMember(Order = 5)]
        public string WorkingDirectory { get; set; }

        [DataMember(Order = 6)]
        public int TabThemeId { get; set; }

        [DataMember(Order = 7)]
        public LineEndingStyle LineEndingTranslation { get; set; }

        [DataMember(Order = 8)]
        public Guid TerminalThemeId { get; set; }

        [DataMember(Order = 9)]
        public ICollection<KeyBinding> KeyBindings { get; set; } = new List<KeyBinding>();

        public string TranslateLineEndings(string content)
        {
            switch (LineEndingTranslation)
            {
                case LineEndingStyle.ToCRLF:
                    return NewlinePattern.Replace(content, "\r\n");
                case LineEndingStyle.ToLF:
                    return NewlinePattern.Replace(content, "\n");
                case LineEndingStyle.DoNotModify:
                default:
                    return content;
            }
        }

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
    }
}