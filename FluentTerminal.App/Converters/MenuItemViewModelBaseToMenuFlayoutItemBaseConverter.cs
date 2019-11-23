using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.ViewModels.Menu;

// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class MenuItemViewModelBaseToMenuFlayoutItemBaseConverter : IValueConverter
    {
        #region Static

        private static readonly KeyBindingViewModelToKeyboardAcceleratorConverter KeyBindingConverter =
            new KeyBindingViewModelToKeyboardAcceleratorConverter();

        private static MenuFlyoutItem GetItem(MenuItemViewModel viewModel)
        {
            var item = new MenuFlyoutItem();

            item.SetBinding(MenuFlyoutItem.TextProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModel.Text)),
                Mode = BindingMode.OneWay
            });
            item.SetBinding(ToolTipService.ToolTipProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModel.Description)),
                Mode = BindingMode.OneWay
            });
            item.SetBinding(MenuFlyoutItem.CommandProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModel.Command)),
                Mode = BindingMode.OneTime
            });

            if (viewModel.KeyBinding is MenuItemKeyBindingViewModel keyBinding)
            {
                item.KeyboardAccelerators.Add((KeyboardAccelerator) KeyBindingConverter.Convert(keyBinding));
            }

            return item;
        }

        #endregion Static

        #region IValueConverter

        // ReSharper disable once AssignNullToNotNullAttribute
        public object Convert(object value, Type targetType = null, object parameter = null, string language = null)
        {
            if (targetType != null && !typeof(MenuFlyoutItemBase).IsAssignableFrom(targetType))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(targetType)} argument value: {targetType}. {typeof(MenuFlyoutItemBase)} expected.",
                    nameof(targetType));
            }

            if (value == null)
            {
                return null;
            }

            return value is MenuItemViewModel menuItemViewModel
                ? GetItem(menuItemViewModel)
                : throw new ArgumentException(
                    $"Invalid {nameof(value)} argument type: {value.GetType()}. {typeof(MenuItemViewModel)} expected.",
                    nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();

        #endregion IValueConverter
    }
}