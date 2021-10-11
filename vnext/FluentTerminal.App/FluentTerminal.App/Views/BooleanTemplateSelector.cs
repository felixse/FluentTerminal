using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public class BooleanTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TrueTemplate { get; set; }
        public DataTemplate FalseTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is bool value)
            {
                return value ? TrueTemplate : FalseTemplate;
            }
            return null;
        }
    }
}
