using FluentTerminal.Models;
using System;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class TabThemeSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TabTheme theme && parameter is string name)
            {
                return theme.Name == name;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
