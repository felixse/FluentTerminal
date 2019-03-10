using FluentTerminal.Models;
using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;
using System.Linq;

namespace FluentTerminal.App.Services
{
    public class SystemFontService : ISystemFontService
    {
        public IEnumerable<FontInfo> GetSystemFontFamilies()
        {
            return CanvasFontSet.GetSystemFontSet().Fonts.Select(x => new FontInfo { Name = x.FamilyNames.Values.FirstOrDefault(), IsMonospaced = x.IsMonospaced }).Distinct(new FontInfoComparer());
        }

        private class FontInfoComparer : IEqualityComparer<FontInfo>
        {
            public bool Equals(FontInfo x, FontInfo y)
            {
                return x.Name == y.Name;
            }

            public int GetHashCode(FontInfo obj)
            {
                return obj.Name.GetHashCode();
            }
        }
    }
}