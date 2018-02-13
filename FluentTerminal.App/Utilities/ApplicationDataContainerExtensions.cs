using Newtonsoft.Json;
using System;
using Windows.Storage;

namespace FluentTerminal.App.Utilities
{
    public static class ApplicationDataContainerExtensions
    {
        public static T ReadValueFromJson<T>(this ApplicationDataContainer container, string name, T fallbackValue)
        {
            if (container.Values.TryGetValue(name, out object value))
            {
                if (fallbackValue == null)
                {
                    fallbackValue = Activator.CreateInstance<T>();
                }

                JsonConvert.PopulateObject((string)value, fallbackValue);
            }
            return fallbackValue;
        }

        public static void WriteValueAsJson<T>(this ApplicationDataContainer container, string name, T value)
        {
            var serialized = JsonConvert.SerializeObject(value);
            container.Values[name] = serialized;
        }
    }
}
