using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface ISystemFontService
    {
        IEnumerable<string> GetSystemFontFamilies();
    }
}
