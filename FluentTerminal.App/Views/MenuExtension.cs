using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.Converters;
using FluentTerminal.App.ViewModels.Menu;

namespace FluentTerminal.App.Views
{
    // https://stackoverflow.com/questions/33839279/bind-obserable-collection-to-menuflyoutsubitem-in-uwp
    public static class MenuExtension
    {
        private static readonly MenuItemViewModelBaseToMenuFlayoutItemBaseConverter Converter =
            new MenuItemViewModelBaseToMenuFlayoutItemBaseConverter();

        public static IEnumerable<MenuItemViewModel> GetSubItems(DependencyObject obj) =>
            (IEnumerable<MenuItemViewModel>) obj.GetValue(SubItemsProperty);

        public static void SetSubItems(DependencyObject obj, IEnumerable<MenuItemViewModel> value) =>
            obj.SetValue(SubItemsProperty, value);

        public static readonly DependencyProperty SubItemsProperty =
            DependencyProperty.Register("SubItems", typeof(IEnumerable<MenuItemViewModel>), typeof(MenuExtension),
                new PropertyMetadata(new List<MenuItemViewModel>(), (sender, e) =>
                {
                    if (sender is MenuFlyoutSubItem menuSubItem)
                    {
                        menuSubItem.Items?.Clear();

                        if (e.NewValue is IEnumerable<MenuItemViewModel> viewModels)
                        {
                            foreach (var item in viewModels.Select(vm => (MenuFlyoutItemBase) Converter.Convert(vm)))
                            {
                                menuSubItem.Items?.Add(item);
                            }

                            // Hack to enforce changing items in view
                            //menuSubItem.UpdateLayout();
                            //menuSubItem.InvalidateArrange();
                            menuSubItem.InvalidateMeasure();
                        }
                    }
                }));
    }
}