using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using FluentTerminal.App.ViewModels;

namespace FluentTerminal.App.Views
{
    // https://stackoverflow.com/questions/33839279/bind-obserable-collection-to-menuflyoutsubitem-in-uwp
    public static class MenuExtension
    {
        public static IEnumerable<ProfileCommandViewModel> GetSubItems(DependencyObject obj) =>
            (IEnumerable<ProfileCommandViewModel>) obj.GetValue(SubItemsProperty);

        public static void SetSubItems(DependencyObject obj, IEnumerable<ProfileCommandViewModel> value) =>
            obj.SetValue(SubItemsProperty, value);

        public static readonly DependencyProperty SubItemsProperty =
            DependencyProperty.Register("SubItems", typeof(IEnumerable<MenuFlyoutItem>), typeof(MenuExtension),
                new PropertyMetadata(new List<ProfileCommandViewModel>(), (sender, e) =>
                {
                    if (sender is MenuFlyoutSubItem subItemsRoot && subItemsRoot.Items != null)
                    {
                        subItemsRoot.Items.Clear();

                        if (e.NewValue is IEnumerable<ProfileCommandViewModel> commands)
                        {
                            foreach (var command in commands)
                            {
                                subItemsRoot.Items.Add(new MenuFlyoutItem {Text = command.Profile.Name, Command = command.Command});
                            }

                            // Hack to enforce changing items in view
                            //subItemsRoot.UpdateLayout();
                            //subItemsRoot.InvalidateArrange();
                            subItemsRoot.InvalidateMeasure();
                        }
                    }
                }));
    }
}