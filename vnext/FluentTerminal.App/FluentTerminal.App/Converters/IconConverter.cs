using FluentTerminal.App.ViewModels.Menu;
using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Mdl2Icon mdl2Icon)
            {
                var fontIcon = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = mdl2Icon.Glyph };

                if(!string.IsNullOrWhiteSpace(mdl2Icon.Color))
                {
                    //fontIcon.Foreground = new SolidColorBrush(mdl2Icon.Color.ToColor());
                }

                return fontIcon;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}