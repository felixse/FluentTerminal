using FluentTerminal.App.Services.Utilities;
using System;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class I18NConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Enum enumValue && parameter is string enumType)
            {
                return I18N.Translate($"{enumType}.{enumValue}");
            }
            else if (parameter is string resource)
            {
                return I18N.Translate(resource);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
