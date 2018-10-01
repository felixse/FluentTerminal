using System.ComponentModel;

namespace FluentTerminal.Models.Enums
{
    public enum LineEndingStyle
    {
        [Description("Convert to CRLF")]
        ToCRLF,

        [Description("Convert to LF")]
        ToLF,

        [Description("Do not modify")]
        DoNotModify
    }
}
