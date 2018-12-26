using System;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class ToolTipValueToPixelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return string.Format("{0} px", value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
