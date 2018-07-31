using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;

namespace FluentTerminal.App.Services.Adapters
{
    public class ApplicationDataContainerAdapter : IApplicationDataContainer
    {
        private readonly ApplicationDataContainer _applicationDataContainer;

        public ApplicationDataContainerAdapter(ApplicationDataContainer applicationDataContainer)
        {
            _applicationDataContainer = applicationDataContainer;
        }

        public void Delete(string key)
        {
            _applicationDataContainer.Values.Remove(key);
        }

        public IEnumerable<object> GetAll()
        {
            return _applicationDataContainer.Values.Select(x => x.Value);
        }

        public T ReadValueFromJson<T>(string name, T fallbackValue)
        {
            if (_applicationDataContainer.Values.TryGetValue(name, out object value))
            {
                if (EqualityComparer<T>.Default.Equals(fallbackValue, default))
                {
                    fallbackValue = Activator.CreateInstance<T>();
                }

                JsonConvert.PopulateObject((string)value, fallbackValue);
            }
            return fallbackValue;
        }

        public void SetValue(string key, object value)
        {
            _applicationDataContainer.Values[key] = value;
        }

        public bool TryGetValue(string key, out object value)
        {
            return _applicationDataContainer.Values.TryGetValue(key, out value);
        }

        public void WriteValueAsJson<T>(string name, T value)
        {
            _applicationDataContainer.Values[name] = JsonConvert.SerializeObject(value);
        }
    }
}
