using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    class TextMiddleEllipsisConverter : IValueConverter
    {
        private TextBlock _textBlock = null;

        private double MeasureWidth(string text)
        {
            if (_textBlock == null)
            {
                _textBlock = new TextBlock
                {
                    Text = text,
                    Margin = new Thickness(12, 8, 6, 0),
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
                };
            }
            else
            {
                _textBlock.Text = text;
            }
            _textBlock.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            return _textBlock.DesiredSize.Width;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is String text)
            {
                const int MaxWidth = 136;
                string result = text;

                var width = MeasureWidth(text);
                if (width > MaxWidth)
                {
                    double oversize = width - MaxWidth;
                    double charSize = width / text.Length;
                    int charsToCut = (int)(oversize / charSize) + 3;
                    int cutStart = text.Length / 2 - charsToCut / 2;
                    int cutEnd = text.Length / 2 + (charsToCut - charsToCut / 2);
                    result = text.Substring(0, cutStart) + ".." + text.Substring(cutEnd);
                    width = MeasureWidth(result);
                    while (width > MaxWidth && cutStart > 0 && cutEnd < text.Length)
                    {
                        result = text.Substring(0, --cutStart) + ".." + text.Substring(++cutEnd);
                        width = MeasureWidth(result);
                    }
                }
                return result;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
