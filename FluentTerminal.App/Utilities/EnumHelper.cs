using System;
using System.ComponentModel;

namespace FluentTerminal.App.Utilities
{
    public static class EnumHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes?.Length > 0)
            {
                return attributes[0].Description;
            }
            return value.ToString();
        }
    }
}
