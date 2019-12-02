using System;
using Windows.System;
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

        private static readonly IconConverter IconConverter = new IconConverter();

        private static MenuFlyoutItemBase GetItem(MenuItemViewModelBase viewModel) =>
            viewModel switch
            {
                ExpandableMenuItemViewModel expandle => GetExpandableItem(expandle),
                MenuItemViewModel regular => GetRegularItem(regular),
                SeparatorMenuItemViewModel _ => GetSeparatorItem(),
                _ => throw new NotImplementedException()
            };

        private static MenuFlyoutSeparator GetSeparatorItem()
        {
            return new MenuFlyoutSeparator();
        }

        private static MenuFlyoutItem GetRegularItem(MenuItemViewModel viewModel)
        {
            var item = new MenuFlyoutItem();

            item.SetBinding(MenuFlyoutItem.TextProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Text)),
                Mode = BindingMode.OneWay
            });

            item.SetBinding(ToolTipService.ToolTipProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Description)),
                Mode = BindingMode.OneWay
            });

            item.SetBinding(MenuFlyoutItem.IconProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Icon)),
                Converter = IconConverter,
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
                item.KeyboardAccelerators?.Add(new KeyboardAccelerator
                {
                    Key = (VirtualKey)keyBinding.Key,
                    Modifiers = (VirtualKeyModifiers)keyBinding.KeyModifiers,
                    IsEnabled = true
                });
            }

            return item;
        }

        private static MenuFlyoutSubItem GetExpandableItem(ExpandableMenuItemViewModel viewModel)
        {
            var item = new MenuFlyoutSubItem();

            item.SetBinding(MenuFlyoutSubItem.TextProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Text)),
                Mode = BindingMode.OneWay
            });

            item.SetBinding(ToolTipService.ToolTipProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Description)),
                Mode = BindingMode.OneWay
            });

            item.SetBinding(MenuFlyoutSubItem.IconProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(MenuItemViewModelBase.Icon)),
                Converter = IconConverter,
                Mode = BindingMode.OneWay
            });

            item.SetBinding(MenuExtension.SubItemsProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(ExpandableMenuItemViewModel.SubItems)),
                Mode = BindingMode.OneWay
            });

            return item;
        }

        #endregion Static

        #region IValueConverter

        // ReSharper disable once AssignNullToNotNullAttribute
        public object Convert(object value, Type targetType = null, object parameter = null, string language = null)
        {
            if (value == null)
            {
                return null;
            }

            return value is MenuItemViewModelBase menuItemViewModel
                ? GetItem(menuItemViewModel)
                : throw new ArgumentException(
                    $"Invalid {nameof(value)} argument type: {value.GetType()}. {typeof(MenuItemViewModelBase)} expected.",
                    nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();

        #endregion IValueConverter
    }
}