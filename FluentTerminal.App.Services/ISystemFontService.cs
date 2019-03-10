using FluentTerminal.Models;
using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISystemFontService
    {
        IEnumerable<FontInfo> GetSystemFontFamilies();
    }
}