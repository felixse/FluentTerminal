using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.App.Views;
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

            item.SetBinding(MenuFlyoutItem.TextProperty, GetTextBinding(viewModel));
            item.SetBinding(ToolTipService.ToolTipProperty, GetDescriptionBinding(viewModel));
            item.SetBinding(MenuFlyoutItem.IconProperty, GetIconBinding(viewModel));
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

        private static MenuFlyoutSubItem GetItem(ExpandableMenuItemViewModel viewModel)
        {
            var item = new MenuFlyoutSubItem();

            item.SetBinding(MenuFlyoutItem.TextProperty, GetTextBinding(viewModel));
            item.SetBinding(ToolTipService.ToolTipProperty, GetDescriptionBinding(viewModel));
            item.SetBinding(MenuFlyoutItem.IconProperty, GetIconBinding(viewModel));
            item.SetBinding(MenuExtension.SubItemsProperty, new Binding
            {
                Source = viewModel, 
                Path = new PropertyPath(nameof(ExpandableMenuItemViewModel.SubItems)), 
                Mode = BindingMode.OneWay
            });

            return item;
        }

        private static Binding GetTextBinding(MenuItemViewModelBase viewModel) => new Binding
        {
            Source = viewModel,
            Path = new PropertyPath(nameof(MenuItemViewModelBase.Text)),
            Mode = BindingMode.OneWay
        };

        private static Binding GetDescriptionBinding(MenuItemViewModelBase viewModel) => new Binding
        {
            Source = viewModel,
            Path = new PropertyPath(nameof(MenuItemViewModelBase.Description)),
            Mode = BindingMode.OneWay
        };

        private static Binding GetIconBinding(MenuItemViewModelBase viewModel) => new Binding
        {
            Source = viewModel,
            Path = new PropertyPath(nameof(MenuItemViewModelBase.Icon)),
            Mode = BindingMode.OneWay
        };

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

            if (value is ExpandableMenuItemViewModel expandableItemViewModel)
            {
                return GetItem(expandableItemViewModel);
            }

            if (value is MenuItemViewModel menuItemViewModel)
            {
                return GetItem(menuItemViewModel);
            }

            throw new ArgumentException(
                $"Invalid {nameof(value)} argument type: {value.GetType()}. {typeof(MenuItemViewModel)} or {typeof(ExpandableMenuItemViewModel)} expected.",
                nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();

        #endregion IValueConverter
    }
}