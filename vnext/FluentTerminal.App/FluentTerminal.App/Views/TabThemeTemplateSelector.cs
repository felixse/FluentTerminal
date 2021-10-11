using FluentTerminal.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public class TabThemeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate ColoredTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is TabTheme tabTheme)
            {
                return tabTheme.Id == 0 ? DefaultTemplate : ColoredTemplate;
            }
            return null;
        }
    }
}
