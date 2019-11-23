using System;
using Windows.System;
using Windows.UI.Xaml.Data;
// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class IntToVirtualKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int intValue))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(value)} parameter type: {value?.GetType()}. {typeof(int)} expected.",
                    nameof(value));
            }

            if (targetType != null && targetType != typeof(VirtualKey))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(targetType)} parameter value: {targetType}. {typeof(VirtualKey)} expected.",
                    nameof(targetType));
            }

            return (VirtualKey) intValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is VirtualKey virtualKey))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(value)} parameter type: {value?.GetType()}. {typeof(VirtualKey)} expected.",
                    nameof(value));
            }

            if (targetType != null && targetType != typeof(int))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(targetType)} parameter value: {targetType}. {typeof(int)} expected.",
                    nameof(targetType));
            }

            return (int)virtualKey;
        }
    }
}