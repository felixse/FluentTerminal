using Microsoft.Toolkit.Uwp.Helpers;
using System.Globalization;
using Windows.UI;

namespace FluentTerminal.App.Utilities
{
    public static class ColorExtensions
    {
        public static string ToColorString(this Color color, bool allowTransparency)
        {
            if (allowTransparency)
            {
                return $"rgba({color.R:G}, {color.G:G}, {color.B:G}, {ToDoubleString(color.A)})";
            }
            else
            {
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }

        private static string ToDoubleString(byte alpha)
        {
            return (alpha / 256.0).ToString(CultureInfo.InvariantCulture);
        }

        public static Color FromString(this string colorString)
        {
            if (colorString.Contains("rgba("))
            {
                colorString = colorString.Replace("rgba(", string.Empty);
                colorString = colorString.Replace(")", string.Empty);

                var parts = colorString.Split(", ");

                return new Color
                {
                    A = (byte)(int)(double.Parse(parts[3]) * 256.0),
                    R = byte.Parse(parts[0]),
                    G = byte.Parse(parts[1]),
                    B = byte.Parse(parts[2])
                };
            }
            else 
            {
                return colorString.ToColor();
            }
        }
    }
}
