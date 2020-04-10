using FluentTerminal.App.Services.Utilities;
using System;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class I18NConverter : IValueConverter
    {
        // To avoid empty translations
        private static string Translate(string resource)
        {
            var translation = I18N.Translate(resource);

            return string.IsNullOrEmpty(translation) ? $"[ {resource} ]" : translation;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue && parameter is string enumType)
            {
                return Translate($"{enumType}.{enumValue}");
            }
            else if (value is string stringValue)
            {
                return Translate(stringValue);
            }
            else if (parameter is string resource)
            {
                return Translate(resource);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
