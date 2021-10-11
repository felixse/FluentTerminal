using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.App.Views;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation.Metadata;

// ReSharper disable LocalizableElement

namespace FluentTerminal.App.Converters
{
    public class MenuItemViewModelBaseToMenuFlayoutItemBaseConverter : IValueConverter
    {
        #region Static

        private static readonly IconConverter IconConverter = new IconConverter();

        private static MenuFlyoutItemBase GetItem(MenuItemViewModelBase viewModel)
        {
            if (viewModel is ExpandableMenuItemViewModel expandable)
            {
                return GetExpandableItem(expandable);
            }

            if (viewModel is SeparatorMenuItemViewModel)
            {
                return GetSeparatorItem();
            }

            if (viewModel is MenuItemViewModel regular)
            {
                return GetRegularItem(regular);
            }

            if (viewModel is ToggleMenuItemViewModel toggle)
            {
                return GetToggleItem(toggle);
            }

            if (viewModel is RadioMenuItemViewModel radio)
            {
                return GetRadioItem(radio);
            }

            // Won't happen ever, but still...
            throw new NotImplementedException(
                $"Unexpected {nameof(MenuItemViewModelBase)} type: {viewModel.GetType()}");
        }

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

            if (viewModel.KeyBinding != null)
            {
                if (viewModel.KeyBinding.IsExtendedVirtualKey)
                {
                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 6))
                    {
                        item.KeyboardAcceleratorTextOverride = viewModel.KeyBinding.GetOverrideText();
                    }
                }
                else
                {
                    item.KeyboardAccelerators?.Add(new KeyboardAccelerator
                    {
                        Key = (VirtualKey)viewModel.KeyBinding.Key,
                        Modifiers = (VirtualKeyModifiers)viewModel.KeyBinding.KeyModifiers,
                        IsEnabled = true
                    });
                }
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

        private static ToggleMenuFlyoutItem GetToggleItem(ToggleMenuItemViewModel viewModel)
        {
            var item = new ToggleMenuFlyoutItem();

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

            item.SetBinding(ToggleMenuFlyoutItem.IsCheckedProperty, new Binding
            {
                Source = viewModel.BindingSource,
                Path = new PropertyPath(viewModel.BindingPath),
                Mode = BindingMode.TwoWay
            });

            return item;
        }

        private static RadioMenuFlyoutItem GetRadioItem(RadioMenuItemViewModel viewModel)
        {
            var item = new RadioMenuFlyoutItem();

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

            item.SetBinding(RadioMenuFlyoutItem.GroupNameProperty, new Binding
            {
                Source = viewModel,
                Path = new PropertyPath(nameof(RadioMenuItemViewModel.GroupName)),
                Mode = BindingMode.OneWay
            });

            item.SetBinding(RadioMenuFlyoutItem.IsCheckedProperty, new Binding
            {
                Source = viewModel.BindingSource,
                Path = new PropertyPath(viewModel.BindingPath),
                Mode = BindingMode.TwoWay
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