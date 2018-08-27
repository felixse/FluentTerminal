using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class TabColorFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value ?? Application.Current.Resources[(string)parameter];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
