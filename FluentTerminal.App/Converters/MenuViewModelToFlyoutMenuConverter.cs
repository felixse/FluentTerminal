using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using FluentTerminal.App.ViewModels.Menu;
using Windows.UI.Xaml.Controls.Primitives;

namespace FluentTerminal.App.Converters
{
    public class MenuViewModelToFlyoutMenuConverter : IValueConverter
    {
        private static readonly MenuItemViewModelBaseToMenuFlayoutItemBaseConverter ItemConverter =
            new MenuItemViewModelBaseToMenuFlayoutItemBaseConverter();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return null;
            }

            if (!(value is MenuViewModel menuViewModel))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(value)} argument type: {value.GetType()}. {typeof(MenuViewModel)} expected.");
            }

            var menuFlyout = new MenuFlyout();

            foreach (var menuItemViewModelBase in menuViewModel.Items)
            {
                menuFlyout.Items?.Add((MenuFlyoutItemBase) ItemConverter.Convert(menuItemViewModelBase));
            }

            return menuFlyout;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}