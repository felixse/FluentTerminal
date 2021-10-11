using FluentTerminal.App.ViewModels;
using FluentTerminal.Models.Enums;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace FluentTerminal.App.Converters
{
    public class TabColorFallbackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TabThemeViewModel viewModel && Enum.TryParse<TabThemeKey>((string)parameter, true, out TabThemeKey tabThemeKey))
            {
                var theme = viewModel.Theme;
                switch (tabThemeKey)
                {
                    case TabThemeKey.Background:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundOpacity);
                    case TabThemeKey.BackgroundPointerOver:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundPointerOverOpacity);
                    case TabThemeKey.BackgroundPressed:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundPressedOpacity);
                    case TabThemeKey.BackgroundSelected:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundSelectedOpacity);
                    case TabThemeKey.BackgroundSelectedPointerOver:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundSelectedPointerOverOpacity);
                    case TabThemeKey.BackgroundSelectedPressed:
                        return GetBrush(tabThemeKey, theme.Color, theme.BackgroundSelectedPressedOpacity);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private Brush GetBrush(TabThemeKey tabThemeKey, string color, double opacity)
        {
            if (double.IsNaN(opacity))
            {
                return (Brush)Application.Current.Resources[GetFallbackRessourceKey(tabThemeKey)];
            }
            else
            {
                return CreateBrush(color, opacity);
            }
        }

        private Brush CreateBrush(string hex, double opacity)
        {
            var color = Colors.Red; // hex.ToColor();
            return new SolidColorBrush(color)
            {
                Opacity = opacity
            };
        }

        private string GetFallbackRessourceKey(TabThemeKey tabThemeKey)
        {
            switch (tabThemeKey)
            {
                case TabThemeKey.Background:
                    return "SystemControlTransparentBrush";
                case TabThemeKey.BackgroundPointerOver:
                    return "SystemControlHighlightListLowBrush";
                case TabThemeKey.BackgroundPressed:
                    return "SystemControlHighlightListMediumBrush";
                case TabThemeKey.BackgroundSelected:
                    return "SystemControlHighlightListAccentLowBrush";
                case TabThemeKey.BackgroundSelectedPointerOver:
                    return "SystemControlHighlightListAccentMediumBrush";
                case TabThemeKey.BackgroundSelectedPressed:
                    return "SystemControlHighlightListAccentHighBrush";
            }
            return null;
        }
    }
}
