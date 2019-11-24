using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using FluentTerminal.App.Services.Utilities;
using FluentTerminal.App.ViewModels.Menu;
using FluentTerminal.App.Views;

namespace FluentTerminal.App.Converters
{
    public class AppMenuViewModelToFlyoutMenuConverter : IValueConverter
    {
        private static void BindMenuItem(MenuFlyoutItem item, MenuItemViewModel viewModel)
        {
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

            if (!(viewModel.KeyBinding is MenuItemKeyBindingViewModel keyBinding))
            {
                return;
            }

            item.KeyboardAccelerators?.Add(new KeyboardAccelerator
            {
                Key = (VirtualKey) keyBinding.Key, Modifiers = (VirtualKeyModifiers) keyBinding.KeyModifiers,
                IsEnabled = true
            });
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return null;
            }

            if (!(value is AppMenuViewModel appMenuViewModel))
            {
                throw new ArgumentException(
                    $"Invalid {nameof(value)} argument type: {value.GetType()}. {typeof(AppMenuViewModel)} expected.");
            }

            var menuFlyout = new MenuFlyout();

            var item = new MenuFlyoutItem();

            BindMenuItem(item, appMenuViewModel.TabMenuItem);

            item.Icon = new SymbolIcon(Symbol.Add);

            menuFlyout.Items?.Add(item);

            item = new MenuFlyoutItem();

            BindMenuItem(item, appMenuViewModel.RemoteTabMenuItem);

            item.Icon = new SymbolIcon(Symbol.Add);

            menuFlyout.Items?.Add(item);

            item = new MenuFlyoutItem();

            BindMenuItem(item, appMenuViewModel.QuickTabMenuItem);

            item.Icon = new SymbolIcon(Symbol.Add);

            menuFlyout.Items?.Add(item);

            item = new MenuFlyoutItem();

            BindMenuItem(item, appMenuViewModel.SettingsMenuItem);

            item.Icon = new SymbolIcon(Symbol.Setting);

            menuFlyout.Items?.Add(item);

            var recent = new MenuFlyoutSubItem
            {
                Text = I18N.TranslateWithFallback("Recent.Text", "Recent")
            };

            recent.SetBinding(MenuExtension.SubItemsProperty, new Binding
            {
                Source = appMenuViewModel, 
                Path = new PropertyPath(nameof(AppMenuViewModel.RecentMenuItems)), 
                Mode = BindingMode.OneWay
            });

            recent.Icon = new FontIcon{FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uF738" };

            menuFlyout.Items?.Add(recent);

            item = new MenuFlyoutItem();

            BindMenuItem(item, appMenuViewModel.AboutMenuItem);

            item.Icon = new FontIcon { FontFamily = new FontFamily("Segoe MDL2 Assets"), Glyph = "\uE946" };

            menuFlyout.Items?.Add(item);

            return menuFlyout;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            throw new NotSupportedException();
    }
}