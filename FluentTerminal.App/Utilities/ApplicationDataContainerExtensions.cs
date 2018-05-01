using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Windows.Storage;

namespace FluentTerminal.App.Utilities
{
    public static class ApplicationDataContainerExtensions
    {
        public static T ReadValueFromJson<T>(this ApplicationDataContainer container, string name, T fallbackValue)
        {
            if (container.Values.TryGetValue(name, out object value))
            {
                if (EqualityComparer<T>.Default.Equals(fallbackValue, default))
                {
                    fallbackValue = Activator.CreateInstance<T>();
                }

                JsonConvert.PopulateObject((string)value, fallbackValue);
            }
            return fallbackValue;
        }

        public static void WriteValueAsJson<T>(this ApplicationDataContainer container, string name, T value)
        {
            container.Values[name] = JsonConvert.SerializeObject(value);
        }
    }
}
