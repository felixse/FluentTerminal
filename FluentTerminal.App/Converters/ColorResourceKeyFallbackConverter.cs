using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Converters
{
    public class ColorResourceKeyFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }
            else if (value is string hex)
            {
                var color = hex.ToColor();
                return new SolidColorBrush(color);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
