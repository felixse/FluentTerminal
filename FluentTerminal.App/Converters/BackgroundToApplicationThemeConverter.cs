using FluentTerminal.App.Utilities;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class BackgroundToApplicationThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color;
            if (value is string colorString)
            {
                color = colorString.ToColor();
            } else if (value is Color)
            {
                color = (Color)value;
            }

            if (color != null)
            {
                return ContrastHelper.GetIdealThemeForBackgroundColor(color);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
