using System.Collections.Generic;

namespace FluentTerminal.App.Services
{
    public interface IApplicationDataContainer
    {
        T ReadValueFromJson<T>(string name, T fallbackValue);

        void WriteValueAsJson<T>(string name, T value);

        bool TryGetValue(string key, out object value);

        void SetValue(string key, object value);

        void Delete(string key);

        IEnumerable<object> GetAll();
    }
}