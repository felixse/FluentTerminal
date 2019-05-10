using System.ComponentModel;
// ReSharper disable InconsistentNaming

namespace FluentTerminal.Models.Enums
{
    public enum LineEndingStyle
    {
        [Description("Do not modify")]
        DoNotModify = 0,

        [Description("Convert to CR")]
        ToCR,

        [Description("Convert to CRLF")]
        ToCRLF,

        [Description("Convert to LF")]
        ToLF
    }
}
