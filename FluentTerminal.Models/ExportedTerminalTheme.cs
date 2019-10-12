using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.Models
{
    public class ExportedTerminalTheme : TerminalTheme
    {
        public ExportedTerminalTheme()
        {
        }

        public ExportedTerminalTheme(TerminalTheme other, string encodedImage)
            : base(other)
        {
            EncodedImage = encodedImage;
        }

        public string EncodedImage { get; set; }

        public override bool Equals(object obj)
        {
            return obj is ExportedTerminalTheme theme &&
                   base.Equals(obj) &&
                   EncodedImage == theme.EncodedImage;
        }

        public override int GetHashCode()
        {
            return -1751912210 + EqualityComparer<string>.Default.GetHashCode(EncodedImage);
        }
    }
}
