﻿using System;

namespace FluentTerminal.Models
{
    public class TerminalTheme
    {
        public TerminalTheme()
        {
        }

        public TerminalTheme(TerminalTheme other)
        {
            Id = other.Id;
            Name = other.Name;
            Author = other.Author;
            PreInstalled = other.PreInstalled;
            Colors = new TerminalColors(other.Colors);
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public bool PreInstalled { get; set; }
        public TerminalColors Colors { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is TerminalTheme other)
            {
                return other.Name.Equals(Name)
                    && other.Author == null ? Author == null : other.Author.Equals(Author)
                    && other.Colors.Equals(Colors);
            }
            return false;
        }
    }
}