using System.ComponentModel;

namespace FluentTerminal.Models.Enums
{
    public enum LineEndingStyle
    {
        [Description("Do not modify")]
        DoNotModify,

        [Description("Convert to CRLF")]
        ToCRLF,

        [Description("Convert to LF")]
        ToLF
    }
}
