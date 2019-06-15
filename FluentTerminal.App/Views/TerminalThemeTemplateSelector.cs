using FluentTerminal.Models;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Views
{
    public class TerminalThemeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DefaultTemplate { get; set; }
        public DataTemplate ThemeTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            return SelectTemplateCore(item);
        }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is TerminalTheme theme)
            {
                return theme.Id == Guid.Empty ? DefaultTemplate : ThemeTemplate;
            }
            return null;
        }
    }
}
