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
            return CanvasTextFormat.GetSystemFontFamilies().ToList().Select(x => new FontInfo { Name = x });
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