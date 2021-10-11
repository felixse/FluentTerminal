using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace FluentTerminal.App.Converters
{
    public class IntToVisibilityConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty ElseProperty =
            DependencyProperty.Register(nameof(Else), typeof(Visibility), typeof(IntToVisibilityConverter), new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty IfProperty =
                    DependencyProperty.Register(nameof(If), typeof(int), typeof(IntToVisibilityConverter), new PropertyMetadata(0));

        public static readonly DependencyProperty ThenProperty =
            DependencyProperty.Register(nameof(Then), typeof(Visibility), typeof(IntToVisibilityConverter), new PropertyMetadata(Visibility.Visible));

        public Visibility Else
        {
            get { return (Visibility)GetValue(ElseProperty); }
            set { SetValue(ElseProperty, value); }
        }

        public int If
        {
            get { return (int)GetValue(IfProperty); }
            set { SetValue(IfProperty, value); }
        }

        public Visibility Then
        {
            get { return (Visibility)GetValue(ThenProperty); }
            set { SetValue(ThenProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == If ? Then : Else;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}