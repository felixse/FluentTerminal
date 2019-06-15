using FluentTerminal.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace FluentTerminal.App.Services.Utilities
{
    public class TerminalThemeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(TerminalTheme) && property.PropertyName == nameof(TerminalTheme.PreInstalled))
            {
                property.ShouldSerialize = (_) => false;
            }

            if (property.DeclaringType == typeof(TerminalTheme) && property.PropertyName == nameof(TerminalTheme.Id))
            {
                property.ShouldSerialize = (_) => false;
            }

            return property;
        }
    }
}