using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface IApplicationLanguageService
    {
        IEnumerable<string> Languages { get; }

        void SetLanguage(string language);

        string GetCurrentLanguage();
    }
}
