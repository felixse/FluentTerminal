using System;
using Windows.UI.Xaml.Data;
// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(bool))
            {
                if (value is bool isValue)
                {
                    return !isValue;
                }

                throw new ArgumentException($"{nameof(value)} argument has to be of type bool.", nameof(value));
            }

            if (targetType == typeof(bool?))
            {
                if (value == null)
                {
                    return null;
                }

                if (value is bool isValue)
                {
                    return !isValue;
                }

                throw new ArgumentException($"{nameof(value)} argument has to be of type bool or Nullable<bool>.", nameof(value));
            }

            throw new ArgumentException($"{nameof(targetType)} argument has to {typeof(bool)}.",
                nameof(targetType));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            Convert(value, targetType, parameter, language);
    }
}