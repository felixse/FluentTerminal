using System;
using Windows.UI.Text;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class TrueToBoldConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool bold)
            {
                return bold ? FontWeights.Bold : FontWeights.Normal;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
