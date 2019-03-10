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
                return Equals(other.Name, Name)
                    && Equals(other.Author, Author)
                    && Equals(other.Colors, Colors);
            }
            return false;
        }
    }
}