using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public class SystemFontService : ISystemFontService
    {
        public IEnumerable<string> GetSystemFontFamilies()
        {
            return CanvasTextFormat.GetSystemFontFamilies();
        }
    }
}
