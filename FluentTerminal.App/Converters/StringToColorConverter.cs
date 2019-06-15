using FluentTerminal.App.Utilities;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string colorString)
            {
                return colorString.FromString();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Color color && bool.TryParse((string)parameter, out bool allowTransparency))
            {
                return color.ToColorString(allowTransparency);
            }
            return null;
        }
    }
}