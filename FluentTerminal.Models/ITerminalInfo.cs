using System;
using FluentTerminal.Models.Enums;

namespace FluentTerminal.Models
{
    public interface ITerminalInfo
    {
        LineEndingStyle LineEndingTranslation { get; set; }

        Guid TerminalThemeId { get; set; }

        int TabThemeId { get; set; }

        bool UseConPty { get; set; }
    }
}