using FluentTerminal.App.ViewModels.Settings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public class TerminalKeybindTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate KeyBindingsTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is KeyBindingsViewModel keyBindings)
            {
                return keyBindings.KeyBindings.Count == 0 ? DefaultTemplate : KeyBindingsTemplate;
            }
            return null;
        }
    }
}
