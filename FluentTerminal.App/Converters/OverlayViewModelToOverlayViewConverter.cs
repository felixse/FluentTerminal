using System;
using Windows.UI.Xaml.Data;
using FluentTerminal.App.ViewModels;
using FluentTerminal.App.Views;

namespace FluentTerminal.App.Converters
{
    public class OverlayViewModelToOverlayViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            if (value is OverlayViewModel overlayViewModel)
            {
                return new OverlayView(overlayViewModel);
            }

            // ReSharper disable once LocalizableElement
            throw new ArgumentException(
                $"Parameter {nameof(value)} is of type {value.GetType()}. Expected type: {typeof(OverlayViewModel)}.",
                nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}