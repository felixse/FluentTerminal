using System;
using System.Collections.Generic;
using System.Text;

namespace FluentTerminal.App.ViewModels.Menu
{
    public sealed class Mdl2Icon
    {
        public string Color { get; }
        public string Glyph { get; }

        private Mdl2Icon(string glyph, string color = null)
        {
            Glyph = glyph;
            Color = color;
        }

        public static Mdl2Icon Link(string color = null) => new Mdl2Icon("\uE71B", color);

        public static Mdl2Icon Copy(string color = null) => new Mdl2Icon("\uE8C8", color);

        public static Mdl2Icon Paste(string color = null) => new Mdl2Icon("\uE77F", color);

        public static Mdl2Icon Edit(string color = null) => new Mdl2Icon("\uE70F", color);

        public static Mdl2Icon Search(string color = null) => new Mdl2Icon("\uE721", color);

        public static Mdl2Icon Add(string color = null) => new Mdl2Icon("\uE710", color);

        public static Mdl2Icon Cancel(string color = null) => new Mdl2Icon("\uE711", color);

        public static Mdl2Icon PaginationDotOutline10(string color = null) => new Mdl2Icon("\uF126", color);

        public static Mdl2Icon PaginationDotSolid10(string color = null) => new Mdl2Icon("\uF127", color);

        public static Mdl2Icon NewWindow(string color = null) => new Mdl2Icon("\uE78B", color);

        public static Mdl2Icon History(string color = null) => new Mdl2Icon("\uE81C", color);

        public static Mdl2Icon Settings(string color = null) => new Mdl2Icon("\uE713", color);

        public static Mdl2Icon Info(string color = null) => new Mdl2Icon("\uE946", color);
    }
}
