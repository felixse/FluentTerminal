using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class EnumValueToVisibiltyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.GetType().IsEnum && Enum.TryParse(value.GetType(), parameter.ToString(), true, out object result))
            {
                return Equals(value, result) ? Visibility.Visible : Visibility.Collapsed;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}