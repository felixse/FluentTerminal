using FluentTerminal.App.Utilities;
using FluentTerminal.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace FluentTerminal.App.Converters
{
    class SetBackgroundThemeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var theme = value as ThemeViewModel;

            if (theme.BackgroundThemeFile != null)
            {
                return new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(
                        theme.BackgroundThemeFile.Path, 
                        UriKind.Absolute))
                };
            }

            return new AcrylicBrush
            {
                BackgroundSource = AcrylicBackgroundSource.HostBackdrop,
                FallbackColor = theme.Background.FromString(),
                TintColor = theme.Background.FromString(),
                TintOpacity = theme.BackgroundOpacity
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
