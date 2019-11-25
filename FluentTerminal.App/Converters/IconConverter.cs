using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            if (value is IconElement icon)
            {
                return icon;
            }

            if (value is Symbol symbol)
            {
                return new SymbolIcon(symbol);
            }

            if (value is int intSymbol)
            {
                if (Enum.IsDefined(typeof(Symbol), intSymbol))
                {
                    return new SymbolIcon((Symbol) intSymbol);
                }

                throw new ArgumentException($"Integer value isn't defined in {nameof(Symbol)} enum.", nameof(value));
            }

            if (value is string glyph)
            {
                return new FontIcon {FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = glyph};
            }

            throw new ArgumentException($"Invalid {nameof(value)} parameter type: {value.GetType()}.", nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}