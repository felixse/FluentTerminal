using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class FalseToVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Boolean collapsed)
            {
                return collapsed ? Visibility.Collapsed : Visibility.Visible;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Collapsed;
            }
            return null;
        }
    }
}