using System;
using FluentTerminal.App.Utilities;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace FluentTerminal.App.Converters
{
    class ColorTupleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string colorString)
            {
                // Strings that start with a "#" are good to return immediately as a brush with 1.0 opacity
                if (colorString.StartsWith("#"))
                {
                    Color color = colorString.FromString();
                    SolidColorBrush brush = new SolidColorBrush(color);
                    brush.Opacity = 1.0;
                    return brush;
                }
                // If it starts with "?", then that means we need to invoke a function to ask for the colour
                else if (colorString.StartsWith("?"))
                {
                    string func = colorString.Substring(1, 2);
                    string arg = colorString.Substring(4, colorString.Length - 5);
                    Brush retval;

                    switch (func)
                    {
                        case "CH":
                            string[] args = arg.Split(',');
                            string colorKey = args[0];
                            string tabBackgroundColor = (args[1].StartsWith("#") ?
                                args[1] :
                                ((SolidColorBrush)Application.Current.Resources[args[1]]).Color.ToColorString(false));

                            Color brushColor = ContrastHelper.GetColor(
                                    colorKey,
                                    ContrastHelper.GetIdealThemeForBackgroundColor(tabBackgroundColor)) ?? 
                                Color.FromArgb(255, 128, 128, 128);
                            retval = new SolidColorBrush(brushColor);
                            retval.Opacity = 1.0;
                            break;
                        default:
                            retval = null;
                            break;
                    }

                    return retval;
                }
                // Non-colour literals are indicated by a starting "!"
                else if (colorString.StartsWith("!"))
                {
                    string literal = colorString.Substring(1);
                    return literal;
                }
                // Otherwise, look up the color string name in the Resources, as it should refer to a 
                // System color.
                else
                {
                    Brush brush = (Brush)Application.Current.Resources[colorString];
                    return brush;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
