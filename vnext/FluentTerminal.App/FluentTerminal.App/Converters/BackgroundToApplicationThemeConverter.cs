using FluentTerminal.App.Utilities;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI;

namespace FluentTerminal.App.Converters
{
    public class BackgroundToApplicationThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color = Colors.Red;
            if (value is string colorString)
            {
                //color = colorString.ToColor();
            }
            else if (value is Color)
            {
                color = (Color)value;
            }

            return ContrastHelper.GetIdealThemeForBackgroundColor(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}