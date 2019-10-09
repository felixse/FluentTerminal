using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentTerminal.App.Converters
{
    public class ImageUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new BitmapImage();
            }
            return new BitmapImage(new Uri(value.ToString(), UriKind.Absolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is BitmapImage bitmapImage)
            {
                return bitmapImage.UriSource?.LocalPath;
            }

            return null;
        }
    }
}
