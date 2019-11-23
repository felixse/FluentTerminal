using System;
using Windows.System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.ViewModels.Menu;

namespace FluentTerminal.App.Converters
{
    public class KeyBindingViewModelToKeyboardAcceleratorConverter : IValueConverter
    {
        // ReSharper disable once AssignNullToNotNullAttribute
        public object Convert(object value, Type targetType = null, object parameter = null, string language = null)
        {
            if (targetType != null && targetType != typeof(KeyboardAccelerator))
            {
                // ReSharper disable once LocalizableElement
                throw new ArgumentException(
                    $"Invalid {nameof(targetType)} argument value: {targetType}. {typeof(KeyboardAccelerator)} expected.",
                    nameof(targetType));
            }

            if (value == null)
            {
                return null;
            }

            if (!(value is MenuItemKeyBindingViewModel keyBinding))
            {
                // ReSharper disable once LocalizableElement
                throw new ArgumentException(
                    $"Invalid input argument type: {value.GetType()}. {nameof(MenuItemKeyBindingViewModel)} expected.",
                    nameof(value));
            }

            return new KeyboardAccelerator
            {
                Key = (VirtualKey) keyBinding.Key,
                Modifiers = (VirtualKeyModifiers) keyBinding.KeyModifiers,
                IsEnabled = true
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}