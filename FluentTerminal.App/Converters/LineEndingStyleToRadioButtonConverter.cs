using System;
using FluentTerminal.Models.Enums;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    class LineEndingStyleToRadioButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // value is the Enum, parameter is the string.
            if ((value is LineEndingStyle) && (parameter is string))
            {
                LineEndingStyle profileSetting = (LineEndingStyle)value;
                LineEndingStyle paramValue = Enum.Parse<LineEndingStyle>((string)parameter);
                return profileSetting == paramValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((value is bool) && (bool)value)
            {
                return Enum.Parse<LineEndingStyle>((string)parameter);
            }
            return null;
        }
    }
}
