using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FluentTerminal.App.Services.Utilities
{
    public class PreserveDictionaryKeyCaseContractResolver : DefaultContractResolver
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
            { ContractResolver = new PreserveDictionaryKeyCaseContractResolver() };

        protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            var contract = base.CreateDictionaryContract(objectType);

            contract.DictionaryKeyResolver = key => key;

            return contract;
        }
    }
}
