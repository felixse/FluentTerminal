using System;
using FluentTerminal.App.Services.Utilities;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // value is the Enum, parameter is the string.
            if (value is Enum)
            {
                return EnumHelper.GetEnumDescription((Enum)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}
