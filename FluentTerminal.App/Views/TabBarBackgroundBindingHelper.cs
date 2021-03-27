using FluentTerminal.Models.Enums;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace FluentTerminal.App.Views
{
    /// <summary>
    /// see: https://stackoverflow.com/questions/33573929/uwp-binding-in-style-setter-not-working
    /// </summary>
    public static class TabBarBackgroundBindingHelper
    {
        public static readonly DependencyProperty BackgroundBindingPathProperty =
            DependencyProperty.RegisterAttached("BackgroundBindingPath", typeof(string), typeof(TabBarBackgroundBindingHelper), new PropertyMetadata(null, BindingPathPropertyChanged));

        public static string GetBackgroundBindingPath(DependencyObject obj)
        {
            return (string)obj.GetValue(BackgroundBindingPathProperty);
        }

        public static void SetBackgroundBindingPath(DependencyObject obj, string value)
        {
            obj.SetValue(BackgroundBindingPathProperty, value);
        }

        private static void BindingPathPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is string propertyPath)
            {
                var binding = new Binding
                {
                    Path = new PropertyPath($"Content.{propertyPath}"),
                    Mode = BindingMode.OneWay,
                    RelativeSource = new RelativeSource
                    {
                        Mode = RelativeSourceMode.Self
                    },
                    Converter = (IValueConverter)Application.Current.Resources["TabColorFallbackConverter"],
                    ConverterParameter = TabThemeKey.Background.ToString()
                };
                BindingOperations.SetBinding(obj, ListViewItem.BackgroundProperty, binding);
            }
        }
    }
}