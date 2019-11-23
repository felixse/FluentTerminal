using System.Collections.Generic;
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

        public static IEnumerable<MenuItemViewModelBase> GetSubItems(DependencyObject obj) =>
            (IEnumerable<MenuItemViewModelBase>) obj.GetValue(SubItemsProperty);

        public static void SetSubItems(DependencyObject obj, IEnumerable<MenuItemViewModelBase> value) =>
            obj.SetValue(SubItemsProperty, value);

        public static readonly DependencyProperty SubItemsProperty =
            DependencyProperty.Register("SubItems", typeof(IEnumerable<MenuItemViewModelBase>), typeof(MenuExtension),
                new PropertyMetadata(new List<MenuItemViewModelBase>(), (sender, e) =>
                {
                    var items = (sender as MenuFlyout)?.Items ?? (sender as MenuFlyoutSubItem)?.Items;

                    if (items != null)
                    {
                        items.Clear();

                        if (e.NewValue is IEnumerable<MenuItemViewModelBase> viewModels)
                        {
                            foreach (var viewModel in viewModels)
                            {
                                items.Add((MenuFlyoutItemBase) Converter.Convert(viewModel));
                            }

                            // Hack to enforce changing items in view
                            //(sender as UIElement)?.UpdateLayout();
                            //(sender as UIElement)?.InvalidateArrange();
                            (sender as UIElement)?.InvalidateMeasure();
                        }
                    }
                }));
    }
}